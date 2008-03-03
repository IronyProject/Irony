#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace Irony.Compiler {
  
  // This class contains all complex logic of extracting Parser/Scanner's DFA tables and other control information
  // from the language grammar. 
  // Warning: unlike other classes in this project, understanding what's going on here requires some knowledge of 
  // LR/LALR parsing algorithms. Go read the Dragon book or other compiler construction book.
  public class GrammarDataBuilder {
    class ShiftTable : Dictionary<string, LR0ItemList> { }
    private ParserStateTable _stateHash;
    private GrammarData _data;
    Grammar _grammar;


    public GrammarDataBuilder() {
    }
    public GrammarData Build(Grammar grammar) {
      _grammar = grammar;
      _data = new GrammarData();
      _data.Grammar = grammar;

      try {
        _data.ScannerRecoverySymbols = grammar.WhitespaceChars + grammar.Delimiters;
        if (grammar.Root == null) {
          AddError("Root property of the grammar is not set.");
          Cancel();
        }
        //Create the augmented root for the grammar
        _data.AugmentedRoot = new NonTerminal(grammar.Root.Name + "'", new BnfExpression(grammar.Root));
        //Collect all terminals and non-terminals into corresponding collections
        CollectAllElements();
        //Adjust case for Symbols for case-insensitive grammar (change keys to lowercase)
        if (!_grammar.CaseSensitive)
          AdjustCaseForSymbols();
        //Build hash table of terminals for fast lookup by current input char
        BuildTerminalsLookupTable();
        //Create productions and LR0Items 
        CreateProductions();
        //Calculate nullability, Firsts and TailFirsts collections of all non-terminals
        CalculateNullability();
        CalculateFirsts();
        CalculateTailFirsts();
        //Create parser states list, including initial and final states 
        CreateParserStates();
        //Propagate Lookaheads
        PropagateLookaheads();
        //now run through all states and create Reduce actions
        CreateReduceActions();
        //finally check for conflicts and detect Operator-based actions
        CheckActionConflicts();
        //call Init on all elements in the grammar
        InitAll();
      } catch(Exception e) {
        _data.Errors.Add(e.ToString());
        Trace.Write(e.ToString());
        //throw;// Cancel();
      }
      return _data;
    }//method

    private void Cancel() {
      _data.AnalysisCanceled = true; 
      throw new ApplicationException("Grammar analysis canceled.");
    }
    #region Collecting non-terminals
    int _unnamedCount; //internal counter for generating names for unnamed non-terminals
    private void CollectAllElements() {
      _data.NonTerminals.Clear();
      _data.Terminals.Clear();
      _data.Terminals.AddRange(_grammar.ExtraTerminals);
      _unnamedCount = 0;
      CollectAllElementsRecursive(_data.AugmentedRoot);
      _data.Terminals.Sort(Terminal.ByName);
      if (_data.AnalysisCanceled)
        Cancel();
    }

    private void CollectAllElementsRecursive(BnfElement element) {
      //Terminal
      Terminal term = element as Terminal;
      // Do not add pseudo terminals defined as static singletons in Grammar class (Empty, Eof, etc)
      //  We will never see these terminals in the input stream.
      //   Filter them by type - their type is exactly "Terminal", not derived class. 
      if (term != null && !_data.Terminals.Contains(term) && term.GetType() != typeof(Terminal)) {
        _data.Terminals.Add(term);
        return;
      }
      //NonTerminal
      NonTerminal nt = element as NonTerminal;
      if (nt == null || _data.NonTerminals.Contains(nt))
        return;
      if (nt.Name == null)
        nt.Name = "$NT" + (_unnamedCount++);
      _data.NonTerminals.Add(nt);
      if (nt.Rule == null) {
        AddError("Non-terminal {0} has uninitialized Rule property.", nt.Name);
        _data.AnalysisCanceled = true;
        return;
      }
      //check all child elements
      foreach(BnfElementList elemList in nt.Rule.Data)
        for(int i = 0; i < elemList.Count; i++) {
          BnfElement child = elemList[i];
          //Check for nested expression - convert to non-terminal
          BnfExpression expr = child as BnfExpression;
          if (expr != null) {
            child = new NonTerminal(null, expr);
            elemList[i] = child;
          }
          CollectAllElementsRecursive(child);
        }
    }//method

    private void AdjustCaseForSymbols() {
      if (_grammar.CaseSensitive) return;
      foreach (Terminal term in _data.Terminals)
        if (term is SymbolTerminal)
          term.Key = term.Key.ToLower();
    }

    private void BuildTerminalsLookupTable() {
      _data.TerminalsLookup.Clear();
      _data.TerminalsWithoutPrefixes.Clear();
      foreach (Terminal term in _data.Terminals) {
        IList<string> prefixes = term.GetStartSymbols();
        if (prefixes == null || prefixes.Count == 0) {
          _data.TerminalsWithoutPrefixes.Add(term);
          continue;
        }
        //Go through prefixes one-by-one
        foreach (string prefix in prefixes) {
          if (string.IsNullOrEmpty(prefix)) continue;
          //Calculate hash key for the prefix
          char hashKey = prefix[0];
          if (!_grammar.CaseSensitive)
            hashKey = char.ToLower(hashKey);
          TerminalList currentList;
          if (!_data.TerminalsLookup.TryGetValue(hashKey, out currentList)) {
            //if list does not exist yet, create it
            currentList = new TerminalList();
            _data.TerminalsLookup[hashKey] = currentList;
          }
          //add terminal to the list
          currentList.Add(term);
        }
      }//foreach term
      //Now add _noPrefixTerminals to every list in table
      if (_data.TerminalsWithoutPrefixes.Count > 0)
        foreach (TerminalList list in _data.TerminalsLookup.Values)
          list.AddRange(_data.TerminalsWithoutPrefixes);
      //Sort all terminal lists by reverse priority, so that terminal with higher priority comes first in the list
      foreach (TerminalList list in _data.TerminalsLookup.Values)
        if (list.Count > 1)
          list.Sort(Terminal.ByPriorityReverse);
    }//method


    #endregion

    #region Creating Productions
    private void CreateProductions() {
      _data.Productions.Clear();
      //each LR0Item gets its unique ID, last assigned (max) Id is kept in static field
      LR0Item._maxID = 0; 
      foreach(NonTerminal nt in _data.NonTerminals) {
        //create productions only once; they may have been created before, in case of cached singletons like "NewLine?"
        if (nt.Productions.Count > 0) continue;
        //Get data (sequences) from both Rule and ErrorRule
        BnfExpressionData allData = new BnfExpressionData();
        allData.AddRange(nt.Rule.Data);
        if (nt.ErrorRule != null) 
          allData.AddRange(nt.ErrorRule.Data);
        //actually create productions for each sequence
        foreach (BnfElementList prodOperands in allData) {
          bool isInitial = (nt == _data.AugmentedRoot);
          Production prod = new Production(isInitial, nt, prodOperands);
          nt.Productions.Add(prod);
          _data.Productions.Add(prod);
        }//foreach prodOperands
      }
    }
    #endregion

    #region Nullability calculation
    private void CalculateNullability() {
      NonTerminalList undecided = _data.NonTerminals;
      while(undecided.Count > 0) {
        NonTerminalList newUndecided = new NonTerminalList();
        foreach(NonTerminal nt in undecided)
          if (!CalculateNullability(nt, undecided))
           newUndecided.Add(nt);
        if (undecided.Count == newUndecided.Count) return;  //we didn't decide on any new, so we're done
        undecided = newUndecided;
      }//while
    }

    private bool CalculateNullability(NonTerminal nonTerminal, NonTerminalList undecided) {
      foreach (Production prod in nonTerminal.Productions) {
        //If production has terminals, it is not nullable and cannot contribute to nullability
        if (prod.HasTerminals)   continue;
        if (prod.IsEmpty()) {
          nonTerminal.Nullable = true;
          return true; //Nullable
        }//if 
        //Go thru all elements of production and check nullability
        bool allNullable = true;
        foreach (BnfElement elem in prod.RValues) {
          NonTerminal nt = elem as NonTerminal;
          if (nt != null)
            allNullable &= nt.Nullable;
        }//foreach nt
        if (allNullable) {
          nonTerminal.Nullable = true;
          return true;
        }
      }//foreach prod
      return false; //cannot decide
    }
    #endregion

    #region Calculating Firsts
    private void CalculateFirsts() {
      //1. Calculate PropagateTo lists and put initial terminals into Firsts lists
      foreach (Production prod in _data.Productions) {
        foreach (BnfElement oper in prod.RValues) {
          NonTerminal ntOper = oper as NonTerminal;
          if (ntOper == null) { //it is terminal, so add it to Firsts and that's all with this production
            prod.LValue.Firsts.Add(oper.Key); // Add terminal to Firsts (note: Add ignores repetitions)
            break; //from foreach oper
          }//if ntOper == null
          if (!ntOper.PropagateFirstsTo.Contains(prod.LValue))
            ntOper.PropagateFirstsTo.Add(prod.LValue); //ignores repetitions
          if (!ntOper.Nullable) break; //if not nullable we're done
        }//foreach oper
      }//foreach prod
      
      //2. Propagate all firsts thru all dependencies
      NonTerminalList workList = _data.NonTerminals;
      while (workList.Count > 0) {
        NonTerminalList newList = new NonTerminalList();
        foreach (NonTerminal nt in workList) {
          foreach (NonTerminal toNt in nt.PropagateFirstsTo)
            foreach (string symbolKey in nt.Firsts) {
              if (!toNt.Firsts.Contains(symbolKey)) {
                toNt.Firsts.Add(symbolKey);
                if (!newList.Contains(toNt))
                  newList.Add(toNt);
              }//if
            }//foreach symbolKey
        }//foreach nt in workList
        workList = newList;
      }//while
    }//method

    #endregion

    #region Calculating Tail Firsts
    private void CalculateTailFirsts() {
      foreach (Production prod in _data.Productions) {
        KeyList accumulatedFirsts = new KeyList();
        bool allNullable = true;
        //We are going backwards in LR0Items list
        for(int i = prod.LR0Items.Count-1; i >= 0; i--) {
          LR0Item item = prod.LR0Items[i];
          if (i >= prod.LR0Items.Count-2) {
            //Last and before last items have empty tails
            item.TailIsNullable = true;
            item.TailFirsts.Clear();
            continue;
          }
          BnfElement elem = prod.RValues[item.Position + 1];  //Element after-after-dot
          NonTerminal ntElem = elem as NonTerminal;
          if (ntElem == null || !ntElem.Nullable) { //elem is a terminal or non-nullable NonTerminal
            //Elem is not nullable, so we clear all old firsts and add this elem
            accumulatedFirsts.Clear();
            allNullable = false;
            item.TailIsNullable = false;   
            if (ntElem == null) {
              item.TailFirsts.Add(elem.Key);//elem is terminal so add its key
              accumulatedFirsts.Add(elem.Key);
            } else {
              item.TailFirsts.AddRange(ntElem.Firsts); //nonterminal
              accumulatedFirsts.AddRange(ntElem.Firsts);
            }
            continue;
          }
          //if we are here, then ntElem is a nullable NonTerminal. We add 
          accumulatedFirsts.AddRange(ntElem.Firsts);
          item.TailFirsts.AddRange(accumulatedFirsts);
          item.TailIsNullable = allNullable;
        }//for i
      }//foreach prod
    }//method

    #endregion

    #region Creating parser states
    private void CreateParserStates() {
      _data.States.Clear();
      _stateHash = new ParserStateTable();
      //Create initial state
      //there is always just one initial production Root' -> Root + LF, and we're interested in LR item at 0 index
      LR0ItemList itemList = new LR0ItemList();
      itemList.Add(_data.AugmentedRoot.Productions[0].LR0Items[0]);
      _data.InitialState = FindOrCreateState(itemList); //it is actually create
      _data.InitialState.Items[0].Lookaheads.Add(Grammar.Eof.Key);
      //create final state - we need to create it explicitly to assign to _data.FinalState property
      // final state is based on the same initial production, but different LRItem - the one with dot AFTER the root nonterminal.
      // it is item at index 1. 
      itemList = new LR0ItemList();
      itemList.Add(_data.AugmentedRoot.Productions[0].LR0Items[1]);
      _data.FinalState = FindOrCreateState(itemList);

      // Iterate through states (while new ones are created) and create shift transitions and new states 
      for (int index = 0; index < _data.States.Count; index++) {
        ParserState state = _data.States[index];
        AddClosureItems(state);
        //Get keys of all possible shifts
        ShiftTable shiftTable = GetStateShifts(state);
        //Each key in shifts dict is an input element 
        // Value is LR0ItemList of shifted LR0Items for this input element.
        foreach (string input in shiftTable.Keys) {
          LR0ItemList shiftedCoreItems = shiftTable[input];
          ParserState newState = FindOrCreateState(shiftedCoreItems);
          state.Actions[input] = new ActionRecord(input, newState);
          //link original LRItems in original state to derived LRItems in newState
          foreach (LR0Item coreItem in shiftedCoreItems) {
            LRItem fromItem = FindItem(state, coreItem.Production, coreItem.Position - 1);
            LRItem toItem = FindItem(newState, coreItem.Production, coreItem.Position);
            fromItem.PropagateTargets.Add(toItem);
          }//foreach coreItem
        }//foreach input
      } //for index
    }//method

    private string AdjustCase(string key) {
      return _grammar.CaseSensitive ? key : key.ToLower();
    }
    private LRItem TryFindItem(ParserState state, LR0Item core) {
      foreach (LRItem item in state.Items)
        if (item.Core == core)
          return item;
      return null;
    }//method

    private LRItem FindItem(ParserState state, Production production, int position) {
      foreach(LRItem item in state.Items) 
        if (item.Core.Production == production && item.Core.Position == position)
          return item;
      string msg = string.Format("Failed to find an LRItem in state {0} by production [{1}] and position {2}. ",
        state, production.ToString(), position.ToString());
      throw new ApplicationException(msg);
    }//method

    private ShiftTable GetStateShifts(ParserState state) {
      ShiftTable shifts = new ShiftTable();
      LR0ItemList list;
      foreach (LRItem item in state.Items) {
        BnfElement elem = item.Core.NextElement;
        if (elem == null)  continue;
        LR0Item shiftedItem = item.Core.Production.LR0Items[item.Core.Position + 1];
        if (!shifts.TryGetValue(elem.Key, out list))
          shifts[elem.Key] = list = new LR0ItemList();
        list.Add(shiftedItem);
      }//foreach
      return shifts;
    }//method

    private ParserState FindOrCreateState(LR0ItemList lr0Items) {
      string key = CalcItemListKey(lr0Items);
      if(_stateHash.ContainsKey(key))
        return _stateHash[key];
      ParserState newState = new ParserState("S" + _data.States.Count, lr0Items);
      _data.States.Add(newState);
      _stateHash[key] = newState;
      return newState;
    }

    //Creates closure items with "spontaneously generated" lookaheads
    private bool AddClosureItems(ParserState state) {
      bool result = false;
      //note that we change collection while we iterate thru it, so we have to use "for i" loop
      for(int i = 0; i < state.Items.Count; i++) {
        LRItem item = state.Items[i];
        NonTerminal nextNT = item.Core.NextElement as NonTerminal;
        if (nextNT == null)  continue;
        //1. Add normal closure items
        foreach (Production prod in nextNT.Productions) {
          LR0Item core = prod.LR0Items[0]; //item at zero index is the one that starts with dot
          LRItem newItem = TryFindItem(state, core);
          if (newItem == null) {
            newItem = new LRItem(core);
            state.Items.Add(newItem);
            result = true;
          }
          #region Comments on lookaheads processing
          // The general idea of generating ("spontaneously") the lookaheads is the following:
          // Let's the original item be in the form 
          //   [A -> alpha . B beta , lset]
          //     where <B> is a non-terminal and <lset> is a set of lookaheads, 
          //      <beta> is some string (B's tail in our terminology)
          // Then the closure item on non-teminal B is an item
          //   [B -> x, firsts(beta + lset)]
          //      (the lookahead set is expression after the comma).
          // To generate lookaheads on a closure item, we simply take "firsts" 
          //   from the tail <beta> of the NonTerminal <B>. 
          //   Normally if tail <beta> is nullable we would add ("propagate") 
          //   the <lset> lookaheads from <A> to <B>.
          //   We dont' do it right here - we simply add a propagation link.
          //   We propagate all lookaheads later in a separate process.
          #endregion
          newItem.Lookaheads.AddRange(item.Core.TailFirsts);
          if (item.Core.TailIsNullable)
            item.PropagateTargets.Add(newItem);
        }//foreach prod
      }//for i (LRItem)
      return result;
    }


    #region comments
    //Parser states are distinguished by the subset of kernel LR0 items. 
    // So when we derive new LR0-item list by shift operation, 
    // we need to find out if we have already a state with the same LR0Item list.
    // We do it by looking up in a state hash by a key - [LR0 item list key]. 
    // Each list's key is a concatenation of items' IDs separated by ','.
    // Before producing the key for a list, the list must be sorted; 
    //   thus we garantee one-to-one correspondence between LR0Item sets and keys.
    // And of course, we count only kernel items (with dot NOT in the first position).
    #endregion
    private string CalcItemListKey(LR0ItemList items) {
      items.Sort(); //Sort by ID
      if (items.Count == 0) return "";
      //quick shortcut
      if (items.Count == 1 && items[0].IsKernel) 
        return items[0].ID.ToString();
      StringBuilder sb = new StringBuilder(1024);
      foreach (LR0Item item in items) {
        if (item.IsKernel) {
          sb.Append(item.ID);
          sb.Append(",");
        }
      }//foreach
      return sb.ToString();
    }
    #endregion

    #region Lookaheads propagation
    private void PropagateLookaheads() {
      LRItemList currentList = new LRItemList();
      //first collect all items
      foreach (ParserState state in _data.States)
        currentList.AddRange(state.Items);
      //Main loop - propagate until done
      while (currentList.Count > 0) {
        LRItemList newList = new LRItemList();
        foreach (LRItem item in currentList) {
          foreach (LRItem targetItem in item.PropagateTargets) {
            int oldCount = targetItem.Lookaheads.Count;
            targetItem.Lookaheads.AddRange(item.Lookaheads);
            //if count changed, we added new lookaheads, so we need to process targetItem in next round
            if (targetItem.Lookaheads.Count > oldCount && !newList.Contains(targetItem))
              newList.Add(targetItem);
          }//foreach targetItem
        }
        currentList = newList; 
      }//while         
    }//method
    #endregion

    #region Final actions: createReduceActions
    private void CreateReduceActions() {
      foreach(ParserState state in _data.States) {
        foreach (LRItem item in state.Items) {
          //we are interested only in "dot  at the end" items
          if (item.Core.NextElement != null)   continue;
          foreach (string lookahead in item.Lookaheads) {
            ActionRecord action;
            if (state.Actions.TryGetValue(lookahead, out action)) 
              action.ReduceProductions.Add(item.Core.Production);
            else
              state.Actions[lookahead] = new ActionRecord(lookahead, item.Core.Production);
          }//foreach lookahead
        }//foreach item
      }// foreach state
    } //method
    
    #endregion

    #region Check for shift-reduce conflicts
    private void CheckActionConflicts() {
      StringDictionary errorTable = new StringDictionary();
      foreach (ParserState state in _data.States) {
        foreach (ActionRecord action in state.Actions.Values) {
          //1. Pure shift
          if (action.NewState != null && action.ReduceProductions.Count == 0)
            continue; //ActionType is shift by default
          //2. Pure reduce
          if (action.NewState == null && action.ReduceProductions.Count == 1) {
            action.ActionType = ParserActionType.Reduce; 
            continue;
          }
          //3. Shift-reduce conflict
          if (action.NewState != null && action.ReduceProductions.Count > 0) {
            //it might be an operation, with resolution by precedence/associativity
            SymbolTerminal opTerm = SymbolTerminal.GetSymbol(action.Key);
            if (opTerm != null && opTerm.IsFlagSet(BnfFlags.IsOperator)) {
              action.ActionType = ParserActionType.Operator;
            } else {
                AddErrorForInput(errorTable, action.Key, "Shift-reduce conflict in state {0}, reduce production: {1}",
                    state, action.ReduceProductions[0]); 
              //NOTE: don't do "continue" here, we need to proceed to reduce-reduce conflict check
            }//if...else
          }//if action....
          //4. Reduce-reduce conflicts
          if (action.ReduceProductions.Count > 1) {
            AddErrorForInput(errorTable, action.Key, "Reduce-reduce conflict in state {0} in productions: {1} ; {2}",
                state, action.ReduceProductions[0], action.ReduceProductions[1]);
          }

        }//foreach action
      }//foreach state
      //copy errors to Errors collection; In errorTable keys are error messages, values are inputs for this message 
      foreach (string msg in errorTable.Keys) {
        _data.Errors.Add(msg + " on inputs: " + errorTable[msg]);
      }
    }//methods

    //Aggregate error messages for different inputs (lookaheads) in errors dictionary
    private void AddErrorForInput(StringDictionary errors, string input, string template, params object[] args) {
      string msg = string.Format(template, args);
      string tmpInputs;
      errors.TryGetValue(msg, out tmpInputs);
      errors[msg] = tmpInputs + input + " ";
    }

    private bool ContainsProduction(ProductionList productions, NonTerminal nonTerminal) {
      foreach (Production prod in productions)
        if (prod.LValue == nonTerminal) return true;
      return false;
    }
    #endregion

    #region Initialize elements
    private void InitAll() {
      foreach (Terminal term in _data.Terminals)
        term.Init(_grammar);
      foreach (NonTerminal nt in _data.NonTerminals)
        nt.Init(_grammar);
      foreach (TokenFilter filter in _grammar.TokenFilters)
        filter.Init(_grammar);
    }
    #endregion

    #region error handling: AddError
    private void AddError(string message, params object[] args) {
      if (args != null && args.Length > 0)
        message = string.Format(message, args);
      _data.Errors.Add(message);
    }
    #endregion

  }//class


}//namespace

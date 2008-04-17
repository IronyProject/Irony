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
  // LR/LALR parsing algorithms. For this I refer you to the Dragon book or any other book on compiler/parser construction.
  public class GrammarDataBuilder {
    class ShiftTable : Dictionary<string, LR0ItemList> { }
    private ParserStateTable _stateHash;
    public readonly GrammarData Data;
    Grammar _grammar;


    public GrammarDataBuilder(Grammar grammar) {
      _grammar = grammar;
      Data = new GrammarData();
      Data.Grammar = _grammar;
    }
    public void Build() {
      try {
        Data.ScannerRecoverySymbols = _grammar.WhitespaceChars + _grammar.Delimiters;
        if (_grammar.Root == null) 
          Cancel("Root property of the grammar is not set.");
        //Create the augmented root for the grammar
        Data.AugmentedRoot = new NonTerminal(_grammar.Root.Name + "'", new BnfExpression(_grammar.Root));
        //Collect all terminals and non-terminals into corresponding collections
        CollectAllElements();
        //Adjust case for Symbols for case-insensitive grammar (change keys to lowercase)
        if (!_grammar.CaseSensitive)
          AdjustCaseForSymbols();
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
        //Debug.WriteLine("Time of PropagateLookaheads: " + time);
        //now run through all states and create Reduce actions
        CreateReduceActions();
        //finally check for conflicts and detect Operator-based actions
        CheckActionConflicts();
        //call Init on all elements in the grammar
        InitAll();
        //Build hash table of terminals for fast lookup by current input char; note that this must be run after Init
        BuildTerminalsLookupTable();
        //Validate
        ValidateAll();
      } catch (GrammarErrorException e) {
        Data.Errors.Add(e.Message);
        Data.AnalysisCanceled = true; 
      }
    }//method

    private void Cancel(string msg) {
      if (msg == null) msg = "Grammar analysis canceled.";
      throw new GrammarErrorException(msg);
    }

    #region Collecting non-terminals
    int _unnamedCount; //internal counter for generating names for unnamed non-terminals
    private void CollectAllElements() {
      Data.NonTerminals.Clear();
      Data.Terminals.Clear();
      //set IsNonGrammar flag in all NonGrammarTerminals and add them to Terminals collection
      foreach (Terminal t in _grammar.NonGrammarTerminals) {
        t.SetOption(TermOptions.IsNonGrammar);
        Data.Terminals.Add(t);
      }
      _unnamedCount = 0;
      CollectAllElementsRecursive(Data.AugmentedRoot);
      Data.Terminals.Sort(Terminal.ByName);
      if (Data.AnalysisCanceled)
        Cancel(null);
    }

    private void CollectAllElementsRecursive(BnfTerm element) {
      //Terminal
      Terminal term = element as Terminal;
      // Do not add pseudo terminals defined as static singletons in Grammar class (Empty, Eof, etc)
      //  We will never see these terminals in the input stream.
      //   Filter them by type - their type is exactly "Terminal", not derived class. 
      if (term != null && !Data.Terminals.Contains(term) && term.GetType() != typeof(Terminal)) {
        Data.Terminals.Add(term);
        return;
      }
      //NonTerminal
      NonTerminal nt = element as NonTerminal;
      if (nt == null || Data.NonTerminals.Contains(nt))
        return;

      if (nt.Name == null) {
        if (nt.Rule != null && !string.IsNullOrEmpty(nt.Rule.Name))
          nt.Name = nt.Rule.Name;
        else 
          nt.Name = "NT" + (_unnamedCount++);
      }
      Data.NonTerminals.Add(nt);
      if (nt.Rule == null) {
        AddError("Non-terminal {0} has uninitialized Rule property.", nt.Name);
        Data.AnalysisCanceled = true;
        return;
      }
      //check all child elements
      foreach(BnfTermList elemList in nt.Rule.Data)
        for(int i = 0; i < elemList.Count; i++) {
          BnfTerm child = elemList[i];
          if (child == null) {
            AddError("Rule for NonTerminal {0} contains null as an operand in position {1} in one of productions.", nt, i);
            continue; //for i loop 
          }
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
      foreach (Terminal term in Data.Terminals)
        if (term is SymbolTerminal)
          term.Key = term.Key.ToLower();
    }

    private void BuildTerminalsLookupTable() {
      Data.TerminalsLookup.Clear();
      Data.FallbackTerminals.AddRange(Data.Grammar.FallbackTerminals);
      foreach (Terminal term in Data.Terminals) {
        IList<string> prefixes = term.GetFirsts();
        if (prefixes == null || prefixes.Count == 0) {
          if (!Data.FallbackTerminals.Contains(term))
            Data.FallbackTerminals.Add(term);
          continue; //foreach term
        }
        //Go through prefixes one-by-one
        foreach (string prefix in prefixes) {
          if (string.IsNullOrEmpty(prefix)) continue;
          //Calculate hash key for the prefix
          char hashKey = prefix[0];
          if (!_grammar.CaseSensitive)
            hashKey = char.ToLower(hashKey);
          TerminalList currentList;
          if (!Data.TerminalsLookup.TryGetValue(hashKey, out currentList)) {
            //if list does not exist yet, create it
            currentList = new TerminalList();
            Data.TerminalsLookup[hashKey] = currentList;
          }
          //add terminal to the list
          currentList.Add(term);
        }
      }//foreach term
      //Sort all terminal lists by reverse priority, so that terminal with higher priority comes first in the list
      foreach (TerminalList list in Data.TerminalsLookup.Values)
        if (list.Count > 1)
          list.Sort(Terminal.ByPriorityReverse);
    }//method


    #endregion

    #region Creating Productions
    private void CreateProductions() {
      Data.Productions.Clear();
      //each LR0Item gets its unique ID, last assigned (max) Id is kept in static field
      LR0Item._maxID = 0; 
      foreach(NonTerminal nt in Data.NonTerminals) {
        nt.Productions.Clear();
        //Get data (sequences) from both Rule and ErrorRule
        BnfExpressionData allData = new BnfExpressionData();
        allData.AddRange(nt.Rule.Data);
        if (nt.ErrorRule != null) 
          allData.AddRange(nt.ErrorRule.Data);
        //actually create productions for each sequence
        foreach (BnfTermList prodOperands in allData) {
          bool isInitial = (nt == Data.AugmentedRoot);
          Production prod = new Production(isInitial, nt, prodOperands);
          nt.Productions.Add(prod);
          Data.Productions.Add(prod);
        }//foreach prodOperands
      }
    }
    #endregion

    #region Nullability calculation
    private void CalculateNullability() {
      NonTerminalList undecided = Data.NonTerminals;
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
        foreach (BnfTerm term in prod.RValues) {
          NonTerminal nt = term as NonTerminal;
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
      foreach (Production prod in Data.Productions) {
        foreach (BnfTerm term in prod.RValues) {
          if (term is Terminal) { //it is terminal, so add it to Firsts and that's all with this production
            prod.LValue.Firsts.Add(term.Key); // Add terminal to Firsts (note: Add ignores repetitions)
            break; //from foreach term
          }//if
          NonTerminal nt = term as NonTerminal;
          if (!nt.PropagateFirstsTo.Contains(prod.LValue))
            nt.PropagateFirstsTo.Add(prod.LValue); //ignores repetitions
          if (!nt.Nullable) break; //if not nullable we're done
        }//foreach oper
      }//foreach prod
      
      //2. Propagate all firsts thru all dependencies
      NonTerminalList workList = Data.NonTerminals;
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
      foreach (Production prod in Data.Productions) {
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
          BnfTerm term = prod.RValues[item.Position + 1];  //Element after-after-dot
          NonTerminal ntElem = term as NonTerminal;
          if (ntElem == null || !ntElem.Nullable) { //term is a terminal or non-nullable NonTerminal
            //term is not nullable, so we clear all old firsts and add this term
            accumulatedFirsts.Clear();
            allNullable = false;
            item.TailIsNullable = false;   
            if (ntElem == null) {
              item.TailFirsts.Add(term.Key);//term is terminal so add its key
              accumulatedFirsts.Add(term.Key);
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
      Data.States.Clear();
      _stateHash = new ParserStateTable();
      //Create initial state
      //there is always just one initial production Root' -> Root + LF, and we're interested in LR item at 0 index
      LR0ItemList itemList = new LR0ItemList();
      itemList.Add(Data.AugmentedRoot.Productions[0].LR0Items[0]);
      Data.InitialState = FindOrCreateState(itemList); //it is actually create
      Data.InitialState.Items[0].NewLookaheads.Add(Grammar.Eof.Key);
      //create final state - we need to create it explicitly to assign to _data.FinalState property
      // final state is based on the same initial production, but different LRItem - the one with dot AFTER the root nonterminal.
      // it is item at index 1. 
      itemList = new LR0ItemList();
      itemList.Add(Data.AugmentedRoot.Productions[0].LR0Items[1]);
      Data.FinalState = FindOrCreateState(itemList);

      // Iterate through states (while new ones are created) and create shift transitions and new states 
      for (int index = 0; index < Data.States.Count; index++) {
        ParserState state = Data.States[index];
        AddClosureItems(state);
        //Get keys of all possible shifts
        ShiftTable shiftTable = GetStateShifts(state);
        //Each key in shifts dict is an input element 
        // Value is LR0ItemList of shifted LR0Items for this input element.
        foreach (string input in shiftTable.Keys) {
          LR0ItemList shiftedCoreItems = shiftTable[input];
          ParserState newState = FindOrCreateState(shiftedCoreItems);
          state.Actions[input] = new ActionRecord(input,ParserActionType.Shift, newState, null);
          //link original LRItems in original state to derived LRItems in newState
          foreach (LR0Item coreItem in shiftedCoreItems) {
            LRItem fromItem = FindItem(state, coreItem.Production, coreItem.Position - 1);
            LRItem toItem = FindItem(newState, coreItem.Production, coreItem.Position);
            if (!fromItem.PropagateTargets.Contains(toItem))
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
      throw new IronyException(msg);
    }//method

    private ShiftTable GetStateShifts(ParserState state) {
      ShiftTable shifts = new ShiftTable();
      LR0ItemList list;
      foreach (LRItem item in state.Items) {
        BnfTerm term = item.Core.NextElement;
        if (term == null)  continue;
        LR0Item shiftedItem = item.Core.Production.LR0Items[item.Core.Position + 1];
        if (!shifts.TryGetValue(term.Key, out list))
          shifts[term.Key] = list = new LR0ItemList();
        list.Add(shiftedItem);
      }//foreach
      return shifts;
    }//method

    private ParserState FindOrCreateState(LR0ItemList lr0Items) {
      string key = CalcItemListKey(lr0Items);
      ParserState result;
      if (_stateHash.TryGetValue(key, out result))
        return result; 
      result = new ParserState("S" + Data.States.Count, lr0Items);
      Data.States.Add(result);
      _stateHash[key] = result;
      return result;
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
            newItem = new LRItem(state, core);
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
          newItem.NewLookaheads.AddRange(item.Core.TailFirsts);
          if (item.Core.TailIsNullable && !item.PropagateTargets.Contains(newItem))
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
      items.Sort(ById); //Sort by ID
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
    private static int ById(LR0Item x, LR0Item y) {
      if (x.ID < y.ID) return -1;
      if (x.ID == y.ID) return 0;
      return 1;
    }

    #endregion

    #region Lookaheads propagation
    private void PropagateLookaheads() {
      LRItemList currentList = new LRItemList();
      //first collect all items
      foreach (ParserState state in Data.States)
        currentList.AddRange(state.Items);
      //Main loop - propagate until done
      while (currentList.Count > 0) {
        LRItemList newList = new LRItemList();
        foreach (LRItem item in currentList) {
          if (item.NewLookaheads.Count == 0) continue;
          int oldCount = item.Lookaheads.Count;
          item.Lookaheads.AddRange(item.NewLookaheads);
          if (item.Lookaheads.Count != oldCount) {
            foreach (LRItem targetItem in item.PropagateTargets) {
              targetItem.NewLookaheads.AddRange(item.NewLookaheads);
              newList.Add(targetItem);
            }//foreach targetItem
          }//if
          item.NewLookaheads.Clear();
        }//foreach item
        currentList = newList;
      }//while         
    }//method
    #endregion

    #region Final actions: createReduceActions
    private void CreateReduceActions() {
      foreach(ParserState state in Data.States) {
        foreach (LRItem item in state.Items) {
          //we are interested only in "dot  at the end" items
          if (item.Core.NextElement != null)   continue;
          foreach (string lookahead in item.Lookaheads) {
            ActionRecord action;
            if (state.Actions.TryGetValue(lookahead, out action)) 
              action.ReduceProductions.Add(item.Core.Production);
            else
              state.Actions[lookahead] = new ActionRecord(lookahead, ParserActionType.Reduce, null, item.Core.Production);
          }//foreach lookahead
        }//foreach item
      }// foreach state
    } //method
    
    #endregion

    #region Check for shift-reduce conflicts
    private void CheckActionConflicts() {
      StringDictionary errorTable = new StringDictionary();
      foreach (ParserState state in Data.States) {
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
            if (opTerm != null && opTerm.IsSet(TermOptions.IsOperator)) {
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
        Data.Errors.Add(msg + " on inputs: " + errorTable[msg]);
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
      foreach (Terminal term in Data.Terminals)
        term.Init(_grammar);
      foreach (NonTerminal nt in Data.NonTerminals)
        nt.Init(_grammar);
      foreach (TokenFilter filter in _grammar.TokenFilters)
        filter.Init(_grammar);
    }
    #endregion

    private void ValidateAll() {
      //Check rule on all non-terminals
      KeyList ntList = new KeyList();
      foreach(NonTerminal nt in Data.NonTerminals) {
        if (nt == Data.AugmentedRoot) continue; //augm root does not count
        BnfExpressionData data = nt.Rule.Data;
        if (data.Count == 1 && data[0].Count == 1 && data[0][0] is NonTerminal)
          ntList.Add(nt.Name);
      }//foreach
      if (ntList.Count > 0) 
        AddError("Warning: Possible non-terminal duplication. The following non-terminals have rules containing a single non-terminal: \r\n {0}. \r\n" +
         "Consider merging two non-terminals; you may need to use 'nt1 = nt2;' instead of 'nt1.Rule=nt2'.", ntList.ToString(", "));
    }

    #region error handling: AddError
    private void AddError(string message, params object[] args) {
      if (args != null && args.Length > 0)
        message = string.Format(message, args);
      Data.Errors.Add(message);
    }
    #endregion

  }//class


}//namespace

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Irony.Parsing.Construction { 

  // Methods constructing LALR automaton.
  // See _about_parser_construction.txt file in this folder for important comments

  internal partial class ParserDataBuilder {
    LanguageData _language;
    internal ParserData Data;
    Grammar _grammar;
    ParserStateHash _stateHash = new ParserStateHash();
    LRItemSet _itemsNeedLookaheads = new LRItemSet(); 


    internal ParserDataBuilder(LanguageData language) {
      _language = language;
      _grammar = _language.Grammar;
    }

    public void Build() {
      _stateHash.Clear();
      Data = _language.ParserData;
      CreateParserStates(); 
      ComputeTransitions();
      ComputeLookaheads();
      ComputeAndResolveConflicts();
      CreateRemainingReduceActions(); 
      ComputeStatesExpectedTerminals();
    }//method

    #region Creating parser states
    private void CreateParserStates() {
      var grammarData = _language.GrammarData;

      //1. Base automaton: create states for main augmented root for the grammar
      Data.InitialState = CreateInitialState(grammarData.AugmentedRoot);
      ExpandParserStateList(0);
      CreateAcceptAction(Data.InitialState, grammarData.AugmentedRoot); 

      //2. Expand automaton: add parser states from additional roots
      foreach(var augmRoot in grammarData.AugmentedSnippetRoots) {
        var initialState = CreateInitialState(augmRoot);
        ExpandParserStateList(Data.States.Count - 1); //start with just added state - it is the last state in the list
        CreateAcceptAction(initialState, augmRoot); 
      }
    }

    private void CreateAcceptAction(ParserState initialState, NonTerminal augmentedRoot) {
      var root = augmentedRoot.Productions[0].RValues[0];
      var shiftOverRootState = initialState.Actions[root].NewState;
      shiftOverRootState.Actions[_grammar.Eof] = new ParserAction(ParserActionType.Accept, null, null); 
    }


    private ParserState CreateInitialState(NonTerminal augmentedRoot) {
      //for an augmented root there is an initial production "Root' -> .Root"; so we need the LR0 item at 0 index
      var iniItemSet = new LR0ItemSet();
      iniItemSet.Add(augmentedRoot.Productions[0].LR0Items[0]);
      var initialState = FindOrCreateState(iniItemSet);
      var rootNt = augmentedRoot.Productions[0].RValues[0] as NonTerminal; 
      Data.InitialStates[rootNt] = initialState; 
      return initialState;
    }

    private void ExpandParserStateList(int initialIndex) {
      // Iterate through states (while new ones are created) and create shift transitions and new states 
      for (int index = initialIndex; index < Data.States.Count; index++) {
        var state = Data.States[index];
        //Get all possible shifts
        foreach (var term in state.BuilderData.ShiftTerms) {
          var shiftItems = state.BuilderData.ShiftItems.SelectByCurrent(term);
          //Get set of shifted cores and find/create target state
          var shiftedCoreItems = shiftItems.GetShiftedCores(); 
          var newState = FindOrCreateState(shiftedCoreItems);
          //Create shift action
          var newAction = new ParserAction(ParserActionType.Shift, newState, null);
          state.Actions[term] = newAction;
          //Link items in old/new states
          foreach (var shiftItem in shiftItems) {
            shiftItem.ShiftedItem = newState.BuilderData.AllItems.FindByCore(shiftItem.Core.ShiftedItem);
          }//foreach shiftItem
        }//foreach term
      } //for index
    }//method

    private ParserState FindOrCreateState(LR0ItemSet coreItems) {
      string key = ComputeLR0ItemSetKey(coreItems);
      ParserState state;
      if (_stateHash.TryGetValue(key, out state))
        return state;
      //create new state
      state = new ParserState("S" + Data.States.Count);
      state.BuilderData = new ParserStateData(state, coreItems);
      Data.States.Add(state);
      _stateHash[key] = state;
      return state;
    }

    #endregion

    #region computing lookaheads
    //We compute only transitions that are really needed to compute lookaheads in inadequate states.
    // We start with reduce items in inadequate state and find their lookbacks - this is initial list of transitions.
    // Then for each transition in the list we check if it has items with nullable tails; for those items we compute
    // lookbacks - these are new or already existing transitons - and so on, we repeat the operation until no new transitions
    // are created. 
    private void ComputeTransitions() {
      var newItemsNeedLookbacks = _itemsNeedLookaheads = GetReduceItemsInInadequateState();
      while(newItemsNeedLookbacks.Count > 0) {
        var newTransitions = CreateLookbackTransitions(newItemsNeedLookbacks);
        newItemsNeedLookbacks = SelectNewItemsThatNeedLookback(newTransitions);
      }
    }

    private LRItemSet SelectNewItemsThatNeedLookback(TransitionList transitions) {
      //Select items with nullable tails that don't have lookbacks yet
      var items = new LRItemSet();
      foreach(var trans in transitions) {
        foreach(var item in trans.Items.SelectItemsWithNullableTails()) 
          if(item.Lookbacks.Count == 0) //only if it does not have lookbacks yet
            items.Add(item);
      }
      return items; 
    }

    private LRItemSet GetReduceItemsInInadequateState() {
      var result = new LRItemSet(); 
      foreach(var state in Data.States) {
        if (state.BuilderData.IsInadequate) 
          result.UnionWith(state.BuilderData.ReduceItems); 
      }
      return result;     
    }

    private TransitionList  CreateLookbackTransitions(LRItemSet sourceItems) {
      var newTransitions = new TransitionList();
      //Build set of initial cores - this is optimization for performance
      //We need to find all initial items in all states that shift into one of sourceItems
      // Each such initial item would have the core from the "initial" cores set that we build from source items.
      var iniCores = new LR0ItemSet();
      foreach(var sourceItem in sourceItems)
        iniCores.Add(sourceItem.Core.Production.LR0Items[0]);
      //find 
      foreach(var state in Data.States) {
        foreach(var iniItem in state.BuilderData.InitialItems) {
          if (!iniCores.Contains(iniItem.Core)) continue; 
          var currItem = iniItem;
          while(currItem != null) {
            if(sourceItems.Contains(currItem)) {
              //iniItem is initial item for currItem (one of source items) 
              // check if transition for iniItem's non-terminal exists
              var ntLeft = iniItem.Core.Production.LValue;
              Transition trans; 
              if(!state.BuilderData.Transitions.TryGetValue(ntLeft, out trans)) {
                trans = new Transition(iniItem.State, iniItem.Core.Production.LValue);
                newTransitions.Add(trans);
              }
              //Now for currItem, either add trans to Lookbackbacks, or "include" it into currItem.Transition
              if(currItem.Core.IsFinal)
                currItem.Lookbacks.Add(trans);
              else if(currItem.Transition != null)
                currItem.Transition.Include(trans);
            }//if 
            //move to next items
            currItem = currItem.ShiftedItem;
          }//while
        }//foreach iniItem
      }//foreach state
      return newTransitions;
    }

    private void ComputeLookaheads() {
      var sourceStates = new ParserStateSet(); 
      foreach(var reduceItem in _itemsNeedLookaheads) {
        //First collect all states that contribute lookaheads
        sourceStates.Clear(); 
        foreach(var lookbackTrans in reduceItem.Lookbacks) {
          sourceStates.Add(lookbackTrans.ToState); 
          sourceStates.UnionWith(lookbackTrans.ToState.BuilderData.ReadStateSet);
          foreach(var includeTrans in lookbackTrans.Includes) {
            sourceStates.Add(includeTrans.ToState); 
            sourceStates.UnionWith(includeTrans.ToState.BuilderData.ReadStateSet);
          }//foreach includeTrans
        }//foreach lookbackTrans
        //Now merge all shift terminals from all source states
        foreach(var state in sourceStates) 
          reduceItem.Lookaheads.UnionWith(state.BuilderData.ShiftTerminals);
        //Remove SyntaxError - it is pseudo terminal
        if (reduceItem.Lookaheads.Contains(_grammar.SyntaxError))
          reduceItem.Lookaheads.Remove(_grammar.SyntaxError);
        //Sanity check
        if (reduceItem.Lookaheads.Count == 0)
          _language.Errors.Add(GrammarErrorLevel.InternalError, reduceItem.State, "Reduce item '{0}' in state {1} has no lookaheads.", reduceItem.Core, reduceItem.State);
      }//foreach reduceItem
    }//method

    #endregion

    #region Analyzing and resolving conflicts 
    private void ComputeAndResolveConflicts() {
      foreach(var state in Data.States) {
        if(!state.BuilderData.IsInadequate)
          continue;
        //first detect conflicts
        var stateData = state.BuilderData;
        stateData.Conflicts.Clear();
        var allLkhds = new BnfTermSet();
        //reduce/reduce --------------------------------------------------------------------------------------
        foreach(var item in stateData.ReduceItems) {
          foreach(var lkh in item.Lookaheads) {
            if(allLkhds.Contains(lkh)) {
              state.BuilderData.Conflicts.Add(lkh);
            }
            allLkhds.Add(lkh);
          }//foreach lkh
        }//foreach item
        //shift/reduce ---------------------------------------------------------------------------------------
        foreach(var term in stateData.ShiftTerminals)
          if(allLkhds.Contains(term)) {
            stateData.Conflicts.Add(term);
          }

        //Now resolve conflicts by hints and precedence -------------------------------------------------------
        if(stateData.Conflicts.Count > 0) {
          //Hints
          foreach (var conflict in stateData.Conflicts)
            ResolveConflictByHints(state, conflict);
          stateData.Conflicts.ExceptWith(state.BuilderData.ResolvedConflicts); 
          //Precedence
          foreach (var conflict in stateData.Conflicts)
            ResolveConflictByPrecedence(state, conflict);
          stateData.Conflicts.ExceptWith(state.BuilderData.ResolvedConflicts); 
          //if we still have conflicts, report and assign default action
          if (stateData.Conflicts.Count > 0)
            ReportAndCreateDefaultActionsForConflicts(state); 
        }//if Conflicts.Count > 0
      }
    }//method

    private void ResolveConflictByHints(ParserState state, Terminal conflict) {
      var stateData = state.BuilderData;
      //reduce hints
      var reduceItems = stateData.ReduceItems.SelectByLookahead(conflict);
      foreach(var reduceItem in reduceItems)
        if(reduceItem.Core.Hints.Find(h => h.HintType == HintType.ResolveToReduce) != null) {
          state.Actions[conflict] = new ParserAction(ParserActionType.Reduce, null, reduceItem.Core.Production);
          state.BuilderData.ResolvedConflicts.Add(conflict);
          return; 
        }
      
      //Shift hints
      var shiftItems = stateData.ShiftItems.SelectByCurrent(conflict);
      foreach (var shiftItem in shiftItems)
        if(shiftItem.Core.Hints.Find(h => h.HintType == HintType.ResolveToShift) != null) {
          //shift action is already there
          state.BuilderData.ResolvedConflicts.Add(conflict);
          return; 
        }
      //code hints
      // first prepare data for conflict action: reduceProduction (for possible reduce) and newState (for possible shift)
      var reduceProduction = reduceItems.First().Core.Production; //take first of reduce productions
      ParserState newState = (state.Actions.ContainsKey(conflict) ? state.Actions[conflict].NewState : null); 
      // Get all items that might contain hints;
      var allItems = new LRItemList();
      allItems.AddRange(state.BuilderData.ShiftItems.SelectByCurrent(conflict)); 
      allItems.AddRange(state.BuilderData.ReduceItems.SelectByLookahead(conflict)); 
      // Scan all items and try to find hint with resolution type Code
      foreach (var item in allItems)
        if(item.Core.Hints.Find(h => h.HintType == HintType.ResolveInCode) != null) {
          state.Actions[conflict] = new ParserAction(ParserActionType.Code, newState, reduceProduction);
          state.BuilderData.ResolvedConflicts.Add(conflict);
          return; 
        }
    }

    private void ResolveConflictByPrecedence(ParserState state, Terminal conflict) {
      if (!conflict.FlagIsSet(TermFlags.IsOperator)) return; 
      var stateData = state.BuilderData;
      if (!stateData.ShiftTerminals.Contains(conflict)) return; //it is not shift-reduce
      var shiftAction = state.Actions[conflict];
      //now get shift items for the conflict
      var shiftItems = stateData.ShiftItems.SelectByCurrent(conflict);
      //get reduce item
      var reduceItems = stateData.ReduceItems.SelectByLookahead(conflict);
      if (reduceItems.Count > 1) return; // if it is reduce-reduce conflict, we cannot fix it by precedence
      var reduceItem = reduceItems.First(); 
      shiftAction.ChangeToOperatorAction(reduceItem.Core.Production); 
      stateData.ResolvedConflicts.Add(conflict);
    }//method

    //Resolve to default actions
    private void ReportAndCreateDefaultActionsForConflicts(ParserState state) {
      var shiftReduceConflicts = state.BuilderData.GetShiftReduceConflicts();
      var reduceReduceConflicts = state.BuilderData.GetReduceReduceConflicts();
      var stateData = state.BuilderData;
      if (shiftReduceConflicts.Count > 0) 
        _language.Errors.Add(GrammarErrorLevel.Conflict, state, Resources.ErrSRConflict, state, shiftReduceConflicts.ToString());
      if (reduceReduceConflicts.Count > 0)
        _language.Errors.Add(GrammarErrorLevel.Conflict, state, Resources.ErrRRConflict, state, reduceReduceConflicts.ToString());
      //Create default actions for these conflicts. For shift-reduce, default action is shift, and shift action already
      // exist for all shifts from the state, so we don't need to do anything, only report it
      //For reduce-reduce create reduce actions for the first reduce item (whatever comes first in the set). 
      foreach (var conflict in reduceReduceConflicts) {
        var reduceItems = stateData.ReduceItems.SelectByLookahead(conflict);
        var firstProd = reduceItems.First().Core.Production;
        var action = new ParserAction(ParserActionType.Reduce, null, firstProd);
        state.Actions[conflict] = action;
      }
      //Update ResolvedConflicts and Conflicts sets
      stateData.ResolvedConflicts.UnionWith(shiftReduceConflicts);
      stateData.ResolvedConflicts.UnionWith(reduceReduceConflicts);
      stateData.Conflicts.ExceptWith(stateData.ResolvedConflicts);
    }

    #endregion

    #region final actions: creating remaining reduce actions, computing expected terminals, cleaning up state data
    private void CreateRemainingReduceActions() {
      foreach (var state in Data.States) {
        var stateData = state.BuilderData;
        if (stateData.ShiftItems.Count == 0 && stateData.ReduceItems.Count == 1) {
          state.DefaultAction = new ParserAction(ParserActionType.Reduce, null, stateData.ReduceItems.First().Core.Production);
          continue; 
        } 
        //now create actions
        foreach (var item in state.BuilderData.ReduceItems) {
          var action = new ParserAction(ParserActionType.Reduce, null, item.Core.Production);
          foreach (var lkh in item.Lookaheads) {
            if (state.Actions.ContainsKey(lkh)) continue;
            state.Actions[lkh] = action;
          }
        }//foreach item
      }//foreach state
    }

    //Note that for states with a single reduce item the result is empty 
    private void ComputeStatesExpectedTerminals() {
      foreach (var state in Data.States) {
        state.ExpectedTerminals.UnionWith(state.BuilderData.ShiftTerminals);
        //Add lookaheads from reduce items
        foreach (var reduceItem in state.BuilderData.ReduceItems) 
          state.ExpectedTerminals.UnionWith(reduceItem.Lookaheads);
        RemoveTerminals(state.ExpectedTerminals, _grammar.SyntaxError, _grammar.Eof);
      }//foreach state
    }

    private void RemoveTerminals(TerminalSet terms, params Terminal[] termsToRemove) {
      foreach(var termToRemove in termsToRemove)
        if (terms.Contains(termToRemove)) terms.Remove(termToRemove); 
    }

    public void CleanupStateData() {
      foreach (var state in Data.States)
        state.ClearData();
    }
    #endregion

    #region Utilities: ComputeLR0ItemSetKey
    //Parser states are distinguished by the subset of kernel LR0 items. 
    // So when we derive new LR0-item list by shift operation, 
    // we need to find out if we have already a state with the same LR0Item list.
    // We do it by looking up in a state hash by a key - [LR0 item list key]. 
    // Each list's key is a concatenation of items' IDs separated by ','.
    // Before producing the key for a list, the list must be sorted; 
    //   thus we garantee one-to-one correspondence between LR0Item sets and keys.
    // And of course, we count only kernel items (with dot NOT in the first position).
    public static string ComputeLR0ItemSetKey(LR0ItemSet items) {
      if (items.Count == 0) return string.Empty;
      //Copy non-initial items to separate list, and then sort it
      LR0ItemList itemList = new LR0ItemList();
      foreach (var item in items)
        itemList.Add(item);
      //quick shortcut
      if (itemList.Count == 1)
        return itemList[0].ID.ToString();
      itemList.Sort(CompareLR0Items); //Sort by ID
      //now build the key
      StringBuilder sb = new StringBuilder(100);
      foreach (LR0Item item in itemList) {
        sb.Append(item.ID);
        sb.Append(",");
      }//foreach
      return sb.ToString();
    }

    private static int CompareLR0Items(LR0Item x, LR0Item y) {
      if (x.ID < y.ID) return -1;
      if (x.ID == y.ID) return 0;
      return 1;
    }
    #endregion

  }//class


}//namespace



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Irony.Parsing.Construction;

namespace Irony.Parsing {

  /// <summary> A hint to use precedence. </summary>
  /// <remarks>
  /// Not used directly in grammars; injected automatically by system in states having conflicts on operator symbols. 
  /// The purpose of the hint is make handling precedence similar to other conflict resolution methods - through hints
  /// activated during parser construction. The hint code analyzes the conflict and resolves it by adding custom or general action
  /// for a conflicting input. 
  /// </remarks>
  public class PrecedenceHint : GrammarHint {
    public override void CheckParserState(LanguageData language, LRItem owner) {
      var state = owner.State;
      var allConflicts = state.BuilderData.Conflicts;
      if (allConflicts.Count == 0)
        return; 
      //Find all conflicts that can be resolved by operator precedence
      var operConflicts = state.BuilderData.Conflicts.ToList().FindAll(c => c.Flags.IsSet(TermFlags.IsOperator));
      foreach (var conflict in operConflicts) {
        var newState = state.BuilderData.GetNextState(conflict);
        var reduceItem = state.BuilderData.ReduceItems.SelectByLookahead(conflict).First(); //should be only one
        state.Actions[conflict] = new PrecedenceBasedParserAction(newState, reduceItem.Core.Production);
        allConflicts.Remove(conflict);
      }//foreach conflict
    }

    private void ResolveConflictByPrecedence(ParserState state, Terminal conflict) {
      if (!conflict.Flags.IsSet(TermFlags.IsOperator)) return;
      var stateData = state.BuilderData;
      if (!stateData.ShiftTerminals.Contains(conflict))
        return; //it is not shift-reduce
      var reduceItems = stateData.ReduceItems.SelectByLookahead(conflict);
      if (reduceItems.Count != 1)
        return; // if it is reduce-reduce conflict, we cannot fix it by precedence
      var reduceItem = reduceItems.First();
      var newShiftState = stateData.GetNextState(conflict);
      var precAction = new PrecedenceBasedParserAction(newShiftState, reduceItem.Core.Production);
      state.Actions[conflict] = precAction;
      stateData.Conflicts.Remove(conflict);

    }//method
  }//class



}//namespace

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
    public override void Apply(LanguageData language, LRItem owner) {
      var state = owner.State;
      var allConflicts = state.BuilderData.Conflicts;
      if (allConflicts.Count == 0)
        return; 
      //Find all conflicts that can be resolved by operator precedence
      // SL does not support Find extension, so we do it with explicit loop
      var operConflicts = new List<Terminal>(); 
      foreach(var c in allConflicts)
        if (c.Flags.IsSet(TermFlags.IsOperator))
          operConflicts.Add(c);
      foreach (var conflict in operConflicts) {
        var newState = state.BuilderData.GetNextState(conflict);
        var reduceItem = state.BuilderData.ReduceItems.SelectByLookahead(conflict).First(); //should be only one
        state.Actions[conflict] = new PrecedenceBasedParserAction(conflict, newState, reduceItem.Core.Production);
        allConflicts.Remove(conflict);
      }//foreach conflict
    }

  }//class



}//namespace

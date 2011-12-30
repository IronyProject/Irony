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

namespace Irony.Parsing {

  public class ResolveInCodeHint: GrammarHint {
    public override void Apply(LanguageData language, Construction.LRItem owner) {
      
/*
      var state = owner.State;
      var resolveAction = new ResolveInCodeParserAction(); 
      foreach (var conflict in state.BuilderData.Conflicts) {
        if (state.Actions.ContainsKey(conflict)) continue;
        state.Actions[conflict] = resolveAction; 
      }
      state.BuilderData.Conflicts.Clear(); 
 */ 
    }//method

    /*  OLD CODE from parser builder 
         //code hints
         // first prepare data for conflict action: reduceProduction (for possible reduce) and newState (for possible shift)
         var reduceProduction = reduceItems.First().Core.Production; //take first of reduce productions
         ParserState newState = (state.Actions.ContainsKey(conflict) ? state.Actions[conflict].NewState : null); 
         // Get all items that might contain hints;
         var allItems = new LRItemList();
         allItems.AddRange(state.BuilderData.ShiftItems.SelectByCurrent(conflict)); 
         allItems.AddRange(state.BuilderData.ReduceItems.SelectByLookahead(conflict)); 
         // Scan all items and try to find hint with resolution type Code
         foreach (var item in allItems) {
           if (item.Core.Hints.Find(h => h.HintType == HintType.ResolveInCode) != null) {
             state.Actions[conflict] = new ParserAction(PreferredActionType.Code, newState, reduceProduction);
             state.BuilderData.ResolvedConflicts.Add(conflict);
             return; 
           }
         }
   */

  }//Hint class

  public class ResolveInCodeParserAction : ParserAction {
    public override void Execute(ParsingContext context) {
      var args = new ConflictResolutionArgs(context, this);
      context.Language.Grammar.OnResolvingConflict(args);
      switch (args.Result) {
        case PreferredActionType.Shift:
          break; 
        case PreferredActionType.Reduce:
          break; 
      }//switch
    }//method

  }//class

}//ns

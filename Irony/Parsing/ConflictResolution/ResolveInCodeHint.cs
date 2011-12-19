using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {
  public class ResolveInCodeHint: GrammarHint {
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
  
  }//class
}

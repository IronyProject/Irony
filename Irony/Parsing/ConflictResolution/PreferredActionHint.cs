using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing.Construction;

namespace Irony.Parsing {

  public class PreferredActionHint : GrammarHint {
    PreferredActionType ActionType;
    public PreferredActionHint(PreferredActionType actionType) {
      ActionType = actionType;
    }
    public override void CheckParserState(LanguageData language, LRItem owner) {
      var state = owner.State;
      var conflicts = state.BuilderData.Conflicts;
      if (conflicts.Count == 0) return;
      switch (ActionType) {
        case PreferredActionType.Shift:
          var currTerm = owner.Core.Current as Terminal;
          if (currTerm == null || !conflicts.Contains(currTerm)) return; //nothing to do
          //Current term for shift item (hint owner) is a conflict - resolve it with shift action
          var newState = owner.ShiftedItem.State;
          var shiftAction = new ShiftParserAction(newState);
          state.Actions[currTerm] = shiftAction;
          conflicts.Remove(currTerm);
          return;
        case PreferredActionType.Reduce:
          if (!owner.Core.IsFinal) return; //we take care of reduce items only here
          //we have a reduce item with "Reduce" hint. Check if any of lookaheads are in conflict
          ReduceParserAction reduceAction = null;
          foreach (var lkhead in owner.Lookaheads)
            if (conflicts.Contains(lkhead)) {
              if (reduceAction == null)
                reduceAction = new ReduceParserAction(owner.Core.Production);
              state.Actions[lkhead] = reduceAction;
              conflicts.Remove(lkhead);
            }
          return;
      }//switch
    }//method
  }//class


}

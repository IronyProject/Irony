using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {

  public enum PreferredActionType {
    Shift,
    Reduce,
  }

  public delegate bool ParserActionCondition(ParsingContext context);


  public class ConditionBoundAction {
    public ParserActionCondition Condition;
    public ParserAction Action;
  }
  public class ConditionBoundActionList : List<ConditionBoundAction> { }

  public class ConditionalParserAction : ParserAction {
    public ConditionBoundActionList ConditionalActions;
    public ParserAction DefaultAction;

    public override void Execute(ParsingContext context) {
      for (int i = 0; i < ConditionalActions.Count; i++) {
        var ca = ConditionalActions[i];
        if (ca.Condition(context)) {
          ca.Action.Execute(context);
          return; 
        }
      }
      //if no conditions matched, execute default action
      DefaultAction.Execute(context);
    }//method

  }//class
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {

  public class PrecedenceBasedParserAction : ConditionalParserAction {
    ShiftParserAction _shift;
    ReduceParserAction _reduce; 

    public PrecedenceBasedParserAction(ParserState newShiftState, Production reduceProduction)  {
      _reduce = new ReduceParserAction(reduceProduction);
      var v = new ConditionBoundAction() { Condition = MustReduce, Action = _reduce };
      base.DefaultAction = _shift = new ShiftParserAction(newShiftState);
    }

    private static bool MustReduce(ParsingContext context) {
      var input = context.CurrentParserInput;
      for (int i = context.ParserStack.Count - 1; i >= 0; i--) {
        var prevNode = context.ParserStack[i];
        if (prevNode == null) continue;
        if (prevNode.Precedence == BnfTerm.NoPrecedence) continue;
        //if previous operator has the same precedence then use associativity
        if (prevNode.Precedence == input.Precedence)
          return (input.Associativity == Associativity.Left); //if true then Reduce
        else
          return (prevNode.Precedence > input.Precedence); //if true then Reduce
      }
      //If no operators found on the stack, do shift
      return false;
    }

    public override string ToString() {
      return string.Format(Resources.LabelActionOp, _shift.NewState.Name, _reduce.Production.ToStringQuoted());
    }

  }//class


}//namespace

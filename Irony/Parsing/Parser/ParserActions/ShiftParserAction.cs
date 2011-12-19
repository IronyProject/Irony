using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {
  public class ShiftParserAction: ParserAction {
    public readonly ParserState NewState;
    
    public ShiftParserAction(ParserState newState) {
      NewState = newState;
    }

    public override void Execute(ParsingContext context) {
      context.ParserStack.Push(context.CurrentParserInput, NewState);
      context.CurrentParserState = NewState;
      context.CurrentParserInput = null;
    }

    public override string ToString() {
      return string.Format(Resources.LabelActionShift, NewState.Name);
    }
  
  }//class
}

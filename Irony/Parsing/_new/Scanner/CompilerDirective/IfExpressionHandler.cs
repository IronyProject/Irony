using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing.New {
 

  public class IfExpressionGrammarCSharp : Grammar {
    public IfExpressionGrammarCSharp(bool extendedCVersion) : base(true) {

    }
  }

  public class IfExpressionHandler {
    public bool EvaluationExpression(ParsingContext context, string expression) {
      return false;
    }
  }//

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Irony.Ast {

  public class AstBuilder<TAstNode> {

    public Type DefaultNodeType;
    public Type DefaultLiteralNodeType; //default node type for literals
    public Type DefaultIdentifierNodeType; //default node type for identifiers

    public AstBuilder(LanguageData language) {
    }

    public TAstNode BuildAstBottomUp(ParseTree parseTree) {
    }
  }//class

}

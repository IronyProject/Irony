using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Compiler;
using Irony.Runtime;

namespace Irony.Samples.Scheme {
  public class IdentifierNode : NamedNode {
    public IdentifierNode(AstNodeArgs args) : base(args) {  }
    public IdentifierNode(AstNodeArgs args, AstNode idNode) : base(args, idNode) {  }
    public IdentifierNode(AstNodeArgs args, string name) : base(args, name) { }

    public override void Evaluate(EvaluationContext context) {
      try {
        if (Address.ScopeLevel == -1)
          throw new RuntimeException("Variable " + Name + " is not declared.");
        context.CurrentResult = context.CurrentFrame.GetValue(Address);
      } catch (RuntimeException rex) {
        rex.Location = this.Location;
        throw;
      }
    }

  }//class

}

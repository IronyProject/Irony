using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Runtime;

namespace Irony.Compiler.AST {
  public class AssigmentNode : AstNode {
    public IdentifierNode Identifier;
    public AstNode Expression;

    public AssigmentNode(NodeArgs args, IdentifierNode id, AstNode expression) : base(args) {
      ChildNodes.Clear();
      Identifier = id;
      Identifier.Flags |= AstNodeFlags.AllocateSlot | AstNodeFlags.NotRValue;
      Identifier.Access = AccessType.Write;
      AddChild("Name", Identifier);
      Expression = expression;
      AddChild("Expr", Expression);
    }

    protected override void DoEvaluate(EvaluationContext context) {
      Expression.Evaluate(context);
      Identifier.Evaluate(context); //writes the value into the slot
    }

  }
}

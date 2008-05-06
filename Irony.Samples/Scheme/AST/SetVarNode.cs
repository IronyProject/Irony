using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Compiler;
using Irony.Runtime;

namespace Irony.Samples.Scheme {
  public class SetVarNode : AstNode {
    public IdentifierNode Identifier;
    public AstNode Expression;

    public SetVarNode(AstNodeArgs args)  : base(args) {
      SetFields(args.ChildNodes[0] as IdentifierNode, args.ChildNodes[1]);
    }
    public SetVarNode(AstNodeArgs args, IdentifierNode id, AstNode expression) : base(args) {
      SetFields(id, expression);
    }
    private void SetFields(IdentifierNode id, AstNode expression) {
      Identifier = id;
      Identifier.Tag = "Name";
      Expression = expression;
      Expression.Tag = "Expr";
      ReplaceChildNodes(Identifier, Expression);
    }

    public override void OnAstProcessing(CompilerContext context, AstProcessingPhase phase) {
      base.OnAstProcessing(context, phase);
      switch (phase) {
        case AstProcessingPhase.Allocating:
          Scope.CreateSlot(Identifier.Name);
          break;
      }
    }
    
    public override void Evaluate(EvaluationContext context) {
      Expression.Evaluate(context);
      context.CurrentFrame.SetValue(Identifier.Address, context.CurrentResult);
    }
  }
}

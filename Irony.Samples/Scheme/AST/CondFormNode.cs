using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Compiler;
using Irony.Runtime;

namespace Irony.Samples.Scheme {
  public class CondFormNode : AstNode {
    public AstNodeList Clauses;
    public AstNode ElseClause;

    public CondFormNode(AstNodeArgs args)  : base(args) {
      SetFields(args.ChildNodes[1].ChildNodes, args.ChildNodes[2]);
    }
    public CondFormNode(AstNodeArgs args, AstNodeList clauses, AstNode elseClause) : base(args) {
      SetFields(clauses, elseClause);
    }
    private void SetFields(AstNodeList clauses, AstNode elseCase) {
      Clauses = clauses;
      ElseClause = elseCase;
      if (ElseClause != null)
        ElseClause.Tag = "else";
      ReplaceChildNodes(Clauses);
      AddChild(ElseClause);
    }

    public override void OnAstProcessing(CompilerContext context, AstProcessingPhase phase) {
      base.OnAstProcessing(context, phase);
      switch (phase) {
        case AstProcessingPhase.MarkTailCalls:
          if (IsSet(AstNodeFlags.IsTail)) {
            foreach (CondClauseNode clause in Clauses)
              clause.Flags |= AstNodeFlags.IsTail;
            ElseClause.Flags |= AstNodeFlags.IsTail;
          }
          break;
      }
    }

    public override void Evaluate(EvaluationContext context) {
      foreach (CondClauseNode clause in Clauses) {
        clause.Test.Evaluate(context);
        if (context.Ops.IsTrue(context.CurrentResult)) {
          clause.Expressions.Evaluate(context);
          return;
        }
      }//foreach
      if (ElseClause != null)
        ElseClause.Evaluate(context);
    }
  }
}

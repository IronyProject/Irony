using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Compiler;
using Irony.Runtime;

namespace Irony.Samples.Scheme {
  //This node also serves as StatementListNode, so Datum might be a statement
  public class DatumListNode : AstNode {
    public DatumListNode(AstNodeArgs args) : base(args) { }
    public DatumListNode(AstNodeArgs args, AstNodeList statements) : base(args) {
      ReplaceChildNodes(statements);
    }

    public override void OnAstProcessing(CompilerContext context, AstProcessingPhase phase) {
      base.OnAstProcessing(context, phase);
      switch (phase) {
        case AstProcessingPhase.MarkTailCalls:
          if (IsSet(AstNodeFlags.IsTail) && ChildNodes.Count > 0)
            ChildNodes[ChildNodes.Count - 1].Flags |= AstNodeFlags.IsTail;
          break;
      }
    }

    
    public override void Evaluate(Irony.Runtime.EvaluationContext context) {
      foreach(AstNode node in ChildNodes) {
        node.Evaluate(context);
        switch (context.Jump) {
          case JumpType.Goto:
            //TODO: implement GOTO
            break;
          case JumpType.Break:
          case JumpType.Continue:
          case JumpType.Return:
            return; 
          case JumpType.None:
            continue; //nothing to do, just continue
        }//switch
      }//foreach
    }
  }

}//namespace

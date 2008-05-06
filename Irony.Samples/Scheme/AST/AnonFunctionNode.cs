using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Compiler;
using Irony.Runtime;

namespace Irony.Samples.Scheme {

  public class AnonFunctionNode : AstNode  {
    public AstNode Parameters;
    public AstNode Body;
    protected Scope OwnerScope; //just in case we need it

    public AnonFunctionNode(AstNodeArgs args)  : base(args) {
      SetFields(args.ChildNodes[0],  args.ChildNodes[1]);
    }
    public AnonFunctionNode(AstNodeArgs args, AstNode parameters, AstNode body) : base(args) {
      SetFields(parameters, body);
    }
    private void SetFields(AstNode parameters, AstNode body) {
      Parameters = parameters;
      Parameters.Tag = "Params";
      Body = body;
      Body.Tag = "Body";
      ReplaceChildNodes(parameters, body);
    }

    public override void OnAstProcessing(CompilerContext context, AstProcessingPhase phase) {
      switch (phase) {
        case AstProcessingPhase.CreatingScopes:
          base.OnAstProcessing(context, phase);
          //Scope is already assigned by base method; change it to new scope that has 
          OwnerScope = base.Scope;
          base.Scope = new Scope(this, OwnerScope);
          break;
        case AstProcessingPhase.Allocating:
          foreach (IdentifierNode id in Parameters.ChildNodes)
            Scope.CreateSlot(id.Name);
          break;
        case AstProcessingPhase.MarkTailCalls:
          Body.Flags |= AstNodeFlags.IsTail; //unconditionally set body's tail flag
          break; 
      }//switch
    }

    public override void Evaluate(EvaluationContext context) {
      context.CurrentResult = new Closure(context.CurrentFrame, this, Body.Evaluate);
    }//method


  }//class
}
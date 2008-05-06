using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Compiler;
using Irony.Runtime;

namespace Irony.Samples.Scheme {

  public class FunctionDefNode : NamedNode  {
    public AnonFunctionNode Function;

    public FunctionDefNode(AstNodeArgs args)  : base(args) {
    }
    public FunctionDefNode(AstNodeArgs args, IdentifierNode id, AstNode parameters, AstNode body) : base(args,id) {
      Function = new AnonFunctionNode(args, parameters, body);
      ReplaceChildNodes(Function);
    }

    public override void OnAstProcessing(CompilerContext context, AstProcessingPhase phase) {
      base.OnAstProcessing(context, phase);
      switch (phase) {
        case AstProcessingPhase.Allocating:
          Scope.CreateSlot(Name);
          break;
      }
    } 


    public override void Evaluate(EvaluationContext context) {
      Function.Evaluate(context); //creates anon function closure; we need to save it using the Address
      context.CurrentFrame.SetValue(Address,  context.CurrentResult);
    }//method


  }//class
}

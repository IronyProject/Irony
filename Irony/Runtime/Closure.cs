using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Compiler;

namespace Irony.Runtime {

  public class Closure {
    public string MethodName; //either BindingInfo.Name, or name of the variable storing lambda expression 
    public readonly Frame ParentFrame;
    public readonly AstNode Node;
    public readonly FunctionBindingInfo BindingInfo;
    public Closure(Frame parentFrame, AstNode node, FunctionBindingInfo bindingInfo) {
      MethodName = bindingInfo.Name;
      ParentFrame = parentFrame;
      Node = node; 
      BindingInfo = bindingInfo;
    }
    public void Evaluate(EvaluationContext context) {
      context.PushFrame(MethodName, Node, ParentFrame);
      try {
        BindingInfo.Evaluate(context);
      } finally {
        context.PopFrame();
      }//finally
    }//method

  }//class



}//namespace

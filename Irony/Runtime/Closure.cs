using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Compiler;

namespace Irony.Runtime {

  public interface IInvokeTarget {
    void Invoke(EvaluationContext context, ValueList args);
  }

  public delegate void ClosureTarget(EvaluationContext context);
  public class Closure : IInvokeTarget {
    Frame _parentFrame;
    AstNode _node;
    //public readonly int ArgCount;
    ClosureTarget _target;
    public Closure(Frame parentFrame, AstNode node, ClosureTarget target) {
      _parentFrame = parentFrame;
      //_argCount = argCount;
      _node = node; 
      _target = target;
    }
    public void Invoke(EvaluationContext context, ValueList args) {
      try {
        context.PushFrame(_node, _parentFrame, args);
        _target(context);
      } finally {
        context.PopFrame();
      }
    }
  }//class



}//namespace

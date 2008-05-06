using System;
using System.Collections.Generic;
using System.Text;
using Irony.Compiler;

namespace Irony.Runtime {
  public enum JumpType {
    None = 0,
    Break,
    Continue,
    Return,
    Goto,
    Exception,
  }

  public class EvaluationContext  {
    public LanguageOps Ops;

    public Frame CurrentFrame;
    public object CurrentResult;

    public JumpType Jump = JumpType.None;
    public AstNode GotoTarget;

    public IInvokeTarget Tail;
    public ValueList TailArgs;

    public EvaluationContext(LanguageOps ops, AstNode rootNode) {
      Ops = ops;
      PushFrame(rootNode, null, new ValueList());
    }

    public void PushFrame(AstNode node, Frame parent, ValueList args) {
      CurrentFrame = new Frame(node, CurrentFrame, parent, args);
    }
    public void PopFrame() {
      CurrentFrame = CurrentFrame.Caller;
    }

  }//class

}

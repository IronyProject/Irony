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
  }

  public class EvaluationContext {
    public Scope CurrentScope;
    public JumpType Jump = JumpType.None;
    public AstNode GotoTarget;
  }

}

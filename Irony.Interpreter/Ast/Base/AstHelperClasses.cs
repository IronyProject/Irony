using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Interpreter.Ast {

  public delegate object EvaluateMethod(ScriptThread thread);
  public delegate void ValueSetterMethod(ScriptThread thread, object value);

  [Flags]
  public enum AstNodeFlags {
    None = 0x0,
    IsTail = 0x01,     //the node is in tail position
    //IsScope = 0x02,     //node defines scope for local variables
  }

  [Flags]
  public enum NodeUseType {
    Unknown,
    Name, //identifier used as a Name container - system would not use it's Evaluate method directly
    CallTarget,
    ValueRead,
    ValueWrite,
    ValueReadWrite,
    Parameter,
    Keyword,
    SpecialSymbol,
  }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Interpreter.Ast {

  public class Closure {
    public readonly Scope Scope; 
    public readonly ICallTarget Method;
    public readonly object[] Arguments;
    public Closure(Scope scope, ICallTarget method, object[] arguments) {
      Scope = scope; 
      Method = method;
      Arguments = arguments;
    }

  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Interpreter.Ast {
  public class Closure : ICallTarget {
    //The scope that created closure - used as Creator property of new scope, and will be used to find Parents (enclosing scopes) 
    public Scope CreatorScope; 
    public LambdaNode Lamda;
    public Closure(Scope parentScope, LambdaNode targetNode) {
      CreatorScope = parentScope;
      Lamda = targetNode;
    }

    public object Call(ScriptThread thread, object[] parameters) {
      return Lamda.Call(CreatorScope, thread, parameters);
    }

    public override string ToString() {
      return Lamda.ToString(); //returns nice string like "<function add>"
    }

  } //class
}

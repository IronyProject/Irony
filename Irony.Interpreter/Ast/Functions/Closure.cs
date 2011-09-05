using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Interpreter.Ast {
  public class Closure : ICallTarget {
    //The scope that created closure - used as Creator property of new scope, and will be used to find Parents (enclosing scopes) 
    public Scope CreatorScope; 
    public FunctionDefNode TargetNode;
    public Closure(Scope parentScope, FunctionDefNode targetNode) {
      CreatorScope = parentScope;
      TargetNode = targetNode;
    }

    public object Call(ScriptThread thread, object[] parameters) {
      var save = thread.CurrentNode; //prolog, not standard - the caller is NOT target node's parent
      thread.CurrentNode = TargetNode;
      thread.PushClosureScope(TargetNode.DependentScopeInfo, CreatorScope, parameters);
      TargetNode.Parameters.Evaluate(thread); // pre-process parameters
      var result = TargetNode.Body.Evaluate(thread);
      thread.PopScope();
      thread.CurrentNode = save; //epilog, restoring caller 
      return result; 
    }

  } //class
}

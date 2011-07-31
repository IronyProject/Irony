using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace Irony.Interpreter {
  /// <summary> Represents a running thread in script application.  </summary>
  public sealed class ScriptThread {
    public readonly ScriptApp App;
    public readonly LanguageRuntime Runtime; 

    public Scope CurrentScope;
    public AstNode CurrentNode;
    public Closure Tail;

    public ScriptThread(ScriptApp env) {
      App = env;
      Runtime = App.Info.Runtime; 
      CurrentScope = env.MainScope;
    }

    public ScopeBase PushScope(ScopeInfo scope, Scope parent, object[] parameters) {
      CurrentScope = new Scope(scope, CurrentScope, parent, parameters);
      return CurrentScope;
    }

    public void PopScope() {
      CurrentScope = CurrentScope.Caller;
    }

    #region Exception handling
    public object HandleError(Exception exception) {
      if (exception is ScriptException)
        throw exception;
      var stack = GetStackTrace();
      var rex = new ScriptException(exception.Message, exception, CurrentNode.ErrorAnchor, stack);
      throw rex;
    }

    // Throws ScriptException exception.
    public void ThrowScriptError(string message, params object[] args) {
      if (args != null && args.Length > 0)
        message = string.Format(message, args);
      var loc = GetCurrentLocation();
      var stack = GetStackTrace(); 
      throw new ScriptException(message, null, loc, stack);
    }

    //TODO: add construction of Script Call stack
    public ScriptStackTrace GetStackTrace() {
      return new ScriptStackTrace();
    }

    private SourceLocation GetCurrentLocation() {
      return this.CurrentNode == null ? new SourceLocation() : CurrentNode.Location;
    }

    #endregion

  }//class
}

//#define REUSE_SCOPES


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace Irony.Interpreter {
  /// <summary> Represents a running thread in script application.  </summary>
  public sealed class ScriptThread {
    public readonly ScriptApp App;
    public readonly LanguageRuntime Runtime; 

    public Scope CurrentScope;
    public AstNode CurrentNode;

    // Tail call parameters
    public ICallTarget Tail;
    public object[] TailArgs;

#if REUSE_SCOPES
    private static object[] _nulls = new object[1000]; 
#endif
 
    public ScriptThread(ScriptApp app) {
      App = app;
      Runtime = App.Runtime;
      CurrentScope = app.MainScope;
    }

    public void PushScope(ScopeInfo scopeInfo, object[] parameters) {
      CurrentScope = new Scope(scopeInfo, CurrentScope, CurrentScope, parameters);
    }



    public void PushClosureScope(ScopeInfo scopeInfo, Scope closureParent, object[] parameters) {
#if REUSE_SCOPES
      //Experiment: reusing scopes. Reduces GC collections by 50%, but perf degrades by 5%. 
      // The label REUSE_SCOPES is defined at the beginning of this file.
      var scope = Interlocked.Exchange(ref scopeInfo.ScopeInstance, null);
      if (scope != null) {
        scope.Caller = CurrentScope;
        scope.Creator = closureParent;
        scope.Parameters = parameters;
        CurrentScope = scope;
        return; 
      }      
#endif
      CurrentScope = new Scope(scopeInfo, CurrentScope, closureParent, parameters);
    }

    public void PopScope() {
#if REUSE_SCOPES
      Array.Copy(_nulls, CurrentScope.Values, CurrentScope.Values.Length);
      Interlocked.Exchange(ref CurrentScope.Info.ScopeInstance, CurrentScope);
#endif
      CurrentScope = CurrentScope.Caller;
    }

    public Binding Bind(string symbol, BindingOptions options) {
      var request = new BindingRequest(this, CurrentNode, symbol, options);
      var binding = Runtime.BindSymbol(request);
      if (binding == null)
        ThrowScriptError("Unknown symbol '{0}'.", symbol); 
      return binding; 
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

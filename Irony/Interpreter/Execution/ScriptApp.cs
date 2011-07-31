using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace Irony.Interpreter {

  public enum AppStatus {
    Ready,
    Evaluating,
    WaitingMoreInput, //command line only
    SyntaxError,
    RuntimeError,
    Aborted
  }

  public enum AppMode {
    CommandLine,
    RunAll,
  }


  /// <summary> Represents a running instance of a script application.  </summary>
  public sealed class ScriptApp {
    public ScriptAppInfo Info;
    public Scope[] ModuleScopes;
    public Scope MainScope; 
    public IDictionary<string, object> Globals {get; private set;}
    public StringBuilder OutputBuffer = new StringBuilder();
    private object _lockObject = new object();

    // Current mode/status variables
    // 07/31/2011 - incompleted, just copied all variables from old Interpreter class. TO BE COMPLETED.
    public AppStatus Status;
    public AppMode Mode; 
    public long EvaluationTime;
    public ParserMessageList ParserMessages;
    public Exception LastException;



    #region Constructors
    public ScriptApp(LanguageRuntime runtime) : this(new ScriptAppInfo(runtime, null)) { }

    public ScriptApp(ScriptAppInfo info) {
      Info = info;
      //Create module scopes
      ModuleScopes = new Scope[Info.Modules.Length];
      for (int i = 0; i < Info.Modules.Length; i++)
        ModuleScopes[i] = new Scope(Info.Modules[i], null, null, null);
      MainScope = ModuleScopes[0];
      Globals = MainScope.AsDictionary();
    }
    #endregion

    #region Async execution - not implemented for now
    public bool AsyncExecuting() {
      return Status == AppStatus.Evaluating;
    }

    public void AsyncAbort() {
      //TODO: implement this
    }

    public void AsyncExecute(string script) {

    }

    #endregion



    #region Output writing
    public void Write(string text) {
      lock(_lockObject){
        OutputBuffer.Append(text);
      }
    }
    public void WriteLine(string text) {
      lock(_lockObject){
        OutputBuffer.AppendLine(text);
      }
    }
    public void ClearOutputBuffer() {
      lock(_lockObject){
        OutputBuffer.Clear();
      }
    }
    public string GetOutput() {
      lock(_lockObject){
        return OutputBuffer.ToString();
      }
    }
    #endregion



  }//class
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Security;
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

    public Scope[] StaticScopes;
    public Scope MainScope; 

    public IDictionary<string, object> Globals {get; private set;}
    public StringBuilder OutputBuffer = new StringBuilder();
    private object _lockObject = new object();
    private IList<Assembly> ImportedAssemblies = new List<Assembly>();

    // Current mode/status variables
    // TODO:  TO BE COMPLETED.
    // 07/31/2011 - incomplete, just copied all variables from old Interpreter class.
    public AppStatus Status;
    public AppMode Mode; 
    public long EvaluationTime;

    public ParserMessageList ParserMessages;
    public Exception LastException;

    public ParseTree LastScript { get; private set; } //the root node of the last executed script


    #region Constructors
    public ScriptApp(LanguageData language) {
      var grammar = language.Grammar as InterpretedLanguageGrammar;
      var runtime = grammar.CreateRuntime(language); 
      Info = new ScriptAppInfo(runtime); 
      Init(); 
    }

    public ScriptApp(LanguageRuntime runtime)  { 
      Info = new ScriptAppInfo(runtime);
      Init(); 
    }

    public ScriptApp(ScriptAppInfo info) {
      Info = info;
      Init(); 
    }

    [SecuritySafeCritical]
    private void Init() {
      //Create static scopes
      MainScope = new Scope(Info.MainModule.ScopeInfo, null, null, null);
      StaticScopes = new Scope[Info.StaticScopeInfos.Count];
      StaticScopes[0] = MainScope;
      Globals = MainScope.AsDictionary();
    }
    
    #endregion

    // Utilities
    public IEnumerable<Assembly> GetImportAssemblies() {
      //simple default case - return all assemblies loaded in domain
      return AppDomain.CurrentDomain.GetAssemblies(); 
    }

    #region Evaluation
    public object Evaluate(string script) {
      var parsedScript = Info.Parser.Parse(script);
      if (parsedScript.HasErrors())
        throw new Exception("Syntax errors found.");
      LastScript = parsedScript;
      var result = EvaluateParsedScript(); 
      return result;
    }

    // Irony interpreter requires that once a script is executed in a ScriptApp, it is bound to ScriptAppInfo object, 
    // and all later script executions should be performed only in the context of the same app.
    // (because first execution sets up a lot of things, like slots, scopes, etc, which are bound to ScriptAppInfo).
    // We save the app instance in Tag property of the parsed tree and reuse it if we find it there on 
    // consequitive re-runs. 
    public object Evaluate(ParseTree parsedScript) {
      var root = parsedScript.Root.AstNode as AstNode;
      if (root.Parent != null && root.Parent != Info.ProgramRoot)
        throw new Exception("Cannot evaluate parsed script. It had been already evaluated in a different application.");
      LastScript = parsedScript;
      return EvaluateParsedScript(); 
    }

    public object EvaluateAgain() {
      if (LastScript == null)
        throw new Exception("No previously parsed/evaluated script.");
      return EvaluateParsedScript(); 
    }

    //Actual implementation
    private object EvaluateParsedScript() {
      
      LastScript.Tag = Info;
      var root = LastScript.Root.AstNode as AstNode;
      root.DependentScopeInfo = MainScope.Info;

      ScriptThread thread = null;
      try {
        thread = new ScriptThread(this);
        var result = root.Evaluate(thread);
        if (result != null)
          thread.App.WriteLine(result.ToString());
        return result; 
      } catch (ScriptException se) {
        se.Location = thread.CurrentNode.Location;
        se.ScriptStackTrace = thread.GetStackTrace();  
        throw;
      }
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

#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
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

  /// <summary> Represents a running instance of a script application.  </summary>
  public sealed class ScriptApp {
    public readonly LanguageData Language;
    public readonly LanguageRuntime Runtime;
    public Parser Parser { get; private set; }

    public AppDataMap DataMap;

    public Scope[] StaticScopes;
    public Scope MainScope;
    public IDictionary<string, object> Globals {get; private set;}
    private IList<Assembly> ImportedAssemblies = new List<Assembly>();

    public StringBuilder OutputBuffer = new StringBuilder();
    private object _lockObject = new object();

    // Current mode/status variables
    public AppStatus Status;
    public long EvaluationTime;
    public Exception LastException;
    public bool RethrowExceptions = true;  

    public ParseTree LastScript { get; private set; } //the root node of the last executed script


    #region Constructors
    public ScriptApp(LanguageData language) {
      Language = language;
      var grammar = language.Grammar as InterpretedLanguageGrammar;
      Runtime = grammar.CreateRuntime(language);
      DataMap = new AppDataMap(Language.Grammar.CaseSensitive); 
      Init(); 
    }

    public ScriptApp(LanguageRuntime runtime)  {
      Runtime = runtime;
      Language = Runtime.Language;
      DataMap = new AppDataMap(Language.Grammar.CaseSensitive);
      Init(); 
    }

    public ScriptApp(AppDataMap dataMap) {
      DataMap = dataMap;
      Init(); 
    }

    [SecuritySafeCritical]
    private void Init() {
      Parser = new Parser(Language);
      //Create static scopes
      MainScope = new Scope(DataMap.MainModule.ScopeInfo, null, null, null);
      StaticScopes = new Scope[DataMap.StaticScopeInfos.Count];
      StaticScopes[0] = MainScope;
      Globals = MainScope.AsDictionary();
    }
    
    #endregion

    public ParserMessageList GetParserMessages() {
      return Parser.Context.CurrentParseTree.ParserMessages;
    }
    // Utilities
    public IEnumerable<Assembly> GetImportAssemblies() {
      //simple default case - return all assemblies loaded in domain
      return AppDomain.CurrentDomain.GetAssemblies(); 
    }

    public ParseMode ParserMode {
      get { return Parser.Context.Mode; }
      set { Parser.Context.Mode = value; }
    }

    #region Evaluation
    public object Evaluate(string script) {
      var parsedScript = Parser.Parse(script);
      if (parsedScript.HasErrors()) {
        Status = AppStatus.SyntaxError;
        if (RethrowExceptions)
          throw new ScriptException("Syntax errors found.");
        return null; 
      }
 
      if (ParserMode == ParseMode.CommandLine && Parser.Context.Status == ParserStatus.AcceptedPartial) {
        Status = AppStatus.WaitingMoreInput;
        return null; 
      }
      LastScript = parsedScript;
      var result = EvaluateParsedScript(); 
      return result;
    }

    // Irony interpreter requires that once a script is executed in a ScriptApp, it is bound to AppDataMap object, 
    // and all later script executions should be performed only in the context of the same app (or at least by an App with the same DataMap).
    // The reason is because the first execution sets up a data-binding fields, like slots, scopes, etc, which are bound to ScopeInfo objects, 
    // which in turn is part of DataMap.
    public object Evaluate(ParseTree parsedScript) {
      var root = parsedScript.Root.AstNode as AstNode;
      if (root.Parent != null && root.Parent != DataMap.ProgramRoot)
        throw new Exception("Cannot evaluate parsed script. It had been already evaluated in a different application.");
      LastScript = parsedScript;
      return EvaluateParsedScript(); 
    }

    public object Evaluate() {
      if (LastScript == null)
        throw new Exception("No previously parsed/evaluated script.");
      return EvaluateParsedScript(); 
    }

    //Actual implementation
    private object EvaluateParsedScript() {
      LastScript.Tag = DataMap;
      var root = LastScript.Root.AstNode as AstNode;
      root.DependentScopeInfo = MainScope.Info;

      Status = AppStatus.Evaluating;
      ScriptThread thread = null;
      try {
        thread = new ScriptThread(this);
        var result = root.Evaluate(thread);
        if (result != null)
          thread.App.WriteLine(result.ToString());
        Status = AppStatus.Ready;
        return result; 
      } catch (ScriptException se) {
        Status = AppStatus.RuntimeError;
        se.Location = thread.CurrentNode.Location;
        se.ScriptStackTrace = thread.GetStackTrace();
        LastException = se;
        if (RethrowExceptions)
          throw;
        return null;
      } catch (Exception ex) {
        Status = AppStatus.RuntimeError;
        var se = new ScriptException(ex.Message, ex, thread.CurrentNode.Location, thread.GetStackTrace()); 
        LastException = se;
        if (RethrowExceptions)
          throw se;
        return null;

      }//catch

    }
    #endregion


    #region Output writing
    #region ConsoleWrite event
    public event EventHandler<ConsoleWriteEventArgs> ConsoleWrite;
    private void OnConsoleWrite(string text) {
      if (ConsoleWrite != null) {
        ConsoleWriteEventArgs args = new ConsoleWriteEventArgs(text);
        ConsoleWrite(this, args);
      }
    }
    #endregion



    public void Write(string text) {
      lock(_lockObject){
        OnConsoleWrite(text); 
        OutputBuffer.Append(text);
      }
    }
    public void WriteLine(string text) {
      lock(_lockObject){
        OnConsoleWrite(text + Environment.NewLine);
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

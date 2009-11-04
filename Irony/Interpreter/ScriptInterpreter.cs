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
using System.Linq;
using System.Text;
using System.Threading;
using Irony.Ast; 
using Irony.Parsing;

namespace Irony.Interpreter {

  public enum InterpreterStatus {
    Ready,
    Evaluating,
    WaitingMoreInput, //command line only
    SyntaxError,
    RuntimeError,
    Aborted
  }

  public class ScriptInterpreter {
    #region Fields and properties
    public readonly LanguageData Language;
    public InterpreterStatus Status { get; private set; }
    public EvaluationContext EvaluationContext;
    public readonly ValueSet Globals = new ValueSet();
    public LanguageRuntime Runtime; 
    public bool RethrowExceptions = true;

    public Parser Parser;
    public ParseMode ParseMode {
      get { return Parser.Context.Mode; }
      set { Parser.Context.Mode = value; }
    }
    public ParseTree ParsedScript { get; private set; }
    public Thread WorkerThread { get; private set; }
    #endregion 

    #region constructors
    public ScriptInterpreter(Grammar grammar) : this(new LanguageData(grammar)) { }

    public ScriptInterpreter(LanguageData language) {
      Language = language;
      Runtime = Language.Grammar.CreateRuntime();
      Status = InterpreterStatus.Ready;
      Parser = new Parser(Language);
      var topFrame = new StackFrame(Globals);
      EvaluationContext = new EvaluationContext(Runtime, topFrame);
    }
    #endregion 

    public void Evaluate(string script) {
      if (script == null) return;
      if (script.Trim() == string.Empty && Status == InterpreterStatus.Ready) return;
      Status = InterpreterStatus.Evaluating;
      _lastException = null;
      ParsedScript = this.Parser.Parse(script, "source");
      Evaluate(ParsedScript);
    }

    public void Evaluate(ParseTree parsedScript) {
      int start = Environment.TickCount;
      try {
        ParsedScript = parsedScript;
        Check(ParsedScript != null, "Parsed tree is null, cannot evaluate.");
        UpdateStatusFromParseStatus();
        if (Status != InterpreterStatus.Evaluating) return;
        Check(ParsedScript.Root != null, "Parse tree root is null, cannot evaluate.");
        var astNode = ParsedScript.Root.AstNode;
        Check(astNode != null, "Root AST node is null, cannot evaluate.");
        var iInterpNode = astNode as IInterpretedAstNode;
        Check (iInterpNode != null, "Root AST node does not implement IInterpretedAstNode interface, cannot evaluate.");
        _lastException = null;
        EvaluationContext.ClearLastResult();
        start = Environment.TickCount;
        iInterpNode.Evaluate(EvaluationContext, AstMode.Read);
        EvaluationContext.EvaluationTime = Environment.TickCount - start;
        if (EvaluationContext.HasLastResult)
          EvaluationContext.Write(EvaluationContext.LastResult + Environment.NewLine);
        Status = InterpreterStatus.Ready;
      } catch (Exception ex) {
        _lastException = ex;
        EvaluationContext.EvaluationTime = Environment.TickCount - start;
        Status = InterpreterStatus.RuntimeError;
        if (RethrowExceptions)
          throw;
      }
    }

    private void UpdateStatusFromParseStatus() {
      switch (ParsedScript.Status) {
        case ParseTreeStatus.Error:
          Status = InterpreterStatus.SyntaxError;
          return;
        case ParseTreeStatus.Partial:
          Status = InterpreterStatus.WaitingMoreInput;
          return;
        default:
          Status = InterpreterStatus.Evaluating;
          return;
      }
    }


    private void Check(bool condition, string message) {
      if (!condition)
        throw new RuntimeException(message); 
    }

    public string GetOutput() {
      return EvaluationContext.OutputBuffer.ToString(); 
    }
    public void ClearOutputBuffer() {
      EvaluationContext.OutputBuffer.Length = 0;
    }

    public Exception LastException {
      get { return _lastException; }
    } Exception _lastException;

    public ParserMessageList GetParserMessages() {
      if (ParsedScript == null)
        return new ParserMessageList();
      else
        return ParsedScript.ParserMessages;
    }

    #region Async evaluation
    public void EvaluateAsync(string script) {
      if (script == null) return; 
      if (script.Trim() == string.Empty && Status == InterpreterStatus.Ready) return;
      if (IsBusy())
        throw new Exception("Interpreter is busy.");
      Status = InterpreterStatus.Evaluating;
      WorkerThread = new Thread(AsyncThreadStart);
      WorkerThread.Start(script);
    }
    public bool IsBusy() {
      return Status == InterpreterStatus.Evaluating;
    }

    public void Abort() {
      if (WorkerThread == null || WorkerStateIs(ThreadState.Stopped))
        return;
      WorkerThread.Abort();
      WorkerThread.Join(100);
      WorkerThread = null; 
    }

    private void AsyncThreadStart(object data) {
      var script = data as string;
      Evaluate(script);
    }
    public bool WorkerStateIs(ThreadState state) {
      if (WorkerThread == null) return false;
      return (WorkerThread.ThreadState & state) != 0;
    }
    #endregion


  }//class

}//namespace

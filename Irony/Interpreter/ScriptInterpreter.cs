using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast; 
using Irony.Parsing;

namespace Irony.Interpreter {

  public enum InterpreterState {
    Success,
    WaitingMoreInput,
    SyntaxError,
    RuntimeError
  }

  public class ScriptInterpreter {
    public readonly LanguageData Language;
    public readonly ValueSet Globals = new ValueSet();
    public Parser Parser;
    public ParsingContext ParsingContext;
    public LanguageRuntime Runtime; 
    StackFrame TopFrame;
    public EvaluationContext EvaluationContext;
    public bool RethrowExceptions = true; 

    public ScriptInterpreter(Grammar grammar) : this(new LanguageData(grammar)) { }

    public ScriptInterpreter(LanguageData language) {
      Language = language;
      Parser = new Parser(Language);
      ParsingContext = new ParsingContext(Parser);
      TopFrame = new StackFrame(Globals);
      Runtime = language.Grammar.CreateRuntime();
      EvaluationContext = new EvaluationContext(Runtime, TopFrame);
    }

    public InterpreterState Evaluate(string script) {
      _lastException = null;
      _lastParseTree = null;
      EvaluationContext.ClearLastResult();;
      _lastParseTree = this.Parser.Parse(ParsingContext, script, "source");
      if (_lastParseTree.Errors.Count > 0)
        return InterpreterState.SyntaxError;
      return Evaluate(_lastParseTree);
    }

    public InterpreterState Evaluate(ParseTree parsedScript) {
      _lastException = null;
      _lastParseTree = parsedScript;
      if (_lastParseTree.Errors.Count > 0)
        return InterpreterState.SyntaxError;
      EvaluationContext.ClearLastResult(); ;
      try {
        var astNode = _lastParseTree.Root.AstNode;
        var iInterpNode = astNode as IInterpretedAstNode;
        if (iInterpNode == null) return InterpreterState.Success;
        iInterpNode.Evaluate(EvaluationContext, 0);
        if (EvaluationContext.HasLastResult)
          EvaluationContext.Write(EvaluationContext.LastResult + Environment.NewLine);
        return InterpreterState.Success;
      } catch (Exception ex) {
        _lastException = ex;
        if (RethrowExceptions)
          throw;
        return InterpreterState.RuntimeError;
      }
    }
    public string GetOutput() {
      return EvaluationContext.OutputBuffer.ToString(); 
    }

    public ParseTree LastParseTree {
      get { return _lastParseTree; }
    } ParseTree _lastParseTree;

    public Exception LastException {
      get { return _lastException; }
    } Exception _lastException;

    public SyntaxErrorList GetSyntaxErrors() {
      if (_lastParseTree == null)
        return new SyntaxErrorList();
      else
        return LastParseTree.Errors;
    }

  }//class

}//namespace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast; 
using Irony.Parsing;

namespace Irony.Interpreter {

  public class ScriptInterpreter {
    public readonly LanguageData Language;
    public readonly ValueSet Globals = new ValueSet(); 
    StackFrame TopFrame;
    public EvaluationContext EvaluationContext;
    public Parser Parser;
    public ParsingContext ParsingContext; 

    public ScriptInterpreter(LanguageData language) {
      Language = language;
      Parser = new Parser(Language);
      ParsingContext = new ParsingContext(Parser);
      TopFrame = new StackFrame(Globals);
      var runtime = language.Grammar.CreateRuntime();
      EvaluationContext = new EvaluationContext(runtime, TopFrame);  
    }

    public void Evaluate(string script) {
      var tree = this.Parser.Parse(ParsingContext, script, "source");
      var astNode = tree.Root.AstNode;
      var iInterpNode = astNode as IInterpretedAstNode;
      iInterpNode.Evaluate(EvaluationContext, 0);
      //printout
    }
  
  }//class

}//namespace

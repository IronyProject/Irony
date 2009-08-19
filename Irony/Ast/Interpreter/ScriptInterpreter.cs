using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Irony.Ast.Interpreter {

  public class ScriptInterpreter {
    public readonly LanguageData Language;
    public readonly ValueSet Globals = new ValueSet(); 
    StackFrame TopFrame;
    public EvaluationContext EvaluationContext;
    public Compiler Compiler;
    public CompilerContext CompilerContext; 

    public ScriptInterpreter(LanguageData language) {
      Language = language;
      Compiler = new Compiler(Language);
      CompilerContext = new CompilerContext(Compiler);
      TopFrame = new StackFrame(Globals);
      var runtime = language.Grammar.CreateRuntime();
      EvaluationContext = new EvaluationContext(runtime, TopFrame);  
    }

    public void Evaluate(string script) {
      var tree = this.Compiler.Parse(CompilerContext, script, "source");
      var astNode = tree.Root.AstNode;
      var iInterpNode = astNode as IInterpretedNode;
      iInterpNode.Evaluate(EvaluationContext, AstUseMode.Read);
      //printout
    }
  
  }//class

}//namespace

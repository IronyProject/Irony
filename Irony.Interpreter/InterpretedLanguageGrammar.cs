using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace Irony.Interpreter {
  /// <summary> Base class for languages that use Irony Interpreter to execute scripts. </summary>
  public class InterpretedLanguageGrammar : Grammar, ICanRunSample {
    public InterpretedLanguageGrammar(bool caseSensitive)  : base(caseSensitive) {
      this.DefaultLiteralNodeType = typeof(LiteralValueNode);  //default node type for literals
      this.DefaultIdentifierNodeType = typeof(IdentifierNode); 
      this.LanguageFlags = LanguageFlags.CreateAst;
    }


    // This method allows custom implementation of running a sample in Grammar Explorer
    // By default it evaluates a parse tree using default interpreter.
    // Irony's interpeter has one restriction: once a script (represented by AST node) is evaluated by an interpreter (ScriptApp), 
    // its internal fields in AST nodes become tied to this particular instance of ScriptApp (more precisely ScriptAppInfo).
    // If you want to evaluate the AST tree again, you have to do it in the context of the same ScriptAppInfo. 
    // Grammar Explorer may call RunSample method repeatedly for evaluation of the same parsed script. So we keep ScriptApp instance in 
    // the field, and if we get the same script node, then we reuse the ScriptApp thus satisfying the requirement. 
    private ScriptApp _app;
    private AstNode _prevRoot;
    
    public virtual string RunSample(RunSampleArgs args) {
      var astRoot = args.ParsedSample.Root.AstNode as AstNode;
      if (astRoot == null)
        throw new Exception("Cannot execute sample: AST root is null.");
      if (_app == null || _prevRoot != astRoot)
        _app = new ScriptApp(args.Language); 
      _prevRoot = astRoot;

      for (int i = 0; i < 10000; i++) //for perf measurements, to execute 1000 times
        _app.Evaluate(args.ParsedSample);
      return _app.OutputBuffer.ToString();
    }

    public virtual LanguageRuntime CreateRuntime(LanguageData language) {
      return new LanguageRuntime(language); 
    }  
  } //grammar class

}

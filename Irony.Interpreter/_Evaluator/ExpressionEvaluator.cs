using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using System.Threading;
using Irony.Interpreter.Ast; 

namespace Irony.Interpreter.Evaluator {
  public class ExpressionEvaluator {
    public ExpressionEvaluatorGrammar Grammar {get; private set;}
    public Parser Parser {get; private set;} 
    public LanguageData Language {get; private set;}
    public LanguageRuntime Runtime {get; private set;} 
    public ScriptApp App {get; private set;}

    public IDictionary<string, object> Globals {
      get { return App.Globals; }
    }

    public ExpressionEvaluator() {
      Grammar = new ExpressionEvaluatorGrammar();
      Language = new LanguageData(Grammar);
      Init();
    }

    public ExpressionEvaluator(LanguageData language) {
      Language = language;
      Grammar = Language.Grammar as ExpressionEvaluatorGrammar;
      Init(); 
    }

    private void Init() {
      Parser = new Parser(Language);
      Runtime = Grammar.CreateRuntime(Language);
      //Import static methods of types
      Runtime.AddBuiltInMethods(typeof(System.Math));
      Runtime.AddBuiltInMethods(typeof(Environment));
      // Import instance methods of CurrentThread
      Runtime.AddBuiltInMethods(Thread.CurrentThread);
      
      App = new ScriptApp(Runtime);
    }
    
    public object Evaluate(string script) {
      var result = App.Evaluate(script);
      return result; 
    }

    public object EvaluateAgain() {
      return App.EvaluateAgain(); 
    }

    public object Evaluate(ParseTree parsedScript) {
      var result = App.Evaluate(parsedScript);
      return result;
    }


  }//class
}

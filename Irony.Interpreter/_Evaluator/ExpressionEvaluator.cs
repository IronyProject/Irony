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

    //Default constructor, creates default evaluator 
    public ExpressionEvaluator() : this(new ExpressionEvaluatorGrammar()) { }

    //Allows creating customized evaluators
    public ExpressionEvaluator(ExpressionEvaluatorGrammar grammar) {
      Grammar = grammar;
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
      Runtime.BuiltIns.AddStaticMembers(typeof(System.Math));
      Runtime.BuiltIns.AddStaticMembers(typeof(Environment));
      App = new ScriptApp(Runtime);
    }
    
    public object Evaluate(string script) {
      var result = App.Evaluate(script);
      return result; 
    }

    public object Evaluate(ParseTree parsedScript) {
      var result = App.Evaluate(parsedScript);
      return result;
    }

    //Evaluates again the previously parsed/evaluated script
    public object Evaluate() {
      return App.Evaluate(); 
    }


  }//class
}

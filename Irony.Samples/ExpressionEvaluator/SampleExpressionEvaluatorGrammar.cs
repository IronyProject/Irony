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
using Irony.Parsing;
using Irony.Interpreter.Evaluator;

namespace Irony.Samples {
  //The purpose of this class is pure convenience - to make expression evaluator grammar (which is in Irony.Interpreter assembly) to appear 
  // with other sample grammars. 
  [Language("SampleExpressionEvaluator", "1.0", "Multi-line expression evaluator")]
  public class SampleExpressionEvaluatorGrammar : ExpressionEvaluatorGrammar {
    public SampleExpressionEvaluatorGrammar() {
      this.GrammarComments =
@"Sample derivative of Irony expression evaluator. 
Case-insensitive. Supports big integers, float data types, variables, assignments,
arithmetic operations, augmented assignments (+=, -=), inc/dec (++,--), strings with embedded expressions.";
    }

    public override Interpreter.LanguageRuntime CreateRuntime(Parsing.LanguageData language) {
      var runtime = base.CreateRuntime(language);
      return runtime; 
    }

    #region Running in Grammar Explorer
    private static ExpressionEvaluator _evaluator;
    public override string RunSample(RunSampleArgs args) {
      if (_evaluator == null) {
        _evaluator = new ExpressionEvaluator(args.Language);
        // Example: importing a single instance member or all instance members of some object
        // Import instance methods of GreetingsHelper. Note: because Irony currently uses reflection to call methods, methods must be public
        var helper = new GreetingsHelper();
        _evaluator.Runtime.BuiltIns.AddInstanceMember("GetDate", this);
        _evaluator.Runtime.BuiltIns.AddInstanceMembers(helper);
      }
      _evaluator.App.ClearOutputBuffer();
      _evaluator.Evaluate(args.ParsedSample);
      return _evaluator.App.OutputBuffer.ToString();
    }

    public string GetDate() {
      return DateTime.Now.ToLongDateString(); 
    }
    #endregion

    //An example of adding a built-in method
    class GreetingsHelper {
      public string GetGreeting(string user) {
        return "Hello, " + user + "!";
      }
    }

  }//class

}//namespace



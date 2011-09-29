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
using Irony.Interpreter; 
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

    //Intercept creating runtime and add custom builtin method
    public override LanguageRuntime CreateRuntime(Parsing.LanguageData language) {
      var runtime = base.CreateRuntime(language);
      runtime.BuiltIns.AddMethod(MethodHelper.GetGreeting, "GetGreeting", 1, 1, "user");
      runtime.BuiltIns.AddMethod(MethodHelper.GetDate, "GetDate");
      return runtime; 
    }

    #region Running in Grammar Explorer
    private static ExpressionEvaluator _evaluator;
    public override string RunSample(RunSampleArgs args) {
      if (_evaluator == null) 
        _evaluator = new ExpressionEvaluator(this);
      _evaluator.App.ClearOutputBuffer();
      //for (int i = 0; i < 1000; i++)  //for perf measurements, to execute 1000 times
        _evaluator.Evaluate(args.ParsedSample);
      return _evaluator.App.OutputBuffer.ToString();
    }

    #endregion

    //An example of adding a built-in method
    static class MethodHelper {
      public static string GetGreeting(ScriptThread thread, object[] args) {
        return "Hello, " + args[0] + "!";
      }
      public static string GetDate(ScriptThread thread, object[] args) {
        return DateTime.Today.ToLongDateString();
      }
    }

  }//class

}//namespace



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
      if (_evaluator == null) {
        _evaluator = new ExpressionEvaluator(this);
        //Adding custom object to demo accessing to properties, fields and methods
        _evaluator.Globals["foo"] = new Foo();
        //Adding an array to demo access by index
        _evaluator.Globals["primes"] = new int[] {3, 5, 7, 11, 13};
        var nums = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        nums["one"] = "1";
        nums["two"] = "2";
        nums["three"] = "2";
        _evaluator.Globals["nums"] = nums;
        //Check data row access
        var t = new System.Data.DataTable();
        t.Columns.Add("Name", typeof(string));
        t.Columns.Add("Age", typeof(int)); 
        var row = t.NewRow();
        row["Name"] = "John";
        row["Age"] = 30;
        _evaluator.Globals["row"] = row;        
      }
      _evaluator.App.ClearOutputBuffer();
      //for (int i = 0; i < 1000; i++)  //for perf measurements, to execute 1000 times
        _evaluator.Evaluate(args.ParsedSample);
      return _evaluator.App.OutputBuffer.ToString();
    }

    #endregion

    public class Foo {
      public string SampleField = "Sample field value";
      public string SampleProp { get; set; }

      public Foo() {
        SampleProp = "Sample prop value.";
      }
      public string GetStuff() {
        return "stuff";
      }

      public string this[int i] {
        get { return "#" + i; }
        set { }
      }
      public string this[string key] {
        get { return "Value-for-" + key; }
        set { }
      }
    }

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



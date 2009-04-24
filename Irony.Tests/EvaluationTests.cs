using System;
using System.Collections.Generic;
using System.Text;
using Irony.CompilerServices;
using Irony.Samples;
using Irony.Samples.Scheme;
using Irony.RuntimeServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Irony.Tests {

  [TestClass]
  public class EvaluationTests  {

    private void Evaluate(Grammar grammar, string script, bool expectError, object result) {
      var compiler = new Compiler(grammar);
      var context = new CompilerContext(compiler);
      var source = new SourceStream(script, "source"); 
      AstNode root = compiler.Parse(context, source);
      compiler.AnalyzeCode(root, context); 
      if (!expectError && context.Errors.Count > 0)
        Assert.Fail("Unexpected source error(s) found: " + context.Errors[0].Message);
      var evalContext = new EvaluationContext(context.Runtime, root);
      root.Evaluate(evalContext);
      Assert.AreEqual(evalContext.CurrentResult, result, "Evaluation result is null, expected " + result);
    }

    private void Parse(Grammar grammar, string script, bool expectError) {
      var compiler = new Compiler(grammar);
      var context = new CompilerContext(compiler);
      var source = new SourceStream(script, "source");
      AstNode root = compiler.Parse(context, source);
      compiler.AnalyzeCode(root, context);
      if (!expectError && context.Errors.Count > 0)
        Assert.Fail("Unexpected source error(s) found: " + context.Errors[0].Message);
    }

    [TestMethod]
    public void EvaluateSchemeFib() {
      string fib = @"
(define (fib n)
  (if (< n 2) 
     1 
     (+ (fib (- n 1)) (fib (- n 2)))))
(fib 10) ;; ==89
";
      Evaluate(new SchemeGrammar(), fib, false, 89);  
    }//method

    [TestMethod]
    public void EvaluateSchemeArithm() {
      string formula = "(* (+ 2 3) (/ 6 3) ) ;=10";
      Evaluate(new SchemeGrammar(), formula, false, 10);
    }//method

    [TestMethod]
    public void EvaluateExprEvaluator() {
      string formula = @"
x = 3 + 5
y = 3
x / 2 + y * 2 - 2 #result= 8
";
      Evaluate(new ExpressionEvaluatorGrammar(), formula, false, 8);
    }//method

    [TestMethod]
    public void ParseCSharpParsing() {
      string test = @"
namespace Test {
  class @class
  {
	  public static void @static(bool @bool) {
		  if (@bool)
			  System.Console.WriteLine(""true"");
		  else
			  System.Console.WriteLine(""false"");
	  }	
  }
}
";
      Parse(new Irony.Samples.CSharp.CSharpGrammar(), test, false);
    }//method
  
  }//class
}//namespace


/* example for c# from 3.0 spec:
namespace Test {
  class @class
  {
	  public static void @static(bool @bool) {
		  if (@bool)
			  System.Console.WriteLine("true");
		  else
			  System.Console.WriteLine("false");
	  }	
  }
}
 
class Class1
{
	static void M() {
		cl\u0061ss.st\u0061tic(true);
	}
}

*/
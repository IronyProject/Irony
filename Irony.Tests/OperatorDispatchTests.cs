using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Irony.Tests {
  using Irony.Runtime;
  using System.Diagnostics;
  using BigInteger = Microsoft.Scripting.Math.BigInteger;
  using Complex = Microsoft.Scripting.Math.Complex64;

  //Note: currently test not functional - doesn't compile or run. May need and bring back to life some day.

  /// <summary>
  /// Tests OperatorDispatch class
  /// </summary>
  [TestClass]
  public class OperatorDispatchTests {
    public OperatorDispatchTests() {
    }

    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext {
      get {
        return testContextInstance;
      }
      set {
        testContextInstance = value;
      }
    }

    #region Additional test attributes
    //
    // You can use the following additional attributes as you write your tests:
    //
    // Use ClassInitialize to run code before running the first test in the class
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // Use ClassCleanup to run code after all tests in a class have run
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // Use TestInitialize to run code before running each test 
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // Use TestCleanup to run code after each test has run
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //
    #endregion

    private void CheckResult(object value, object expected, string testName) {
      Assert.IsTrue(value != null && value.GetType() == expected.GetType() && value.Equals(expected),
          "Test [" + testName + "] failed.");
    }

    [TestMethod]
    public void TestOperatorDispatchPlus() {
      LanguageRuntime runtime = new LanguageRuntime();
      CallDispatcher dispatcher = runtime.GetDispatcher("+");
      object v;
      
      v = dispatcher.Evaluate(2, 3);
      CheckResult(v, 5, "int + int");

      v = dispatcher.Evaluate(2, 3.0);
      CheckResult(v, 5.0, "int + double");

      v = dispatcher.Evaluate(2, "3");
      CheckResult(v, "23", "int + string");

      v = dispatcher.Evaluate("2", "3");
      CheckResult(v, "23", "string + string");

      //Note that for all operations on bytes/sbytes/int16 types our implementation returns int32
      v = dispatcher.Evaluate((sbyte)2, (sbyte)3);
      CheckResult(v, 5, "sbyte + sbyte");

      v = dispatcher.Evaluate((byte)2, (byte)3);
      CheckResult(v, 5, "byte + byte");

      v = dispatcher.Evaluate((Int16)2, (Int16)3);
      CheckResult(v, 5, "Int16 + Int16");

      v = dispatcher.Evaluate((byte)2, 3);
      CheckResult(v, 5, "byte + int");

      v = dispatcher.Evaluate(int.MaxValue, 10); //we get overflow here, and switch to Int64
      CheckResult(v, (Int64)int.MaxValue + 10, "Int32 overflow");

      BigInteger xBig = BigInteger.Create(1000000000); //one billion
      xBig = xBig * xBig * xBig;
      xBig = xBig * xBig * xBig - 1; //that makes it really big
      v = dispatcher.Evaluate(xBig, 2);
      Assert.IsTrue(v != null && v.GetType() == typeof(BigInteger) && v.ToString().EndsWith("0001"), "Test [BigInteger + int] failed");

      v = dispatcher.Evaluate(new Complex(1, 2), new Complex(2, 3)); 
      CheckResult(v, new Complex(3, 5), "complex + complex");
    }


    // Uncomment the TestMethod attribute and run test in Release mode to see the performance numbers in the Output Window
    [TestMethod]
    public void PerformanceEvaluation() {
      LanguageRuntime runtime = new LanguageRuntime();
      CallDispatcher dispatcher = runtime.GetDispatcher("+");

      Trace.WriteLine("");
      Trace.WriteLine("Performance evaluation, time in nanoseconds: ------------------------------------");
      DispatchMultiple(dispatcher, 2, 3);
      DispatchMultiple(dispatcher, 2.0, 3);
      DispatchMultiple(dispatcher, 2.0, 3.0);
      DispatchMultiple(dispatcher, 2, 3.0f);
      DispatchMultiple(dispatcher, (Int64)2,(Int16) 3);
      DispatchMultiple(dispatcher, (UInt64)2, 3);
      DispatchMultiple(dispatcher, (byte)2, (byte)3);
      DispatchMultiple(dispatcher, 2, "3");
      Trace.WriteLine("");

    }
    private void DispatchMultiple(CallDispatcher dispatcher, object x, object y) {
      int count = 1000000; // one million
      int index = count / 10; //do it in 10-packs to reduce the impact of the loop counting
      Stopwatch sw = new Stopwatch();
      sw.Start();
      while (index-- > 0) {
        dispatcher.Evaluate(x, y);
        dispatcher.Evaluate(x, y);
        dispatcher.Evaluate(x, y);
        dispatcher.Evaluate(x, y);
        dispatcher.Evaluate(x, y);
        dispatcher.Evaluate(x, y);
        dispatcher.Evaluate(x, y);
        dispatcher.Evaluate(x, y);
        dispatcher.Evaluate(x, y);
        dispatcher.Evaluate(x, y);
      }
      sw.Stop();
      long timeNsPerOper = sw.ElapsedMilliseconds * 1000000 / count; //timeMs * (one million ns per ms) divided by number of runs
      string msg = x.GetType().Name +  " + " + y.GetType().Name;
      Trace.WriteLine(msg.PadRight(20) + ": " + timeNsPerOper);
    }


  }//class
}//namespace

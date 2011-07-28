using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;

namespace Irony.Tests {
#if USE_NUNIT
  using NUnit.Framework;
  using TestClass = NUnit.Framework.TestFixtureAttribute;
  using TestMethod = NUnit.Framework.TestAttribute;
  using TestInitialize = NUnit.Framework.SetUpAttribute;
#else
  using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

  [TestClass]
  public class LineContinuationTests : TerminalTestsBase {

    [TestMethod]
    public void TestSimpleContinuationTerminal() {
      SetTerminal(new LineContinuationTerminal("LineContinuation", "\\"));
      TryMatch("\\\r\t");
      Assert.IsTrue(_token.Category == TokenCategory.Outline, "Failed to read simple line continuation terminal");
    }

    [TestMethod]
    public void TestDefaultContinuationTerminal()
    {
      SetTerminal(new LineContinuationTerminal("LineContinuation"));
      TryMatch("_\r\n\t");
      Assert.IsTrue(_token.Category == TokenCategory.Outline, "Failed to read default line continuation terminal");

      TryMatch("\\\v    ");
      Assert.IsTrue(_token.Category == TokenCategory.Outline, "Failed to read default line continuation terminal");
    }

    [TestMethod]
    public void TestComplexContinuationTerminal()
    {
      SetTerminal(new LineContinuationTerminal("LineContinuation", @"\continue", @"\cont", "++CONTINUE++"));
      TryMatch("\\cont   \r\n    ");
      Assert.IsTrue(_token.Category == TokenCategory.Outline, "Failed to read complex line continuation terminal");

      TryMatch("++CONTINUE++\t\v");
      Assert.IsTrue(_token.Category == TokenCategory.Outline, "Failed to read complex line continuation terminal");
    }

    [TestMethod]
    public void TestIncompleteContinuationTerminal()
    {
      SetTerminal(new LineContinuationTerminal("LineContinuation"));
      TryMatch("\\   garbage");
      Assert.IsTrue(_token.Category == TokenCategory.Error, "Failed to read incomplete line continuation terminal");

      SetTerminal(new LineContinuationTerminal("LineContinuation"));
      TryMatch("_");
      Assert.IsTrue(_token.Category == TokenCategory.Error, "Failed to read incomplete line continuation terminal");
    }  
  }
}

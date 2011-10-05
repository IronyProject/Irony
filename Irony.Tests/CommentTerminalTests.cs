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
  public class CommentTerminalTests : TerminalTestsBase {

    [TestMethod]
    public void TestCommentTerminal() {
      SetTerminal(new CommentTerminal("Comment", "/*", "*/"));
      TryMatch("/* abc  */");
      Assert.IsTrue(_token.Category == TokenCategory.Comment, "Failed to read comment");

      SetTerminal(new CommentTerminal("Comment", "//", "\n"));
      TryMatch("// abc  \n   ");
      Assert.IsTrue(_token.Category == TokenCategory.Comment, "Failed to read line comment");

    }//method

  }//class
}//namespace

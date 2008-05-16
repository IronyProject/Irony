using System;
using System.Collections.Generic;
using System.Text;
using Irony.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Irony.Tests {

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

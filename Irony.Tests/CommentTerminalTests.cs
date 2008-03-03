using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using NUnit.Framework.Constraints;
using Irony.Compiler;

namespace Irony.Tests {

  [TestFixture]
  public class CommentTerminalTests : TerminalTestsBase {

    [Test]
    public void TestCommentTerminal() {
      _terminal = new CommentTerminal("Comment", "/*", "*/");
      _terminal.Init(_grammar);
      TryMatch("/* abc  */");
      Assert.That(_token.Category == TokenCategory.Comment, "Failed to read comment");

      _terminal = new CommentTerminal("Comment", "//", "\n");
      _terminal.Init(_grammar);
      TryMatch("// abc  \n   ");
      Assert.That(_token.Category == TokenCategory.Comment, "Failed to read line comment");

    }//method

  }//class
}//namespace

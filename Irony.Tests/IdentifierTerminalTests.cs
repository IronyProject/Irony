using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using NUnit.Framework.Constraints;
using Irony.Compiler;

namespace Irony.Tests {

  [TestFixture]
  public class IdentifierTerminalTests : TerminalTestsBase {

    [Test]
    public void TestCSharpIdentifier() {
      _terminal = TerminalFactory.CreateCSharpIdentifier("Identifier");
      _terminal.Init(_grammar);
      TryMatch("x ");
      Assert.That(_token.Term.Name == "Identifier", "Failed to parse identifier");
      Assert.That((string)_token.Value == "x", "Failed to parse identifier");
      TryMatch("_a01 ");
      Assert.That(_token.Term.Name == "Identifier", "Failed to parse identifier starting with _");
      Assert.That((string)_token.Value == "_a01", "Failed to parse identifier starting with _");

      TryMatch("0abc ");
      Assert.That(_token == null, "Erroneously recognized an identifier.");

      TryMatch(@"_\u0061bc ");
      Assert.That(_token.Term.Name == "Identifier", "Failed to parse identifier starting with _");
      Assert.That((string)_token.Value == "_abc", "Failed to parse identifier containing escape sequence \\u");

      TryMatch(@"a\U00000062c_ ");
      Assert.That(_token.Term.Name == "Identifier", "Failed to parse identifier starting with _");
      Assert.That((string)_token.Value == "abc_", "Failed to parse identifier containing escape sequence \\U");
    }//method

  }//class
}//namespace


/* example for c# from 3.0 spec:
class @class
{
	public static void @static(bool @bool) {
		if (@bool)
			System.Console.WriteLine("true");
		else
			System.Console.WriteLine("false");
	}	
}
class Class1
{
	static void M() {
		cl\u0061ss.st\u0061tic(true);
	}
}

*/
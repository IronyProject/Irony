using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Irony.Tests {

  [TestClass]
  public class IdentifierTerminalTests : TerminalTestsBase {

    [TestMethod]
    public void TestCSharpIdentifier() {
      SetTerminal(TerminalFactory.CreateCSharpIdentifier("Identifier"));
      TryMatch("x ");
      Assert.IsTrue(_token.Terminal.Name == "Identifier", "Failed to parse identifier");
      Assert.IsTrue((string)_token.Value == "x", "Failed to parse identifier");
      TryMatch("_a01 ");
      Assert.IsTrue(_token.Terminal.Name == "Identifier", "Failed to parse identifier starting with _");
      Assert.IsTrue((string)_token.Value == "_a01", "Failed to parse identifier starting with _");

      TryMatch("0abc ");
      Assert.IsTrue(_token == null, "Erroneously recognized an identifier.");

      TryMatch(@"_\u0061bc ");
      Assert.IsTrue(_token.Terminal.Name == "Identifier", "Failed to parse identifier starting with _");
      Assert.IsTrue((string)_token.Value == "_abc", "Failed to parse identifier containing escape sequence \\u");

      TryMatch(@"a\U00000062c_ ");
      Assert.IsTrue(_token.Terminal.Name == "Identifier", "Failed to parse identifier starting with _");
      Assert.IsTrue((string)_token.Value == "abc_", "Failed to parse identifier containing escape sequence \\U");
    }//method

    [TestMethod]
    public void TestIdentifierCaseRestrictions() {
      var id = new IdentifierTerminal("identifier"); 
      id.CaseRestriction = CaseRestriction.None;
      SetTerminal(id);

      TryMatch("aAbB");
      Assert.IsTrue(_token != null, "Failed to scan an identifier aAbB.");

      id.CaseRestriction = CaseRestriction.FirstLower;
      SetTerminal(id);
      TryMatch("BCD");
      Assert.IsTrue(_token == null, "Erroneously recognized an identifier BCD with FirstLower restriction.");
      TryMatch("bCd ");
      Assert.IsTrue(_token != null && _token.ValueString == "bCd", "Failed to scan identifier bCd with FirstLower restriction.");

      id.CaseRestriction = CaseRestriction.FirstUpper;
      SetTerminal(id);
      TryMatch("cDE");
      Assert.IsTrue(_token == null, "Erroneously recognized an identifier cDE with FirstUpper restriction.");
      TryMatch("CdE");
      Assert.IsTrue(_token != null && _token.ValueString == "CdE", "Failed to scan identifier CdE with FirstUpper restriction.");

      id.CaseRestriction = CaseRestriction.AllLower;
      SetTerminal(id);
      TryMatch("DeF");
      Assert.IsTrue(_token == null, "Erroneously recognized an identifier DeF with AllLower restriction.");
      TryMatch("def");
      Assert.IsTrue(_token != null && _token.ValueString == "def", "Failed to scan identifier def with AllLower restriction.");

      id.CaseRestriction = CaseRestriction.AllUpper;
      SetTerminal(id);
      TryMatch("EFg ");
      Assert.IsTrue(_token == null, "Erroneously recognized an identifier EFg with AllUpper restriction.");
      TryMatch("EFG");
      Assert.IsTrue(_token != null && _token.ValueString == "EFG", "Failed to scan identifier EFG with AllUpper restriction.");
    }//method

    /*
    [TestMethod]
    public void TestSqlIdentifier() {
      var id = TerminalFactory.CreateSqlExtIdentifier(_grammar, "identifier");
      SetTerminal(id.OutputTerminal);
      TryMatch(@"[a b c]  ");
      Assert.IsTrue((string)_token.Value == "a b c", "Failed to process bracketted identifier [a b c]");
      TryMatch("\"a b c\"  "); //"a b c"
      Assert.IsTrue((string)_token.Value == "a b c", "Failed to process double-quoted identifier \"a b c\"");
    }
    */

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
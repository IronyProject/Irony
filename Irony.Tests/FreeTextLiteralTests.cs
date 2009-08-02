using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Irony.Tests {
  [TestClass]
  public class FreeTextLiteralTests: TerminalTestsBase  {

    //Helper method - replaces single quote with double quote and then calls TryMatch, just to make it easier to write
    // strings with quotes for tests
    private void TryMatchDoubles(string input) {
      input = input.Replace("'", "\"");
      TryMatch(input);
    }

    //The following test method and a fix are contributed by ashmind codeplex user
    [TestMethod]
    public void TestFreeTextLiteral() {
      var term = new FreeTextLiteral("FreeText", ",", ")");
      term.Escapes.Add(@"\\", @"\");
      term.Escapes.Add(@"\,", @",");
      term.Escapes.Add(@"\)", @")"); 

      SetTerminal(term);
      TryMatch(@"abc\\de\,\)fg,");
      Assert.IsNotNull(_token, "Failed to produce a token on valid string.");
      Assert.AreEqual(term, _token.Terminal, "Failed to scan a string - invalid Terminal in the returned token.");
      Assert.AreEqual(_token.Value.ToString(), @"abc\de,)fg", "Failed to scan a string");
    }

  }//class
}//namespace

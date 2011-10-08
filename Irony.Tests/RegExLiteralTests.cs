using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
  public class RegexLiteralTests: TerminalTestsBase  {

    //Helper method - replaces single quote with double quote and then calls TryMatch, just to make it easier to write
    // strings with quotes for tests
    private void TryMatchDoubles(string input) {
      input = input.Replace("'", "\"");
      TryMatch(input);
    }

    //The following test method and a fix are contributed by ashmind codeplex user
    [TestMethod]
    public void TestRegExLiteral() {
      var term = new RegexLiteral("RegEx");
      SetTerminal(term);
      TryMatch(@"/abc\\\/de/gm  ");
      Assert.IsNotNull(_token, "Failed to produce a token on valid string.");
      Assert.AreEqual(term, _token.Terminal, "Failed to scan a string - invalid Terminal in the returned token.");
      Assert.IsNotNull(_token.Value, "Token Value field is null - should be Regex object.");
      var regex = _token.Value as Regex;
      Assert.IsNotNull(regex, "Failed to create Regex object.");
      var match = regex.Match(@"00abc\/de00"); 
      Assert.AreEqual(match.Index, 2, "Failed to match a regular expression");
    }

  }//class
}//namespace

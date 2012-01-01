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
  public class FreeTextLiteralTests  {

    //The following test method and a fix are contributed by ashmind codeplex user
    [TestMethod]
    public void TestFreeTextLiteral_Escapes() {
      Parser parser; Token token;

      //Escapes test
      var term = new FreeTextLiteral("FreeText", ",", ")");
      term.Escapes.Add(@"\\", @"\");
      term.Escapes.Add(@"\,", @",");
      term.Escapes.Add(@"\)", @")"); 

      parser = TestHelper.CreateParser(term, null);
      token = parser.ParseInput(@"abc\\de\,\)fg,");
      Assert.IsNotNull(token, "Failed to produce a token on valid string.");
      Assert.AreEqual(term, token.Terminal, "Failed to scan a string - invalid Terminal in the returned token.");
      Assert.AreEqual(token.Value.ToString(), @"abc\de,)fg", "Failed to scan a string");

      term = new FreeTextLiteral("FreeText", FreeTextOptions.AllowEof, ";");
      parser = TestHelper.CreateParser(term, null);
      token = parser.ParseInput(@"abcdefg");
      Assert.IsNotNull(token, "Failed to produce a token for free text ending at EOF.");
      Assert.AreEqual(term, token.Terminal, "Failed to scan a free text ending at EOF - invalid Terminal in the returned token.");
      Assert.AreEqual(token.Value.ToString(), @"abcdefg", "Failed to scan a free text ending at EOF");

      //The following test method and a fix are contributed by ashmind codeplex user
      //VAR
      //MESSAGE:STRING80;
      //(*_ORError Message*)
      //END_VAR
      term = new FreeTextLiteral("varContent", "END_VAR");
      term.Firsts.Add("VAR"); 
      parser = TestHelper.CreateParser(term, null);
      token = parser.ParseInput("VAR\r\nMESSAGE:STRING80;\r\n(*_ORError Message*)\r\nEND_VAR");
      Assert.IsNotNull(token, "Failed to produce a token on valid string.");
      Assert.AreEqual(term, token.Terminal, "Failed to scan a string - invalid Terminal in the returned token.");
      Assert.AreEqual(token.ValueString, "\r\nMESSAGE:STRING80;\r\n(*_ORError Message*)\r\n", "Failed to scan a string");

      term = new FreeTextLiteral("freeText", FreeTextOptions.AllowEof);
      parser = TestHelper.CreateParser(term, terminator: null, suppressWhitespace: true);
      token = parser.ParseInput(" ");
      Assert.IsNotNull(token, "Failed to produce a token on valid string.");
      Assert.AreEqual(term, token.Terminal, "Failed to scan a string - invalid Terminal in the returned token.");
      Assert.AreEqual(token.ValueString, " ", "Failed to scan a string");

    }

  }//class
}//namespace

using Irony.Parsing;

namespace Irony.Tests {
#if USE_NUNIT
    using NUnit.Framework;
    using TestClass = NUnit.Framework.TestFixtureAttribute;
    using TestMethod = NUnit.Framework.TestAttribute;
    using TestInitialize = NUnit.Framework.SetUpAttribute;
  using Assert = NUnit.Framework.Legacy.ClassicAssert;
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

  [TestClass]
  public class NewLineTests
  {

    [TestMethod]
    public void TestLineFeed() {
      Parser parser; Token token;

      NumberLiteral number = new NumberLiteral("Number");
      parser = TestHelper.CreateParser(number);

      token = parser.ParseInput("\n\n   word");
      Assert.AreEqual(2, token.Location.Line, "Location line problem");
      Assert.AreEqual(3, token.Location.Column, "Location column problem");
      Assert.AreEqual(5, token.Location.Position, "Location position problem");
    }//method

    [TestMethod]
    public void TestCarriageReturn() {
      Parser parser; Token token;

      NumberLiteral number = new NumberLiteral("Number");
      parser = TestHelper.CreateParser(number);

      token = parser.ParseInput("\r\r   word");
      Assert.AreEqual(2, token.Location.Line, "Location line problem");
      Assert.AreEqual(3, token.Location.Column, "Location column problem");
      Assert.AreEqual(5, token.Location.Position, "Location position problem");
    }//method

    [TestMethod]
    public void TestCarriageReturnPlusLineFeed() {
      Parser parser; Token token;

      NumberLiteral number = new NumberLiteral("Number");
      parser = TestHelper.CreateParser(number);

      token = parser.ParseInput("\r\n\r\n   word");
      Assert.AreEqual(2, token.Location.Line, "Location line problem");
      Assert.AreEqual(3, token.Location.Column, "Location column problem");
      Assert.AreEqual(7, token.Location.Position, "Location position problem");
    }//method

  }//class
}//namespace

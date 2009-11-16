using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Irony.Tests {

  [TestClass]
  public class DataLiteralsTests : TerminalTestsBase {
  
    [TestMethod]
    public void TestDataLiterals() {
      Terminal term;

      // FixedLengthLiteral ---------------------------------------------------------
      term = new FixedLengthLiteral("fixedLengthInteger", 2, TypeCode.Int32);
      SetTerminal(term);
      
      TryMatch("1200");
      Assert.IsTrue(_token != null && _token.Value != null, "Failed to parse fixed-length integer.");
      Assert.IsTrue((int)_token.Value == 12, "Failed to parse fixed-length integer - result value does not match.");

      term = new FixedLengthLiteral("fixedLengthString", 2, TypeCode.String);
      SetTerminal(term);
      TryMatch("abcd");
      Assert.IsTrue(_token != null && _token.Value != null, "Failed to parse fixed-length string.");
      Assert.IsTrue((string)_token.Value == "ab", "Failed to parse fixed-length string - result value does not match");

      // DsvLiteral ----------------------------------------------------------------
      term = new DsvLiteral("DsvInteger", TypeCode.Int32, ",");
      SetTerminal(term);
      TryMatch("12,");
      Assert.IsTrue(_token != null && _token.Value != null, "Failed to parse CSV integer.");
      Assert.IsTrue((int)_token.Value == 12, "Failed to parse CSV integer - result value does not match.");

      term = new DsvLiteral("DsvInteger", TypeCode.String, ",");
      SetTerminal(term);
      TryMatch("ab,");
      Assert.IsTrue(_token != null && _token.Value != null, "Failed to parse CSV string.");
      Assert.IsTrue((string)_token.Value == "ab", "Failed to parse CSV string - result value does not match.");
    
      // QuotedValueLiteral ----------------------------------------------------------------
      term = new QuotedValueLiteral ("QVDate", "#", TypeCode.DateTime);
      SetTerminal(term);
      TryMatch("#11/15/2009#");
      Assert.IsTrue(_token != null && _token.Value != null, "Failed to parse quoted date.");
      Assert.IsTrue((DateTime)_token.Value == new DateTime(2009, 11, 15), "Failed to parse quoted date - result value does not match.");

    }//method


  }//class

}//namespace

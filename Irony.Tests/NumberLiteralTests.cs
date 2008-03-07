using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using NUnit.Framework.Constraints;
using Irony.Compiler;

namespace Irony.Tests {

  [TestFixture]
  public class NumberLiteralTests : TerminalTestsBase {

    [Test]
    public void GeneralTest() {
      _terminal = new NumberLiteral("Number", TermOptions.NumberAllowBigInts);
      _terminal.Init(_grammar);
      TryMatch("123");
      Assert.That((int)_token.Value == 123, "Failed to read int value");
      TryMatch("123.4");
      Assert.That(Math.Abs((double)_token.Value - 123.4) < 0.000001, "Failed to read float value");
      //100 digits
      string sbig = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
      TryMatch(sbig);
      Assert.That(_token.Value.ToString() == sbig, "Failed to read big integer value");
    }//method

    [Test]
    public void TestCSharpNumber() {
      double eps = 0.0001;
      _terminal = TerminalFactory.CreateCSharpNumber("Number");
      _terminal.Init(_grammar);

      //Simple integers and suffixes
      TryMatch("123 ");
      CheckType(typeof(int));
      //Check that NumberScanInfo record is present in Attributes. The only case it's not there is for single-digit integers, when system
      // uses a quick parse method
      Assert.That(_token.Details != null, "ScanDetails object not found in token.");
      Assert.That((int)_token.Value == 123, "Failed to read int value");
      
      TryMatch("123U ");
      CheckType(typeof(uint));
      Assert.That((uint)_token.Value == 123, "Failed to read uint value");

      TryMatch("123L ");
      CheckType(typeof(long));
      Assert.That((long)_token.Value == 123, "Failed to read long value");

      TryMatch("123uL ");
      CheckType(typeof(ulong));
      Assert.That((ulong)_token.Value == 123, "Failed to read ulong value");

      //Hex representation
      TryMatch("0x012 ");
      CheckType(typeof(int));
      Assert.That((int)_token.Value == 0x012, "Failed to read hex int value");

      TryMatch("0x12U ");
      CheckType(typeof(uint));
      Assert.That((uint)_token.Value == 0x012, "Failed to read hex uint value");

      TryMatch("0x012L ");
      CheckType(typeof(long));
      Assert.That((long)_token.Value == 0x012, "Failed to read hex long value");

      TryMatch("0x012uL ");
      CheckType(typeof(ulong));
      Assert.That((ulong)_token.Value == 0x012, "Failed to read hex ulong value");

      //Floating point types
      TryMatch("123.4 ");
      CheckType(typeof(double));
      Assert.That(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value #1");

      TryMatch("1234e-1 ");
      CheckType(typeof(double));
      Assert.That(Math.Abs((double)_token.Value - 1234e-1) < eps, "Failed to read double value #2");

      TryMatch("12.34e+01 ");
      CheckType(typeof(double));
      Assert.That(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value  #3");

      TryMatch("0.1234E3 ");
      CheckType(typeof(double));
      Assert.That(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value  #4");

      TryMatch("123.4f ");
      CheckType(typeof(float));
      Assert.That(Math.Abs((Single)_token.Value - 123.4) < eps, "Failed to read float(single) value");

      TryMatch("123.4m ");
      CheckType(typeof(decimal));
      Assert.That(Math.Abs((decimal)_token.Value - 123.4m) < Convert.ToDecimal(eps), "Failed to read decimal value");

      //Quick parse
      TryMatch("1 ");
      CheckType(typeof(int));
      //When going through quick parse path (for one-digit numbers), the NumberScanInfo record is not created and hence is absent in Attributes
      Assert.That(_token.Details == null, "Quick parse test failed: ScanDetails object is found in token - quick parse path should not produce this object.");
      Assert.That((int)_token.Value == 1, "Failed to read quick-parse value");
    }

    [Test]
    public void TestVBNumber() {
      double eps = 0.0001;
      _terminal = TerminalFactory.CreateVbNumber("Number");
      _terminal.Init(_grammar);

      //Simple integer
      TryMatch("123 ");
      CheckType(typeof(int));
      Assert.That(_token.Details != null, "ScanDetails object not found in token.");
      Assert.That((int)_token.Value == 123, "Failed to read int value");

      //Test all suffixes
      TryMatch("123S ");
      CheckType(typeof(short));
      Assert.That((short)_token.Value == 123, "Failed to read short value");

      TryMatch("123I ");
      CheckType(typeof(int));
      Assert.That((int)_token.Value == 123, "Failed to read int value");

      TryMatch("123% ");
      CheckType(typeof(int));
      Assert.That((int)_token.Value == 123, "Failed to read int value");

      TryMatch("123L ");
      CheckType(typeof(long));
      Assert.That((long)_token.Value == 123, "Failed to read long value");

      TryMatch("123& ");
      CheckType(typeof(long));
      Assert.That((long)_token.Value == 123, "Failed to read long value");

      TryMatch("123us ");
      CheckType(typeof(ushort));
      Assert.That((ushort)_token.Value == 123, "Failed to read ushort value");

      TryMatch("123ui ");
      CheckType(typeof(uint));
      Assert.That((uint)_token.Value == 123, "Failed to read uint value");

      TryMatch("123ul ");
      CheckType(typeof(ulong));
      Assert.That((ulong)_token.Value == 123, "Failed to read ulong value");

      //Hex and octal 
      TryMatch("&H012 ");
      CheckType(typeof(int));
      Assert.That((int)_token.Value == 0x012, "Failed to read hex int value");

      TryMatch("&H012L ");
      CheckType(typeof(long));
      Assert.That((long)_token.Value == 0x012, "Failed to read hex long value");

      TryMatch("&O012 ");
      CheckType(typeof(int));
      Assert.That((int)_token.Value == 10, "Failed to read hex int value"); //12(oct) = 10(dec)

      TryMatch("&o012L ");
      CheckType(typeof(long));
      Assert.That((long)_token.Value == 10, "Failed to read hex long value");

      //Floating point types
      TryMatch("123.4 ");
      CheckType(typeof(double));
      Assert.That(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value #1");

      TryMatch("1234e-1 ");
      CheckType(typeof(double));
      Assert.That(Math.Abs((double)_token.Value - 1234e-1) < eps, "Failed to read double value #2");

      TryMatch("12.34e+01 ");
      CheckType(typeof(double));
      Assert.That(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value  #3");

      TryMatch("0.1234E3 ");
      CheckType(typeof(double));
      Assert.That(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value  #4");

      TryMatch("123.4R ");
      CheckType(typeof(double));
      Assert.That(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value #5");

      TryMatch("123.4# ");
      CheckType(typeof(double));
      Assert.That(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value #6");

      TryMatch("123.4f ");
      CheckType(typeof(float));
      Assert.That(Math.Abs((Single)_token.Value - 123.4) < eps, "Failed to read float(single) value");

      TryMatch("123.4! ");
      CheckType(typeof(float));
      Assert.That(Math.Abs((Single)_token.Value - 123.4) < eps, "Failed to read float(single) value");

      TryMatch("123.4D ");
      CheckType(typeof(decimal));
      Assert.That(Math.Abs((decimal)_token.Value - 123.4m) < Convert.ToDecimal(eps), "Failed to read decimal value");

      TryMatch("123.4@ ");
      CheckType(typeof(decimal));
      Assert.That(Math.Abs((decimal)_token.Value - 123.4m) < Convert.ToDecimal(eps), "Failed to read decimal value");

      //Quick parse
      TryMatch("1 ");
      CheckType(typeof(int));
      //When going through quick parse path (for one-digit numbers), the NumberScanInfo record is not created and hence is absent in Attributes
      Assert.That(_token.Details == null, "Quick parse test failed: ScanDetails object is found in token - quick parse path should not produce this object.");
      Assert.That((int)_token.Value == 1, "Failed to read quick-parse value");
    }


    [Test]
    public void TestPythonNumber() {
      double eps = 0.0001;
      _terminal = TerminalFactory.CreatePythonNumber("Number");
      _terminal.Init(_grammar);

      //Simple integers and suffixes
      TryMatch("123 ");
      CheckType(typeof(int));
      Assert.That(_token.Details != null, "ScanDetails object not found in token.");
      Assert.That((int)_token.Value == 123, "Failed to read int value");

      TryMatch("123L ");
      CheckType(typeof(long));
      Assert.That((long)_token.Value == 123, "Failed to read long value");

      //Hex representation
      TryMatch("0x012 ");
      CheckType(typeof(int));
      Assert.That((int)_token.Value == 0x012, "Failed to read hex int value");

      TryMatch("0x012l "); //with small "L"
      CheckType(typeof(long));
      Assert.That((long)_token.Value == 0x012, "Failed to read hex long value");

      //Floating point types
      TryMatch("123.4 ");
      CheckType(typeof(double));
      Assert.That(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value #1");

      TryMatch("1234e-1 ");
      CheckType(typeof(double));
      Assert.That(Math.Abs((double)_token.Value - 1234e-1) < eps, "Failed to read double value #2");

      TryMatch("12.34e+01 ");
      CheckType(typeof(double));
      Assert.That(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value  #3");

      TryMatch("0.1234E3 ");
      CheckType(typeof(double));
      Assert.That(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value  #4");

      //Big integer
      string sbig = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"; //100 digits
      TryMatch(sbig);
      Assert.That(_token.Value.ToString() == sbig, "Failed to read big integer value");

      //Quick parse
      TryMatch("1,");
      CheckType(typeof(int));
      Assert.That(_token.Details == null, "Quick parse test failed: ScanDetails object is found in token - quick parse path should not produce this object.");
      Assert.That((int)_token.Value == 1, "Failed to read quick-parse value");

    }

  
  }//class
}//namespace

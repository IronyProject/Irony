using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using NUnit.Framework.Constraints;
using Irony.Compiler;

namespace Irony.Tests {
  [TestFixture]
  public class StringLiteralTests: TerminalTestsBase  {

    //Helper method - replaces single quote with double quote and then calls TryMatch, just to make it easier to write
    // strings with quotes for tests
    private void TryMatchDoubles(string input) {
      input = input.Replace("'", "\"");
      TryMatch(input);
    }

    [Test]
    public void TestPythonString() {
      _terminal = TerminalFactory.CreatePythonString("String");
      _terminal.Init(_grammar);
      //1. Single quotes
      TryMatch(@"'00\a\b\t\n\v\f\r\'\\00'  ");
      Assert.That((string)_token.Value == "00\a\b\t\n\v\f\r\'\\00", "Failed to process escaped characters.");
      TryMatch("'abcd\nefg'  ");
      Assert.That(_token.Category == TokenCategory.Error, "Failed to detect erroneous multi-line string.");
      TryMatch("'''abcd\nefg'''  ");
      Assert.That((string)_token.Value == "abcd\nefg", "Failed to process line break in triple-quote string.");
      TryMatch(@"'''abcd\" + "\n" + "efg'''  ");
      Assert.That((string)_token.Value == "abcd\nefg", "Failed to process escaped line-break char.");
      TryMatch(@"r'00\a\b\t\n\v\f\r00'  ");
      Assert.That((string)_token.Value == @"00\a\b\t\n\v\f\r00", "Failed to process string with disabled escapes.");
      
      //2. Double quotes - we use TryMatchDoubles which replaces single quotes with doubles and then calls TryMatch
      TryMatchDoubles(@"'00\a\b\t\n\v\f\r\'\\00'  ");
      Assert.That((string)_token.Value == "00\a\b\t\n\v\f\r\"\\00", "Failed to process escaped characters.");
      TryMatchDoubles("'abcd\nefg'  ");
      Assert.That(_token.Category == TokenCategory.Error, "Failed to detect erroneous multi-line string. (Double quotes)");
      TryMatchDoubles("'''abcd\nefg'''  ");
      Assert.That((string)_token.Value == "abcd\nefg", "Failed to process line break in triple-quote string. (Double quotes)");
      TryMatchDoubles(@"'''abcd\" + "\n" + "efg'''  ");
      Assert.That((string)_token.Value == "abcd\nefg", "Failed to process escaped line-break char. (Double quotes)");
      TryMatchDoubles(@"r'00\a\b\t\n\v\f\r00'  ");
      Assert.That((string)_token.Value == @"00\a\b\t\n\v\f\r00", "Failed to process string with disabled escapes. (Double quotes)");
    }//method

    [Test]
    public void TestCSharpString() {
      _terminal = TerminalFactory.CreateCSharpString("String");
      _terminal.Init(_grammar);
      //with Escapes
      TryMatchDoubles(@"'00\a\b\t\n\v\f\r\'\\00'  ");
      Assert.That((string)_token.Value == "00\a\b\t\n\v\f\r\"\\00", "Failed to process escaped characters.");
      TryMatchDoubles("'abcd\nefg'  ");
      Assert.That(_token.Category == TokenCategory.Error, "Failed to detect erroneous multi-line string.");
      //with disabled escapes
      TryMatchDoubles(@"@'00\a\b\t\n\v\f\r00'  ");
      Assert.That((string)_token.Value == @"00\a\b\t\n\v\f\r00", "Failed to process @-string with disabled escapes.");
      TryMatchDoubles("@'abc\ndef'  ");
      Assert.That((string)_token.Value == "abc\ndef", "Failed to process @-string with linebreak.");
      //Unicode and hex
      TryMatchDoubles(@"'abc\u0040def'  ");
      Assert.That((string)_token.Value == "abc@def", "Failed to process unicode escape \\u.");
      TryMatchDoubles(@"'abc\U00000040def'  ");
      Assert.That((string)_token.Value == "abc@def", "Failed to process unicode escape \\u.");
      TryMatchDoubles(@"'abc\x0040xyz'  ");
      Assert.That((string)_token.Value == "abc@xyz", "Failed to process hex escape (4 digits).");
      TryMatchDoubles(@"'abc\x040xyz'  ");
      Assert.That((string)_token.Value == "abc@xyz", "Failed to process hex escape (3 digits).");
      TryMatchDoubles(@"'abc\x40xyz'  ");
      Assert.That((string)_token.Value == "abc@xyz", "Failed to process hex escape (2 digits).");
      //octals
      TryMatchDoubles(@"'abc\0601xyz'  "); //the last digit "1" should not be included in octal number
      Assert.That((string)_token.Value == "abc01xyz", "Failed to process octal escape (3 + 1 digits).");
      TryMatchDoubles(@"'abc\060xyz'  ");
      Assert.That((string)_token.Value == "abc0xyz", "Failed to process octal escape (3 digits).");
      TryMatchDoubles(@"'abc\60xyz'  ");
      Assert.That((string)_token.Value == "abc0xyz", "Failed to process octal escape (2 digits).");
      TryMatchDoubles(@"'abc\0xyz'  ");
      Assert.That((string)_token.Value == "abc\0xyz", "Failed to process octal escape (1 digit).");
    }

    [Test]
    public void TestCSharpChar() {
      _terminal = TerminalFactory.CreateCSharpChar("Char");
      _terminal.Init(_grammar);
      TryMatch("'a'  ");
      Assert.That((char)_token.Value == 'a', "Failed to process char.");
      TryMatch(@"'\n'  ");
      Assert.That((char)_token.Value == '\n', "Failed to process new-line char.");
      TryMatch(@"''  ");
      Assert.That(_token.Category == TokenCategory.Error, "Failed to recognize empty quotes as invalid char literal.");
      TryMatch(@"'abc'  ");
      Assert.That(_token.Category == TokenCategory.Error, "Failed to recognize multi-char sequence as invalid char literal.");
      //Note: unlike strings, c# char literals don't allow the "@" prefix
    }

    [Test]
    public void TestVbString() {
      _terminal = TerminalFactory.CreateVbString("String");
      _terminal.Init(_grammar);
      //VB has no escapes - so make sure term doesn't catch any escapes
      TryMatchDoubles(@"'00\a\b\t\n\v\f\r\\00'  ");
      Assert.That((string)_token.Value == @"00\a\b\t\n\v\f\r\\00", "Failed to process string with \\ characters.");
      TryMatchDoubles("'abcd\nefg'  ");
      Assert.That(_token.Category == TokenCategory.Error, "Failed to detect erroneous multi-line string.");
      TryMatchDoubles("'abcd''efg'  "); // doubled quote should change to single
      Assert.That((string)_token.Value == "abcd\"efg", "Failed to process a string with doubled double-quote char.");
      //Test char suffix "c"
      TryMatchDoubles("'A'c  "); 
      Assert.That((char)_token.Value == 'A', "Failed to process a character");
      TryMatchDoubles("''c  ");
      Assert.That(_token.Category == TokenCategory.Error, "Failed to detect an error for an empty char.");
      TryMatchDoubles("'ab'C  ");
      Assert.That(_token.Category == TokenCategory.Error, "Failed to detect error in multi-char sequence.");
    }

  }//class
}//namespace

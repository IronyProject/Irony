using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Irony.Compiler {
  public static class TerminalFactory {

    public static StringLiteral CreateCSharpString(string name) {
      StringLiteral term = new StringLiteral(name, TermOptions.None);
      term.AddStartEnd("\"", ScanFlags.AllowAllEscapes);
      term.AddPrefixFlag("@", ScanFlags.DisableEscapes | ScanFlags.AllowLineBreak | ScanFlags.AllowDoubledQuote);
      return term;
    }
    public static StringLiteral CreateCSharpChar(string name) {
      StringLiteral term = new StringLiteral(name, TermOptions.None);
      term.AddStartEnd("'", ScanFlags.IsChar);
      return term;
    }

    public static StringLiteral CreateVbString(string name) {
      StringLiteral term = new StringLiteral(name, TermOptions.SpecialIgnoreCase);
      term.AddStartEnd("\"", ScanFlags.DisableEscapes | ScanFlags.AllowDoubledQuote);
      term.AddSuffixCode("$", TypeCode.String);
      term.AddSuffixCode("c", TypeCode.Char);
      return term;
    }

    public static StringLiteral CreatePythonString(string name) {
      StringLiteral term = new StringLiteral(name, TermOptions.SpecialIgnoreCase);
      term.AddStartEnd("'", ScanFlags.AllowAllEscapes);
      term.AddStartEnd("'''", ScanFlags.AllowAllEscapes | ScanFlags.AllowLineBreak);
      term.AddStartEnd("\"", ScanFlags.AllowAllEscapes);
      term.AddStartEnd("\"\"\"", ScanFlags.AllowAllEscapes | ScanFlags.AllowLineBreak);

      term.AddPrefixFlag("u", ScanFlags.AllowAllEscapes);
      term.AddPrefixFlag("r", ScanFlags.DisableEscapes );
      term.AddPrefixFlag("ur", ScanFlags.DisableEscapes);
 
      return term;
    }

    public static NumberLiteral CreateCSharpNumber(string name) {
      //NumberTerminal term = new NumberTerminal(name, BnfFlags.SpecialIgnoreCase);
      NumberLiteral term = new NumberLiteral(name, TermOptions.SpecialIgnoreCase);
      term.AddPrefixFlag("0x", ScanFlags.Hex);
      term.AddSuffixCode("u", TypeCode.UInt32);
      term.AddSuffixCode("l", TypeCode.Int64);
      term.AddSuffixCode("ul", TypeCode.UInt64);
      term.AddSuffixCode("f", TypeCode.Single);
      term.AddSuffixCode("m", TypeCode.Decimal);
      return term;
    }
    public static NumberLiteral CreateVbNumber(string name) {
      NumberLiteral term = new NumberLiteral(name, TermOptions.SpecialIgnoreCase);
      term.AddPrefixFlag("&H", ScanFlags.Hex);
      term.AddPrefixFlag("&O", ScanFlags.Octal);
      term.AddSuffixCode("S", TypeCode.Int16);
      term.AddSuffixCode("I", TypeCode.Int32);
      term.AddSuffixCode("%", TypeCode.Int32);
      term.AddSuffixCode("L", TypeCode.Int64);
      term.AddSuffixCode("&", TypeCode.Int64);
      term.AddSuffixCode("D", TypeCode.Decimal);
      term.AddSuffixCode("@", TypeCode.Decimal);
      term.AddSuffixCode("F", TypeCode.Single);
      term.AddSuffixCode("!", TypeCode.Single);
      term.AddSuffixCode("R", TypeCode.Double);
      term.AddSuffixCode("#", TypeCode.Double);
      term.AddSuffixCode("US", TypeCode.UInt16);
      term.AddSuffixCode("UI", TypeCode.UInt32);
      term.AddSuffixCode("UL", TypeCode.UInt64);
      return term;
    }
    public static NumberLiteral CreatePythonNumber(string name) {
      NumberLiteral term = new NumberLiteral(name, TermOptions.NumberAllowBigInts | TermOptions.SpecialIgnoreCase);
      term.AddPrefixFlag("0x", ScanFlags.Hex);
      term.AddPrefixFlag("0", ScanFlags.Octal);
      term.AddSuffixCode("L", TypeCode.Int64);
      return term;
    }

    public static IdentifierTerminal CreateCSharpIdentifier(string name) {
      IdentifierTerminal id = new IdentifierTerminal(name);
      string strKeywords =
            "abstract as base bool break byte case catch char checked " +
            "class	const	continue decimal default delegate  do double else enum event explicit extern false finally " +
            "fixed float for foreach goto if implicit in int interface internal is lock long namespace " +
            "new null object operator out override params private protected public " +
            "readonly ref return sbyte sealed short sizeof stackalloc static string " +
            "struct switch this throw true try typeof uint ulong unchecked unsafe ushort using virtual void " +
            "volatile while";
      string[] arrKeywords = strKeywords.Split(' ');
      foreach (string keyw in arrKeywords) {
        string kw = keyw.Trim();
        if (!string.IsNullOrEmpty(kw))
          id.ReservedWords.Add(keyw.Trim());
      }
      id.AddPrefixFlag("@", ScanFlags.IsNotKeyword | ScanFlags.DisableEscapes );
      //From spec:
      //Start char is "_" or letter-character, which is a Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nl 
      id.StartCharCategories.AddRange(new UnicodeCategory[] {
         UnicodeCategory.UppercaseLetter, //Ul
         UnicodeCategory.LowercaseLetter, //Ll
         UnicodeCategory.TitlecaseLetter, //Lt
         UnicodeCategory.ModifierLetter,  //Lm
         UnicodeCategory.OtherLetter,     //Lo
         UnicodeCategory.LetterNumber     //Nl
      });
      //Internal chars
      /* From spec:
      identifier-part-character: letter-character | decimal-digit-character | connecting-character |  combining-character |
          formatting-character
*/
      id.CharCategories.AddRange(id.StartCharCategories); //letter-character categories
      id.CharCategories.AddRange(new UnicodeCategory[] {
        UnicodeCategory.DecimalDigitNumber, //Nd
        UnicodeCategory.ConnectorPunctuation, //Pc
        UnicodeCategory.SpacingCombiningMark, //Mc
        UnicodeCategory.NonSpacingMark,       //Mn
        UnicodeCategory.Format                //Cf
      });
      //Chars to remove from final identifier
      id.CharsToRemoveCategories.Add(UnicodeCategory.Format);
      return id;
    }


  }//class
}//namespace

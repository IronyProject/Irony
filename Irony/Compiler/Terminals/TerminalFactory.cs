#region License
/* **********************************************************************************
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion
//Authors: Roman Ivantsov, Philipp Serr

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
      term.AddSuffixCodes("$", TypeCode.String);
      term.AddSuffixCodes("c", TypeCode.Char);
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

		//http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-334.pdf section 9.4.4
    public static NumberLiteral CreateCSharpNumber(string name) {
      NumberLiteral term = new NumberLiteral(name, TermOptions.EnableQuickParse | TermOptions.SpecialIgnoreCase);
      term.DefaultIntTypes = new TypeCode[] { TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64 };
      term.DefaultFloatType = TypeCode.Double;
      term.AddPrefixFlag("0x", ScanFlags.Hex);
      term.AddSuffixCodes("u", TypeCode.UInt32, TypeCode.UInt64);
      term.AddSuffixCodes("l", TypeCode.Int64, TypeCode.UInt64);
      term.AddSuffixCodes("ul", TypeCode.UInt64);
      term.AddSuffixCodes("f", TypeCode.Single);
			term.AddSuffixCodes("d", TypeCode.Double);
      term.AddSuffixCodes("m", TypeCode.Decimal);
      return term;
    }
    //http://www.microsoft.com/downloads/details.aspx?FamilyId=6D50D709-EAA4-44D7-8AF3-E14280403E6E&displaylang=en section 2
    public static NumberLiteral CreateVbNumber(string name) {
      NumberLiteral term = new NumberLiteral(name, TermOptions.EnableQuickParse | TermOptions.SpecialIgnoreCase);
      term.DefaultIntTypes = new TypeCode[] { TypeCode.Int32, TypeCode.Int64 };
      //term.DefaultFloatType = TypeCode.Double; it is default
      term.AddPrefixFlag("&H", ScanFlags.Hex);
      term.AddPrefixFlag("&O", ScanFlags.Octal);
      term.AddSuffixCodes("S", TypeCode.Int16);
      term.AddSuffixCodes("I", TypeCode.Int32);
      term.AddSuffixCodes("%", TypeCode.Int32);
      term.AddSuffixCodes("L", TypeCode.Int64);
      term.AddSuffixCodes("&", TypeCode.Int64);
      term.AddSuffixCodes("D", TypeCode.Decimal);
      term.AddSuffixCodes("@", TypeCode.Decimal);
      term.AddSuffixCodes("F", TypeCode.Single);
      term.AddSuffixCodes("!", TypeCode.Single);
      term.AddSuffixCodes("R", TypeCode.Double);
      term.AddSuffixCodes("#", TypeCode.Double);
      term.AddSuffixCodes("US", TypeCode.UInt16);
      term.AddSuffixCodes("UI", TypeCode.UInt32);
      term.AddSuffixCodes("UL", TypeCode.UInt64);
      return term;
    }
    //http://docs.python.org/ref/numbers.html
    public static NumberLiteral CreatePythonNumber(string name) {
      NumberLiteral term = new NumberLiteral(name, TermOptions.EnableQuickParse | TermOptions.SpecialIgnoreCase | TermOptions.NumberAllowStartEndDot);
      //default int types are Integer (32bit) -> LongInteger (BigInt); Try Int64 before BigInt: Better performance?
      term.DefaultIntTypes = new TypeCode[] { TypeCode.Int32, TypeCode.Int64, NumberLiteral.TypeCodeBigInt };
      // term.DefaultFloatType = TypeCode.Double; -- it is default
      //float type is implementation specific, thus try decimal first (higher precision)
      //term.DefaultFloatTypes = new TypeCode[] { TypeCode.Decimal, TypeCode.Double };
      term.AddPrefixFlag("0x", ScanFlags.Hex);
      term.AddPrefixFlag("0", ScanFlags.Octal);
      term.AddSuffixCodes("L", TypeCode.Int64, NumberLiteral.TypeCodeBigInt);
      term.AddSuffixCodes("J", NumberLiteral.TypeCodeImaginary);
      return term;
    }

    //Note - this is incomplete implementation; need to add functionality to NumberTerminal class to support type detection based 
    // on exponent symbol. 
    // From R6RS:
    //  ... representations of number objects may be written with an exponent marker that indicates the desired precision 
    // of the inexact representation. The letters s, f, d, and l specify the use of short, single, double, and long precision, respectively. 
    public static NumberLiteral CreateSchemeNumber(string name) {
      NumberLiteral term = new NumberLiteral(name, TermOptions.EnableQuickParse | TermOptions.SpecialIgnoreCase);
      term.DefaultIntTypes = new TypeCode[] { TypeCode.Int32, TypeCode.Int64, NumberLiteral.TypeCodeBigInt };
      term.DefaultFloatType = TypeCode.Double; // it is default
      term.ExponentSymbols = "sfdl";
      term.AddPrefixFlag("#b", ScanFlags.Binary);
      term.AddPrefixFlag("#o", ScanFlags.Octal);
      term.AddPrefixFlag("#x", ScanFlags.Hex);
      term.AddPrefixFlag("#d", ScanFlags.None);
      term.AddPrefixFlag("#i", ScanFlags.None); // inexact prefix, has no effect
      term.AddPrefixFlag("#e", ScanFlags.None); // exact prefix, has no effect
      term.AddSuffixCodes("J", NumberLiteral.TypeCodeImaginary);
      return term;
    }


    public static IdentifierTerminal CreateCSharpIdentifier(string name) {
      IdentifierTerminal id = new IdentifierTerminal(name);
      id.SetOption(TermOptions.CanStartWithEscape);
      string strKeywords =
            "abstract as base bool break byte case catch char checked " +
            "class	const	continue decimal default delegate  do double else enum event explicit extern false finally " +
            "fixed float for foreach goto if implicit in int interface internal is lock long namespace " +
            "new null object operator out override params private protected public " +
            "readonly ref return sbyte sealed short sizeof stackalloc static string " +
            "struct switch this throw true try typeof uint ulong unchecked unsafe ushort using virtual void " +
            "volatile while";
      id.AddKeywordList(strKeywords);

      id.AddPrefixFlag("@", ScanFlags.IsNotKeyword | ScanFlags.DisableEscapes);
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

    public static IdentifierTerminal CreatePythonIdentifier(string name) {
      IdentifierTerminal id = new IdentifierTerminal("Identifier"); //defaults are OK
      id.AddKeywords("and", "del", "from", "not", "while", "as", "elif", "global", "or", "with",
                                  "assert", "else", "if", "pass", "yield", "break", "except", "import", "print",
                                  "class", "exec", "in", "raise", "continue", "finally", "is", "return",
                                  "def", "for", "lambda", "try");
      return id;
    }

  }//class
}//namespace

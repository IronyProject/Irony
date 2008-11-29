#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace Irony.Compiler {
  #region About compound terminals
  /*
   As  it turns out, many terminal types in real-world languages have 3-part structure: prefix-body-suffix
   The body is essentially the terminal "value", while prefix and suffix are used to specify additional 
   information (options), while not  being a part of the terminal itself. 
   For example:
   1. c# numbers, may have 0x prefix for hex representation, and suffixes specifying 
     the exact data type of the literal (f, l, m, etc)
   2. c# string may have "@" prefix which disables escaping inside the string
   3. c# identifiers may have "@" prefix and escape sequences inside - just like strings
   4. Python string may have "u" and "r" prefixes, "r" working the same way as @ in c# strings
   5. VB string literals may have "c" suffix identifying that the literal is a character, not a string
   6. VB number literals and identifiers may have suffixes identifying data type
   
   So it seems like all these terminals have the format "prefix-body-suffix". 
   The CompoundTerminalBase base class implements base functionality supporting this multi-part structure.
   The IdentifierTerminal, NumberLiteral and StringLiteral classes inherit from this base class. 
   The methods in TerminalFactory static class demonstrate that with this architecture we can define the whole 
   variety of terminals for c#, Python and VB.NET languages. 
*/
  #endregion


  public class ScanDetails {
    public bool Negative;
    public string Prefix;
    public string Body;
    public string Suffix;
    public ScanFlags Flags;
    public string Error;
    public TypeCode[] TypeCodes;  
    public string ControlSymbol_;  
    public string ExponentSymbol;  //exponent symbol for Number literal
    public string StartSymbol;     //string start and end symbols
    public string EndSymbol;
    public object Value; 
    public bool IsSet(ScanFlags flag) {
      return (Flags & flag) != 0;
    }//method
    public bool HasError() {
      return !string.IsNullOrEmpty(Error);
    }
  }

  public abstract class CompoundTerminalBase : Terminal {

    #region Nested classes
    public class ScanFlagTable : Dictionary<string, ScanFlags> { }
    public class TypeCodeTable : Dictionary<string, TypeCode[]> { }
    #endregion 

    #region constructors and initialization
    public CompoundTerminalBase(string name, TermOptions options) : base(name) {
      SetOption(options);
      Escapes = TextUtils.GetDefaultEscapes();
    }
    public CompoundTerminalBase(string name)  : this(name, TermOptions.None) {  }

    public void AddPrefixFlag(string prefix, ScanFlags flags) {
      PrefixFlags.Add(prefix, flags);
      Prefixes.Add(prefix);
    }
		public void AddSuffixCodes(string suffix, params TypeCode[] codes) {
			SuffixTypeCodes.Add(suffix, codes);
			Suffixes.Add(suffix);
		}
    #endregion

    #region public Properties/Fields
    public Char EscapeChar = '\\';
    public EscapeTable Escapes = new EscapeTable();
    public ScanFlags DefaultFlags = ScanFlags.None;
    public TypeCode DefaultType;
    #endregion


    #region private fields
    protected readonly ScanFlagTable PrefixFlags = new ScanFlagTable();
    protected readonly TypeCodeTable SuffixTypeCodes = new TypeCodeTable();
    protected StringList Prefixes = new StringList();
    protected StringList Suffixes = new StringList();
    string _prefixesFirsts; //first chars of all prefixes, for fast prefix detection
    string _suffixesFirsts; //first chars of all suffixes, for fast suffix detection
    private TypeCode[] _defaultTypes;
    #endregion

    #region events: ConvertingValue
    public event EventHandler<ScannerConvertingValueEventArgs> ConvertingValue;
    #endregion


    #region overrides: Init, TryMatch
    public override void Init(Grammar grammar) {
      base.Init(grammar);
      _defaultTypes = new TypeCode[] { DefaultType };
      //collect all suffixes, prefixes in lists and create strings of first chars for both
      Prefixes.Sort(StringList.LongerFirst);
      _prefixesFirsts = string.Empty;
      foreach (string pfx in Prefixes)
        _prefixesFirsts += pfx[0];

      Suffixes.Sort(StringList.LongerFirst);
      _suffixesFirsts = string.Empty;
      foreach (string sfx in Suffixes)
        _suffixesFirsts += sfx[0]; //we don't care if there are repetitions

      if (IsSet(TermOptions.SpecialIgnoreCase)) {
        _prefixesFirsts = _prefixesFirsts.ToLower() + _prefixesFirsts.ToUpper();
        _suffixesFirsts = _suffixesFirsts.ToLower() + _suffixesFirsts.ToUpper();
      }
    }//method

    public override IList<string> GetFirsts() {
      return Prefixes;
    }

    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      Token token = null;
      if (IsSet(TermOptions.EnableQuickParse)) {
        token = QuickParse(context, source);
        if (token != null) return token;
      }

      source.Position = source.TokenStart.Position;
      ScanDetails details = new ScanDetails();
      details.Flags = DefaultFlags;
			details.TypeCodes = _defaultTypes;

      ReadPrefix(source, details);
      if (!ReadBody(source, details))
        return null;
      if (details.HasError()) 
        return context.CreateErrorTokenAndReportError(source.TokenStart, source.CurrentChar.ToString(), details.Error);
      ReadSuffix(source, details);

      if (!ConvertValue(details)) 
        return context.CreateErrorTokenAndReportError(source.TokenStart, source.CurrentChar.ToString(), "Failed to convert the value: " + details.Error);

      token = CreateToken(context, source, details);
      return token; 
    }

    protected virtual Token CreateToken(CompilerContext context, ISourceStream source, ScanDetails details) {
      string lexeme = source.GetLexeme();
      Token token = Token.Create(context, this, source.TokenStart, lexeme, details.Value);
      token.Details = details;
      return token;
    }

    protected virtual Token QuickParse(CompilerContext context, ISourceStream source) {
      return null;
    }

    protected virtual void ReadPrefix(ISourceStream source, ScanDetails details) {
      if (_prefixesFirsts.IndexOf(source.CurrentChar) < 0)
        return;
      bool ignoreCase = IsSet(TermOptions.SpecialIgnoreCase);
      foreach (string pfx in Prefixes) {
        if (!source.MatchSymbol(pfx, ignoreCase)) continue; 
        //We found prefix
        details.Prefix = pfx;
        source.Position += pfx.Length;
        //Set numeric base flag from prefix
        ScanFlags pfxFlags;
        if (!string.IsNullOrEmpty(details.Prefix) && PrefixFlags.TryGetValue(details.Prefix, out pfxFlags))
          details.Flags |= pfxFlags;
        return;
      }//foreach
    }//method

    protected virtual void ReadSuffix(ISourceStream source, ScanDetails details) {
      if (_suffixesFirsts.IndexOf(source.CurrentChar) < 0) return;
      bool ignoreCase = IsSet(TermOptions.SpecialIgnoreCase);
      foreach (string sfx in Suffixes) {
        if (!source.MatchSymbol(sfx, ignoreCase)) continue;
        //We found suffix
        details.Suffix = sfx;
        source.Position += sfx.Length;
        //Set TypeCode from suffix
        TypeCode[] codes;
        if (!string.IsNullOrEmpty(details.Suffix) && SuffixTypeCodes.TryGetValue(details.Suffix, out codes))
          details.TypeCodes = codes;
        return;
      }//foreach
    }//method

    protected virtual bool ReadBody(ISourceStream source, ScanDetails details) {
      return false;
    }

    protected virtual bool ConvertValue(ScanDetails details) {
      details.Value = details.Body;
      //Fire event and give a chance to custom code to convert the value
      if (ConvertingValue != null) {
        bool result = OnConvertingValue(details);
        return result; 
      }
      return false; 
    }
    protected virtual bool OnConvertingValue(ScanDetails details) {
      if (ConvertingValue == null) return false;
      ScannerConvertingValueEventArgs args = new ScannerConvertingValueEventArgs(details);
      ConvertingValue(this, args);
      return args.Converted;
    }


    #endregion



  }//class

}//namespace

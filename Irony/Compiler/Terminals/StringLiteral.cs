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

  public enum StringOptions {
    None = 0,
    AllowLineBreak    = 0x01,  // string allows line breaks
    LineBreakEscaped  = 0x02,  // string continuation is marked by "\" symbol at the end of the line 
    DoubleStartEndAsSingle = 0x04,  // doubled end symbol inside the literal is converted to a single symbol
    DisableEscape     = 0x08,  // escaping symbols is disabled
    IsChar            = 0x10,  // Length should be == 1
    EnableUEscapes    = 0x0100,
    EnableXEscapes     = 0x0400,
    EnableOctalEscapes = 0x0800,
  }

  public class StringLiteral : Terminal {
    #region embedded classes
    public class EscapeTable : Dictionary<char, char> { }
    public class OptionTable : Dictionary<string, StringOptions> { }

    public class StringScanInfo {
      public string Prefix;
      public string StartSymbol;
      public string Body;
      public string Suffix;
      public StringOptions Options;
      public object Value; //can be string or char
      public string Error;
      public bool IsSet(StringOptions option) {
        return (Options & option) != 0;
      }
    }
    #endregion

    #region constructors and initialization
    public StringLiteral(string name, string startEndSymbol, StringOptions options)
         : this(name, startEndSymbol, options, BnfFlags.NumberIgnoreCase) { }

    public StringLiteral(string name, string startEndSymbol, StringOptions options, BnfFlags flags) : this(name, flags) {
      this.StartEndSymbolTable.Add(startEndSymbol, options);
    }
    public StringLiteral(string name, BnfFlags flags) : base(name) {
      SetFlag(flags);
      Escapes = GetDefaultEscapes();
    }
    public void AddPrefix(string prefix, StringOptions options) {
      PrefixTable.Add(prefix, options);
    }
    public void AddStartEnd(string startEndSymbol, StringOptions options) {
      StartEndSymbolTable.Add(startEndSymbol, options);
    }
    public void AddSuffix(string suffix, StringOptions options) {
      SuffixTable.Add(suffix, options);
    }
    #endregion

    #region public Properties/Fields
    public Char EscapeChar = '\\';
    public readonly EscapeTable Escapes = new EscapeTable();
    public readonly OptionTable PrefixTable = new OptionTable();
    public readonly OptionTable SuffixTable = new OptionTable();
    public readonly OptionTable StartEndSymbolTable = new OptionTable();
    public const string ScanInfoKey = "ScanInfo";
    #endregion

    #region private fields
    string _startEndFirsts; //first chars  of start-end symbols
    KeyList _startEndSymbolList = new KeyList();
    string _prefixesFirsts; //first chars of all prefixes, for fast prefix detection
    KeyList _prefixList = new KeyList();
    string _suffixesFirsts; //first chars of all suffixes, for fast suffix detection
    KeyList _suffixList = new KeyList(); 
    #endregion

    #region overrides: Init, GetStartSymbols, TryMatch
    public override void Init(Grammar grammar) {
      base.Init(grammar);
      //collect all start-end symbols, suffixes, prefixes in lists and create strings of first chars for both
      _startEndSymbolList.Clear();
      _startEndSymbolList.AddRange(StartEndSymbolTable.Keys);
      _startEndSymbolList.Sort(KeyList.LongerFirst);
      _startEndFirsts = string.Empty;
      foreach (string st in _startEndSymbolList)
        _startEndFirsts += st[0];
 
      _prefixList.Clear();
      _prefixList.AddRange(PrefixTable.Keys);
      _prefixList.Sort(KeyList.LongerFirst);
      _prefixesFirsts = string.Empty;
      foreach (string pfx in _prefixList)
        _prefixesFirsts += pfx[0];

      _suffixList.Clear();
      _suffixList.AddRange(SuffixTable.Keys);
      _suffixList.Sort(KeyList.LongerFirst);
      _suffixesFirsts = string.Empty;
      foreach (string sfx in _suffixList)
        _suffixesFirsts += sfx[0]; //we don't care if there are repetitions

      if (IsFlagSet(BnfFlags.NumberIgnoreCase)) {
        _prefixesFirsts = _prefixesFirsts.ToLower() + _prefixesFirsts.ToUpper();
        _suffixesFirsts = _suffixesFirsts.ToLower() + _suffixesFirsts.ToUpper();
      }
    }//method

    public override IList<string> GetStartSymbols() {
      StringList result = new StringList();
      result.AddRange(_prefixList);
      //we assume that prefix is always optional, so string can start with start-end symbol
      result.AddRange(_startEndSymbolList);
      return result;
    }

    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      StringScanInfo info = new StringScanInfo();
      ReadPrefix(source, info);
      ReadStartSymbol(source, info);
      if (info.StartSymbol == null)
        return null;
      ReadBody(source, info);
      if (info.Error != null) 
        return Grammar.CreateSyntaxErrorToken(source.TokenStart, info.Error);
      ReadSuffix(source, info);

      ConvertValue(info);
      if (info.Error != null)
        return Grammar.CreateSyntaxErrorToken(source.TokenStart, info.Error);
      //create and return the token
      string lexeme = source.GetLexeme();
      Token token = new Token(this, source.TokenStart, lexeme, info.Value);
      token.Attributes[ScanInfoKey] = info;
      return token; 
    }
    private void ReadPrefix(ISourceStream source, StringScanInfo info) {
      if (_prefixesFirsts.IndexOf(source.CurrentChar) < 0)
        return;
      bool ignoreCase = IsFlagSet(BnfFlags.StringIgnoreCase);
      foreach (string pfx in _prefixList) {
        if (string.Compare(source.Text, source.Position, pfx, 0, pfx.Length, ignoreCase) != 0)
          continue;
        //We found prefix
        info.Options |= PrefixTable[pfx];
        source.Position += pfx.Length;
        return;
      }//foreach
    }//method

    private void ReadStartSymbol(ISourceStream source, StringScanInfo info) {
      if (_startEndFirsts.IndexOf(source.CurrentChar) < 0)
        return;
      bool ignoreCase = IsFlagSet(BnfFlags.StringIgnoreCase);
      foreach (string startEnd in _startEndSymbolList) {
        if (string.Compare(source.Text, source.Position, startEnd, 0, startEnd.Length, ignoreCase) != 0)
          continue;
        //We found start symbol
        info.StartSymbol = startEnd;
        info.Options |= StartEndSymbolTable[startEnd];
        source.Position += startEnd.Length;
        return;
      }//foreach
    }//method

    private void ReadSuffix(ISourceStream source, StringScanInfo info) {
      if (_suffixesFirsts.IndexOf(source.CurrentChar) < 0) return;
      bool ignoreCase = IsFlagSet(BnfFlags.StringIgnoreCase);
      foreach (string sfx in _suffixList) {
        if (string.Compare(source.Text, source.Position, sfx, 0, sfx.Length, ignoreCase) != 0)
          continue;
        //We found suffix
        info.Options |= SuffixTable[sfx];
        source.Position += sfx.Length;
        return;
      }//foreach
    }//method

    private void ReadBody(ISourceStream source, StringScanInfo info) {
      bool escapeEnabled = !info.IsSet(StringOptions.DisableEscape);
      bool ignoreCase = IsFlagSet(BnfFlags.StringIgnoreCase);
      int start = source.Position;
      //1. Find the string end
      // first get the position of the next line break; we are interested in it to detect malformed string, 
      //  therefore do it only if linebreak is NOT allowed; if linebreak is allowed, set it to -1 (we don't care).  
      int nlPos = info.IsSet(StringOptions.AllowLineBreak) ? -1 : source.Text.IndexOf('\n', source.Position);
      while (true) {
        int endPos = source.Text.IndexOf(info.StartSymbol, source.Position);
        //Check for malformed string: either EndSymbol not found, or LineBreak is found before EndSymbol
        bool malFormed = endPos < 0 || nlPos >= 0 && nlPos < endPos;
        if (malFormed) {
          //Set source position for recovery: move to the next line if linebreak is not allowed.
          if (nlPos > 0) endPos = nlPos;
          if (endPos > 0) source.Position = endPos + 1;
          info.Error = "Mal-formed  string literal - cannot find termination symbol.";
          return;
        }
        //We found EndSymbol - check if it is escaped; if yes, skip it and continue search
        if (escapeEnabled && source.Text[endPos - 1] == EscapeChar) {
          source.Position = endPos + info.StartSymbol.Length;
          continue; //searching for end symbol
        }
        //Check if it is doubled end symbol
        if (info.IsSet(StringOptions.DoubleStartEndAsSingle)) {
          if ((endPos + info.StartSymbol.Length  * 2 < source.Text.Length) &&
            string.Compare(source.Text, endPos + info.StartSymbol.Length, info.StartSymbol, 0, info.StartSymbol.Length, ignoreCase) == 0) {
            source.Position = endPos + info.StartSymbol.Length * 2;
            continue;
          }//if
        }//checking for doubled end symbol

        //Ok, this is normal endSymbol that terminates the string. Advance source position and get out from the loop
        info.Body = source.Text.Substring(start, endPos - start);
        source.Position = endPos + info.StartSymbol.Length;
        return; //if we come here it means we're done - we found string end.
      }  //end of loop to find string end; 
      //Get the string body
    }

    //Extract the string content from lexeme, adjusts the escaped and double-end symbols
    //TODO: add support for unicode, hex and octal escapes
    private void ConvertValue(StringScanInfo info) {
      string value = info.Body;
      bool escapeEnabled = !info.IsSet(StringOptions.DisableEscape);
      //Fix all escapes
      if (escapeEnabled && value.IndexOf(EscapeChar) >= 0) {
        string[] arr = value.Split(EscapeChar);
        bool ignoreNext = false;
        //we skip the 0 element as it is not preceeded by "\"
        for (int i = 1; i < arr.Length; i++) {
          if (ignoreNext) {
            ignoreNext = false;
            continue;
          }
          string s = arr[i];
          if (string.IsNullOrEmpty(s)) {
            //it is "\\" - escaped escape symbol. 
            arr[i] = @"\";
            ignoreNext = true;
            continue; 
          }
          //The char is being escaped is the first one; replace it with char in Escapes table
          char first = s[0];
          char newFirst;
          if (Escapes.TryGetValue(first, out newFirst))
            arr[i] = newFirst + s.Substring(1);
          else {
            arr[i] = HandleSpecialEscape(arr[i], info);
          }//else
        }//for i
        value = string.Join(string.Empty, arr);
      }// if EscapeEnabled 

      //Check for doubled end symbol
      if (info.IsSet(StringOptions.DoubleStartEndAsSingle) && value.IndexOf(info.StartSymbol) >= 0)
        value = value.Replace(info.StartSymbol + info.StartSymbol, info.StartSymbol);
      
      //Check char length - must be exactly 1
      if (info.IsSet(StringOptions.IsChar) && value.Length != 1) {
        info.Error = "Invalid length of char literal - should be 1.";
        return;
      }

      //Finally assign the value; it is string or char
      if (info.IsSet(StringOptions.IsChar)) {
        info.Value = value[0]; //set as a char
      } else
        info.Value = value;    // set as string
      
      //TODO: Investigate unescaped linebreak, with  Flags == BnfFlags.StringAllowLineBreak | BnfFlags.StringLineBreakEscaped
      //      also investigate what happens in this case in Windows where default linebreak is "\r\n", not "\n"
    }

    const string _hexes = "1234567890aAbBcCdDeEfF";
    const string _octals = "01234567";
    //Should support:  \Udddddddd, \udddd, \xdddd, \N{name}, \0, \ddd (octal),  
    protected virtual string HandleSpecialEscape(string segment, StringScanInfo info) {
      if (string.IsNullOrEmpty(segment)) return string.Empty;
      int len, p; string digits; char ch; string result; 
      char first = segment[0];
      switch (first) {
        case 'u':
        case 'U':
          if (info.IsSet(StringOptions.EnableUEscapes)) {
            len = (first == 'u' ? 4 : 8);
            if (segment.Length < len + 1) {
              info.Error = "Invalid unicode escape (" + segment.Substring(len + 1) + "), expected " + len + " hex digits.";
              return segment;
            }
            digits = segment.Substring(1, len);
            ch = (char) Convert.ToUInt32(digits, 16);
            result = ch + segment.Substring(len + 1);
            return result; 
          }//if
          break;
        case 'x':
          if (info.IsSet(StringOptions.EnableXEscapes)) {
            //x-escape allows variable number of digits, from one to 4; let's count them
            p = 1; //current position
            while (p < 5 && p < segment.Length) {
              if (_hexes.IndexOf(segment[p]) < 0) break;
              p++;
            }
            //p now point to char right after the last digit
            if (p <= 1) {
              info.Error = "Invalid \\x escape, at least one digit expected.";
              return segment;
            }
            digits = segment.Substring(1, p - 1);
            ch = (char) Convert.ToUInt32(digits, 16);
            result = ch + segment.Substring(p);
            return result;
          }//if
          break;
        case '0':  case '1':  case '2':  case '3':  case '4':  case '5':   case '6': case '7':
          if (info.IsSet(StringOptions.EnableOctalEscapes)) {
            //octal escape allows variable number of digits, from one to 3; let's count them
            p = 0; //current position
            while (p < 3 && p < segment.Length) {
              if (_octals.IndexOf(segment[p]) < 0) break;
              p++;
            }
            //p now point to char right after the last digit
            if (p == 0) {
              info.Error = "Invalid octal escape, at least one digit expected.";
              return segment;
            }
            digits = segment.Substring(0, p);
            ch = (char)Convert.ToUInt32(digits, 8);
            result = ch + segment.Substring(p);
            return result;
          }//if
          break;
      }//switch
      info.Error = "Invalid escape: " + segment;
      return segment; 
    }//method
    #endregion

    #region Utilities: GetDefaultEscapes
    public static EscapeTable GetDefaultEscapes() {
      EscapeTable escapes = new EscapeTable();
      escapes.Add('a', '\u0007');
      escapes.Add('b', '\b');
      escapes.Add('t', '\t');
      escapes.Add('n', '\n');
      escapes.Add('v', '\v');
      escapes.Add('f', '\f');
      escapes.Add('r', '\r');
      escapes.Add('"', '"');
      escapes.Add('\'', '\'');
      escapes.Add('\\', '\\');
      escapes.Add(' ', ' ');
      escapes.Add('\n', '\n'); //this is a special escape of the linebreak itself, 
                               // when string ends with "\" char and continues on the next line
      return escapes;
    }
    #endregion


  }//class

}//namespace

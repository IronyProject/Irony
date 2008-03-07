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


  public class StringLiteral : CompoundTerminalBase {

    #region constructors and initialization
    public StringLiteral(string name, string startEndSymbol, ScanFlags stringFlags)
      : this(name, startEndSymbol, stringFlags, TermOptions.SpecialIgnoreCase) { }

    public StringLiteral(string name, string startEndSymbol, ScanFlags stringFlags, TermOptions options) : this(name, options) {
      this.StartEndSymbolTable.Add(startEndSymbol, stringFlags);
    }
    public StringLiteral(string name, TermOptions options) : base(name) {
      SetOption(options);
      base.Escapes = TextUtils.GetDefaultEscapes();
    }
    public void AddStartEnd(string startEndSymbol, ScanFlags stringFlags) {
      StartEndSymbolTable.Add(startEndSymbol, stringFlags);
    }
    #endregion

    #region public Properties/Fields
    protected readonly ScanFlagTable StartEndSymbolTable = new ScanFlagTable();
    #endregion

    #region private fields
    string _startEndFirsts; //first chars  of start-end symbols
    KeyList _startEndSymbols = new KeyList();
    #endregion

    #region overrides: Init, GetFirsts, ReadBody, etc...
    public override void Init(Grammar grammar) {
      base.Init(grammar);
      //collect all start-end symbols, suffixes, prefixes in lists and create strings of first chars for both
      _startEndSymbols.Clear();
      _startEndSymbols.AddRange(StartEndSymbolTable.Keys);
      _startEndSymbols.Sort(KeyList.LongerFirst);
      _startEndFirsts = string.Empty;
      foreach (string st in _startEndSymbols)
        _startEndFirsts += st[0];
 
      if (IsSet(TermOptions.SpecialIgnoreCase)) 
        _startEndFirsts = _startEndFirsts.ToLower() + _startEndFirsts.ToUpper();
    }//method

    public override IList<string> GetFirsts() {
      StringList result = new StringList();
      result.AddRange(Prefixes);
      //we assume that prefix is always optional, so string can start with start-end symbol
      result.AddRange(_startEndSymbols);
      return result;
    }

    protected override bool ReadBody(ISourceStream source, ScanDetails details) {
      if (!ReadStartSymbol(source, details)) return false;

      bool escapeEnabled = !details.IsSet(ScanFlags.DisableEscapes);
      bool ignoreCase = IsSet(TermOptions.SpecialIgnoreCase);
      int start = source.Position;
      string startS = details.ControlSymbol;
      string startS2 = startS + startS; //doubled start symbol
      //1. Find the string end
      // first get the position of the next line break; we are interested in it to detect malformed string, 
      //  therefore do it only if linebreak is NOT allowed; if linebreak is allowed, set it to -1 (we don't care).  
      int nlPos = details.IsSet(ScanFlags.AllowLineBreak) ? -1 : source.Text.IndexOf('\n', source.Position);
      while (!source.EOF()) {
        int endPos = source.Text.IndexOf(startS, source.Position);
        //Check for malformed string: either EndSymbol not found, or LineBreak is found before EndSymbol
        bool malformed = endPos < 0 || nlPos >= 0 && nlPos < endPos;
        if (malformed) {
          //Set source position for recovery: move to the next line if linebreak is not allowed.
          if (nlPos > 0) endPos = nlPos;
          if (endPos > 0) source.Position = endPos + 1;
          details.Error = "Mal-formed  string literal - cannot find termination symbol.";
          return true;
        }
        
        //We found EndSymbol - check if it is escaped; if yes, skip it and continue search
        if (escapeEnabled && source.Text[endPos - 1] == EscapeChar) {
          source.Position = endPos + startS.Length;
          continue; //searching for end symbol
        }
        
        //Check if it is doubled end symbol
        source.Position = endPos;
        if (details.IsSet(ScanFlags.AllowDoubledQuote) && source.MatchSymbol(startS2, ignoreCase)) {
          source.Position = endPos + startS.Length * 2;
          continue;
        }//checking for doubled end symbol
        
        //Ok, this is normal endSymbol that terminates the string. 
        // Advance source position and get out from the loop
        details.Body = source.Text.Substring(start, endPos - start);
        source.Position = endPos + startS.Length;
        return true; //if we come here it means we're done - we found string end.
      }  //end of loop to find string end; 
      return false;
    }

    private bool ReadStartSymbol(ISourceStream source, ScanDetails details) {
      if (_startEndFirsts.IndexOf(source.CurrentChar) < 0)
        return false;
      bool ignoreCase = IsSet(TermOptions.SpecialIgnoreCase);
      foreach (string startEnd in _startEndSymbols) {
        if (!source.MatchSymbol(startEnd, ignoreCase))
          continue; 
        //We found start symbol
        details.ControlSymbol = startEnd;
        details.Flags |= StartEndSymbolTable[startEnd];
        source.Position += startEnd.Length;
        return true;
      }//foreach
      return false; 
    }//method


    //Extract the string content from lexeme, adjusts the escaped and double-end symbols
    //TODO: add support for unicode, hex and octal escapes
    protected override object ConvertValue(ScanDetails details) {
      string value = details.Body;
      bool escapeEnabled = !details.IsSet(ScanFlags.DisableEscapes);
      //Fix all escapes
      if (escapeEnabled && value.IndexOf(EscapeChar) >= 0) {
        details.Flags |= ScanFlags.HasEscapes;
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
            arr[i] = HandleSpecialEscape(arr[i], details);
          }//else
        }//for i
        value = string.Join(string.Empty, arr);
      }// if EscapeEnabled 

      //Check for doubled end symbol
      string startS = details.ControlSymbol;
      if (details.IsSet(ScanFlags.AllowDoubledQuote) && value.IndexOf(startS) >= 0)
        value = value.Replace(startS + startS, startS);

      if (details.IsSet(ScanFlags.IsChar))
        details.TypeCode = TypeCode.Char;
      //Check char length - must be exactly 1
      if (details.TypeCode == TypeCode.Char && value.Length != 1) {
        details.Error = "Invalid length of char literal - should be 1.";
        return value;
      }

      object result = (details.TypeCode == TypeCode.Char ? (object) value[0] : value);
      return result; 
      
      //TODO: Investigate unescaped linebreak, with  Flags == BnfFlags.StringAllowLineBreak | BnfFlags.StringLineBreakEscaped
      //      also investigate what happens in this case in Windows where default linebreak is "\r\n", not "\n"
    }

    //Should support:  \Udddddddd, \udddd, \xdddd, \N{name}, \0, \ddd (octal),  
    protected virtual string HandleSpecialEscape(string segment, ScanDetails details) {
      if (string.IsNullOrEmpty(segment)) return string.Empty;
      int len, p; string digits; char ch; string result; 
      char first = segment[0];
      switch (first) {
        case 'u':
        case 'U':
          if (details.IsSet(ScanFlags.AllowUEscapes)) {
            len = (first == 'u' ? 4 : 8);
            if (segment.Length < len + 1) {
              details.Error = "Invalid unicode escape (" + segment.Substring(len + 1) + "), expected " + len + " hex digits.";
              return segment;
            }
            digits = segment.Substring(1, len);
            ch = (char) Convert.ToUInt32(digits, 16);
            result = ch + segment.Substring(len + 1);
            return result; 
          }//if
          break;
        case 'x':
          if (details.IsSet(ScanFlags.AllowXEscapes)) {
            //x-escape allows variable number of digits, from one to 4; let's count them
            p = 1; //current position
            while (p < 5 && p < segment.Length) {
              if (TextUtils.HexDigits.IndexOf(segment[p]) < 0) break;
              p++;
            }
            //p now point to char right after the last digit
            if (p <= 1) {
              details.Error = "Invalid \\x escape, at least one digit expected.";
              return segment;
            }
            digits = segment.Substring(1, p - 1);
            ch = (char) Convert.ToUInt32(digits, 16);
            result = ch + segment.Substring(p);
            return result;
          }//if
          break;
        case '0':  case '1':  case '2':  case '3':  case '4':  case '5':   case '6': case '7':
          if (details.IsSet(ScanFlags.AllowOctalEscapes)) {
            //octal escape allows variable number of digits, from one to 3; let's count them
            p = 0; //current position
            while (p < 3 && p < segment.Length) {
              if (TextUtils.OctalDigits.IndexOf(segment[p]) < 0) break;
              p++;
            }
            //p now point to char right after the last digit
            digits = segment.Substring(0, p);
            ch = (char)Convert.ToUInt32(digits, 8);
            result = ch + segment.Substring(p);
            return result;
          }//if
          break;
      }//switch
      details.Error = "Invalid escape sequence: \\" + segment;
      return segment; 
    }//method
    #endregion


  }//class

}//namespace

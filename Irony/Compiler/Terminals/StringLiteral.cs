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

  [Flags]
  public enum StringFlags {
    None = 0,
    IsChar = 0x01,
    AllowsDoubledQuote = 0x02, //Convert doubled start/end symbol to a single symbol; for ex. in SQL, '' -> '
    AllowsLineBreak = 0x04,
    HasEscapes = 0x08,     //also used by IdentifierTerminal
    NoEscapes = 0x10, //also used by IdentifierTerminal
    AllowsUEscapes = 0x20, //also used by IdentifierTerminal
    AllowsXEscapes = 0x40,
    AllowsOctalEscapes = 0x80,
    AllowsAllEscapes = AllowsUEscapes | AllowsXEscapes | AllowsOctalEscapes,

  }
  public class StringLiteral : CompoundTerminalBase {

    #region StringKindInfo, StringKindInfoTable classes 
    class StringKindInfo {
      internal readonly string Start, End;
      internal readonly StringFlags Flags;
      internal int ScannerState = 0;
      internal StringKindInfo(string start, string end, StringFlags flags) {
        Start = start;
        End = end;
        Flags = flags; 
      }
      internal static int LongerStartFirst(StringKindInfo x, StringKindInfo y) {
        try {//in case any of them is null
          if (x.Start.Length > y.Start.Length) return -1;
        } catch { }
        return 0;
      }
    }
    class StringKindInfoList : List<StringKindInfo> {
      internal void Add(string start, string end, StringFlags flags) {
        base.Add(new StringKindInfo(start, end, flags)); 
      }
    } 
    #endregion

    #region constructors and initialization
    //12/07/2008 - Attention - breaking change! Constructor with a single parameter no longer adds default startEnd symbol!
    public StringLiteral(string name) : base(name) {
      base.Escapes = TextUtils.GetDefaultEscapes();
      base.Category = TokenCategory.Literal;
    }
    public StringLiteral(string name, string startEndSymbol, StringFlags stringFlags) : this(name) {
      this._stringKinds.Add(startEndSymbol, startEndSymbol, stringFlags);
    }    
    public StringLiteral(string name, string startEndSymbol) : this(name, startEndSymbol, StringFlags.None) {
    }

    public void AddStartEnd(string startEndSymbol, StringFlags stringFlags) {
      _stringKinds.Add(startEndSymbol, startEndSymbol, stringFlags);
    }
    public void AddStartEnd(string startSymbol, string endSymbol, StringFlags stringFlags) {
      _stringKinds.Add(startSymbol, endSymbol, stringFlags);
    }
    public void AddPrefix(string prefix, StringFlags flags) {
      base.AddPrefixFlag(prefix, (int)flags); 
    }

    #endregion

    #region Properties/Fields
    private readonly StringKindInfoList _stringKinds = new StringKindInfoList();
    string _startSymbolsFirsts; //first chars  of start-end symbols
    #endregion

    #region overrides: Init, GetFirsts, ReadBody, etc...
    public override void Init(Grammar grammar) {
      base.Init(grammar);
      if (_stringKinds.Count == 0) {
        grammar.Errors.Add("Error in string literal [" + this.Name + "]: No start/end symbols specified.");
      }
      //collect all start-end symbols in lists and create strings of first chars for both
      _stringKinds.Sort(StringKindInfo.LongerStartFirst); 
      _startSymbolsFirsts = string.Empty;
      foreach (StringKindInfo info in _stringKinds) {
        _startSymbolsFirsts += info.Start[0].ToString();
      }
 
      if (!CaseSensitive) 
        _startSymbolsFirsts = _startSymbolsFirsts.ToLower() + _startSymbolsFirsts.ToUpper();
      if (this.EditorInfo == null)
        this.EditorInfo = new TokenEditorInfo(TokenType.String, TokenColor.String, TokenTriggers.None);
    }//method

    public override IList<string> GetFirsts() {
      StringList result = new StringList();
      result.AddRange(Prefixes);
      //we assume that prefix is always optional, so string can start with start-end symbol
      foreach (char ch in _startSymbolsFirsts)
        result.Add(ch.ToString()); 
      return result;
    }

    public override void RegisterForMultilineScan(ScannerControlData data) {
      foreach (StringKindInfo info in _stringKinds) {
        if ((info.Flags & StringFlags.AllowsLineBreak) != 0) {
          info.ScannerState = data.RegisterMultiline(this); 
        }
      }      
    }

    protected override bool ReadBody(ISourceStream source, CompoundTokenDetails details) {
      if (!ReadStartSymbol(source, details)) return false;

      bool escapeEnabled = !FlagIsSet(details, StringFlags.NoEscapes);
      int start = source.Position;
      string endQuoteSymbol = details.EndSymbol;
      string endQuoteDoubled = endQuoteSymbol + endQuoteSymbol; //doubled quote symbol
      //1. Find the string end
      // first get the position of the next line break; we are interested in it to detect malformed string, 
      //  therefore do it only if linebreak is NOT allowed; if linebreak is allowed, set it to -1 (we don't care).  
      int nlPos = FlagIsSet(details, StringFlags.AllowsLineBreak) ? -1 : source.Text.IndexOf('\n', source.Position);
      while (!source.EOF()) {
        int endPos = source.Text.IndexOf(endQuoteSymbol, source.Position);
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
        if (escapeEnabled && IsEndQuoteEscaped(source.Text, endPos)) {
          source.Position = endPos + endQuoteSymbol.Length;
          continue; //searching for end symbol
        }
        
        //Check if it is doubled end symbol
        source.Position = endPos;
        if (FlagIsSet(details, StringFlags.AllowsDoubledQuote) && source.MatchSymbol(endQuoteDoubled, !CaseSensitive)) {
          source.Position = endPos + endQuoteDoubled.Length;
          continue;
        }//checking for doubled end symbol
        
        //Ok, this is normal endSymbol that terminates the string. 
        // Advance source position and get out from the loop
        details.Body = source.Text.Substring(start, endPos - start);
        source.Position = endPos + endQuoteSymbol.Length;
        return true; //if we come here it means we're done - we found string end.
      }  //end of loop to find string end; 
      return false;
    }

    protected override void ReadSuffix(ISourceStream source, CompoundTerminalBase.CompoundTokenDetails details) {
      base.ReadSuffix(source, details);
      //"char" type can be identified by suffix (like VB where c suffix identifies char)
      // in this case we have details.TypeCodes[0] == char  and we need to set the IsChar flag
      if (details.TypeCodes != null && details.TypeCodes[0] == TypeCode.Char)
        details.Flags |= (int)StringFlags.IsChar;
      else
        //we may have IsChar flag set (from startEndSymbol, like in c# single quote identifies char)
        // in this case set type code
        if (FlagIsSet(details, StringFlags.IsChar))
          details.TypeCodes = new TypeCode[] { TypeCode.Char }; 
    }

    private bool IsEndQuoteEscaped(string text, int quotePosition) {
      bool escaped = false;
      int p = quotePosition - 1;
      while (p > 0 && text[p] == EscapeChar) {
        escaped = !escaped;
        p--;
      }
      return escaped;
    }

    private bool ReadStartSymbol(ISourceStream source, CompoundTokenDetails details) {
      if (_startSymbolsFirsts.IndexOf(source.CurrentChar) < 0)
        return false;
      foreach (StringKindInfo stringKind in _stringKinds) {
        if (!source.MatchSymbol(stringKind.Start, !CaseSensitive))
          continue; 
        //We found start symbol
        details.StartSymbol = stringKind.Start;
        details.EndSymbol = stringKind.End;
        details.Flags |= (int) stringKind.Flags;
        source.Position += stringKind.Start.Length;
        return true;
      }//foreach
      return false; 
    }//method


    //Extract the string content from lexeme, adjusts the escaped and double-end symbols
    protected override bool ConvertValue(CompoundTokenDetails details) {
      string value = details.Body;
      bool escapeEnabled = !FlagIsSet(details, StringFlags.NoEscapes);
      //Fix all escapes
      if (escapeEnabled && value.IndexOf(EscapeChar) >= 0) {
        details.Flags |= (int) StringFlags.HasEscapes;
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
      string endSymbol = details.EndSymbol;
      if (FlagIsSet(details, StringFlags.AllowsDoubledQuote) && value.IndexOf(endSymbol) >= 0)
        value = value.Replace(endSymbol + endSymbol, endSymbol);

      if (FlagIsSet(details, StringFlags.IsChar)) {
        if (value.Length != 1) {
          details.Error = "Invalid length of char literal - should be 1.";
          return false;
        }
        details.Value = value[0]; 
      } else {
        details.TypeCodes = new TypeCode[] { TypeCode.String };
        details.Value = value; 
      }
      //Check char length - must be exactly 1
      return true; 
    }

    //Should support:  \Udddddddd, \udddd, \xdddd, \N{name}, \0, \ddd (octal),  
    protected virtual string HandleSpecialEscape(string segment, CompoundTokenDetails details) {
      if (string.IsNullOrEmpty(segment)) return string.Empty;
      int len, p; string digits; char ch; string result; 
      char first = segment[0];
      switch (first) {
        case 'u':
        case 'U':
          if (FlagIsSet(details, StringFlags.AllowsUEscapes)) {
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
          if (FlagIsSet(details, StringFlags.AllowsXEscapes)) {
            //x-escape allows variable number of digits, from one to 4; let's count them
            p = 1; //current position
            while (p < 5 && p < segment.Length) {
              if (TextUtils.HexDigits.IndexOf(segment[p]) < 0) break;
              p++;
            }
            //p now point to char right after the last digit
            if (p <= 1) {
              details.Error = @"Invalid \x escape, at least one digit expected.";
              return segment;
            }
            digits = segment.Substring(1, p - 1);
            ch = (char) Convert.ToUInt32(digits, 16);
            result = ch + segment.Substring(p);
            return result;
          }//if
          break;
        case '0':  case '1':  case '2':  case '3':  case '4':  case '5':   case '6': case '7':
          if (FlagIsSet(details, StringFlags.AllowsOctalEscapes)) {
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

    protected static bool FlagIsSet(CompoundTokenDetails details, StringFlags flag) {
      return (details.Flags & (int)flag) != 0;
    }

  }//class

}//namespace

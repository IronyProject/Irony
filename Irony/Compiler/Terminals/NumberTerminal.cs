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
  using BigInteger = Microsoft.Scripting.Math.BigInteger;


  //TODO: For VB, we may need to add a flag to automatically use long instead of int (default) when number is too large
  public class NumberTerminal : Terminal {

    #region Nested classes
    public class BasePrefixTable : Dictionary<string, byte> { }
    public class SuffixTable : Dictionary<string, TypeCode> { }

    public class NumberScanInfo {
      public string BasePrefix;
      public int Base = 10;          // 10/8/16 - derived from base prefix 
      public string AllowedDigits = _decimals;                 
      public char ExponentChar = char.MinValue;
      public string TypeSuffix; // c#: U,L, F, M;   VB: @,!,L,S,I;  Python: L
      public TypeCode Type;     //derived from TypeSuffix
      public bool HasDotOrExponent;  //set to true if we meet dot or exponent symbol  
      public string Body;      //main body, without prefix or suffix
      public object Value;
      public string Error;
    }

    #endregion 

    #region constructors
    public NumberTerminal(string name, BnfFlags flags)  : this(name) {
      SetFlag(flags);
    }
    public NumberTerminal(string name) : base(name) {
      base.MatchMode = TokenMatchMode.ByType;
    }
    public NumberTerminal(string name, string alias) : base(name) {
      base.Alias = alias;
    }
    #endregion 

    #region Public fields/properties: ExponentSymbols, Suffixes
    public string ExponentSymbols = "eE"; //most of the time; in some languages (Scheme) we have more
    public readonly BasePrefixTable BasePrefixes = new BasePrefixTable();
    public readonly SuffixTable Suffixes = new SuffixTable();  
    //Default types are assigned to literals without suffixes
    public TypeCode DefaultIntType = TypeCode.Int32;  
    public TypeCode DefaultFloatType = TypeCode.Double;
    public const string ScanInfoKey = "ScanInfo";

    public void AddPrefix(string prefix, byte numericBase) {
      BasePrefixes.Add(prefix, numericBase);
    }
    public void AddSuffix(string suffix, TypeCode type) {
      Suffixes.Add(suffix, type);
    }
    #endregion

    #region events: ConvertingValue
    public event EventHandler<NumberScanEventArgs> ConvertingValue; 
    #endregion

    #region Private fields
    string _prefixesFirsts; //first symbols of all prefixes, for fast prefix detection
    KeyList _prefixList = new KeyList();
    string _suffixesFirsts; //first symbols of all suffixes, for fast suffix detection
    KeyList _suffixList = new KeyList(); 
    const string _decimals = "1234567890";
    const string _octals = "12345670";
    const string _hexes = "1234567890aAbBcCdDeEfF";
    const string _quickParseDelimiters = " ,;)]}+-*/>=\n\t";
    #endregion

    #region overrides
    public override void Init(Grammar grammar) {
      base.Init(grammar);
      //collect all suffixes and base prefixes in lists and create strings of first chars for both
      _prefixList.Clear();
      _prefixList.AddRange(BasePrefixes.Keys);
      _prefixList.Sort(KeyList.LongerFirst);
      _prefixesFirsts = string.Empty;
      foreach (string pfx in _prefixList)
        _prefixesFirsts += pfx[0];

      _suffixList.Clear();
      _suffixList.AddRange(Suffixes.Keys);
      _suffixList.Sort(KeyList.LongerFirst);
      _suffixesFirsts = string.Empty;
      foreach (string sfx in _suffixList)
        _suffixesFirsts += sfx[0]; //we don't care if there are repetitions

      if (IsFlagSet(BnfFlags.NumberIgnoreCase)) {
        _prefixesFirsts = _prefixesFirsts.ToLower() + _prefixesFirsts.ToUpper();
        _suffixesFirsts = _suffixesFirsts.ToLower() + _suffixesFirsts.ToUpper();
      }
    }
    
    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      //quick check
      Token token = QuickParse(source);
      if (token != null) return token;

      //Read prefix, body, suffix, convert the value
      NumberScanInfo info = new NumberScanInfo();
      ReadPrefix(source, info);
      ReadBody(source, info);
      ReadSuffix(source, info);

      //Take the lexeme and convert it to a numeric value 
      string lexeme = source.GetLexeme();
      ConvertValue(info);
      if (!string.IsNullOrEmpty(info.Error))
        context.AddError(source.TokenStart, info.Error);
      token = new Token(this, source.TokenStart, lexeme, info.Value);
      token.Attributes[NumberTerminal.ScanInfoKey] = info;
      return token;
    }//TryMatch method

    //Most numbers in source programs are just one-digit instances of 0, 1, 2, and maybe others until 9
    // so we try to do a quick parse for these, without starting the whole general process
    private Token QuickParse(ISourceStream source) {
      char current = source.CurrentChar;
      if (char.IsDigit(current) && _quickParseDelimiters.IndexOf(source.NextChar) >= 0) {
        int value = (int)(current - '0');
        Token token = new Token(this, source.TokenStart, current.ToString(), value);
        source.Position++;
        return token;
      } else 
        return null;
    }

    private void ReadPrefix(ISourceStream source, NumberScanInfo info) {
      if (_prefixesFirsts.IndexOf(source.CurrentChar) < 0)
        return;
      bool ignoreCase = IsFlagSet(BnfFlags.NumberIgnoreCase);
      foreach (string pfx in _prefixList) {
        if (string.Compare(source.Text, source.Position, pfx, 0, pfx.Length, ignoreCase) != 0) 
          continue;
        //check that prefix is not followed by dot; this may happen in Python for number "0.123" - we can mistakenly take "0" as octal prefix
        int nextIndex = source.Position + pfx.Length;
        bool nextIsDot = (nextIndex < source.Text.Length) && source.Text[nextIndex] == '.';
        if (nextIsDot) continue; 
        //We found prefix
        info.Base = BasePrefixes[pfx];
        switch (info.Base) {
          case 8: info.AllowedDigits = _octals; break;
          case 16: info.AllowedDigits = _hexes; break;
        }
        source.Position += pfx.Length;
        return;          
      }//foreach
    }//method

    private void ReadBody(ISourceStream source, NumberScanInfo info) {
      int start = source.Position;  //remember start - it may be different from source.TokenStart, we may have skipped 
      while (!source.EOF()) {
        char current = source.CurrentChar;
        //1. If it is a digit, just continue going
        if (info.AllowedDigits.IndexOf(current) >= 0) {
          source.Position++;
          continue;
        }
        //2. Check if it is a dot
        if (current == '.') {
          //If we had seen already a dot or exponent, don't accept this one; also, we accept dot only if it is followed by a digit
          if (info.HasDotOrExponent || (info.AllowedDigits.IndexOf(source.NextChar) < 0))
            break; //from while loop
          info.HasDotOrExponent = true;
          source.Position++;
          continue;
        }
        //3. Only for decimals - check if it is (the first) exponent symbol
        if ((info.Base == 10) && (info.ExponentChar == char.MinValue) && (ExponentSymbols.IndexOf(current) >= 0)) {
          char next = source.NextChar;
          bool nextIsSign = next == '+' || next == '-';
          bool nextIsDigit = info.AllowedDigits.IndexOf(next) >= 0;
          if (!nextIsSign && !nextIsDigit)
            break;  //Exponent should be followed by either sign or digit
          //ok, we've got real exponent
          info.ExponentChar = current; //remember the exp char
          info.HasDotOrExponent = true;
          source.Position++;
          if (nextIsSign)
            source.Position++; //skip +/- explicitly so we don't have to deal with them on the next iteration
          continue; 
        }
        //4. It is something else (not digit, not dot or exponent) - we're done
        break; //from while loop
      }//while
      int end = source.Position;
      info.Body = source.Text.Substring(start, end - start);
    }

    private void ReadSuffix(ISourceStream source, NumberScanInfo info) {
      info.Type = info.HasDotOrExponent ? DefaultFloatType : DefaultIntType;
      if( _suffixesFirsts.IndexOf(source.CurrentChar) < 0) return;
      bool ignoreCase = IsFlagSet(BnfFlags.NumberIgnoreCase);
      foreach (string sfx in _suffixList) {
        if (string.Compare(source.Text, source.Position, sfx, 0, sfx.Length, ignoreCase) != 0)
          continue;
        //We found suffix
        info.Type = Suffixes[sfx];
        source.Position += sfx.Length;
        return;
      }//foreach
    }//method

    private void ConvertValue(NumberScanInfo info) {
      if (ConvertingValue != null) {
        OnConvertingValue(info);
        if (info.Value != null || info.Error != null) return; 
      }
      try {
        switch (info.Base) {
          case 8: 
          case 16:
            ulong tmp = Convert.ToUInt64(info.Body, info.Base);
            info.Value = Convert.ChangeType(tmp, info.Type);
            break; 
          case 10:
            info.Value = Convert.ChangeType(info.Body, info.Type);
            break;
        }//switch
        return; 
      } catch (OverflowException) {
        //just continue after the try/catch
      } catch (Exception) {
        info.Error = "Invalid number.";
        return;
      }//catch

      //If we are here, we failed to convert, so let's try something else
      //Try it as big int
      if (IsFlagSet(BnfFlags.NumberAllowBigInts)) {
        BigInteger bigInt;
        if (BigIntegerTryParse(info.Body, out bigInt)) 
          info.Value = bigInt;
        else 
          info.Error = "Invalid number - overflow";
        return;
      }//if
      //Try converting to double
      if (IsFlagSet(BnfFlags.NumberUseFloatOnIntOverflow)) {
        double dbl;
        if (double.TryParse(info.Body,  out dbl)) 
          info.Value = dbl;
        else
          info.Error = "Invalid number - overflow";
        return;
      }
    }//method

    protected void OnConvertingValue(NumberScanInfo info) {
      NumberScanEventArgs args = new NumberScanEventArgs(info);
      ConvertingValue(this, args);
    }

    public override IList<string> GetStartSymbols() {
      StringList result = new StringList();
      result.AddRange(_prefixList);
      //we assume that prefix is always optional, so number can always start with plain digit
      result.AddRange(new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" });
      return result; 
    }
    #endregion

    #region static utilities
    //The following method is taken from Numbers.cs in IronScheme distribution
    public static bool BigIntegerTryParse(string number, out BigInteger result) {
      result = null;
      if (number == null)
        return false;
      int i = 0, len = number.Length, sign = 1;

      char c;
      bool digits_seen = false;
      BigInteger val = new BigInteger(0);
      if (number[i] == '+') {
        i++;
      } else if (number[i] == '-') {
        sign = -1;
        i++;
      }
      for (; i < len; i++) {
        c = number[i];
        if (c == '\0') {
          i = len;
          continue;
        }
        if (c >= '0' && c <= '9') {
          val = val * 10 + (c - '0');
          digits_seen = true;
        } else {
          if (Char.IsWhiteSpace(c)) {
            for (i++; i < len; i++) {
              if (!Char.IsWhiteSpace(number[i]))
                return false;
            }
            break;
          } else
            return false;
        }
      }
      if (!digits_seen)
        return false;

      result = val * sign;

      return true;
    }

    #endregion

    #region Factory methods for specific versions
    #endregion

  }//class


}

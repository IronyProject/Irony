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
  //TODO: implement support for hex,oct and binary-based presentations
  // Ruby may require implementing custom Number terminal, to allow expressions like "5.times"
  public class NumberTerminal : Terminal {
    public NumberTerminal(string name, bool detectInts)  : this(name) {
      _detectInts = detectInts; 
    }
    public NumberTerminal(string name) : base(name) {
      base.MatchMode = TokenMatchMode.ByType;
    }
    public NumberTerminal(string name, string alias) : base(name) {
      base.Alias = alias;
    }
    public bool DetectInts  {
      get {return _detectInts;}
      set {_detectInts = value;}
    } bool  _detectInts;

    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      char ch = source.CurrentChar;
      bool firstOk = char.IsDigit(ch);
      if (!firstOk) 
        return null;
      bool isFloat = false, prevIsExp = false;
      const string allowedChars = "0123456789.Ee";
      while (!source.EOF()) {
        char current = source.CurrentChar;
        bool charOk = allowedChars.IndexOf(current) >= 0 || prevIsExp && (current == '+' || current == '-');
        prevIsExp = (current == 'E' || current == 'e'); //calc for next loop
        isFloat |= current == '.' || prevIsExp;
        if (!charOk) break;
        source.Position++;
      }//while
      string text = source.GetLexeme();
      if (_detectInts && !isFloat) {
        long value;
        bool success = long.TryParse(text, out value);
        if (!success) {
          context.AddError(source.TokenStart, "Invalid number.");
          value = 0;
        }
        return new Token(this, source.TokenStart, text, value);
      } else {
        double value;
        bool success = double.TryParse(text, out value);
        if (!success) {
          context.AddError(source.TokenStart, "Invalid number.");
          value = double.NaN;
        }
        return new Token(this, source.TokenStart, text, value);
      }
    }//method

    public override IList<string> GetPrefixes() {
      return new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
    }

  }//class


}

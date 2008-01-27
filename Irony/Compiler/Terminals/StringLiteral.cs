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

  public class CharTable : Dictionary<string, char> { }
  
 /* //currently not used but should be!
   
    public enum StringFlags {
    AllowNewLine = 0x01,
    ChangeDoubleStartToSingle = 0x02,
    EnableEscape = 0x04,  //escape maybe disabled like in c# strings like @"abc\d"
    Default = EnableEscape | ChangeDoubleStartToSingle,
    }
    Other properties: 
    prefixes; (python's u, U etc, c# @)
     
  }*/

  //TODO: implement support for char escapes
  // also, it may make sense to have multiple Start/End symbol pairs, for languages like Python.
  public class StringLiteral : Terminal {
    public StringLiteral() : this("StringLiteral", "string") { }
    public StringLiteral(string name) : this(name, "string") { }
    public StringLiteral(string name, string alias) : base(name) {
      base.Alias = alias;
      Escapes = GetDefaultEscapes();
      base.MatchMode = TokenMatchMode.ByType;
    }
    public StringLiteral(string name, string startSymbol, string endSymbol) : this(name) {
      StartSymbol = startSymbol;
      EndSymbol = endSymbol;
    }
    public StringLiteral(string name, string alias, string startSymbol, string endSymbol) : this(name, startSymbol, endSymbol) {
      this.Alias = alias;
    }

    #region fields
    public string StartSymbol = "\"";
    public string EndSymbol = "\"";
    //public StringFlags Flags = StringFlags.Default;
    public Char EscapeChar = '\\';
    public readonly CharTable Escapes;

    #endregion

    #region overrides: TryMatch, GetPrefixes
    //TODO: add support for escapes
    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      if (source.CurrentChar != StartSymbol[0]) return null;
      int startPos = source.TokenStart.Position;
      string text = source.Text;
      if (startPos + StartSymbol.Length >= text.Length)
        return null;
      if (string.Compare(text, startPos, StartSymbol, 0, StartSymbol.Length, true) != 0)
        return null;
      int endPos = text.IndexOf(EndSymbol, startPos + StartSymbol.Length);
      //TODO: add search for escapes and newLine here
      if (endPos < 0) {
        context.AddError (source.TokenStart, "Cannot find the end of string literal");
        source.Position = text.IndexOf("\n", startPos) ;
      } else 
        source.Position = endPos + EndSymbol.Length;
      string lexeme = source.GetLexeme();
      string value = lexeme.Substring(StartSymbol.Length, lexeme.Length - StartSymbol.Length - EndSymbol.Length);
      Token tkn = new Token(this, source.TokenStart, lexeme, value);
      return tkn;
    }
    public override IList<string> GetPrefixes() {
      return new string[] { StartSymbol };
    }
    #endregion

    #region Utilities: GetDefaultEscapes
    public static CharTable GetDefaultEscapes() {
      CharTable escapes = new CharTable();
      escapes.Add("a", '\u0007');
      escapes.Add("b", '\b');
      escapes.Add("t", '\t');
      escapes.Add("n", '\n');
      escapes.Add("v", '\v');
      escapes.Add("f", '\f');
      escapes.Add("r", '\r');
      escapes.Add("\"", '"');
      escapes.Add("\\", '\\');
      escapes.Add(" ", ' ');
      return escapes;
    }
    #endregion

  }//class

}//namespace

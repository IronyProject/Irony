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

  public class CommentTerminal : Terminal {
    public CommentTerminal(string name, string startSymbol, string endSymbol) : base(name, TokenCategory.Comment) {
      this.StartSymbol = startSymbol;
      this.EndSymbol = endSymbol;
    }

    public string StartSymbol;
    public string EndSymbol;


    #region overrides
    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      //Check starting symbol
      if (string.Compare(source.Text, source.Position, StartSymbol, 0,  StartSymbol.Length, !Grammar.CaseSensitive) != 0)
        return null;
      //Find end symbol
      int endPos = source.Text.IndexOf(EndSymbol, source.Position);
      if (endPos < 0) {
        source.Position = source.Text.Length;
        return Grammar.CreateSyntaxErrorToken(source.TokenStart, "Unclosed comment block");
      }
      source.Position = endPos + EndSymbol.Length;
      string lexeme = source.GetLexeme();
      return new Token(this, source.TokenStart, lexeme);
    }
    public override IList<string> GetStartSymbols() {
      return new string[] { StartSymbol };
    }
    #endregion
  }//CommentTerminal class


}

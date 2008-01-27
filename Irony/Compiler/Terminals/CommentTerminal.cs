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
    public CommentTerminal() : this("Comment") { }
    public CommentTerminal(string name)
      : base(name) {
      Category = TokenCategory.Comment;
    }
    public CommentTerminal(string name, string lineStartSymbol, string startSymbol, string endSymbol)
      : this(name) {
      LineStartSymbol = lineStartSymbol;
      StartSymbol = startSymbol;
      EndSymbol = endSymbol;
    }

    public string LineStartSymbol = "//";
    public string StartSymbol = "/*";
    public string EndSymbol = "*/";


    #region overrides
    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      return TryMatchLineComment(context, source) ?? TryMatchExtComment(context, source);
    }
    private Token TryMatchLineComment(CompilerContext context, ISourceStream source) {
      source.Position = source.TokenStart.Position;
      //quick check
      if (string.IsNullOrEmpty(LineStartSymbol)) return null; 
      if (source.CurrentChar != LineStartSymbol[0]) return null;
      source.Position += LineStartSymbol.Length;
      if (source.GetLexeme() != LineStartSymbol) return null;
      int endPos = source.Text.IndexOf('\n', source.Position);
      if (endPos == -1) endPos = source.Text.Length;
      source.Position = endPos;
      string lexeme = source.GetLexeme();
      return new Token(this, source.TokenStart, lexeme);
    }
    private Token TryMatchExtComment(CompilerContext context, ISourceStream source) {
      source.Position = source.TokenStart.Position;
      //quick check
      if (string.IsNullOrEmpty(StartSymbol)) return null;
      if (source.CurrentChar != StartSymbol[0]) return null;
      source.Position += StartSymbol.Length;
      if (source.GetLexeme() != StartSymbol) return null;
      int endPos = source.Text.IndexOf(EndSymbol, source.Position);
      if (endPos < 0) {
        source.Position = source.Text.Length;
        return Grammar.CreateSyntaxErrorToken(source.TokenStart, "Unclosed comment block");
      }
      source.Position = endPos + EndSymbol.Length;
      string lexeme = source.GetLexeme();
      return new Token(this, source.TokenStart, lexeme);
    }
    public override IList<string> GetPrefixes() {
      return new string[] { StartSymbol, LineStartSymbol };
    }
    #endregion
  }//CommentTerminal class


}

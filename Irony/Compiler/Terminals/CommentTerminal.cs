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
    public CommentTerminal(string name, string startSymbol, params string[] endSymbols) : base(name, TokenCategory.Comment) {
      this.StartSymbol = startSymbol;
      this.EndSymbols = new StringList();
      EndSymbols.AddRange(endSymbols);
    }

    public string StartSymbol;
    public StringList EndSymbols;
    private char[] _endSymbolsFirsts;
    private bool _isLineComment; //true if NewLine is one of EndSymbols; if yes, EOF is also considered a valid end symbol


    #region overrides
    public override void Init(Grammar grammar) {
      base.Init(grammar);
      //_endSymbolsFirsts char array is used for fast search for end symbols using String's method IndexOfAny(...)
      _endSymbolsFirsts = new char[EndSymbols.Count];
      for (int i = 0; i < EndSymbols.Count; i++) {
        string sym = EndSymbols[i];
        _endSymbolsFirsts[i] = sym[0];
        _isLineComment |= sym.Contains("\n");
      }
      if (this.EditorInfo == null) {
        TokenType ttype = _isLineComment ? TokenType.LineComment : TokenType.Comment;
        this.EditorInfo = new TokenEditorInfo(ttype, TokenColor.Comment, TokenTriggers.None);
      }
    }

    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      bool ignoreCase = !Grammar.CaseSensitive;
      //Check starting symbol
      if (!source.MatchSymbol(StartSymbol, ignoreCase)) return null;
      //Find end symbol
      source.Position += StartSymbol.Length;

      while(!source.EOF()) {
        int firstCharPos;
        if (EndSymbols.Count == 1)
          firstCharPos = source.Text.IndexOf(EndSymbols[0], source.Position);
        else 
          firstCharPos = source.Text.IndexOfAny(_endSymbolsFirsts, source.Position);
        if (firstCharPos < 0) {
          source.Position = source.Text.Length;
          if (_isLineComment) //if it is LineComment, it is ok to hit EOF without final line-break; just return all until end.
            return Token.Create(this, context, source.TokenStart, source.GetLexeme());
          else 
            return context.CreateErrorTokenAndReportError( source.TokenStart, string.Empty, "Unclosed comment block");
        }
        //We found a character that might start an end symbol; let's see if it is true.
        source.Position = firstCharPos;
        foreach (string endSymbol in EndSymbols)
          if (source.MatchSymbol(endSymbol, ignoreCase)) {
            //We found end symbol; eat end symbol only if it is not line comment.
            // For line comment, leave LF symbol there, it might be important to have a separate LF token
            if (!_isLineComment) 
              source.Position += endSymbol.Length;
            return Token.Create(this, context, source.TokenStart, source.GetLexeme());
          }//if
        source.Position++; //move to the next char and try again    
      }//while
      return null; //never happens
    }//method

    public override IList<string> GetFirsts() {
      return new string[] { StartSymbol };
    }
    #endregion
  }//CommentTerminal class


}

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

  //Adds newLine, indent, unindent tokes to scanner's output stream for languages like Python.
  // Scanner ignores new lines and indentations as whitespace; this filter produces these symbols based 
  // on col/line information in content tokens. 
  // Note: this needs to be redone, to generate Grammar.Eos (end-of-statement) token instead of newLine;
  //  also need to recognize line-continuation symbols ("\" in python, "_" in VB); or incomplete statement indicators
  //  that signal that line continues (if line ends with operator in Ruby, it means statement continues on 
  //  the next line).
  public class CodeOutlineFilter : TokenFilter {
    int _prevLine;
    Stack<int> _indents = new Stack<int>();

    public CodeOutlineFilter(bool trackIndents) {
      _trackIndents = trackIndents;
    }

    public bool TrackIndents {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _trackIndents; }
      set { _trackIndents = value; }
    } bool _trackIndents = true;

    public override IEnumerable<Token> BeginFiltering(CompilerContext context, IEnumerable<Token> tokens) {
      _prevLine = 0;
      _indents.Clear();
      foreach (Token token in tokens) {
        if (token.Terminal == Grammar.Eof) {
          yield return CreateNewLine(token.Location); //this is necessary, because grammar rules reference newLine terminator
          //unindent all buffered indents
          if (_trackIndents)
            foreach (int i in _indents)
              yield return CreateDedent(token.Location);
          _indents.Clear();
          yield return token;
          yield break;
        }//if Eof

        //Now deal with normal, non-EOF tokens
        //We intercept only content tokens on new lines  
        if (token.Terminal.Category != TokenCategory.Content || token.Location.Line == _prevLine) {
          yield return token;
          continue;
        }
        //if we are here, we have content token on new line; produce newLine token and possibly indents 
        yield return CreateNewLine(token.Location);
        _prevLine = token.Location.Line;
        if (!_trackIndents) {
          yield return token;
          continue;
        }
        //Now  take care of indents
        int currIndent = token.Location.Column;
        int prevIndent = _indents.Count == 0 ? 0 : _indents.Peek();
        if (currIndent > prevIndent) {
          _indents.Push(currIndent);
          yield return CreateIndent(token.Location);
        } else if (currIndent < prevIndent) {
          //produce one or more dedent tokens while popping indents from stack
          while (_indents.Peek() > currIndent) {
            _indents.Pop();
            yield return CreateDedent(token.Location);
          }
          if (_indents.Peek() != currIndent) {
            yield return Grammar.CreateSyntaxErrorToken (token.Location, "Invalid dedent level, no previous matching indent found.");
            //TODO: add error recovery here
          }
        }//else if currIndent < prevIndent
        yield return token;
      } //foreach token
    }//method

    [System.Diagnostics.DebuggerStepThrough]
    private Token CreateNewLine(SourceLocation location) {
      return new Token(Grammar.NewLine, location, "<LF>");
    }
    [System.Diagnostics.DebuggerStepThrough]
    private Token CreateIndent(SourceLocation location) {
      return new Token(Grammar.Indent, location, "<Indent>");
    }
    [System.Diagnostics.DebuggerStepThrough]
    private Token CreateDedent(SourceLocation location) {
      return new Token(Grammar.Dedent, location, "<Dedent>");
    }


  }//class
}

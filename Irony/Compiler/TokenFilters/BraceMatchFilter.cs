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

  #region Description
  // This filter controls matching of left/right symbol pairs in the source.
  // "Braces" are not necessarily curly braces "{' - it can be [], (), <> or any 
  // begin/end block symbol or keyword pair defined in particular language. 
  // Why do we need this filter?
  // Some languages allow different pairs of symbols to be used interchangabely. 
  // For example, Scheme allows using either () or [] symbol pairs. Ruby allows either do...end or {} 
  // as block enclosing symbols. However in both languages the Matching Rule still applies - 
  // an opening symbol must be matched by the closing symbol from the pair, not by a symbol from another pair.
  // If we try to express this in grammar, we'll have to write every production 
  // involving such symbols several times, once for each pair. This can be a real hassle. 
  // The alternative is to declare two nonTerminals, one for all opening symbols and one 
  // for all closing ones, and then use them in productions. 
  // In this case the resulting grammar (and parser) ignores the matching rule, 
  // so we must provide the match checking outside the parser. 
  // That's what this filter is doing. It is analyzing the token stream
  // produced by the scanner and validates that each closing brace matches the opening one.
  // At initialization the filter must be provided with all the brace pairs defined in the language thru
  // method AddBracePair.
  // Don't use this filter to validate "normal" pairs that never interchange - this validation 
  //  should be embedded into normal parsing process.
  #endregion
  //TODO: This must be refactored. (_stack implementation is ugly)
  public class BraceMatchFilter : TokenFilter {
    StringList _stack = new StringList();
    //We don't use dictionaries as number of symbols would be small - two or four at most,
    // so hashtables/dictionaries give no advantage
    StringList _braces = new StringList();

    public void AddPair(string left, string right) {
      _braces.Add(left);
      _braces.Add(right);
    }
    public override IEnumerable<Token> BeginFiltering(CompilerContext context, IEnumerable<Token> tokens) {
      foreach (Token token in tokens) {
        string tokenKey = token.Terminal.Key;
        int idx = _braces.IndexOf(tokenKey);
        if (idx >= 0) {
          if (idx % 2 == 0)
            //it is left brace
            _stack.Add(_braces[idx + 1]);
          else if (_stack.Count == 0)
            yield return Grammar.CreateErrorToken(token.Location, "Unexpected closing symbol - no matching opening symbol.");
          else if (_stack[_stack.Count - 1] != tokenKey) {
            //closing brace mismatch
            string expected = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1); //still remove "unmatched" symbol from stack
            yield return Grammar.CreateErrorToken(token.Location, "'" + expected + "' expected.");
            //yield "correct" closing symbol, to let the grammar continue.
            yield return new Token(SymbolTerminal.GetSymbol(expected), token.Location, expected);
          } else
            _stack.RemoveAt(_stack.Count - 1);
        }// if idx >= 0 ...
        //finally yield token
        yield return token;
      }//foreach token
      yield break;
    }


  }//class
}

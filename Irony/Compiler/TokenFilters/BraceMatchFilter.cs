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

  public class BracePair {
    public readonly Token Open;
    public readonly Token Close;
    public BracePair(Token open, Token close) {
      this.Open = open;
      this.Close = close;
    }
  }
  public class BracePairList : List<BracePair> { }


  public class BraceMatchFilter : TokenFilter {
    StringList _stack = new StringList();
    //We don't use dictionaries as number of symbols would be small - two or four at most,
    // so hashtables/dictionaries give no advantage
    private Stack<Token> _braces = new Stack<Token>();
    public BracePairList BracePairs = new BracePairList();
    public bool BuildPairsList = false; //do not build pairs list


    public override IEnumerable<Token> BeginFiltering(CompilerContext context, IEnumerable<Token> tokens) {
      foreach (Token token in tokens) {
        if (!token.Term.IsSet(TermOptions.IsBrace)) {
          yield return token;
          continue;
        }
        //open brace symbol
        if (token.Term.IsSet(TermOptions.IsOpenBrace)) {
          _braces.Push(token);
          yield return token;
          continue;
        }
        if (token.Term.IsSet(TermOptions.IsCloseBrace)) {
          Token lastOpen = _braces.Peek();
          if (_braces.Count > 0 && lastOpen.Symbol.IsPairFor == token.Symbol) { 
            //everything is ok, there's matching brace on top of the stack
            if (BuildPairsList)
              BracePairs.Add(new BracePair(lastOpen, token));
            _braces.Pop();
            yield return token; //return this token
          } else {
            yield return Grammar.CreateSyntaxErrorToken(context, token.Span.Start, 
                "Unmatched closing brace '{0}' - expected '{1}'", token.Text, lastOpen.Symbol.IsPairFor.Name);
            //TODO: add some error recovery here
          }//else
        }//if token IsCloseBrace
      }//foreach token
      yield break;
    }//method

  }//class
}

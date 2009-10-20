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

namespace Irony.Parsing {

  [Flags]
  public enum OutlineOptions {
    None = 0,
    ProduceIndents = 0x01,
    CheckBraces = 0x02,
  }
  //Adds Eos, indent, unindent tokes to scanner's output stream for languages like Python.
  // Scanner ignores new lines and indentations as whitespace; this filter produces these symbols based 
  // on col/line information in content tokens. 
  //  TODO: need to recognize line-continuation symbols ("\" in python, "_" in VB); or incomplete statement indicators
  //  that signal that line continues (if line ends with operator in Ruby, it means statement continues on 
  //  the next line).
  public class CodeOutlineFilter : TokenFilter {
    public readonly OutlineOptions Options;
    public readonly KeyTerm ContinuationTerminal; //Terminal

    public CodeOutlineFilter(OutlineOptions options) : this(options, null) { }

    public CodeOutlineFilter(OutlineOptions options, KeyTerm continuationTerminal) {
      Options = options;
      ContinuationTerminal = continuationTerminal;
    }

    public override void Init(Irony.Parsing.GrammarData grammarData) {
      base.Init(grammarData);
      //check that line continuation symbol is added to NonGrammarTerminals
      if (ContinuationTerminal != null)
        if (!grammarData.Grammar.NonGrammarTerminals.Contains(ContinuationTerminal))
          grammarData.Language.Errors.Add(GrammarErrorLevel.Warning, null,
            "CodeOutlineFilter: line continuation symbol '{0}' should be added to Grammar.NonGrammarTerminals list.",
            ContinuationTerminal.Name);
    }

    public override IEnumerable<Token> BeginFiltering(ParsingContext context, IEnumerable<Token> tokens) {
      var processor = new OutlineProcessor(this, context, this.GrammarData); 
      foreach (Token token in tokens) {
        processor.ProcessToken(token);
        while(processor.OutputTokens.Count > 0)
          yield return processor.OutputTokens.Pop();
      }//foreach
    }//method

    public bool OptionIsSet(OutlineOptions option) {
      return (Options & option) != 0;
    }
  }//class


  //In multi-threaded processing a grammar instance (and token filters) might be used by several parsers on different
  // threads. Therefore all data related to parsing a sinlge file should NOT be kept in Filter's fields. 
  // We use the following class to keep the data. It's instance is created in Filter.BeginFiltering method and is kept
  // through processing of the entire file. Alternatively we could use local variables in BeginFiltering method
  // but this would increase the load on .NET iterator's save/restore routine. With a single separate object it is a single 
  // local field (object ref) that should be saved/restored between the iterations.
  internal class OutlineProcessor {
    CodeOutlineFilter _filter;
    ParsingContext _context;
    GrammarData _grammarData;
    Grammar _grammar;
    bool _produceIndents;
    bool _checkBraces; 

    public Stack<int> Indents = new Stack<int>();
    public Token CurrentToken;
    public Token PreviousToken;
    public SourceLocation PreviousTokenLocation;
    public TokenStack OutputTokens = new TokenStack();
    private bool _isContinuation, _prevIsContinuation; 

    internal OutlineProcessor(CodeOutlineFilter filter, ParsingContext context, GrammarData grammarData) {
      _filter = filter;
      _context = context;
      _grammarData = grammarData;
      _grammar = _grammarData.Grammar;
      _produceIndents = _filter.OptionIsSet(OutlineOptions.ProduceIndents);
      _checkBraces = _filter.OptionIsSet(OutlineOptions.CheckBraces);
      Indents.Push(0); 
    }


    private void SetCurrentToken(Token token) {
      //Copy CurrentToken to PreviousToken
      if (CurrentToken != null && CurrentToken.Category == TokenCategory.Content) { //remember only content tokens
        PreviousToken = CurrentToken;
        _prevIsContinuation = _isContinuation;
        if (PreviousToken != null)
          PreviousTokenLocation = PreviousToken.Location;
      }
      CurrentToken = token;
      _isContinuation = (token.Terminal == _filter.ContinuationTerminal && _filter.ContinuationTerminal != null);
      if (!_isContinuation)
        OutputTokens.Push(token); //by default input token goes to output, except continuation symbol
    }

    public void ProcessToken(Token token) {
      SetCurrentToken(token);
      //Quick checks
      if (_isContinuation)
        return;
      // if it is content token on the same line, then return, nothing to do 
      if (token.Category == TokenCategory.Content && token.Location.Line == PreviousTokenLocation.Line)
        return;
      // if it is not Content (for ex. comment), but not EOF - then return
      if (token.Category != TokenCategory.Content && token.Terminal != _grammar.Eof)
        return;
      //check EOF
      if (CurrentToken.Terminal == _grammar.Eof) {
        ProcessEofToken();
        return;
      }
      //if we are here, we have content token on new line; 
      // first check if there was continuation symbol before
      // or - if checkBraces flag is set - check if there were open braces
      if (_prevIsContinuation || _checkBraces && _context.Parser.CoreParser.OpenBraces.Count > 0)
        return; //no Eos token in this case
      
      //We need to produce Eos token and indents (if _produceIndents is set). 
      // First check indents - they go first into OutputTokens stack, so they will be popped out last
      if (_produceIndents) {
        var currIndent = token.Location.Column;
        var prevIndent = Indents.Peek();
        if (currIndent > prevIndent) {
          Indents.Push(currIndent); 
          PushOutlineToken(_grammar.Indent, token.Location);
        } else if (currIndent < prevIndent) {
          PushDedents(currIndent); 
          //check that current indent exactly matches the previous indent 
          if (Indents.Peek() != currIndent) {
            //fire error
            OutputTokens.Push(new Token(_grammar.SyntaxError, token.Location, string.Empty, 
              "Invalid dedent level, no previous matching indent found."));
          }
        }
      }//if _produceIndents
      //Finally produce Eos token
      var eosLocation = ComputeEosLocation();
      PushOutlineToken(_grammar.Eos, eosLocation);

    }//method

    //Remember that tokens will be popped of the stack and delivered to parser in opposite order compared to 
    // the order we pushed them into OutputTokens stack
    private void ProcessEofToken() {
      //first unindent all buffered indents
      if (_produceIndents)
        PushDedents(0); 
      //now push Eos token - it will be popped first, then unindents, then EOF token
      var eosLocation = ComputeEosLocation(); 
      PushOutlineToken(_grammar.Eos, eosLocation);
    }

    private void PushDedents(int untilPosition) {
      while (Indents.Peek() > untilPosition) {
        Indents.Pop();
        PushOutlineToken(_grammar.Dedent, CurrentToken.Location);
      }
    }

    private SourceLocation ComputeEosLocation() {
      if (PreviousToken == null)
        return new SourceLocation();
      //Return position at the end of previous token
      var loc = PreviousToken.Location;
      var len = PreviousToken.Length;
      return new SourceLocation(loc.Position + len, loc.Line, loc.Column + len);
    }

    private void PushOutlineToken(Terminal term, SourceLocation location) {
      OutputTokens.Push(new Token(term, location, string.Empty, null));
    }
    
  }//class



}//namespace

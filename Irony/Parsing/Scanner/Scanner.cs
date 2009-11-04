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
using System.Globalization;
using System.Text;

namespace Irony.Parsing {

  //Scanner class. The Scanner's function is to transform a stream of characters into aggregates/words or lexemes, 
  // like identifier, number, literal, etc. 

  public class Scanner  {
    #region Properties and Fields: Data, _source
    public readonly ScannerData Data;
    public readonly Parser Parser;
    Grammar _grammar;
    //buffered tokens can come from expanding a multi-token, when Terminal.TryMatch() returns several tokens packed into one token

    private ParsingContext Context {
      get { return Parser.Context; }
    }
    #endregion

    public Scanner(Parser parser) {
      Parser = parser; 
      Data = parser.Language.ScannerData;
      _grammar = parser.Language.Grammar;
      Context.SourceStream = new SourceStream(this.Data, Context.TabWidth);
      //create token streams
      var tokenStream = GetUnfilteredTokens();
      //chain all token filters
      Context.TokenFilters.Clear();
      _grammar.CreateTokenFilters(Data.Language, Context.TokenFilters);
      foreach (TokenFilter filter in Context.TokenFilters) {
        tokenStream = filter.BeginFiltering(Context, tokenStream);
      }
      Context.FilteredTokens = tokenStream.GetEnumerator();
    }

    internal void OnStatusChanged(ParserStatus oldStatus) {
    }

    public Token GetToken() {
      //get new token from pipeline
      if (!Context.FilteredTokens.MoveNext()) return null;
      var token = Context.FilteredTokens.Current;
      if (Context.Status == ParserStatus.Previewing)
        Context.PreviewTokens.Push(token);
      else 
        Context.CurrentParseTree.Tokens.Add(token);
      return token;
    }

    //This is iterator method, so it returns immediately when called directly
    // returns unfiltered, "raw" token stream
    private IEnumerable<Token> GetUnfilteredTokens() {
      //We don't do "while(!_source.EOF())... because on EOF() we need to continue and produce EOF token 
      while (true) {  
        Context.CurrentScannerToken = FetchToken();
        Context.OnTokenCreated(Context.CurrentScannerToken);
        yield return Context.CurrentScannerToken;
        //Don't yield break, continue returning EOF
      }//while
    }// method

    #region VS Integration methods
    //Use this method for VS integration; VS language package requires scanner that returns tokens one-by-one. 
    // Start and End positions required by this scanner may be derived from Token : 
    //   start=token.Location.Position; end=start + token.Length;
    public Token VsReadToken(ref int state) {
      Context.VsLineScanState.Value = state;
      if (Context.SourceStream.EOF()) return null;
      
      Token result;
      if (state == 0)
        result = FetchToken();
      else {
        Terminal term = Data.MultilineTerminals[Context.VsLineScanState.TerminalIndex - 1];
        result = term.TryMatch(Context, Context.SourceStream); 
      }
      //set state value from context
      state = Context.VsLineScanState.Value;
      if (result != null && result.Terminal == _grammar.Eof)
        result = null; 
      return result;
    }
    public void VsSetSource(string text, int offset) {
      Context.SourceStream.SetText(text, offset, true);
    }
    #endregion

    #region Fetching tokens
    private Token FetchToken() {
      //1. Check if there are buffered tokens
      if (Context.BufferedTokens.Count > 0) 
        return Context.BufferedTokens.Pop(); 
      //2. Skip whitespace. We don't need to check for EOF: at EOF we start getting 0-char, so we'll get out automatically
      while (_grammar.WhitespaceChars.IndexOf(Context.SourceStream.PreviewChar) >= 0)
        Context.SourceStream.PreviewPosition++;
      //3. That's the token start, calc location (line and column)
      Context.SourceStream.MoveLocationToPreviewPosition();
      //4. Check for EOF
      if (Context.SourceStream.EOF()) {
        CreateFinalTokensInBuffer(); //puts Eof and optionally final NewLine tokens into buffer
        return Context.BufferedTokens.Pop();
      }
      //5. Actually scan the source text and construct a new token
      return ScanToken(); 
    }//method

    //Scans the source text and constructs a new token
    private Token ScanToken() {
      //Find matching terminal
      // First, try terminals with explicit "first-char" prefixes, selected by current char in source
      var terms = SelectTerminals(Context.SourceStream.PreviewChar);
      var token = MatchTerminals(terms);
      //If no token, try FallbackTerminals
      if (token == null && terms != Data.FallbackTerminals && Data.FallbackTerminals.Count > 0)
        token = MatchTerminals(Data.FallbackTerminals);
      //If we don't have a token from registered terminals, try Grammar's method
      if (token == null)
        token = _grammar.TryMatch(Context, Context.SourceStream);
      if (token is MultiToken)
        token = UnpackMultiToken(token);
      //If we have normal token then return it
      if (token != null && !token.IsError()) {
        //set position to point after the result token
        Context.SourceStream.PreviewPosition = Context.SourceStream.Location.Position + token.Length;
        Context.SourceStream.MoveLocationToPreviewPosition();
        return token;
      }
      //we have an error: either error token or no token at all
      if (token == null)  //if no token then create error token
        token = Context.SourceStream.CreateErrorToken("Invalid character: '{0}'", Context.SourceStream.PreviewChar);
      Recover();
      return token;
    }

    //If token is MultiToken then push all its child tokens into _bufferdTokens and return the first token in buffer
    private Token UnpackMultiToken(Token token) {
      var mtoken = token as MultiToken;
      if (mtoken == null) return null; 
      for (int i = mtoken.ChildTokens.Count-1; i >= 0; i--)
        Context.BufferedTokens.Push(mtoken.ChildTokens[i]);
      return Context.BufferedTokens.Pop();
    }
    
    //creates final NewLine token (if necessary) and EOF token and puts them into _bufferedTokens
    private void CreateFinalTokensInBuffer() {
      //check if we need extra newline before EOF
      bool currentIsNewLine = Context.CurrentScannerToken != null && Context.CurrentScannerToken.Terminal == _grammar.NewLine;
      var eofToken = new Token(_grammar.Eof, Context.SourceStream.Location, string.Empty, _grammar.Eof.Name);
      Context.BufferedTokens.Push(eofToken); //put it into buffer
      if (_grammar.FlagIsSet(LanguageFlags.NewLineBeforeEOF) && !currentIsNewLine) {
        var newLineToken = new Token(_grammar.NewLine, Context.CurrentScannerToken.Location, "\n", null);
        Context.BufferedTokens.Push(newLineToken); 
      }//if
    }

    private Token MatchTerminals(TerminalList terminals) {
      Token result = null;
      foreach (Terminal term in terminals) {
        // Check if the term has lower priority that result token we already have; 
        //  if term.Priority is lower then we don't need to check anymore, higher priority wins
        // Note that terminals in the list are sorted in descending priority order
        if (result != null && result.Terminal.Priority > term.Priority)
          break;
        //Reset source position and try to match
        Context.SourceStream.PreviewPosition = Context.SourceStream.Location.Position;
        Token token = term.TryMatch(Context, Context.SourceStream);
        if (token != null)
          token = term.InvokeValidateToken(Context, Context.SourceStream, terminals, token); 
        //Take this token as result only if we don't have anything yet, or if it is longer token than previous
        if (token != null && (token.IsError() || result == null || token.Length > result.Length))
          result = token;
        if (result != null && result.IsError()) break;
      }
      return result; 
    }

    //list for filterered terminals
    private TerminalList _filteredTerminals = new TerminalList();
    //reuse single instance to avoid garbage generation
    private SelectTerminalArgs _selectedTerminalArgs = new SelectTerminalArgs(); 
    
    private TerminalList SelectTerminals(char current) {
      TerminalList termList;
      if (!_grammar.CaseSensitive)
        current = char.ToLower(current, CultureInfo.InvariantCulture);
      if (!Data.TerminalsLookup.TryGetValue(current, out termList))
        termList = Data.FallbackTerminals;
      if (termList.Count <= 1)  return termList;

      //We have more than one candidate
      //First try calling grammar method
      _selectedTerminalArgs.SetData(Context, current, termList); 
      _grammar.OnScannerSelectTerminal(_selectedTerminalArgs);
      if (_selectedTerminalArgs.SelectedTerminal != null) {
        _filteredTerminals.Clear();
        _filteredTerminals.Add(_selectedTerminalArgs.SelectedTerminal);
        return _filteredTerminals;
      }
      // Now try filter them by checking with parser which terms it expects but do it only if we're not recovering or previewing
      if (Context.Status == ParserStatus.Recovering || Context.Status == ParserStatus.Previewing)
        return termList;
      var parserState = Context.CurrentParserState;
      if (parserState == null) 
        return termList;
      //we cannot modify termList - it will corrupt the list in TerminalsLookup table; we make a copy
      _filteredTerminals.Clear();
      foreach(var term in termList) {
        if (parserState.ExpectedTerms.Contains(term) || _grammar.NonGrammarTerminals.Contains(term))
          _filteredTerminals.Add(term);
      }
      //Now, if filtered list is empty then ran into error. Don't report it as scanner error - scanner still has options, 
      // let parser report it
      if (_filteredTerminals.Count == 0)
        return termList;
      else 
        return _filteredTerminals;
    }//Select
    #endregion

    #region Error recovery
    private bool Recover() {
      Context.SourceStream.PreviewPosition++;
      while (!Context.SourceStream.EOF()) {
        if(Data.ScannerRecoverySymbols.IndexOf(Context.SourceStream.PreviewChar) >= 0) {
          Context.SourceStream.MoveLocationToPreviewPosition();
          return true;
        }
        Context.SourceStream.PreviewPosition++;
      }
      return false; 
    }
    #endregion 

    #region TokenPreview
    //Preview mode allows custom code in grammar to help parser decide on appropriate action in case of conflict
    // Preview process is simply searching for particular tokens in "preview set", and finding out which of the 
    // tokens will come first.
    // In preview mode, tokens returned by FetchToken are collected in _previewTokens list; after finishing preview
    //  the scanner "rolls back" to original position - either by directly restoring the position, or moving the preview
    //  tokens into _bufferedTokens list, so that they will read again by parser in normal mode.
    // See c# grammar sample for an example of using preview methods
    SourceLocation _previewStartLocation;

    //Switches Scanner into preview mode
    public void BeginPreview() {
      Context.Status = ParserStatus.Previewing;
      _previewStartLocation = Context.SourceStream.Location;
      Context.PreviewTokens.Clear();
    }

    //Ends preview mode
    public void EndPreview(bool keepPreviewTokens) {
      if (keepPreviewTokens) {
        //insert previewed tokens into buffered list, so we don't recreate them again
        while (Context.PreviewTokens.Count > 0)
          Context.BufferedTokens.Push(Context.PreviewTokens.Pop()); 
      }  else
        SetSourceLocation(_previewStartLocation);
      Context.PreviewTokens.Clear();
      Context.Status = ParserStatus.Parsing;
    }
    #endregion

    public void SetSourceLocation(SourceLocation location) {
      /*
       * foreach (var filter in _grammar.TokenFilters)
        filter.OnSetSourceLocation(location); 
      _source.Location = location;
    
       */
    }

  }//class

}//namespace

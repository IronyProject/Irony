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
using System.Collections;
using System.Diagnostics;


namespace Irony.Parsing { 
  // CoreParser class implements LALR parser automaton. Its behavior is controlled by the state transition graph
  // with root in Data.InitialState. Each state contains a dictionary of parser actions indexed by input 
  // element (terminal or non-terminal). 
  public partial class CoreParser {

    #region Constructors
    public CoreParser(Parser parser) {
      Parser = parser; 
      Data = parser.Language.ParserData;
      _grammar = Data.Language.Grammar;
    }
    #endregion

    #region Properties and fields: Parser, Data, _grammar
    public readonly Parser Parser;
    public readonly ParserData Data;
    Grammar _grammar;

    private ParsingContext Context {
      get { return Parser.Context; }
    }
    #endregion

    internal void Reset() {
    }

    #region Parse method
    public void Parse() {
      //main loop
      Context.Status = ParserStatus.Parsing;
      while(Context.Status == ParserStatus.Parsing) {
        ExecuteAction();
      } 
    }//Parse

    #endregion

    #region reading input
    public void ReadInput() {
      var token = GetScannerToken();
      if (token.Terminal.Flags.IsSet(TermFlags.IsBrace))
        token = CheckBraceToken(token);
      Context.CurrentParserInput = new ParseTreeNode(token);
      //attach comments if any accumulated to content token
      if (Context.CurrentCommentBlock != null && token.Terminal.Category == TokenCategory.Content) { 
        Context.CurrentParserInput.Comments = Context.CurrentCommentBlock;
        Context.CurrentCommentBlock = null;
      }
    }

    // Reads token from scanner, skips all non-grammar tokens but accumulates comment tokens in CurrentCommentBlock
    private Token GetScannerToken() {
      Token token;
      Terminal term;
      do {
        token = Parser.Scanner.GetToken();
        term = token.Terminal; 
        if (term.Category == TokenCategory.Comment) {
          if (Context.CurrentCommentBlock == null) 
            Context.CurrentCommentBlock = new CommentBlock();
          Context.CurrentCommentBlock.Tokens.Add(token);
        }
      } while (term.Flags.IsSet(TermFlags.IsNonGrammar) && term != _grammar.Eof);
      return token; 
    }

    private void CheckNewToken(Token token) {
      switch (token.Category) {
        case TokenCategory.Comment:
          if (Context.CurrentCommentBlock == null) 
            Context.CurrentCommentBlock = new CommentBlock();
          Context.CurrentCommentBlock.Tokens.Add(token);
          break; 
        case TokenCategory.Content:
          break; 
      }

    }
    #endregion

    #region execute actions
    private void ExecuteAction() {
      //Read input only if DefaultReduceAction is null - in this case the state does not contain ExpectedSet,
      // so parser cannot assist scanner when it needs to select terminal and therefore can fail
      if (Context.CurrentParserInput == null && Context.CurrentParserState.DefaultReduceAction == null)
        ReadInput();
      //Check scanner error
      if (Context.CurrentParserInput != null && Context.CurrentParserInput.IsError) {
        this.Data.ErrorAction.Execute(Context);
        return;
      }
      //Try getting action
      var action = GetCurrentAction();
      if (action == null) {
        if (CheckPartialInputCompleted()) return;
        this.Data.ErrorAction.Execute(Context);
        return;
      }    
      //We have action. Write trace and execute it
      if (Context.TracingEnabled) 
        Context.AddTrace(action.ToString());
      action.Execute(Context);
    }

    private bool CheckPartialInputCompleted() {
      bool partialCompleted = (Context.Mode == ParseMode.CommandLine && Context.CurrentParserInput.Term == _grammar.Eof);
      if (!partialCompleted) return false;
      Context.Status = ParserStatus.AcceptedPartial;
      // clean up EOF in input so we can continue parsing next line
      Context.CurrentParserInput = null;
      return true;
    }


    public ParserAction GetCurrentAction() {
      var currState = Context.CurrentParserState;
      var currInput = Context.CurrentParserInput;

      if (currState.DefaultReduceAction != null)
        return currState.DefaultReduceAction;
      ParserAction action;
      //First try as keyterm/key symbol; for example if token text = "while", then first try it as a keyword "while";
      // if this does not work, try as an identifier that happens to match a keyword but is in fact identifier
      Token inputToken = currInput.Token;
      if (inputToken != null && inputToken.KeyTerm != null) {
        var keyTerm = inputToken.KeyTerm;
        if (currState.Actions.TryGetValue(keyTerm, out action)) {
          #region comments
          // Ok, we found match as a key term (keyword or special symbol)
          // Backpatch the token's term. For example in most cases keywords would be recognized as Identifiers by Scanner.
          // Identifier would also check with SymbolTerms table and set AsSymbol field to SymbolTerminal if there exist
          // one for token content. So we first find action by Symbol if there is one; if we find action, then we 
          // patch token's main terminal to AsSymbol value.  This is important for recognizing keywords (for colorizing), 
          // and for operator precedence algorithm to work when grammar uses operators like "AND", "OR", etc. 
          //TODO: This might be not quite correct action, and we can run into trouble with some languages that have keywords that 
          // are not reserved words. But proper implementation would require substantial addition to parser code: 
          // when running into errors, we need to check the stack for places where we made this "interpret as Symbol"
          // decision, roll back the stack and try to reinterpret as identifier
          #endregion
          inputToken.SetTerminal(keyTerm);
          currInput.Term = keyTerm;
          currInput.Precedence = keyTerm.Precedence;
          currInput.Associativity = keyTerm.Associativity;
          return action;
        }
      }
      //Try to get by main Terminal, only if it is not the same as symbol
      if (currState.Actions.TryGetValue(currInput.Term, out action))
        return action;
      //If input is EOF and NewLineBeforeEof flag is set, try using NewLine to find action
      if (currInput.Term == _grammar.Eof && _grammar.LanguageFlags.IsSet(LanguageFlags.NewLineBeforeEOF) &&
          currState.Actions.TryGetValue(_grammar.NewLine, out action)) {
        //There's no action for EOF but there's action for NewLine. Let's add newLine token as input, just in case
        // action code wants to check input - it should see NewLine.
            var newLineToken = new Token(_grammar.NewLine, currInput.Token.Location, "\r\n", null);
        var newLineNode = new ParseTreeNode(newLineToken);
        Context.CurrentParserInput = newLineNode;
        return action; 
      }//if
      return null;
    }

    #endregion

    #region Braces handling
    private Token CheckBraceToken(Token token) {
      if (token.Terminal.Flags.IsSet(TermFlags.IsOpenBrace)) {
        Context.OpenBraces.Push(token);
        return token;
      }
      //it is closing brace; check if we have opening brace
      var braces = Context.OpenBraces; 
      var mismatch = (braces.Count == 0 || braces.Peek().Terminal.IsPairFor != token.Terminal);
      if (mismatch)
        return new Token(_grammar.SyntaxError, token.Location, token.Text,
            string.Format(Resources.ErrUnmatchedCloseBrace, token.Text));

      //Link both tokens, pop the stack and return true
      var openingBrace = braces.Pop();
      openingBrace.OtherBrace = token;
      token.OtherBrace = openingBrace; 
      return token;
    }
    #endregion

  }//class



}//namespace

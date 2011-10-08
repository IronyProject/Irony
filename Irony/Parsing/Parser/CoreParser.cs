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
    bool _traceEnabled; 

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
    private void ReadInput() {
      if (Context.ParserInputStack.Count > 0)
        Context.ParserInputStack.Pop(); 
      if (Context.ParserInputStack.Count > 0) {
        Context.CurrentParserInput = Context.ParserInputStack.Top;
        return; 
      }
      FetchToken();
    }

    private void FetchToken() {
      var token = GetScannerToken();
      if (token.Terminal.Flags.IsSet(TermFlags.IsBrace))
        token = CheckBraceToken(token);
      Context.CurrentParserInput = new ParseTreeNode(token);
      //attach comments if any accumulated to content token
      if (Context.CurrentCommentBlock != null && token.Terminal.Category == TokenCategory.Content) { 
        Context.CurrentParserInput.Comments = Context.CurrentCommentBlock;
        Context.CurrentCommentBlock = null;
      }
      Context.ParserInputStack.Push(Context.CurrentParserInput);
    }

    // Reads token from scanner, skips all non-grammar tokens but accumulates comment tokens in CurrentCommentBlock
    private Token GetScannerToken() {
      Token token;
      Terminal term;
      do {
        token = Parser.Scanner.GetToken();
        term = token.Terminal; 
        if (term.Category == TokenCategory.Comment) {
          if (Context.CurrentCommentBlock == null) Context.CurrentCommentBlock = new CommentBlock();
          Context.CurrentCommentBlock.Tokens.Add(token);
        }
      } while (term.Flags.IsSet(TermFlags.IsNonGrammar) && term != _grammar.Eof);
      return token; 
    }
    #endregion

    #region execute actions
    private void ExecuteAction() {
      _traceEnabled = Context.Options.IsSet(ParseOptions.TraceParser);
      //Read input only if DefaultReduceAction is null - in this case the state does not contain ExpectedSet,
      // so parser cannot assist scanner when it needs to select terminal and therefore can fail
      if (Context.CurrentParserInput == null && Context.CurrentParserState.DefaultAction == null)
        ReadInput();
      //Check scanner error
      if (Context.CurrentParserInput != null && Context.CurrentParserInput.IsError) {
        ProcessParserError();
        return;
      }
      //Try getting action
      var action = FindActionForStateAndInput();
      if (action == null) {
        if (CheckPartialInputCompleted())
          return;
        ProcessParserError();
        return;
      }    
      //We have action. First, write trace
      if (_traceEnabled) Context.AddTrace("{0}", action);
      //Execute it
      switch (action.ActionType) {
        case ParserActionType.Shift: ExecuteShift(action); break;
        case ParserActionType.Operator: ExecuteOperatorAction(action); break;
        case ParserActionType.Reduce: ExecuteReduce(action); break;
        case ParserActionType.Code: ExecuteConflictAction(action); break;
        case ParserActionType.Accept: ExecuteAccept(action); break; 
      }
    }

    private bool CheckPartialInputCompleted() {
      bool partialCompleted = (Context.Mode == ParseMode.CommandLine && Context.CurrentParserInput.Term == _grammar.Eof);
      if (!partialCompleted) return false;
      Context.Status = ParserStatus.AcceptedPartial;
      // clean up EOF in input so we can continue parsing next line
      Context.CurrentParserInput = null;
      Context.ParserInputStack.Pop();
      return true;
    }


    private ParserAction FindActionForStateAndInput() {
      if (Context.CurrentParserState.DefaultAction != null)
        return Context.CurrentParserState.DefaultAction;
      ParserAction action;
      //First try as keyterm/key symbol; for example if token text = "while", then first try it as a keyword "while";
      // if this does not work, try as an identifier that happens to match a keyword but is in fact identifier
      Token inputToken = Context.CurrentParserInput.Token;
      if (inputToken != null && inputToken.KeyTerm != null) {
        var keyTerm = inputToken.KeyTerm;
        if (Context.CurrentParserState.Actions.TryGetValue(keyTerm, out action)) {
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
          Context.CurrentParserInput.Term = keyTerm;
          Context.CurrentParserInput.Precedence = keyTerm.Precedence;
          Context.CurrentParserInput.Associativity = keyTerm.Associativity;
          return action;
        }
      }
      //Try to get by main Terminal, only if it is not the same as symbol
      if (Context.CurrentParserState.Actions.TryGetValue(Context.CurrentParserInput.Term, out action))
        return action;
      //If input is EOF and NewLineBeforeEof flag is set, try injecting NewLine into input
      if (Context.CurrentParserInput.Term == _grammar.Eof && _grammar.LanguageFlags.IsSet(LanguageFlags.NewLineBeforeEOF) &&
          Context.CurrentParserState.Actions.TryGetValue(_grammar.NewLine, out action)) {
        InjectNewLineToken(); 
        return action; 
      }//if
      return null;
    }

    private void InjectNewLineToken() {
      var newLineToken = new Token(_grammar.NewLine, Context.CurrentParserInput.Token.Location, "\r\n", null);
      var newLineNode = new ParseTreeNode(newLineToken); 
      Context.ParserInputStack.Push(newLineNode);
      Context.CurrentParserInput = newLineNode; 
    }


    private void ExecuteShift(ParserAction action) {
      Context.ParserStack.Push(Context.CurrentParserInput, action.NewState);
      Context.CurrentParserState = action.NewState;
      Context.CurrentParserInput = null; 
      if (action.NewState.DefaultAction == null) //read only if new state is NOT single-reduce state
        ReadInput(); 
    }

    #region ExecuteReduce
    private void ExecuteReduce(ParserAction action) {
      var reduceProduction = action.ReduceProduction; 
      ParseTreeNode resultNode; 
      if(reduceProduction.IsSet(ProductionFlags.IsListBuilder)) 
        resultNode = ReduceExistingList(action);
      else if(reduceProduction.LValue.Flags.IsSet(TermFlags.IsListContainer)) 
        resultNode = ReduceListContainer(action);
      else if (reduceProduction.LValue.Flags.IsSet(TermFlags.IsTransient))
        resultNode = ReduceTransientNonTerminal(action);
      else 
        resultNode = ReduceRegularNode(action);
      //final reduce actions ----------------------------------------------------------
      Context.ParserStack.Pop(reduceProduction.RValues.Count);
      //Push new node into stack and move to new state
      //First read the state from top of the stack 
      Context.CurrentParserState = Context.ParserStack.Top.State;
      if (_traceEnabled) Context.AddTrace(Resources.MsgTracePoppedState, reduceProduction.LValue.Name);
      // Shift to new state (LALR) - execute shift over non-terminal
      var shift = Context.CurrentParserState.Actions[reduceProduction.LValue];
      Context.ParserStack.Push(resultNode, shift.NewState);
      Context.CurrentParserState = shift.NewState;
      //Copy comment block from first child; if comments precede child node, they precede the parent as well. 
      if (resultNode.ChildNodes.Count > 0)
        resultNode.Comments = resultNode.ChildNodes[0].Comments;
      //Invoke event
      reduceProduction.LValue.OnReduced(Context, reduceProduction, resultNode);
    }

    private ParseTreeNode ReduceExistingList(ParserAction action) {
      int childCount = action.ReduceProduction.RValues.Count;
      int firstChildIndex = Context.ParserStack.Count - childCount;
      var listNode = Context.ParserStack[firstChildIndex]; //get the list already created - it is the first child node
      listNode.Span = ComputeNewNodeSpan(childCount);
      var listMember = Context.ParserStack.Top; //next list member is the last child - at the top of the stack
      if (ShouldSkipChildNode(listMember))
        return listNode; 
      CheckCreateAstNode(listMember);  
      listNode.ChildNodes.Add(listMember);
      return listNode; 
    }

    // Skip punctuation and empty transient nodes
    private bool ShouldSkipChildNode(ParseTreeNode childNode) {
      if (childNode.Term.Flags.IsSet(TermFlags.IsPunctuation))
        return true; 
      if (childNode.Term.Flags.IsSet(TermFlags.IsTransient) && childNode.ChildNodes.Count == 0)
        return true; 
      return false; 
    }

    //List container is created by MakePlusRule, MakeStarRule with allowTrailingDelimiter = true 
    // it is a special case for parser. The "real" list in grammar is the "container", but list members had been accumulated under 
    //  the transient "plus-list" which is a child of this container. So we need to copy all "grandchildren" from child to parent.
    private ParseTreeNode ReduceListContainer(ParserAction action) {
      int childCount = action.ReduceProduction.RValues.Count;
      int firstChildIndex = Context.ParserStack.Count - childCount;
      var span = ComputeNewNodeSpan(childCount);
      var newNode = new ParseTreeNode(action.ReduceProduction, span);
      if(childCount > 0) { //if it is not empty production - might happen for MakeStarRule
        var listNode = Context.ParserStack[firstChildIndex]; //get the transient list with all members - it is the first child node
        newNode.ChildNodes.AddRange(listNode.ChildNodes);    //copy all list members
      }
      return newNode; 
    }

    private ParseTreeNode ReduceTransientNonTerminal(ParserAction action) {
      var topIndex = Context.ParserStack.Count - 1; 
      var childCount = action.ReduceProduction.RValues.Count;
      for(int i = 0; i < childCount; i++) {
        var child = Context.ParserStack[topIndex - i];
        if (ShouldSkipChildNode(child)) continue; 
        CheckCreateAstNode(child);
        return child; 
      }
      //Otherwise return an empty transient node; if it is part of the list, the list will skip it
      var span = ComputeNewNodeSpan(childCount);
      return new ParseTreeNode(action.ReduceProduction, span); 
    }

    // Note that we create AST nodes 
    private ParseTreeNode ReduceRegularNode(ParserAction action) {
      var childCount = action.ReduceProduction.RValues.Count; 
      int firstChildIndex = Context.ParserStack.Count - childCount;
      var span = ComputeNewNodeSpan(childCount);
      var newNode = new ParseTreeNode(action.ReduceProduction, span);
      var newIsOp = newNode.Term.Flags.IsSet(TermFlags.IsOperator); 
      for(int i = 0; i < childCount; i++) {
        var childNode = Context.ParserStack[firstChildIndex + i];
        if(ShouldSkipChildNode(childNode))
          continue; //skip punctuation or empty transient nodes
        //AST nodes are created when we pop the (child) parse node from the stack, not when we push it into the stack. 
        //  See more in comments to CheckCreateAstNode method
        CheckCreateAstNode(childNode); 
        //Inherit precedence and associativity, to cover a standard case: BinOp->+|-|*|/; 
        // BinOp node should inherit precedence from underlying operator symbol. Keep in mind special case of SQL operator "NOT LIKE" which consists
        // of 2 tokens. We therefore inherit "max" precedence from any children
        if(newIsOp && childNode.Precedence != BnfTerm.NoPrecedence && childNode.Precedence > newNode.Precedence) {
          newNode.Precedence = childNode.Precedence;
          newNode.Associativity = childNode.Associativity;
        }
        newNode.ChildNodes.Add(childNode);
      }//for i
      return newNode;     
    }

    private SourceSpan ComputeNewNodeSpan(int childCount) {
      if(childCount == 0)
        return new SourceSpan(Context.CurrentParserInput.Span.Location, 0);
      var first = Context.ParserStack[Context.ParserStack.Count - childCount];
      var last = Context.ParserStack.Top;
      return new SourceSpan(first.Span.Location, last.Span.EndPosition - first.Span.Location.Position);
    }

    //Note that we create AST objects for parse nodes only when we pop the node from the stack (when it is a child being added to to its parent). 
    // So only when we form a parent node, we run thru children in the stack top and check/create their AST nodes.
    // This is done to provide correct initialization of List nodes (created with Plus or Star operation). 
    // We create a parse tree node for a list non-terminal very early, when we encounter its first element. We push the newly created list node into
    // the stack. At this moment it is too early to create the AST node for the list. We should wait until all child nodes are parsed and accumulated
    // in the stack. Only then, when list construction is finished, we can create AST node and provide it with all list elements.  
    private void CheckCreateAstNode(ParseTreeNode parseNode) {
      try {
        //Check preconditions
        if (!_grammar.LanguageFlags.IsSet(LanguageFlags.CreateAst))
          return; 
        if (parseNode.AstNode != null || parseNode.Term.Flags.IsSet(TermFlags.IsTransient) 
            || parseNode.Term.Flags.IsSet(TermFlags.NoAstNode)) return;  
        if (Context.Status != ParserStatus.Parsing || Context.HasErrors) return; 
        //Prepare mapped child node list
        CheckCreateMappedChildNodeList(parseNode); 
        //Actually create node
        _grammar.CreateAstNode(Context, parseNode);
        if (parseNode.AstNode != null)
          parseNode.Term.OnAstNodeCreated(parseNode);
      } catch (Exception ex) {
        Context.AddParserMessage(ParserErrorLevel.Error, parseNode.Span.Location, Resources.ErrFailedCreateNode, parseNode.Term.Name, ex.Message); 
      }
    }

    private bool CheckCreateMappedChildNodeList(ParseTreeNode parseTreeNode) {
      var term = parseTreeNode.Term;
      if (term.AstPartsMap == null) return false; 
      parseTreeNode.MappedChildNodes = new ParseTreeNodeList();
      foreach (var index in term.AstPartsMap)
        parseTreeNode.MappedChildNodes.Add(parseTreeNode.ChildNodes[index]);
      return true; 
    }

    #endregion

    private void ExecuteConflictAction(ParserAction action) {
      var args = action.ResolveConflict(_grammar, Context);
      switch(args.Result) {
        case ParserActionType.Reduce:
          ExecuteReduce(new ParserAction(ParserActionType.Reduce, null, args.ReduceProduction));
          break; 
        case ParserActionType.Operator:
          ExecuteOperatorAction(new ParserAction(ParserActionType.Operator, action.NewState, args.ReduceProduction));
          break;
        case ParserActionType.Shift:
        default:
          ExecuteShift(action); 
          break; 
      }
      if (_traceEnabled) Context.AddTrace(Resources.MsgTraceConflictResolved);
    }

    private void ExecuteAccept(ParserAction action) {
      var root = Context.ParserStack.Pop(); //Pop root
      CheckCreateAstNode(root); 
      Context.CurrentParseTree.Root = root;
      Context.Status = ParserStatus.Accepted;
    }

    private void ExecuteOperatorAction(ParserAction action) {
      var realActionType = GetActionTypeForOperation(action);
      if (_traceEnabled) Context.AddTrace(Resources.MsgTraceOpResolved, realActionType);
      switch (realActionType) {
        case ParserActionType.Shift: ExecuteShift(action); break;
        case ParserActionType.Reduce: ExecuteReduce(action); break; 
      }//switch
    }

    private ParserActionType GetActionTypeForOperation(ParserAction action) {
      for (int i = Context.ParserStack.Count - 1; i >= 0; i--) {
        var  prevNode = Context.ParserStack[i];
        if (prevNode == null) continue; 
        if (prevNode.Precedence == BnfTerm.NoPrecedence) continue;
        ParserActionType result;
        //if previous operator has the same precedence then use associativity
        var input = Context.CurrentParserInput;
        if (prevNode.Precedence == input.Precedence)
          result = input.Associativity == Associativity.Left ? ParserActionType.Reduce : ParserActionType.Shift;
        else
          result = prevNode.Precedence > input.Precedence  ? ParserActionType.Reduce : ParserActionType.Shift;
        return result;
      }
      //If no operators found on the stack, do simple shift
      return ParserActionType.Shift;
    }
    #endregion

    #region Braces handling
    private Token CheckBraceToken(Token token) {
      if (token.Terminal.Flags.IsSet(TermFlags.IsOpenBrace)) {
        Context.OpenBraces.Push(token);
        return token;
      }
      //it is closing brace; check if we have opening brace
      if (Context.OpenBraces.Count == 0)
        return CreateBraceMismatchErrorToken(token);
      //check that braces match
      var lastBrace = Context.OpenBraces.Peek();
      if (lastBrace.Terminal.IsPairFor != token.Terminal)
        return CreateBraceMismatchErrorToken(token);
      //Link both tokens, pop the stack and return true
      Token.LinkMatchingBraces(lastBrace, token);
      Context.OpenBraces.Pop();
      return token;
    }

    private Token CreateBraceMismatchErrorToken(Token closingBrace) {
      return new Token(_grammar.SyntaxError, closingBrace.Location, closingBrace.Text,
          string.Format(Resources.ErrUnmatchedCloseBrace, closingBrace.Text));
    }
    #endregion

  }//class



}//namespace

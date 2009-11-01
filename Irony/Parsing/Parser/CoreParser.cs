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
  // CoreParser class implements NLALR parser automaton. Its behavior is controlled by the state transition graph
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

    internal void OnStatusChanged(ParserStatus oldStatus) {
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
      if (Context.ParserInputStack.Count == 0)
        FetchToken();
      else 
        Context.CurrentParserInput = Context.ParserInputStack.Top;
    }

    private void FetchToken() {
      Token token;
      //First skip all non-grammar tokens
      do {
        token = Parser.Scanner.GetToken();
      } while (token.Terminal.OptionIsSet(TermOptions.IsNonGrammar) && token.Terminal != _grammar.Eof);
      if (token.Terminal.OptionIsSet(TermOptions.IsBrace))
        token = CheckBraceToken(token);
      Context.CurrentParserInput = new ParseTreeNode(token);
      Context.ParserInputStack.Push(Context.CurrentParserInput);
    }
    #endregion

    #region execute actions
    private void ExecuteAction() {
      if (Context.CurrentParserInput == null)
        ReadInput();
      //Check scanner error
      if (Context.CurrentParserInput.IsError) {
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
      Context.AddTrace("{0}", action);
      //Execute it
      switch (action.ActionType) {
        case ParserActionType.Shift: ExecuteShift(action.NewState); break;
        case ParserActionType.Operator: ExecuteOperatorAction(action.NewState, action.ReduceProduction); break;
        case ParserActionType.Reduce: ExecuteReduce(action.ReduceProduction); break;
        case ParserActionType.Code: ExecuteConflictAction (action); break;
        case ParserActionType.Jump: ExecuteNonCanonicalJump(action); break;
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
      if (Context.CurrentParserState.DefaultReduceAction != null)
        return Context.CurrentParserState.DefaultReduceAction;
      ParserAction action;
      //First try as keyterm/key symbol; for example if token text = "while", then first try it as a keyword "while";
      // if this does not work, try as an identifier that happens to match a keyword but is in fact identifier
      Token inputToken = Context.CurrentParserInput.Token;
      if (inputToken != null && inputToken.KeyTerm != null) {
        var asSym = inputToken.KeyTerm;
        if (Context.CurrentParserState.Actions.TryGetValue(asSym, out action)) {
          #region comments
          // Ok, we found match as a symbol
          // Backpatch the token's term. For example in most cases keywords would be recognized as Identifiers by Scanner.
          // Identifier would also check with SymbolTerms table and set AsSymbol field to SymbolTerminal if there exist
          // one for token content. So we first find action by Symbol if there is one; if we find action, then we 
          // patch token's main terminal to AsSymbol value.  This is important for recognizing keywords (for colorizing), 
          // and for operator precedence algorithm to work when grammar uses operators like "AND", "OR", etc. 
          //TODO: This is not quite correct action, and we can run into trouble with some languages that have keywords that 
          // are not reserved words. But proper implementation would require substantial addition to parser code: 
          // when running into errors, we need to check the stack for places where we made this "interpret as Symbol"
          // decision, roll back the stack and try to reinterpret as identifier
          #endregion
          inputToken.SetTerminal(asSym);
          Context.CurrentParserInput.Term = asSym;
          Context.CurrentParserInput.Precedence = asSym.Precedence;
          Context.CurrentParserInput.Associativity = asSym.Associativity;
          return action;
        }
      }

      //Try to get by main Terminal, only if it is not the same as symbol
      if (Context.CurrentParserState.Actions.TryGetValue(Context.CurrentParserInput.Term, out action))
        return action;
      //for non-canonical methods, we may encounter reduced lookaheads while the state does not expect it.
      // the reduced lookahead was "created" for other state with canonical conflict, helped resolved it, 
      // but remained on the stack, and now gets as lookahead state that does not expect it. In this case,
      // if we don't find action by reduced term, we should retry it by first child, recursively
      if (Data.ParseMethod != ParseMethod.Lalr) {
        action = GetActionFromChildRec(Context.CurrentParserInput);
        if (action != null)
          return action;
      }
      //Return JumpAction or null if it is not defined
      return Context.CurrentParserState.JumpAction;
    }

    //For NLALR, when non-canonical lookahead had been already reduced, the action for the input in some state
    // might be still for its child term.
    private ParserAction GetActionFromChildRec(ParseTreeNode input) {
      var firstChild = input.FirstChild;
      if (firstChild == null) return null;
      ParserAction action;
      if (Context.CurrentParserState.Actions.TryGetValue(firstChild.Term, out action)) {
        if (action.ActionType == ParserActionType.Reduce) //it applies only to reduce actions
          return action;
      }
      action = GetActionFromChildRec(firstChild);
      return action; 
    }

    private void ExecuteShift(ParserState newState) {
      Context.ParserStack.Push(Context.CurrentParserInput, newState);
      Context.CurrentParserState = newState;
      ReadInput();
    }

    #region ExecuteReduce
    private void ExecuteReduce(Production reduceProduction) {
      var newNode = CreateParseTreeNodeForReduce(reduceProduction);
      //Prepare switching to the new state. First read the state from top of the stack 
      Context.CurrentParserState = Context.ParserStack.Top.State;
      Context.AddTrace("Popped state from stack, pushing {0}", reduceProduction.LValue.Name);
      // Shift to new state (LALR) or push new node into input stack(NLALR, NLALRT)
      if (Data.ParseMethod == ParseMethod.Lalr) {
        //execute shift over non-terminal
        var action = Context.CurrentParserState.Actions[reduceProduction.LValue];
        Context.ParserStack.Push(newNode, action.NewState);
        Context.CurrentParserState = action.NewState;
      } else {
        //NLALR - push it back into input stack
        Context.ParserInputStack.Push(newNode);
        Context.CurrentParserInput = newNode;
      }
    }
    
    private ParseTreeNode CreateParseTreeNodeForReduce(Production reduceProduction) {
      //Special case: reducing list-forming production like "list->list + delim? + elem"
      //in this case the list is already created, so we just get it from stack and add "elem" to it.
      if (reduceProduction.IsSet(ProductionFlags.IsListBuilder))
        return ReduceExistingList(reduceProduction); 
      //"normal" node
      var newNode = new ParseTreeNode(reduceProduction);
      int childCount = reduceProduction.RValues.Count;
      newNode.Span = ComputeNewNodeSpan(childCount);
      if (childCount == 0) 
        return newNode;
      //remember the first child; it might be thrown away later if it's punctuation
      newNode.FirstChild = Context.ParserStack[Context.ParserStack.Count - childCount]; 
      //copy precedence field if there's one child only
      if (childCount == 1 && newNode.FirstChild.Precedence != BnfTerm.NoPrecedence) {
        newNode.Precedence = newNode.FirstChild.Precedence;
        newNode.Associativity = newNode.FirstChild.Associativity;
      }
      //Special case; when we have production with transient list inside with optional punctuation
      // symbols, we copy this transient node's children directly into new node. 
      if (reduceProduction.IsSet(ProductionFlags.TransientListCopy)) {
        CopyTransientLists(childCount, newNode);
        return newNode;
      }
      //Pop child nodes one-by-one; note that they are popped in the reverse of their normal order, so we reverse them after
      foreach(var rvalue in reduceProduction.RValues) {
        var child = PopStackNode();
        if (child != null) 
          newNode.ChildNodes.Add(child); 
      }//for i
      newNode.ChildNodes.Reverse();
      return newNode;
    }

    private void CopyTransientLists(int nodeCount, ParseTreeNode parent) {
      for (int i = 0; i < nodeCount; i++) {
        var node = Context.ParserStack.Pop();
        if (node.Term.OptionIsSet(TermOptions.IsPunctuation)) continue;
        parent.ChildNodes.AddRange(node.ChildNodes);
        //continue loop until all nodes are popped up                
      }
    }

    private SourceSpan ComputeNewNodeSpan(int childCount) {
      if (childCount == 0)
        return new SourceSpan(Context.CurrentParserInput.Span.Location, 0);
      var first = Context.ParserStack[Context.ParserStack.Count - childCount];
      var last = Context.ParserStack.Top;
      return new SourceSpan(first.Span.Location, last.Span.EndPosition - first.Span.Location.Position);
    }

    private ParseTreeNode ReduceExistingList(Production reduceProduction) {
      ParseTreeNode listNode = null;
      var childCount = reduceProduction.RValues.Count; 
      //We are reducing the production of the 
      listNode = Context.ParserStack[Context.ParserStack.Count - childCount]; //get the list already created - it is the first child node
      listNode.Span = ComputeNewNodeSpan(childCount);
      var child = PopStackNode();
      if (child != null) 
        listNode.ChildNodes.Add(child); 
      //now pop the rest one or two nodes, until the list node is popped
      while (Context.ParserStack.Pop() != listNode) { }
      return listNode; 
    }

    private ParseTreeNode PopStackNode() {
      var poppedNode = Context.ParserStack.Pop();
      poppedNode.State = null; //clear the State field, we need only when node is in the stack
      if (poppedNode.Term.OptionIsSet(TermOptions.IsPunctuation)) return null;
      //08/16/09: changed to pop transient node only if child has a single 'grandchild' node
      // that would allow to use Transient in some recursive definitions to eliminate redundant nestings
      // also check it is not a list - list is a different issue.
      if (poppedNode.Term.OptionIsSet(TermOptions.IsTransient) && !poppedNode.Term.OptionIsSet(TermOptions.IsList)
          && poppedNode.ChildNodes.Count == 1) 
         return poppedNode.ChildNodes[0];
      if (_grammar.FlagIsSet(LanguageFlags.CreateAst))
        CreateAstNode(poppedNode);
      return poppedNode;
    }

    private void CreateAstNode(ParseTreeNode parseNode) {
      try {
        _grammar.CreateAstNode(Context, parseNode);
        if (parseNode.AstNode != null && parseNode.Term != null)
          parseNode.Term.OnAstNodeCreated(parseNode);
      } catch (Exception ex) {
        Context.AddParserMessage(ParserErrorLevel.Error, parseNode.Span.Location, "Failed to create AST node for non-terminal [{0}], error: " + ex.Message, parseNode.Term.Name); 
      }
    }
    #endregion

    private void ExecuteConflictAction(ParserAction action) {
      var args = new ConflictResolutionArgs(Context, action);
      _grammar.OnResolvingConflict(args);
      switch(args.Result) {
        case ParserActionType.Reduce:
          ExecuteReduce(args.ReduceProduction);
          break; 
        case ParserActionType.Operator:
          ExecuteOperatorAction(action.NewState, args.ReduceProduction);
          break;
        case ParserActionType.Shift:
        default:
          ExecuteShift(action.NewState); 
          break; 
      }
      Context.AddTrace("Parsing conflict resolved in code.");
 
    }


    private void ExecuteNonCanonicalJump(ParserAction action) {
      Context.CurrentParserState = action.NewState;
    }

    private void ExecuteAccept(ParserAction action) {
      Context.CurrentParseTree.Root = PopStackNode();
      Context.Status = ParserStatus.Accepted;
    }
    



    private void ExecuteOperatorAction(ParserState newShiftState, Production reduceProduction) {
      var realActionType = GetActionTypeForOperation();
      switch (realActionType) {
        case ParserActionType.Shift: ExecuteShift(newShiftState); break;
        case ParserActionType.Reduce: ExecuteReduce(reduceProduction); break; 
      }//switch
      Context.AddTrace("Operator - resolved to {0}", realActionType);
    }

    private ParserActionType GetActionTypeForOperation() {
      for (int i = Context.ParserStack.Count - 1; i >= 0; i--) {
        var  prev = Context.ParserStack[i];
        if (prev == null) continue; 
        if (prev.Precedence == BnfTerm.NoPrecedence) continue;
        ParserActionType result;
        //if previous operator has the same precedence then use associativity
        if (prev.Precedence == Context.CurrentParserInput.Precedence)
          result = Context.CurrentParserInput.Associativity == Associativity.Left ? ParserActionType.Reduce : ParserActionType.Shift;
        else
          result = prev.Precedence > Context.CurrentParserInput.Precedence ? ParserActionType.Reduce : ParserActionType.Shift;
        return result;
      }
      //If no operators found on the stack, do simple shift
      return ParserActionType.Shift;
    }
    #endregion

    #region Braces handling
    private Token CheckBraceToken(Token token) {
      if (token.Terminal.OptionIsSet(TermOptions.IsOpenBrace)) {
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
          string.Format("Unmatched closing brace '{0}'.", closingBrace.Text));
    }
    #endregion

  }//class



}//namespace

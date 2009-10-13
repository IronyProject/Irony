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
using Irony.Diagnostics;


namespace Irony.Parsing { 
  // CoreParser class implements NLALR parser automaton. Its behavior is controlled by the state transition graph
  // with root in Data.InitialState. Each state contains a dictionary of parser actions indexed by input 
  // element (terminal or non-terminal). 
  public partial class CoreParser {

    #region Constructors
    public CoreParser(ParserData parserData, Scanner scanner) {
      Data = parserData;
      _grammar = parserData.Language.Grammar;
      _scanner = scanner;
      
    }
    #endregion

    #region Properties and fields: _grammar, Data, Stack, _context, Input, CurrentState, LineCount, TokenCount
    Grammar _grammar;
    ParsingContext _context;
    public readonly ParserData Data;
    public readonly ParserStack Stack = new ParserStack();
    readonly ParserStack InputStack = new ParserStack();
    Scanner _scanner;
    bool _traceOn;
    
    //"current" stuff
    public ParserState CurrentState {
      get { return _currentState; }
    } ParserState _currentState;

    public ParseTreeNode CurrentInput {
      get { return _currentInput; }
    }  ParseTreeNode _currentInput;

    private ParserTraceEntry _currentTraceEntry; 
    #endregion

    #region Parse method
    public void Parse(ParsingContext context) {
      _context = context;
      _traceOn = _context.OptionIsSet(ParseOptions.TraceParser);
      _currentInput = null;
      InputStack.Clear();
      Stack.Clear();
      _currentState = Data.InitialState; //set the current state to InitialState
      Stack.Push(new ParseTreeNode(Data.InitialState));
      //main loop
      while (ExecuteAction()) {}
    }//Parse
    #endregion

    #region reading input
    private void ReadInput() {
      if (InputStack.Count > 0)
        InputStack.Pop(); 
      if (InputStack.Count == 0)
        FetchToken();
      _currentInput = InputStack.Top;
    }

    private void FetchToken() {
      Token token;
      do {
        token = _scanner.GetToken();
      } while (token.Terminal.OptionIsSet(TermOptions.IsNonGrammar) && token.Terminal != _grammar.Eof);  
      _currentInput = new ParseTreeNode(token);
      InputStack.Push(_currentInput);
      if (_currentInput.IsError)
        TryRecover(); 
    }
    #endregion

    #region execute actions
    private bool ExecuteAction() {
      if (_currentInput == null)
        ReadInput();
      //Trace current state if tracing is on
      if (_traceOn)
        _currentTraceEntry = _context.AddParserTrace(_currentState, Stack.Top, _currentInput);
      if (_currentInput.IsError) {
        ReportErrorFromScanner();
        return TryRecover();
      }
      //Try getting action
      ParserAction action = GetAction();
      if (action == null) {
          ReportParseError();
          return TryRecover();
      }
      //write trace
      if (_currentTraceEntry != null)
        _currentTraceEntry.SetDetails(action.ToString(), _currentState);
      //Execute it
      switch (action.ActionType) {
        case ParserActionType.Shift: ExecuteShift(action.NewState); break;
        case ParserActionType.Operator: ExecuteOperatorAction(action.NewState, action.ReduceProduction); break;
        case ParserActionType.Reduce: ExecuteReduce(action.ReduceProduction); break;
        case ParserActionType.Code: ExecuteConflictAction (action); break;
        case ParserActionType.Jump: ExecuteNonCanonicalJump(action); break;
        case ParserActionType.Accept: ExecuteAccept(action); return false; 
      }
      //add info to trace
      return true; 
    }

    private ParserAction GetAction() {
      if (_currentState.DefaultReduceAction != null)
        return _currentState.DefaultReduceAction;
      ParserAction action;
      //First try as keyterm/key symbol; for example if token text = "while", then first try it as a keyword "while";
      // if this does not work, try as an identifier that happens to match a keyword but is in fact identifier
      Token inputToken = _currentInput.Token;
      if (inputToken != null && inputToken.KeyTerm != null) {
        var asSym = inputToken.KeyTerm;
        if (CurrentState.Actions.TryGetValue(asSym, out action)) {
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
          _currentInput.Term = asSym;
          _currentInput.Precedence = asSym.Precedence;
          _currentInput.Associativity = asSym.Associativity;
          return action;
        }
      }

      //Try to get by main Terminal, only if it is not the same as symbol
      if (_currentState.Actions.TryGetValue(_currentInput.Term, out action))
        return action;
      //for non-canonical methods, we may encounter reduced lookaheads while the state does not expect it.
      // the reduced lookahead was "created" for other state with canonical conflict, helped resolved it, 
      // but remained on the stack, and now gets as lookahead state that does not expect it. In this case,
      // if we don't find action by reduced term, we should retry it by first child, recursively
      if (Data.ParseMethod != ParseMethod.Lalr) {
        action = GetActionFromChildRec(_currentInput);
        if (action != null)
          return action;
      }
      //Return JumpAction or null if it is not defined
      return _currentState.JumpAction;
    }

    //For NLALR, when non-canonical lookahead had been already reduced, the action for the input in some state
    // might be still for its child term.
    private ParserAction GetActionFromChildRec(ParseTreeNode input) {
      var firstChild = input.FirstChild;
      if (firstChild == null) return null;
      ParserAction action;
      if (_currentState.Actions.TryGetValue(firstChild.Term, out action)) {
        if (action.ActionType == ParserActionType.Reduce) //it applies only to reduce actions
          return action;
      }
      action = GetActionFromChildRec(firstChild);
      return action; 
    }

    private void ExecuteShift(ParserState newState) {
      Stack.Push(_currentInput, newState);
      _currentState = newState;
      if (_traceOn) SetTraceDetails("Shift", _currentState); 
      ReadInput();
    }

    #region ExecuteReduce
    private void ExecuteReduce(Production reduceProduction) {
      var newNode = CreateParseTreeNodeForReduce(reduceProduction);
      //Prepare switching to the new state. First read the state from top of the stack 
      _currentState = Stack.Top.State;
      //write to trace
      if (_traceOn)
        SetTraceDetails("Reduce on '" + reduceProduction.ToString() + "'", _currentState);
      // Shift to new state (LALR) or push new node into input stack(NLALR, NLALRT)
      if (Data.ParseMethod == ParseMethod.Lalr) {
        //execute shift over non-terminal
        var action = _currentState.Actions[reduceProduction.LValue];
        Stack.Push(newNode, action.NewState);
        _currentState = action.NewState;
      } else {
        //NLALR - push it back into input stack
        InputStack.Push(newNode);
        _currentInput = newNode;
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
      newNode.FirstChild = Stack[Stack.Count - childCount]; 
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
        var node = Stack.Pop();
        if (node.Term.OptionIsSet(TermOptions.IsPunctuation)) continue;
        parent.ChildNodes.AddRange(node.ChildNodes);
        //continue loop until all nodes are popped up                
      }
    }

    private SourceSpan ComputeNewNodeSpan(int childCount) {
      if (childCount == 0)
        return new SourceSpan(_currentInput.Span.Location, 0);
      var first = Stack[Stack.Count - childCount];
      var last = Stack.Top;
      return new SourceSpan(first.Span.Location, last.Span.EndPosition - first.Span.Location.Position);
    }

    private ParseTreeNode ReduceExistingList(Production reduceProduction) {
      ParseTreeNode listNode = null;
      var childCount = reduceProduction.RValues.Count; 
      //We are reducing the production of the 
      listNode = Stack[Stack.Count - childCount]; //get the list already created - it is the first child node
      listNode.Span = ComputeNewNodeSpan(childCount);
      var child = PopStackNode();
      if (child != null) 
        listNode.ChildNodes.Add(child); 
      //now pop the rest one or two nodes, until the list node is popped
      while (Stack.Pop() != listNode) { }
      return listNode; 
    }

    private ParseTreeNode PopStackNode() {
      var poppedNode = Stack.Pop();
      poppedNode.State = null; //clear the State field, we need only when node is in the stack
      if (poppedNode.Term.OptionIsSet(TermOptions.IsPunctuation)) return null;
      //08/16/09: changed to pop transient node only if child has a single 'grandchild' node
      // that would allow to use Transient in some recursive definitions to eliminate redundant nestings
      // also check it is not a list - list is a different issue.
      if (poppedNode.Term.OptionIsSet(TermOptions.IsTransient) && !poppedNode.Term.OptionIsSet(TermOptions.IsList)
          && poppedNode.ChildNodes.Count == 1) 
         return poppedNode.ChildNodes[0];
      if (_grammar.FlagIsSet(LanguageFlags.CreateAst))
        SafeCreateAstNode(poppedNode);
      return poppedNode;
    }

    private void SafeCreateAstNode(ParseTreeNode parseNode) {
      try {
        _grammar.CreateAstNode(_context, parseNode);
        if (parseNode.AstNode != null && parseNode.Term != null)
          parseNode.Term.OnAstNodeCreated(parseNode);
      } catch (Exception ex) {
        _context.AddError(parseNode.Span.Location, "Failed to create AST node for non-terminal [{0}], error: " + ex.Message, parseNode.Term.Name); 
      }
    }
    #endregion

    private void ExecuteConflictAction(ParserAction action) {
      var args = new ConflictResolutionArgs(_context, action);
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
      if (_currentTraceEntry != null) {
        _currentTraceEntry.Message = "(Conflict resolved in code) " + _currentTraceEntry.Message;
      }

    }


    private void ExecuteNonCanonicalJump(ParserAction action) {
      _currentState = action.NewState;
    }

    private void ExecuteAccept(ParserAction action) {
      _context.CurrentParseTree.Root = PopStackNode();  
    }

    private void ExecuteOperatorAction(ParserState newShiftState, Production reduceProduction) {
      var realActionType = GetActionTypeForOperation();
      switch (realActionType) {
        case ParserActionType.Shift: ExecuteShift(newShiftState); break;
        case ParserActionType.Reduce: ExecuteReduce(reduceProduction); break; 
      }//switch
      if (_currentTraceEntry != null) {
        _currentTraceEntry.Message = "(Operator)" + _currentTraceEntry.Message;
      }

    }

    private ParserActionType GetActionTypeForOperation() {
      for (int i = Stack.Count - 1; i >= 0; i--) {
        var  prev = Stack[i];
        if (prev == null) continue; 
        if (prev.Precedence == BnfTerm.NoPrecedence) continue;
        ParserActionType result;
        //if previous operator has the same precedence then use associativity
        if (prev.Precedence == _currentInput.Precedence)
          result = _currentInput.Associativity == Associativity.Left ? ParserActionType.Reduce : ParserActionType.Shift;
        else
          result = prev.Precedence > _currentInput.Precedence ? ParserActionType.Reduce : ParserActionType.Shift;
        return result;
      }
      //If no operators found on the stack, do simple shift
      return ParserActionType.Shift;
    }
    #endregion

    #region Tracing
    private void AddTraceEntry(string message, ParserState newState) {
      if (!_traceOn) return;
      AddTraceEntry();
      SetTraceDetails(message, newState); 
    }
    private void AddTraceEntry() {
      if (!_traceOn) return; 
      _currentTraceEntry = _context.AddParserTrace(_currentState, Stack.Top, _currentInput);
    }
    private void SetTraceDetails(string message, ParserState newState) {
      if (_currentTraceEntry != null)
        _currentTraceEntry.SetDetails(message, newState); 
    }

    #endregion 
  }//class



}//namespace

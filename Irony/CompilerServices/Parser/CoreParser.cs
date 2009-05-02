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


namespace Irony.CompilerServices {
  // CoreParser class implements NLALR parser automaton. Its behavior is controlled by the state transition graph
  // with root in Data.InitialState. Each state contains a dictionary of parser actions indexed by input 
  // element (terminal or non-terminal). 
  public class CoreParser {

    #region Constructors
    public CoreParser(ParserData parserData, Scanner scanner) {
      Data = parserData;
      _grammar = parserData.Grammar;
      _scanner = scanner;
      
    }
    #endregion

    #region Properties and fields: _grammar, Data, Stack, _context, Input, CurrentState, LineCount, TokenCount
    Grammar _grammar;
    CompilerContext _context;
    public readonly ParserData Data;
    public readonly ParserStack Stack = new ParserStack();
    readonly ParserStack InputStack = new ParserStack();
    Scanner _scanner;
    bool _traceOn; 
    
    //"current" stuff
    public ParserState CurrentState {
      get { return _currentState; }
    } ParserState _currentState;

    private ParseTreeNode CurrentInput {
      get { return _currentInput; }
    }  ParseTreeNode _currentInput;

    private ParserTraceEntry _currentTraceEntry; 
    #endregion

    public void Parse(CompilerContext context) {
      _context = context;
      _traceOn = _context.OptionIsSet(CompilerOptions.TraceParser); 
      InitParser(); 
      //main loop
      while (ExecuteAction()) { }
      context.CurrentParseTree.Root = Stack.Top;
    }//Parse

    private void InitParser() {
      Stack.Clear();
      InputStack.Clear();
      _currentState = Data.InitialState; //set the current state to InitialState
      FetchToken();
      Stack.Push(new ParseTreeNode(Data.InitialState));
      if (_currentInput.IsError)
        Recover();
    }
    #region reading input
    private void ReadInput() {
      InputStack.Pop(); //pop _currentInput from the stack
      if (InputStack.Count == 0)
        FetchToken();
      _currentInput = InputStack.Top;
    }

    private void FetchToken() {
      Token token;
      do {
        token = _scanner.GetToken();
      } while (token.Terminal.IsSet(TermOptions.IsNonGrammar) && token.Terminal != _grammar.Eof);  
      _currentInput = new ParseTreeNode(token);
      InputStack.Push(_currentInput);
      if (_currentInput.IsError)
        Recover(); 
    }
    #endregion

    #region execute actions
    private bool ExecuteAction() {
      //Trace current state if tracing is on
      if (_traceOn)
        _currentTraceEntry = _context.AddParserTrace(_currentState, Stack.Top, _currentInput);
      //Try getting action
      ParserAction action = GetAction();
      if (action == null) {
          ReportParseError();
          return Recover();
      }
      //write trace
      if (_currentTraceEntry != null)
        _currentTraceEntry.SetDetails(action.ToString(), _currentState);
      //Execute it
      switch (action.ActionType) {
        case ParserActionType.Shift: ExecuteShift(action); break;
        case ParserActionType.Operator: ExecuteOperatorAction(action); break;
        case ParserActionType.Reduce: ExecuteReduce(action.ReduceProduction); break;
        case ParserActionType.Jump: ExecuteNonCanonicalJump(action); break;
        case ParserActionType.Accept: return false; 
      }
      //add info to trace
      return true; 
    }

    private ParserAction GetAction() {
      if (_currentState.DefaultReduceAction != null)
        return _currentState.DefaultReduceAction;
      ParserAction action;
      //First try as Symbol; 
      Token inputToken = _currentInput.Token;
      if (inputToken != null && inputToken.AsSymbol != null) {
        var asSym = inputToken.AsSymbol;
        if (CurrentState.Actions.TryGetValue(asSym, out action)) {
          // Ok, we found match as a symbol
          // Backpatch the token's term. For example in most cases keywords would be recognized as Identifiers by Scanner.
          // Identifier would also check with SymbolTerms table and set AsSymbol field to SymbolTerminal if there exist
          // one for token content. So we first find action by Symbol if there is one; if we find action, then we 
          // patch token's main terminal to AsSymbol value.  This is important for recognizing keywords (for colorizing), 
          // and for operator precedence algorithm to work when grammar uses operators like "AND", "OR", etc. 
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

    private ParserAction GetActionFromChildRec(ParseTreeNode input) {
      if (input.FirstChild == null) return null; 
      ParserAction action;
      if (_currentState.Actions.TryGetValue(input.FirstChild.Term, out action)) {
        if (action.ActionType == ParserActionType.Reduce) //it applies only to reduce actions
          return action;
      }
      action = GetActionFromChildRec(input.FirstChild);
      return action; 
    }

    private void ExecuteShift(ParserAction action) {
      Stack.Push(_currentInput, action.NewState);
      _currentState = action.NewState;
      if (_traceOn) SetTraceDetails("Shift", _currentState); 
      ReadInput(); 
    }
    private void ExecuteReduce(Production reduceProduction) {
      //compute new node span
      SourceSpan span;
      ParseTreeNode first = null, last = null; 
      int nodeCount = reduceProduction.RValues.Count;
      if (reduceProduction.RValues.Count == 0)
        span = new SourceSpan(InputStack.Top.Span.Start, 0);
      else {
        first = Stack[Stack.Count - nodeCount];
        last = Stack.Top;
        span = new SourceSpan(first.Span.Start, last.Span.EndPos - first.Span.Start.Position);
      }
      ParseTreeNode newNodeInfo;
      int firstChildIndex = Stack.Count - nodeCount;
      //check if we have a list and it was already created 
      bool alreadyCreatedList = reduceProduction.LValue.IsSet(TermOptions.IsList) && 
         reduceProduction.RValues.Count > 0 && reduceProduction.RValues[0] == reduceProduction.LValue; 
      if (alreadyCreatedList) {
        newNodeInfo = Stack[firstChildIndex]; //get the list already created
        newNodeInfo.Span = span;
        AddChildNode(newNodeInfo.ChildNodes, Stack.Top);
      } else {
        newNodeInfo = new ParseTreeNode(reduceProduction);
        newNodeInfo.Span = span;
        newNodeInfo.FirstChild = first; 
        //Pop child nodes one-by-one
        for (int i = 0; i < nodeCount ; i++) {
          var child = Stack[firstChildIndex + i];
          //check precedence
          if (nodeCount == 1 && child.Precedence != BnfTerm.NoPrecedence) {
            newNodeInfo.Precedence = child.Precedence;
            newNodeInfo.Associativity = child.Associativity;
          }
          child.State = null; //clear the State field, we need only when node is in the stack
          if (child.Term.IsSet(TermOptions.IsPunctuation)) continue;
          AddChildNode(newNodeInfo.ChildNodes, child); 
        }//for i
        if (!newNodeInfo.Term.IsSet(TermOptions.IsTransient))
          _grammar.CreateAstNode(_context, newNodeInfo);
      }//else
      //Remove nodes from stack
      if (nodeCount > 0)
        Stack.RemoveRange(firstChildIndex, nodeCount);//pop these nodes from the stack
      //Read the state from top of the stack, and shift to new state (LALR) or push new node into input stack(NLALR, NLALRT)
      _currentState = Stack.Top.State;
      //write to trace
      if (_traceOn) SetTraceDetails("Reduce on '" + reduceProduction.ToString() + "'", _currentState);
      if (Data.ParseMethod == ParseMethod.Lalr) {
        //execute shift over non-terminal
        var action = _currentState.Actions[reduceProduction.LValue];
        Stack.Push(newNodeInfo, action.NewState);
        _currentState = action.NewState; 
      } else {
        InputStack.Push(newNodeInfo);
        _currentInput = newNodeInfo;
      }
    }
    private void AddChildNode(ParseTreeNodeList childNodes, ParseTreeNode child) {
      if (child.Term.IsSet(TermOptions.IsTransient) && child.ChildNodes != null) {
        childNodes.AddRange(child.ChildNodes);
      } else
        childNodes.Add(child);
    }

    private void ExecuteNonCanonicalJump(ParserAction action) {
      _currentState = action.NewState;
    }

    private void ExecuteOperatorAction(ParserAction action) {
      var realActionType = GetActionTypeForOperation();
      switch (realActionType) {
        case ParserActionType.Shift: ExecuteShift(action); break;
        case ParserActionType.Reduce: ExecuteReduce(action.ReduceProduction); break; 
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

    #region error reporting and recovery
    private void ReportParseError() {
      string msg;
      if (_currentInput.Term == _grammar.Eof)
        msg = "Unexpected end of file.";
      else {
        msg = _grammar.GetSyntaxErrorMessage(_context, _currentState, _currentInput);
        if (msg == null) {
          var expSet = _currentState.ReducedExpectedSet;
          msg = "Syntax error" + (expSet.Count == 0 ? "." : ", expected: " + expSet.ToErrorString());
        }
      }
      _context.ReportError(_currentState, _currentInput.Span.Start, msg);
      if (_currentTraceEntry != null) {
        _currentTraceEntry.Message = msg;
        _currentTraceEntry.IsError = true; 
      }
    }


    private bool Recover() {
      if (_traceOn)
        AddTraceEntry("*** RECOVERING - searching for state with error shift ***", _currentState); //add new trace entry
      //2. We need to find a state in the stack that has a shift item based on error production (with error token), 
      // and error terminal is current. This state would have a shift action on error token. 
      ParserAction errorShiftAction = FindErrorShiftActionInStack();
      if (errorShiftAction == null) return false; //we failed to recover
      //3. Shift error token - execute shift action
      if (_traceOn) AddTraceEntry();
      ExecuteShift(errorShiftAction); 
      //4. Now we need to go along error production until the end, shifting tokens that CAN be shifted and ignoring others.
      //   We shift until we can reduce
      while (_currentInput.Term != _grammar.Eof) {
        //Check if we can reduce
        ParserAction action = FindReduceActionInCurrentState(); 
        if (action != null) {
          ExecuteReduce(action.ReduceProduction);
          InputStack.Top.IsError = true; //mark it as error node
          if (_traceOn)   AddTraceEntry("*** RECOVERED ***", _currentState); //add new trace entry
          return true; //we recovered 
        }
        //No reduce action in current state. Try to shift current token or throw it away or reduce
        action = GetAction();
        if (action != null && action.ActionType == ParserActionType.Shift)
          ExecuteShift(action); //shift input token
        else 
          ReadInput(); //throw away input token
      }
      return false; 
    }//method

    private ParserAction FindErrorShiftActionInStack() {
      // We may have current error token (if it came from scanner), or normal token - in this case we create it and push back. 
      if (_currentInput.Term != _grammar.SyntaxError) {
        _currentInput = new ParseTreeNode(_grammar.SyntaxError);
        InputStack.Push(_currentInput);
      }
      while (Stack.Count > 1) {
        ParserAction errorShiftAction;
        if (_currentState.Actions.TryGetValue(_grammar.SyntaxError, out errorShiftAction) && errorShiftAction.ActionType == ParserActionType.Shift)
          return errorShiftAction;
        if (Stack.Count == 1) 
          return null; //don't pop the initial state
        Stack.Pop();
        _currentState = Stack.Top.State;
      }
      return null; 
    }

    private ParserAction FindReduceActionInCurrentState() {
      if (_currentState.DefaultReduceAction != null) return _currentState.DefaultReduceAction;
      foreach(var action in _currentState.Actions.Values)
        if (action.ActionType == ParserActionType.Reduce)
          return action; 
      return null;
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

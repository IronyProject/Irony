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


namespace Irony.Compiler {
  //Parser class implements LALR(1) parser DFM. Its behavior is controlled by a state transition graph
  // contained in _data.States list. Each state contains a dictionary of parser actions indexed by input 
  // element (token or non-terminal node). Parser takes input token stream and produces Abstract Syntax Tree.
  public class Parser  {

    #region Constructors
    public Parser(GrammarData data) {
      Data = data;
    }
    #endregion

    #region Properties and fields: Data, Stack, _context, Input, CurrentState, LineCount, TokenCount
    public readonly GrammarData Data;
    public readonly ParserStack Stack = new ParserStack();

    private CompilerContext _context;

    public IEnumerator<Token> Input {
      get {return _input;}
    } IEnumerator<Token> _input;

    public ParserState CurrentState {
      get {return _currentState;}
    } ParserState  _currentState;


    public int LineCount {
      get {return _lineCount;}
    } int  _lineCount;

    public int TokenCount  {
      get {return _tokenCount;}
    } int  _tokenCount;

    #endregion

    #region Events: ParserAction, TokenReceived
    public event EventHandler<ParserActionEventArgs> ActionSelected;
    public event EventHandler<ParserActionEventArgs> ActionConflict;
    public event EventHandler<TokenEventArgs> TokenReceived;
    TokenEventArgs _tokenArgs = new TokenEventArgs(null); //declar as field and reuse it to avoid generating garbage

    protected void OnTokenReceived(Token token) {
      if (TokenReceived == null) return;
      _tokenArgs.Token = token;
      TokenReceived(this, _tokenArgs);
    }
    #endregion

    #region Parsing methods
    private void Reset() {
      Stack.Reset();
      _currentState = Data.InitialState;
      _lineCount = 0;
      _tokenCount = 0;
      _context.Errors.Clear();
    }
    
    private Token GetToken() {
      Token token;
      while (_input.MoveNext()) {
        token = _input.Current;
        _tokenCount++;
        _lineCount = token.Location.Line + 1;
        if (TokenReceived != null)
          OnTokenReceived(token);
        //check token category
        switch (token.Terminal.Category) {
          case TokenCategory.Content:
          case TokenCategory.Outline:
            return token;
          case TokenCategory.Comment:
            continue;
          case TokenCategory.Error:
            ReportError(token);
            Recover();
            continue;
        }//switch
      }//while
      //Normally we never get here. 
      // It might happen if the grammar directly uses EOF token in Root definition (which is not recommended);
      //  in this case we keep returning EOF token, to avoid breaking the parser.
      return new Token(Grammar.Eof, new SourceLocation(0, _lineCount - 1, 0), ""); 
    }//method


    public AstNode Parse(CompilerContext context, IEnumerable<Token> tokenStream) {
      _context = context;
      Reset();
      _input = tokenStream.GetEnumerator();
      Token token = GetToken();
      while (true) {
        if (_currentState == Data.FinalState)
          return Stack[0].Node;
        //Check for EOF
        //if (token.Terminal == Grammar.Eof && Stack.Count == 0) 
        //  yield break;
        //Figure out whether to shift or to reduce
        ActionRecord action = GetAction(token);
        if (ActionSelected != null) //just to improve performance we check it here
          OnActionSelected(_currentState, token, action);
        if (action.HasConflict())
          action = OnActionConflict(_currentState, token, action);
        switch(action.ActionType) {
          case ParserActionType.Operator:
            if (GetActionTypeForOperation(token) == ParserActionType.Shift)
              goto case ParserActionType.Shift;
            else
              goto case ParserActionType.Reduce;

          case ParserActionType.Shift:
            Stack.Push(token, token.Location, _currentState);
            _currentState = action.NewState;
            token = GetToken();
            break;

          case ParserActionType.Reduce:
            ExecuteReduceAction(action, token);
            break;
          
          case ParserActionType.Error:
            //TODO: add better error reporting here
            if (token.Terminal == Grammar.Eof) {
              ReportError(_input.Current.Location, "Unexpected end of file.");
              return null;
            } else
              ReportError(_input.Current.Location, "Syntax error.");
            Recover();
            token = GetToken();
            break;
        }//switch
      }//while
    }//Parse
    #endregion

    #region Error handling
    private void ReportError(SourceLocation location, string message, params object[] args) {
      if (args != null && args.Length > 0)
        message = string.Format(message, args);
      _context.AddError(location, message, _currentState);
    }

    private void ReportError(Token errorToken) {
      _context.AddError(errorToken.Location, errorToken.Text, _currentState);
    }

    private Token Recover() {
      //TODO: implement REAL recovery to some consistent state.
      Token token;
      token = GetToken(); //simply shift to next token
      return token;
    }
    #endregion

    #region event handling
    protected void OnActionSelected(ParserState state, Token input, ActionRecord action) {
      if (ActionSelected != null) {
        ParserActionEventArgs args = new ParserActionEventArgs(state, input, action);
        ActionSelected(this, args);
      }
    }
    protected ActionRecord OnActionConflict(ParserState state, Token input, ActionRecord action) {
      if (ActionConflict != null) {
        ParserActionEventArgs args = new ParserActionEventArgs(state, input, action);
        ActionConflict(this, args);
        return args.Action;
      }
      return action;
    }
    #endregion

    private ActionRecord GetAction(Token token) {
      ActionRecord action = null;
      if ((token.Terminal.MatchMode & TokenMatchMode.ByValue) != 0) {
        if (_currentState.Actions.TryGetValue(token.Text, out action))
          return action;
      }
      if ((token.Terminal.MatchMode & TokenMatchMode.ByType) != 0 &&  
        _currentState.Actions.TryGetValue(token.Terminal.Key, out action))
        return action;
      //return error action singleton
      return ActionRecord.ErrorAction;
    }
    private ParserActionType GetActionTypeForOperation(Token current) {
      OperatorInfo opInfo;
      string op = current.Text;
      if (!Data.Grammar.Operators.TryGetValue(op, out opInfo)) return ParserActionType.Shift;
      for (int i = Stack.Count - 2; i >= 0; i--) {
        Token tkn = Stack[i].Node as Token;
        if (tkn == null) continue;
        string prevOp = tkn.Text;
        if (prevOp == op) //if previous operator is the same then use associativity
          return opInfo.Associativity == Associativity.Left ? ParserActionType.Reduce : ParserActionType.Shift;
        OperatorInfo prevOpInfo;
        if (!Data.Grammar.Operators.TryGetValue(prevOp, out prevOpInfo)) continue;
        ParserActionType result = prevOpInfo.Precedence >= opInfo.Precedence ? ParserActionType.Reduce : ParserActionType.Shift;
        return result;
      }
      return ParserActionType.Shift;
    }

    private void ExecuteReduceAction(ActionRecord action, Token current) {
      ParserState oldState = _currentState;
      int popCnt = action.PopCount;

      //Get new node's child nodes - these are nodes being popped from the stack 
      AstNodeList childNodes = new AstNodeList();
      for (int i = 0; i < action.PopCount; i++) {
        AstNode child = Stack[Stack.Count - popCnt + i].Node;
        Token tkn = child as Token;
        if (tkn != null && Data.PunctuationLookup.ContainsKey(tkn.Text))
          continue; //don't add this node, it is punctuation symbol
        childNodes.Add(child);
      }
      //recover state, location and pop the stack
      SourceLocation location = current.Location;
      if (popCnt > 0) {
        location = Stack[Stack.Count - popCnt].Location;
        _currentState = Stack[Stack.Count - popCnt].State;
        Stack.Pop(popCnt);
      }
      //Create new node
      AstNode node = Data.Grammar.CreateNode(_context, action, location, childNodes);
      // Push node/current state into the stack 
      Stack.Push(node, location, _currentState);
      //switch to new state
      ActionRecord gotoAction;
      if (_currentState.Actions.TryGetValue(action.NonTerminal.Key, out gotoAction)) {
        _currentState = gotoAction.NewState;
      } else 
        //should never happen
        throw new ApplicationException( string.Format("Cannot find transition for input {0}; state: {1}, popped state: {2}", action.NonTerminal, oldState, _currentState));
    }//method

  }//class

}//namespace

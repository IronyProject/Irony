using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Error handling methods of CoreParser class
namespace Irony.Parsing { 
  public partial class CoreParser {

    private bool TryRecover() {
      _context.ParserIsRecovering = true;
      try {
        if (_traceOn)
          AddTraceEntry("*** RECOVERING - searching for state with error shift ***", _currentState); //add new trace entry
        var result = TryRecoverImpl();
        if (_traceOn) {
          string msg = (result ? "*** RECOVERED ***" : "*** FAILED TO RECOVER ***");
          AddTraceEntry(msg, _currentState); //add new trace entry
        }//if
        return result;
      } finally {
        _context.ParserIsRecovering = false;
      }
    }

    private bool TryRecoverImpl() {
      //1. We need to find a state in the stack that has a shift item based on error production (with error token), 
      // and error terminal is current. This state would have a shift action on error token. 
      ParserAction errorShiftAction = FindErrorShiftActionInStack();
      if (errorShiftAction == null) return false; //we failed to recover
      //2. Shift error token - execute shift action
      if (_traceOn) AddTraceEntry();
      ExecuteShift(errorShiftAction.NewState);
      //4. Now we need to go along error production until the end, shifting tokens that CAN be shifted and ignoring others.
      //   We shift until we can reduce
      while (_currentInput.Term != _grammar.Eof) {
        //Check if we can reduce
        var action = GetReduceActionInCurrentState();
        if (action != null) {
          //Clear all input token queues and buffered input, reset location back to input position token queues; 
          _scanner.SetSourceLocation(_currentInput.Span.Location);
          _currentInput = null;
          InputStack.Clear();
          //Reduce error production - it creates parent non-terminal that "hides" error inside
          ExecuteReduce(action.ReduceProduction);
          return true; //we recovered 
        }
        //No reduce action in current state. Try to shift current token or throw it away or reduce
        action = GetShiftActionInCurrentState();
        if (action != null)
          ExecuteShift(action.NewState); //shift input token
        else
          ReadInput(); //throw away input token
      }
      return false;
    }//method

    public void ResetLocationAndClearInput(SourceLocation location, int position) {
      _currentInput = null;
      InputStack.Clear();
      _scanner.SetSourceLocation(location);
    }

    private ParserAction FindErrorShiftActionInStack() {
      while (Stack.Count >= 1) {
        ParserAction errorShiftAction;
        if (_currentState.Actions.TryGetValue(_grammar.SyntaxError, out errorShiftAction) && errorShiftAction.ActionType == ParserActionType.Shift)
          return errorShiftAction;
        //pop next state from stack
        if (Stack.Count == 1)
          return null; //don't pop the initial state
        Stack.Pop();
        _currentState = Stack.Top.State;
      }
      return null;
    }

    private ParserAction GetReduceActionInCurrentState() {
      if (_currentState.DefaultReduceAction != null) return _currentState.DefaultReduceAction;
      foreach (var action in _currentState.Actions.Values)
        if (action.ActionType == ParserActionType.Reduce)
          return action;
      return null;
    }

    private ParserAction GetShiftActionInCurrentState() {
      ParserAction result = null;
      if (_currentState.Actions.TryGetValue(_currentInput.Term, out result) ||
         _currentInput.Token != null && _currentInput.Token.AsSymbol != null &&
             _currentState.Actions.TryGetValue(_currentInput.Token.AsSymbol, out result))
        if (result.ActionType == ParserActionType.Shift)
          return result;
      return null;
    }

    #region Error reporting
    private void ReportErrorFromScanner() {
      _context.AddError(_currentInput.Token.Location, _currentInput.Token.Value as string); 
    }

    private void ReportParseError() {
      string msg;
      if (_currentInput.Term == _grammar.Eof)
        msg = "Unexpected end of file.";
      else {
        //See note about multi-threading issues in ComputeReportedExpectedSet comments.
        if (_currentState.ReportedExpectedSet == null)
          _currentState.ReportedExpectedSet = ComputeReportedExpectedSet(_currentState); 
        //TODO: add extra filtering of expected terms from brace-matching filter: while the closing parenthesis ")" might 
        //  be expected term in a state in general, if there was no opening parenthesis in preceding input then we would not
        //  expect a closing one. 
        msg = _grammar.ConstructParserErrorMessage(_context, _currentState, _currentState.ReportedExpectedSet, _currentInput);
        if (string.IsNullOrEmpty(msg))
          msg = "Syntax error";
      }
      _context.AddCompilerMessage(CompilerErrorLevel.Error, _currentState, _currentInput.Span.Location, msg);
      if (_currentTraceEntry != null) {
        _currentTraceEntry.Message = msg;
        _currentTraceEntry.IsError = true;
      }
    }

    //compute set of expected terms in a parser state. While there may be extended list of symbols expected at some point,
    // we want to reorganize and reduce it. For example, if the current state expects all arithmetic operators as an input,
    // it would be better to not list all operators (+, -, *, /, etc) but simply put "operator" covering them all. 
    // To be able to do this, grammar writer can set non-empty DisplayName on Operator non-terminal - this is an indicator for 
    // Irony to wrap all sub-elements of the rule and report them as "operator". The following code takes "raw" list of 
    // expected terms from the state, finds terms that have a DisplayName assinged and removes other terms that are covered 
    // by this display name. 
    // Note about multi-threading. When used in multi-threaded environment (web server), the LanguageData would be shared in 
    // application-wide cache to avoid rebuilding the parser data on every request. The LanguageData is immutable, except 
    // this one case - the expected sets are constructed late by CoreParser on the when-needed basis. 
    // We don't do any locking here, just compute the set and on return from this function the state field is assigned. 
    // We assume that this field assignment is an atomic, concurrency-safe operation. The worst thing that might happen
    // is "double-effort" when two threads start computing the same set around the same time, and the last one to finish would 
    // leave its result in the state field. 
    private BnfTermSet ComputeReportedExpectedSet(ParserState state) {
      //Compute reduced expected terms - to be used in error reporting
      //1. Scan Expected terms, add non-terminals with non-empty DisplayName to reduced set, and collect all their firsts
      var reducedSet = new BnfTermSet();
      var allFirsts = new BnfTermSet();
      foreach (var term in state.ExpectedTerms) {
        var nt = term as NonTerminal;
        if (nt == null) continue;
        if (!reducedSet.Contains(nt) && !string.IsNullOrEmpty(nt.DisplayName) && !allFirsts.Contains(nt)) {
          reducedSet.Add(nt);
          allFirsts.UnionWith(nt.Firsts);
        }
      }
      //2. Now go thru all expected terms and add only those that are NOT in the allFirsts set.
      foreach (var term in state.ExpectedTerms) {
        if (!reducedSet.Contains(term) && !allFirsts.Contains(term) && (term is Terminal || !string.IsNullOrEmpty(term.DisplayName)))
          reducedSet.Add(term);
      }
      //Clean-up reduced set, remove pseudo terms
      if (reducedSet.Contains(_grammar.Eof)) reducedSet.Remove(_grammar.Eof);
      if (reducedSet.Contains(_grammar.SyntaxError)) reducedSet.Remove(_grammar.SyntaxError);
      return reducedSet;
    }
    #endregion


  }//class
}//namespace

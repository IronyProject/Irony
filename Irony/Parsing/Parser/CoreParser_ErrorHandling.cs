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
using System.Linq;
using System.Text;

//Error handling methods of CoreParser class
namespace Irony.Parsing { 
  public partial class CoreParser {

    private void ProcessParserError() {
      Context.Status = ParserStatus.Error;
      ReportParseError();
      if (Context.Mode != ParseMode.CommandLine)
        TryRecoverFromError(); 
    }


    private bool TryRecoverFromError() {
      if (Context.CurrentParserInput.Term == _grammar.Eof)
        return false; //do not recover if we're already at EOF
      Context.Status = ParserStatus.Recovering;
      Context.AddTrace(Resources.MsgTraceRecovering); // *** RECOVERING - searching for state with error shift *** 
      var recovered = TryRecoverImpl();
      string msg = (recovered ? Resources.MsgTraceRecoverSuccess : Resources.MsgTraceRecoverFailed);
      Context.AddTrace(msg); //add new trace entry
      Context.Status = recovered? ParserStatus.Parsing : ParserStatus.Error; 
      return recovered;
    }

    private bool TryRecoverImpl() {
      //1. We need to find a state in the stack that has a shift item based on error production (with error token), 
      // and error terminal is current. This state would have a shift action on error token. 
      ParserAction errorShiftAction = FindErrorShiftActionInStack();
      if (errorShiftAction == null) return false; //we failed to recover
      Context.AddTrace(Resources.MsgTraceRecoverFoundState, Context.CurrentParserState); 
      //2. Shift error token - execute shift action
      Context.AddTrace(Resources.MsgTraceRecoverShiftError, errorShiftAction);
      ExecuteShift(errorShiftAction);
      //4. Now we need to go along error production until the end, shifting tokens that CAN be shifted and ignoring others.
      //   We shift until we can reduce
      Context.AddTrace(Resources.MsgTraceRecoverShiftTillEnd);
      while (true) {
        if (Context.CurrentParserInput == null) 
          ReadInput(); 
        if (Context.CurrentParserInput.Term == _grammar.Eof)
          return false; 
        //Check if we can reduce
        var action = GetReduceActionInCurrentState();
        if (action != null) {
          //Clear all input token queues and buffered input, reset location back to input position token queues; 
          Context.SetSourceLocation(Context.CurrentParserInput.Span.Location);
          Context.ParserInputStack.Clear();
          Context.CurrentParserInput = null;
       
          //Reduce error production - it creates parent non-terminal that "hides" error inside
          Context.AddTrace(Resources.MsgTraceRecoverReducing);
          Context.AddTrace(Resources.MsgTraceRecoverAction, action);
          ExecuteReduce(action);
          return true; //we recovered 
        }
        //No reduce action in current state. Try to shift current token or throw it away or reduce
        action = GetShiftActionInCurrentState();
        if(action != null)
          ExecuteShift(action); //shift input token
        else //simply read input
          ReadInput(); 
      }
    }//method

    public void ResetLocationAndClearInput(SourceLocation location, int position) {
      Context.CurrentParserInput = null;
      Context.ParserInputStack.Clear();
      Context.SetSourceLocation(location);
    }

    private ParserAction FindErrorShiftActionInStack() {
      while (Context.ParserStack.Count >= 1) {
        ParserAction errorShiftAction;
        if (Context.CurrentParserState.Actions.TryGetValue(_grammar.SyntaxError, out errorShiftAction) && errorShiftAction.ActionType == ParserActionType.Shift)
          return errorShiftAction;
        //pop next state from stack
        if (Context.ParserStack.Count == 1)
          return null; //don't pop the initial state
        Context.ParserStack.Pop();
        Context.CurrentParserState = Context.ParserStack.Top.State;
      }
      return null;
    }

    private ParserAction GetReduceActionInCurrentState() {
      if (Context.CurrentParserState.DefaultReduceAction != null) return Context.CurrentParserState.DefaultReduceAction;
      foreach (var action in Context.CurrentParserState.Actions.Values)
        if (action.ActionType == ParserActionType.Reduce)
          return action;
      return null;
    }

    private ParserAction GetShiftActionInCurrentState() {
      ParserAction result = null;
      if (Context.CurrentParserState.Actions.TryGetValue(Context.CurrentParserInput.Term, out result) ||
         Context.CurrentParserInput.Token != null && Context.CurrentParserInput.Token.KeyTerm != null &&
             Context.CurrentParserState.Actions.TryGetValue(Context.CurrentParserInput.Token.KeyTerm, out result))
        if (result.ActionType == ParserActionType.Shift)
          return result;
      return null;
    }

    #region Error reporting

    private void ReportParseError() {
      string msg;
      if (Context.CurrentParserInput.Term == _grammar.SyntaxError) {
        Context.AddParserError(Context.CurrentParserInput.Token.Value as string);
        return; 
      }
      if (Context.CurrentParserInput.Term == _grammar.Eof) {
        Context.AddParserError(Resources.ErrUnexpEof);
        return; 
      }
      if (Context.CurrentParserInput.Term == _grammar.Indent) {
        Context.AddParserError(Resources.ErrUnexpIndent);
        return;
      }
      //General type of error - unexpected input
      //See note about multi-threading issues in ComputeReportedExpectedSet comments.
      if (Context.CurrentParserState.ReportedExpectedSet == null)
        Context.CurrentParserState.ReportedExpectedSet = ComputeReportedExpectedSet(Context.CurrentParserState);
      //Filter out closing braces which are not expected based on previous input.
      // While the closing parenthesis ")" might be expected term in a state in general, 
      // if there was no opening parenthesis in preceding input then we would not
      //  expect a closing one. 
      var expectedSet = FilterBracesInExpectedSet(Context.CurrentParserState.ReportedExpectedSet);
      msg = _grammar.ConstructParserErrorMessage(Context, Context.CurrentParserState, expectedSet, Context.CurrentParserInput);
      if (string.IsNullOrEmpty(msg))
        msg = Resources.ErrSyntaxErrorNoInfo;
      Context.AddParserError(msg);
    }

    // Computes set of expected terms in a parser state. While there may be extended list of symbols expected at some point,
    // we want to reorganize and reduce it. For example, if the current state expects all arithmetic operators as an input,
    // it would be better to not list all operators (+, -, *, /, etc) but simply put "operator" covering them all. 
    // To achieve this grammar writer can group operators (or any other terminals) into named groups using Grammar's methods
    // AddTermReportGroup, AddNoReportGroup etc. Then instead of reporting each operator separately, Irony would include 
    // a single "group name" to represent them all.
    // The "expected report set" is not computed during parser construction (it would bite considerable time), but on demand during parsing, 
    // when error is detected and the expected set is actually needed for error message. 
    // Multi-threading concerns. When used in multi-threaded environment (web server), the LanguageData would be shared in 
    // application-wide cache to avoid rebuilding the parser data on every request. The LanguageData is immutable, except 
    // this one case - the expected sets are constructed late by CoreParser on the when-needed basis. 
    // We don't do any locking here, just compute the set and on return from this function the state field is assigned. 
    // We assume that this field assignment is an atomic, concurrency-safe operation. The worst thing that might happen
    // is "double-effort" when two threads start computing the same set around the same time, and the last one to finish would 
    // leave its result in the state field. 
    private StringSet ComputeReportedExpectedSet(ParserState state) {
      var terms = new TerminalSet(); 
      terms.UnionWith(state.ExpectedTerminals);
      var result = new StringSet(); 
      //Eliminate no-report terminals
      foreach(var group in _grammar.TermReportGroups)
        if (group.GroupType == TermReportGroupType.Exclude) 
            terms.ExceptWith(group.Terminals); 
      //Add normal and operator groups
      foreach(var group in _grammar.TermReportGroups)
        if(group.GroupType == TermReportGroupType.Normal || group.GroupType == TermReportGroupType.Operator && terms.Overlaps(group.Terminals)) {
          result.Add(group.Alias); 
          terms.ExceptWith(group.Terminals);
        }
      //Add remaining terminals "as is"
      foreach(var terminal in terms)
        result.Add(terminal.ErrorAlias); 
      return result;     
    }

    private StringSet FilterBracesInExpectedSet(StringSet stateExpectedSet) {
      var result = new StringSet();
      result.UnionWith(stateExpectedSet);
      //Find what brace we expect
      var nextClosingBrace = string.Empty;
      if (Context.OpenBraces.Count > 0) {
        var lastOpenBraceTerm = Context.OpenBraces.Peek().KeyTerm;
        var nextClosingBraceTerm = lastOpenBraceTerm.IsPairFor as KeyTerm;
        if (nextClosingBraceTerm != null) 
          nextClosingBrace = nextClosingBraceTerm.Text; 
      }

      //Now check all closing braces in result set, and leave only nextClosingBrace
      foreach(var closingBrace in Data.Language.GrammarData.ClosingBraces) {
        if (result.Contains(closingBrace) && closingBrace != nextClosingBrace)
          result.Remove(closingBrace); 
        
      }
      return result; 
    }
    #endregion


  }//class
}//namespace

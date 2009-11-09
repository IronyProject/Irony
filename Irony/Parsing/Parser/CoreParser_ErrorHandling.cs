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
      Context.AddTrace(Resources.MsgTraceRecovering); //add new trace entry
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
      //2. Shift error token - execute shift action
      ExecuteShift(errorShiftAction.NewState);
      //4. Now we need to go along error production until the end, shifting tokens that CAN be shifted and ignoring others.
      //   We shift until we can reduce
      while (Context.CurrentParserInput.Term != _grammar.Eof) {
        //Check if we can reduce
        var action = GetReduceActionInCurrentState();
        if (action != null) {
          //Clear all input token queues and buffered input, reset location back to input position token queues; 
          Parser.Scanner.SetSourceLocation(Context.CurrentParserInput.Span.Location);
          Context.CurrentParserInput = null;
          Context.ParserInputStack.Clear();
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
      Context.CurrentParserInput = null;
      Context.ParserInputStack.Clear();
      Parser.Scanner.SetSourceLocation(location);
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
        if (term.OptionIsSet(TermOptions.IsNotReported)) continue; 
        var nt = term as NonTerminal;
        if (nt == null) continue;
        if (!reducedSet.Contains(nt) && !string.IsNullOrEmpty(nt.DisplayName) && !allFirsts.Contains(nt)) {
          reducedSet.Add(nt);
          allFirsts.UnionWith(nt.Firsts);
        }
      }
      //2. Now go thru all expected terms and add only those that are NOT in the allFirsts set.
      foreach (var term in state.ExpectedTerms) {
        if (term.OptionIsSet(TermOptions.IsNotReported)) continue;
        if (!reducedSet.Contains(term) && !allFirsts.Contains(term) && (term is Terminal || !string.IsNullOrEmpty(term.DisplayName)))
          reducedSet.Add(term);
      }
      //3. Clean-up reduced set: remove pseudo terms
      if (reducedSet.Contains(_grammar.Eof)) reducedSet.Remove(_grammar.Eof);
      if (reducedSet.Contains(_grammar.SyntaxError)) reducedSet.Remove(_grammar.SyntaxError);
      return reducedSet;
    }

    private BnfTermSet FilterBracesInExpectedSet(BnfTermSet defaultSet) {
      var result = new BnfTermSet();
      var lastOpenBrace = Context.OpenBraces.Count > 0 ? Context.OpenBraces.Peek() : null;
      foreach (var bnfTerm in defaultSet) {
        var term = bnfTerm as Terminal;
        if (term == null || !term.OptionIsSet(TermOptions.IsCloseBrace)) {
          result.Add(bnfTerm);
          continue; 
        }
        //we have a close brace
        var skip = lastOpenBrace == null //if there were no opening braces then all close braces should be removed
                 || term != lastOpenBrace.Terminal.IsPairFor;
        if (!skip) 
          result.Add(bnfTerm);
      }//foreach
      return result; 
    }
    #endregion


  }//class
}//namespace

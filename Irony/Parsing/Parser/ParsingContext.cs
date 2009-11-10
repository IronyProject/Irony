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
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Irony.Parsing {

  [Flags]
  public enum ParseOptions {
    GrammarDebugging = 0x01,
    TraceParser = 0x02,
    AnalyzeCode = 0x10,   //run code analysis; effective only in Module mode
  }

  public enum ParseMode {
    File,       //default, continuous input file
    VsLineScan,   // line-by-line scanning in VS integration for syntax highlighting
    CommandLine, //line-by-line from console
  }

  public enum ParserStatus {
    Init, //initial state
    Parsing,
    Previewing, //previewing tokens
    Recovering, //recovering from error
    Accepted,
    AcceptedPartial,
    Error,
  }

  // The purpose of this class is to provide a container for information shared 
  // between parser, scanner and token filters.
  public class ParsingContext {
    public readonly Parser Parser;
    public readonly LanguageData Language;

    //Parser settings
    public ParseOptions Options;
    public ParseMode Mode = ParseMode.File;
    public int MaxErrors = 20; //maximum error count to report
    public int TabWidth = 8;

    #region properties and fields
    //Parser fields
    public ParserState CurrentParserState { get; internal set; }
    public ParseTreeNode CurrentParserInput { get; internal set; }
    internal readonly ParserStack ParserStack = new ParserStack();
    internal readonly ParserStack ParserInputStack = new ParserStack();

    public ParseTree CurrentParseTree { get; internal set; }
    public readonly TokenStack OpenBraces = new TokenStack();
    public ParserTrace ParserTrace = new ParserTrace();

    //Scanner fields
    internal TokenFilterList TokenFilters = new TokenFilterList();
    internal TokenStack BufferedTokens = new TokenStack();
    internal IEnumerator<Token> FilteredTokens; //stream of tokens after filter
    internal TokenStack PreviewTokens = new TokenStack();
    //CurrentToken is used only in Scanner.GetUnfilteredTokens iterator, 
    // but we define it as a field to avoid creating local state in iterator
    internal Token CurrentScannerToken; 

    internal SourceStream SourceStream;
    public ISourceStream Source { get { return SourceStream; } }

    public VsScannerStateMap VsLineScanState; //State variable used in line scanning mode for VS integration

    public ParserStatus Status {
      get { return _parserStatus; }
      set {
        var oldValue = _parserStatus;
        _parserStatus = value;
        OnStatusChanged(oldValue);
      }
    } ParserStatus _parserStatus = ParserStatus.Init;

    //values dictionary to use by custom language implementations to save some temporary values in parse process
    public readonly Dictionary<string, object> Values = new Dictionary<string, object>();
    #endregion 


    #region constructors
    public ParsingContext(Parser parser) {
      this.Parser = parser;
      Language = Parser.Language;
      //We assume that if Irony is compiled in Debug mode, then developer is debugging his grammar/language implementation
#if DEBUG
      Options |= ParseOptions.GrammarDebugging;
#endif
    }
    #endregion


    #region Events: TokenCreated
    public event EventHandler<TokenCreatedEventArgs> TokenCreated;
    TokenCreatedEventArgs _tokenArgs = new TokenCreatedEventArgs(null);

    internal void OnTokenCreated(Token token) {
      if (TokenCreated == null) return;
      _tokenArgs.Token = token;
      TokenCreated(this, _tokenArgs);
    }
    #endregion

    #region Options helper methods
    public bool OptionIsSet(ParseOptions option) {
      return (Options & option) != 0;
    }
    public void SetOption(ParseOptions option, bool value) {
      if (value)
        Options |= option;
      else
        Options &= ~option;
    }
    #endregion

    #region Error handling and tracing
    public void AddParserError(string message, params object[] args) {
      AddParserMessage(ParserErrorLevel.Error, CurrentParserInput.Span.Location, message, args);
    }
    public void AddParserMessage(ParserErrorLevel level, SourceLocation location, string message, params object[] args) {
      if (CurrentParseTree == null) return; 
      if (CurrentParseTree.ParserMessages.Count >= MaxErrors) return;
      if (args != null && args.Length > 0)
        message = string.Format(message, args);
      CurrentParseTree.ParserMessages.Add(new ParserMessage(level, location, message, CurrentParserState));
      if (OptionIsSet(ParseOptions.TraceParser)) 
        ParserTrace.Add( new ParserTraceEntry(CurrentParserState, ParserStack.Top, CurrentParserInput, message, true));
    }

    public void AddTrace(string message, params object[] args) {
      if (!OptionIsSet(ParseOptions.TraceParser)) return;
      if (args != null && args.Length > 0)
        message = string.Format(message, args); 
      ParserTrace.Add(new ParserTraceEntry(CurrentParserState, ParserStack.Top, CurrentParserInput, message, false));
    }

    #endregion

    private void OnStatusChanged(ParserStatus oldStatus) {
      switch (Status) {
        case ParserStatus.Init:
          ParserTrace.Clear();
          CurrentParseTree = null;
          CurrentParserInput = null;
          OpenBraces.Clear();
          CurrentParserState = Language.ParserData.InitialState; //set the current state to InitialState
          ParserStack.Clear();
          ParserStack.Push(new ParseTreeNode(CurrentParserState));
          
          BufferedTokens.Clear();
          ParserInputStack.Clear();
          PreviewTokens.Clear(); 
          break;
      }
      Parser.OnStatusChanged(oldStatus);
      foreach (var filter in TokenFilters)
        filter.OnStatusChanged(oldStatus);
    }

    public void SetSourceLocation(SourceLocation location) {
      foreach (var filter in TokenFilters)
        filter.OnSetSourceLocation(location); 
      SourceStream.Location = location;
    }

  }//class

  // A struct used for packing/unpacking ScannerState int value; used for VS integration.
  // When Terminal produces incomplete token, it sets 
  // this state to non-zero value; this value identifies this terminal as the one who will continue scanning when
  // it resumes, and the terminal's internal state when there may be several types of multi-line tokens for one terminal.
  // For ex., there maybe several types of string literal like in Python. 
  [StructLayout(LayoutKind.Explicit)]
  public struct VsScannerStateMap {
    [FieldOffset(0)]
    public int Value;
    [FieldOffset(0)]
    public byte TerminalIndex;   //1-based index of active multiline term in MultilineTerminals
    [FieldOffset(1)]
    public byte TokenSubType;         //terminal subtype (used in StringLiteral to identify string kind)
    [FieldOffset(2)]
    public short TerminalFlags;  //Terminal flags
  }//struct


}

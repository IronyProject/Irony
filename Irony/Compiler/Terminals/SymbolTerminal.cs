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

namespace Irony.Compiler {

  public class SymbolTerminalTable : Dictionary<string, SymbolTerminal> { }

  //Represents a fixed symbol. 
  // Contains static singleton dictionary, so that SymbolTerminal instances are created only once 
  // for any symbol.
  public class SymbolTerminal : Terminal {
    private SymbolTerminal(string symbol, string name)  : base(name) {
      _symbol = symbol;
      //Overwrite the Key assigned by base class (key derived from name): symbols are matched by value (symbol itself), not by element name
      Key = symbol;
      #region comments
      // Priority - determines the order in which multiple terminals try to match input for a given current char in the input.
      // For a given input char the scanner looks up the collection of terminals that may match this input symbol. It is the order
      // in this collection that is determined by Priority value - the higher the priority, the earlier the terminal gets a chance 
      // to check the input. 
      //Symbols found in grammar by default have lowest priority to allow other terminals (like identifiers)to check the input first.
      // Additionally, longer symbols have higher priority, so symbols like "+=" should have higher priority value than "+" symbol. 
      // As a result, Scanner would first try to match "+=", longer symbol, and if it fails, it will try "+". 
      #endregion
      base.Priority = LowestPriority + symbol.Length;
    }

    public string Symbol {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _symbol; }
    }  string _symbol;

    #region overrides: TryMatch, GetPrefixes(), ToString() 
    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      if (!source.MatchSymbol(_symbol, !Grammar.CaseSensitive))
        return null;
      source.Position += _symbol.Length;
      Token tkn = Token.Create(this, context, source.TokenStart, Symbol);
      return tkn;
    }
    public override IList<string> GetFirsts() {
      return new string[] { _symbol };
    }
    public override string ToString() {
      return _symbol;
    }
    #endregion

    public override void Init(Grammar grammar) {
      base.Init(grammar);
      if (this.EditorInfo != null) return;
      TokenType tknType = TokenType.Identifier;
      if (IsSet(TermOptions.IsOperator))
        tknType |= TokenType.Operator; 
      else if (IsSet(TermOptions.IsDelimiter | TermOptions.IsPunctuation))
        tknType |= TokenType.Delimiter;
      TokenTriggers triggers = TokenTriggers.None;
      if (this.IsSet(TermOptions.IsBrace))
        triggers |= TokenTriggers.MatchBraces;
      this.EditorInfo = new TokenEditorInfo(tknType, TokenColor.Text, triggers);
    }

    //TODO: move Symbols table to Grammar instance
    #region static members: _symbols table of all symbol terminals

    private static SymbolTerminalTable _symbols = new SymbolTerminalTable();
    public static void ClearSymbols() {
      _symbols.Clear();
    }
    public static SymbolTerminal GetSymbol(string symbol) {
      return GetSymbol(symbol, symbol);
    }
    public static SymbolTerminal GetSymbol(string symbol, string name) {
      SymbolTerminal term;
      if (_symbols.TryGetValue(symbol, out term)) {
        //rename symbol if name is provided explicitly (different from symbol itself)
        if (name != symbol && term.Name != name)
          term.Name = name;
        return term;
      }
      string.Intern(symbol);
      term = new SymbolTerminal(symbol, name);
      term.SetOption(TermOptions.IsGrammarSymbol, true);
      _symbols[symbol] = term;
      return term;
    }
    #endregion

    [System.Diagnostics.DebuggerStepThrough]
    public override bool Equals(object obj) {
      return base.Equals(obj);
    }

    [System.Diagnostics.DebuggerStepThrough]
    public override int GetHashCode() {
      return _symbol.GetHashCode();
    }

  }//class


}

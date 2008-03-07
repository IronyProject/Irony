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
      Key = symbol.Trim();  //Symbols are matched by value, not by element name
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
      Token tkn = new Token(this, source.TokenStart, Symbol);
      return tkn;
    }
    public override IList<string> GetFirsts() {
      return new string[] { _symbol };
    }
    public override string ToString() {
      return _symbol;
    }
    #endregion

    #region Operators and Brace-pair information: Precedence, Associativity, IsPairFor
    public int Precedence   {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _precedence; }
      set {_precedence = value;}
    } int  _precedence;

    public Associativity Associativity  {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _associativity; }
      set {_associativity = value;}
    } Associativity  _associativity;

    public SymbolTerminal IsPairFor  {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _isPairFor; }
      set {_isPairFor = value;}
    } SymbolTerminal  _isPairFor;

    #endregion


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

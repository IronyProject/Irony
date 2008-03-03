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

  //Represents a fixed symbol. 
  // Contains static singleton dictionary, so that SymbolTerminal instances are created only once 
  // for any symbol.
  public class SymbolTerminal : Terminal {
    private SymbolTerminal(string symbol, string name)  : base(name) {
      _symbol = symbol;
      Key = symbol.Trim();  //Symbols are matched by value, not by element name
    }

    public string Symbol {
      get { return _symbol; }
    }  string _symbol;

    #region overrides: TryMatch, GetPrefixes(), ToString() 
    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      string text = source.Text;
      int symLen = _symbol.Length;
      if (source.Position + symLen > text.Length)
        return null; 
      if (string.Compare(text, source.Position, _symbol, 0, symLen, !Grammar.CaseSensitive) != 0)
        return null;
      source.Position += symLen;
      Token tkn = new Token(this, source.TokenStart, Symbol);
      return tkn;
    }
    public override IList<string> GetStartSymbols() {
      return new string[] { _symbol };
    }
    public override string ToString() {
      return _symbol;
    }
    #endregion

    #region Operators and Brace-pair information: Precedence, Associativity, IsPairFor
    public int Precedence   {
      get {return _precedence;}
      set {_precedence = value;}
    } int  _precedence;

    public Associativity Associativity  {
      get {return _associativity;}
      set {_associativity = value;}
    } Associativity  _associativity;

    public SymbolTerminal IsPairFor  {
      get {return _isPairFor;}
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
      term.SetFlag(BnfFlags.IsGrammarSymbol, true);
      _symbols[symbol] = term;
      return term;
    }
    #endregion

    public override bool Equals(object obj) {
      return base.Equals(obj);
    }
    public override int GetHashCode() {
      return _symbol.GetHashCode();
    }
  }//class

  public class SymbolTerminalTable : Dictionary<string, SymbolTerminal> { }

}

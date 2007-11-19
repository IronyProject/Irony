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

  //Represents a fixed symbol. Contains static singleton dictionary, so that SymbolTerminal instances are created only once 
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
      if (source.CurrentChar != Symbol[0])
        return null; //quick test for perf improvement
      string text = source.Text;
      int symLen = _symbol.Length;
      if (source.Position + symLen > text.Length)
        return null; 
      if (string.Compare(text, source.TokenStart.Position, _symbol, 0, symLen, !Grammar.CaseSensitive) != 0)
        return null;
      source.Position += symLen;
      Token tkn = new Token(this, source.TokenStart, Symbol);
      return tkn;
    }
    public override IList<string> GetPrefixes() {
      return new string[] { _symbol };
    }
    public override string ToString() {
      return _symbol;
    }
    #endregion


    #region static members: operators,  _symbols table of all symbol terminals
    public static implicit operator SymbolTerminal(string symbol) {
      return GetSymbol(symbol);
    }

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
      _symbols[symbol] = term;
      return term;
    }
    #endregion

  }//class

  public class SymbolTerminalTable : Dictionary<string, SymbolTerminal> { }

}

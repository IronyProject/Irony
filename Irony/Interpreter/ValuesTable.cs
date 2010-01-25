using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Irony.Interpreter {
  public class ValuesTable : Dictionary<Symbol, object> {
    public ValuesTable(int capacity) : base(capacity) { }
  }//class

  public class ValuesList : List<object> { }
/*
  //A global set of global values, attached to top (global) frame.  
  // Allows getting/setting values using string keys while taking into account case sensitivity of the language
  public class GlobalValuesTable : ValuesTable {
    SymbolTable _symbols; 
    bool _caseSensitive;

    internal GlobalValuesTable(int capacity, SymbolTable symbols, bool caseSensitive) : base(capacity) {
      _symbols = symbols;
      _caseSensitive = caseSensitive;
    }

    public object this[string name] {
      get { return base[NameToSymbol(name)]; }
      set { base[NameToSymbol(name)] = value; }
    }

    private Symbol NameToSymbol(string name) {
      var symbol = _symbols.TextToSymbol(name);
      return _caseSensitive? symbol : symbol.LowerSymbol;
    }

  }//class

*/
}

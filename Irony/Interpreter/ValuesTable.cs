using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Irony.Interpreter {
  public class ValuesTable : Dictionary<Symbol, object> {
    public ValuesTable(int capacity) : base(capacity) { }
    public object this[string name] {
      get { return this[SymbolTable.Symbols.TextToSymbol(name)]; }
      set { this[SymbolTable.Symbols.TextToSymbol(name)] = value; }
    }

  }//class

  public class ValuesList : List<object> { }
}

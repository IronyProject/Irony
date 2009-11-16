using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Irony.Tests {
  public class TestGrammar : Grammar {
    public TestGrammar() {
      var root = new NonTerminal("root"); 
      root.Rule = Empty; 
      this.Root = root; 
      this.Delimiters = ",;(){}"; //important for quick-parse tests
    }
  }
}

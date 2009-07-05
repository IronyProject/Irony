using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Irony.Tests {
  public class TestGrammar : Grammar {
    public TestGrammar() {
      this.Root = new NonTerminal("root"); 
      this.Root.Rule = Empty; 
    }
  }
}

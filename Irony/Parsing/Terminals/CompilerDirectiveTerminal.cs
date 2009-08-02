using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {

  public class CompilerDirectiveTerminal : Terminal {
    public string StartSymbol = "#";

    public CompilerDirectiveTerminal(string name) : base(name, TokenCategory.Directive) {
    }

    public override IList<string> GetFirsts() {
      return new string[] { StartSymbol }; 
    }



  }//class
}//namespace

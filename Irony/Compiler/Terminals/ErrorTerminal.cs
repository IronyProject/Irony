using System;
using System.Collections.Generic;
using System.Text;

namespace Irony.Compiler {
  public class ErrorTerminal : Terminal {
    public ErrorTerminal() : base("Error", TokenCategory.Error) { }
    public ErrorTerminal(string name) : base(name, TokenCategory.Error) { }
  }//class
}//namespace

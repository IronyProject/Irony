using System;
using System.Collections.Generic;
using System.Text;

namespace Irony.Compiler {

  public class GrammarErrorException : Exception {
    public GrammarErrorException(string message) : base(message) { }
    public GrammarErrorException(string message, Exception inner) : base(message, inner) { }

  }//class

  public class CompilerException : Exception {
    public CompilerException(string message) : base(message) { }
    public CompilerException(string message, Exception inner) : base(message, inner) { }

  }//class

}//namespace

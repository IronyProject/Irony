using System;
using System.Collections.Generic;
using System.Text;

namespace Irony.Compiler {

  public class GrammarErrorException : Exception {
    public GrammarErrorException(string message) : base(message) { }
    public GrammarErrorException(string message, Exception inner) : base(message, inner) { }

  }//class

  public class IronyException : Exception {
    public IronyException(string message) : base(message) { }
    public IronyException(string message, Exception inner) : base(message, inner) { }

  }//class

}//namespace

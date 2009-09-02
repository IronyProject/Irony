using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Ast {
  public class AstException : Exception {
    public object AstNode; 
    public AstException(object astNode, string message) : base(message) {
      AstNode = astNode; 
    }

  }//class
}

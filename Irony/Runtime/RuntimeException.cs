using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Compiler;

namespace Irony.Runtime {
  public class RuntimeException : Exception {
    public SourceLocation Location;
    public RuntimeException(string message) : base(message) {   }
    public RuntimeException(string message, Exception inner) : base(message, inner) {   }
    public RuntimeException(string message, Exception inner, SourceLocation location) : base(message, inner) {
      Location = location;
    }

  }
}

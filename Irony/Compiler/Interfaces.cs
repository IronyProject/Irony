using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Compiler {
  public interface IParser {
    AstNode Parse(CompilerContext context, IEnumerable<Token> tokenStream);
  }
}

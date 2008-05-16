using System;
using System.Collections.Generic;
using System.Text;
using Irony.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Irony.Tests {
  public class TerminalTestsBase {
    protected CompilerContext _context;
    protected Terminal _terminal;
    protected Token _token;

    [TestInitialize()]
    public void Setup() {
      _context = CompilerContext.CreateDummy();
    }
    protected void SetTerminal(Terminal term) {
      _terminal = term;
      _terminal.Init(_context.Compiler.Grammar);
    }
    //Utilities
    public void TryMatch(string input) {
      SourceFile source = new SourceFile(input, "test");
      _token = _terminal.TryMatch(_context, source);
    }
    public void CheckType(Type type) {
      Assert.IsNotNull(_token, "TryMatch returned null, while token was expected.");
      Type vtype = _token.Value.GetType();
      Assert.IsTrue(vtype == type, "Invalid target type, expected " + type.ToString() + ", found:  " + vtype);
    }

  }//class
}//namespace

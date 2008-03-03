using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using NUnit.Framework.Constraints;
using Irony.Compiler;

namespace Irony.Tests {
  public class TerminalTestsBase {
    //TODO: need to provide a simpler way to run TErminal.TryMatch, without all these extra objects 
    protected Grammar _grammar;
    protected LanguageCompiler _compiler;
    protected CompilerContext _context;
    protected Terminal _terminal;
    protected Token _token;

    [SetUp]
    public void Setup() {
      _grammar = new Grammar();
      _compiler = new LanguageCompiler(_grammar);
      _context = new CompilerContext(_compiler);
    }
    //Utilities
    public void TryMatch(string input) {
      SourceFile source = new SourceFile(input, "test");
      _token = _terminal.TryMatch(_context, source);
      Assert.IsNotNull(_token, "TryMatch returned null, while token was expected.");
    }
    public void CheckType(Type type) {
      Assert.That(_token.Value.GetType() == type, "Invalid target type, expected " + type.ToString() + ".");
    }

  }//class
}//namespace

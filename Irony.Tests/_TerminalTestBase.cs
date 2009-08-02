using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Irony.Tests {
  public class TerminalTestsBase {
    protected TestGrammar _grammar;
    protected LanguageData _language; 
    protected ScannerData _scannerData;
    protected Compiler _compiler; 
    protected CompilerContext _context;
    protected Terminal _terminal;
    protected Token _token;

    [TestInitialize()]
    public void Setup() {
      _grammar = new TestGrammar();
      _language = new LanguageData(_grammar); 
      _compiler = new Compiler(_grammar); 
      _context = new CompilerContext(_compiler);
      _context.CurrentParseTree = new ParseTree(string.Empty, "source"); 
    }
    protected void SetTerminal(Terminal term) {
      _terminal = term;
      _terminal.Init(_compiler.Language.GrammarData);
    }
    //Utilities
    public void TryMatch(string input) {
      SourceStream source = new SourceStream(_language.ScannerData, input);
      _token = _terminal.TryMatch(_context, source);
    }
    public void CheckType(Type type) {
      Assert.IsNotNull(_token, "TryMatch returned null, while token was expected.");
      Type vtype = _token.Value.GetType();
      Assert.IsTrue(vtype == type, "Invalid target type, expected " + type.ToString() + ", found:  " + vtype);
    }

  }//class
}//namespace

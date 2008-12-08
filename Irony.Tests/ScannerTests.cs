using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Irony.Compiler;

namespace Irony.Tests {

  public class ScanTestGrammar : Grammar {
    public ScanTestGrammar() {
      var comment = new CommentTerminal("comment", "/*", "*/");
      base.NonGrammarTerminals.Add(comment);
      var str = new StringLiteral("str", "'");
      var stmt = new NonTerminal("stmt");
      stmt.Rule = str | Empty;
      this.Root = stmt; 
    }
  }//class

  [TestClass]
  public class ScannerTests {
    Grammar _grammar; 
    Token _token;
    Scanner _scanner; 
    CompilerContext _context; 
    int _state; 

    private void Init(Grammar grammar) {
      _grammar = grammar; 
      var cmp = new LanguageCompiler(grammar);
      _scanner = cmp.Scanner;
      _context = new CompilerContext(cmp);
      _context.Mode = CompileMode.VsLineScan;
      _scanner.Prepare(_context, null); 
    }

    private void SetSource(string text) {
      _scanner.VsSetSource(text, 0); 
    }
    private void Read() {
      _token = _scanner.VsReadToken(ref _state);
    }

    [TestMethod]
    public void TestVsScanning() {
      Init(new ScanTestGrammar());
      SetSource(" /*  ");
      Read();
      Assert.IsTrue(_token.IsSet(AstNodeFlags.IsIncomplete), "Expected incomplete token (line 1)");
      Read();
      Assert.IsNull(_token, "NULL expected");
      SetSource(" comment ");
      Read();
      Assert.IsTrue(_token.IsSet(AstNodeFlags.IsIncomplete), "Expected incomplete token (line 2)");
      Read();
      Assert.IsNull(_token, "NULL expected");
      SetSource(" */");
      Read();
      Assert.IsFalse(_token.IsSet(AstNodeFlags.IsIncomplete), "Expected complete token (line 3)");
      Read();
      Assert.IsNull(_token, "Null expected.");

      
    }

  }//class
}//namespace

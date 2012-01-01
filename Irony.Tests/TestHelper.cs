using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Irony.Parsing; 

namespace Irony.Tests {
#if USE_NUNIT
    using NUnit.Framework;
    using TestClass = NUnit.Framework.TestFixtureAttribute;
    using TestMethod = NUnit.Framework.TestAttribute;
    using TestInitialize = NUnit.Framework.SetUpAttribute;
#else
  using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
  
  public static class TestHelper {
    //A skeleton for a grammar with a single terminal, followed by optional terminator
    class TerminalTestGrammar : Grammar {
      public string Terminator;
      public bool SuppressWhitespace; // used in FreeTextLiteral test to suppress skipping whitespace chars
      public TerminalTestGrammar(Terminal terminal, string terminator = null) : base(caseSensitive: true) {
        Terminator = terminator; 
        var rule = new BnfExpression(terminal);
        if (terminator != null) {
          MarkReservedWords(terminator);
          rule += terminator; 
        }
        base.Root = new NonTerminal("Root");
        Root.Rule = rule;
      }

      public override void SkipWhitespace(ISourceStream source) {
        if (SuppressWhitespace)
          return; 
        base.SkipWhitespace(source);
      }
    }//class

    public static Parser CreateParser(Terminal terminal, string terminator = "end", bool suppressWhitespace = false) {
      var grammar = new TerminalTestGrammar(terminal, terminator);
      grammar.SuppressWhitespace = suppressWhitespace;
      var parser = new Parser(grammar);
      return parser; 
    }

    //handy option for stringLiteral tests: we use single quotes in test strings, and they are replaced by double quotes here 
    public static Token ParseInputQ(this Parser parser, string input) {
      return ParseInput(parser, input.Replace("'", "\""));
    }

    public static Token ParseInput(this Parser parser, string input, bool useTerminator = true) {
      var g = (TerminalTestGrammar) parser.Language.Grammar;
      useTerminator &= g.Terminator != null; 
      if (useTerminator)
        input += " " + g.Terminator; 
      var tree = parser.Parse(input);
      //If error, then return this error token, this is probably what is expected.
      var first = tree.Tokens[0];
      if (first.IsError())
        return first; 
      //Verify that last or before-last token is a terminator
      if (useTerminator) {
        Assert.IsTrue(tree.Tokens.Count >= 2, "Wrong # of tokens - expected at least 2. Input: " + input);
        var count = tree.Tokens.Count; 
        //The last is EOF, the one before last should be a terminator
        Assert.AreEqual(g.Terminator, tree.Tokens[count - 2].Text, "Input terminator not found in the second token. Input: " + input); 
      }
      return tree.Tokens[0];
    }

  }//class
}

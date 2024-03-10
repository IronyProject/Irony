using System;
using Irony.Parsing;

namespace Irony.Tests {
#if USE_NUNIT
    using NUnit.Framework;
    using TestClass = NUnit.Framework.TestFixtureAttribute;
    using TestMethod = NUnit.Framework.TestAttribute;
    using TestInitialize = NUnit.Framework.SetUpAttribute;
  using Assert = NUnit.Framework.Legacy.ClassicAssert;
#else
  using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

  [TestClass]
  public class ParseTreeSimplicityTests {

    #region Grammars
    public class IfElseGrammar : Grammar {
      public IfElseGrammar()  {
        var id = new IdentifierTerminal("id");

        Root = new NonTerminal("root");
        Root.Rule = ToTerm("if") + id + id + (ToTerm("else") + id).Q();
      }
    }//if else grammar class

    #endregion

    [TestMethod]
    public void TestIfElse() {

      var grammar = new IfElseGrammar();
      var parser = new Parser(grammar);
      TestHelper.CheckGrammarErrors(parser);

      var parseTree = parser.Parse("if cond a");
      TestHelper.CheckParseErrors(parseTree);
      Assert.IsNotNull(parseTree.Root, "Root not found.");
      Assert.AreEqual("root", parseTree.Root.Term.Name);
      Assert.AreEqual("if", parseTree.Root.ChildNodes[0].Term.Name);
      Assert.AreEqual("id", parseTree.Root.ChildNodes[1].Term.Name);
      Assert.AreEqual("id", parseTree.Root.ChildNodes[2].Term.Name);

      parseTree = parser.Parse("if cond a else b");
      var errorMessage = $"Actual parse tree:{Environment.NewLine}{parseTree.ToXml()}";

      TestHelper.CheckParseErrors(parseTree);
      Assert.IsNotNull(parseTree.Root, "Root not found.");
      Assert.AreEqual("root", parseTree.Root.Term.Name, errorMessage);
      Assert.AreEqual("if", parseTree.Root.ChildNodes[0].Term.Name, errorMessage);
      Assert.AreEqual("id", parseTree.Root.ChildNodes[1].Term.Name, errorMessage);
      Assert.AreEqual("id", parseTree.Root.ChildNodes[2].Term.Name, errorMessage);
      Assert.AreEqual("else", parseTree.Root.ChildNodes[3].ChildNodes[0].Term.Name, errorMessage);
      Assert.AreEqual("id", parseTree.Root.ChildNodes[3].ChildNodes[1].Term.Name, errorMessage);

    }

  }//class
}//namespace

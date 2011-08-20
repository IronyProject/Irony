using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;
using Irony.Tests.Grammars;

namespace Irony.Tests {
#if USE_NUNIT
  using NUnit.Framework;
  using TestClass = NUnit.Framework.TestFixtureAttribute;
  using TestMethod = NUnit.Framework.TestAttribute;
  using TestInitialize = NUnit.Framework.SetUpAttribute;
#else
  using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

  [TestClass]
  public class ConflictResolutionTests {

    // samples to be parsed
    const string FieldSample = "private int SomeField;";
    const string PropertySample = "public string Name {}";
    const string FieldListSample = "private int Field1; public string Field2;";
	const string MixedListSample = @"
      public int Size {}
      private string TableName;
      override void Run()
      {
      }";

    [TestMethod]
    public void TestGrammarWithConflicts1_HasErrors() {
      var grammar = new ConflictResolutionTestGrammar1();
      var parser = new Parser(grammar);
      Assert.IsTrue(parser.Language.Errors.Count > 0);
    }

    [TestMethod]
    public void TestGrammarWithConflicts1_CanParseField() {
      var grammar = new ConflictResolutionTestGrammar1();
      var parser = new Parser(grammar);
      var sample = FieldSample;
      var tree = parser.Parse(sample);
      Assert.IsNotNull(tree);
      Assert.IsFalse(tree.HasErrors());

      Assert.IsNotNull(tree.Root);
      var term = tree.Root.Term as NonTerminal;
      Assert.IsNotNull(term);
      Assert.AreEqual("definition", term.Name);

      Assert.AreEqual(1, tree.Root.ChildNodes.Count);
      var nodes = tree.Root.ChildNodes.Select(t => t.FirstChild).ToArray();
      Assert.AreEqual("fieldModifier", nodes[0].Term.Name);
    }

    [TestMethod]
    public void TestGrammarWithConflicts1_CannotParseProperty() {
      var grammar = new ConflictResolutionTestGrammar1();
      var parser = new Parser(grammar);
      var sample = PropertySample;
      var tree = parser.Parse(sample);
      Assert.IsNotNull(tree);
      Assert.IsTrue(tree.HasErrors());
    }

    [TestMethod]
    public void TestGrammarWithConflicts2_DoesntHaveErrors() {
      var grammar = new ConflictResolutionTestGrammar2();
      var parser = new Parser(grammar);
      Assert.IsTrue(parser.Language.Errors.Count == 0);
    }

    [TestMethod]
    public void TestGrammarWithConflicts2_CanParseField() {
      var grammar = new ConflictResolutionTestGrammar2();
      var parser = new Parser(grammar);
      var sample = FieldSample;
      var tree = parser.Parse(sample);
      Assert.IsNotNull(tree);
      Assert.IsFalse(tree.HasErrors());

      Assert.IsNotNull(tree.Root);
      var term = tree.Root.Term as NonTerminal;
      Assert.IsNotNull(term);
      Assert.AreEqual("definition", term.Name);

      Assert.AreEqual(1, tree.Root.ChildNodes.Count);
      var nodes = tree.Root.ChildNodes.Select(t => t.FirstChild).ToArray();
      Assert.AreEqual("fieldModifier", nodes[0].Term.Name);
    }

    [TestMethod]
    public void TestGrammarWithConflicts2_CanParseProperty() {
      var grammar = new ConflictResolutionTestGrammar2();
      var parser = new Parser(grammar);
      var sample = PropertySample;
      var tree = parser.Parse(sample);
      Assert.IsNotNull(tree);
      Assert.IsFalse(tree.HasErrors());

      Assert.IsNotNull(tree.Root);
      var term = tree.Root.Term as NonTerminal;
      Assert.IsNotNull(term);
      Assert.AreEqual("definition", term.Name);

      Assert.AreEqual(1, tree.Root.ChildNodes.Count);
      var nodes = tree.Root.ChildNodes.Select(t => t.FirstChild).ToArray();
      Assert.AreEqual("propModifier", nodes[0].Term.Name);
    }

    [TestMethod]
    public void TestGrammarWithConflicts3_HasErrors() {
      var grammar = new ConflictResolutionTestGrammar3();
      var parser = new Parser(grammar);
      Assert.IsTrue(parser.Language.Errors.Count > 0);
    }

    [TestMethod]
    public void TestGrammarWithConflicts3_CanParseFieldList() {
      var grammar = new ConflictResolutionTestGrammar3();
      var parser = new Parser(grammar);
      var sample = FieldListSample;
      var tree = parser.Parse(sample);
      Assert.IsNotNull(tree);
      Assert.IsFalse(tree.HasErrors());

      Assert.IsNotNull(tree.Root);
      var term = tree.Root.Term as NonTerminal;
      Assert.IsNotNull(term);
      Assert.AreEqual("StatementList", term.Name);

      Assert.AreEqual(2, tree.Root.ChildNodes.Count);
      var nodes = tree.Root.ChildNodes.Select(t => t.FirstChild).ToArray();
      Assert.AreEqual("fieldDef", nodes[0].Term.Name);
      Assert.AreEqual("fieldDef", nodes[1].Term.Name);
    }

    [TestMethod]
    public void TestGrammarWithConflicts3_CannotParseMixedList() {
      var grammar = new ConflictResolutionTestGrammar3();
      var parser = new Parser(grammar);
      var sample = MixedListSample;
      var tree = parser.Parse(sample);
      Assert.IsNotNull(tree);
      Assert.IsTrue(tree.HasErrors());
    }

    [TestMethod]
    public void TestGrammarWithConflicts4_DoesntHaveErrors() {
      var grammar = new ConflictResolutionTestGrammar4();
      var parser = new Parser(grammar);
      Assert.IsTrue(parser.Language.Errors.Count == 0);
    }

    [TestMethod]
    public void TestGrammarWithConflicts4_CanParseFieldList() {
      var grammar = new ConflictResolutionTestGrammar4();
      var parser = new Parser(grammar);
      var sample = FieldListSample;
      var tree = parser.Parse(sample);
      Assert.IsNotNull(tree);
      Assert.IsFalse(tree.HasErrors());

      Assert.IsNotNull(tree.Root);
      var term = tree.Root.Term as NonTerminal;
      Assert.IsNotNull(term);
      Assert.AreEqual("StatementList", term.Name);

      Assert.AreEqual(2, tree.Root.ChildNodes.Count);
      var nodes = tree.Root.ChildNodes.Select(t => t.FirstChild).ToArray();
      Assert.AreEqual("fieldDef", nodes[0].Term.Name);
      Assert.AreEqual("fieldDef", nodes[1].Term.Name);
    }

    [TestMethod]
    public void TestGrammarWithConflicts4_CanParseMixedList() {
      var grammar = new ConflictResolutionTestGrammar4();
      var parser = new Parser(grammar);
      var sample = MixedListSample;
      var tree = parser.Parse(sample);
      Assert.IsNotNull(tree);
      Assert.IsFalse(tree.HasErrors());

      Assert.IsNotNull(tree.Root);
      var term = tree.Root.Term as NonTerminal;
      Assert.IsNotNull(term);
      Assert.AreEqual("StatementList", term.Name);

      Assert.AreEqual(3, tree.Root.ChildNodes.Count);
      var nodes = tree.Root.ChildNodes.Select(t => t.FirstChild).ToArray();
      Assert.AreEqual("propDef", nodes[0].Term.Name);
      Assert.AreEqual("fieldDef", nodes[1].Term.Name);
      Assert.AreEqual("methodDef", nodes[2].Term.Name);
    }
  }
}

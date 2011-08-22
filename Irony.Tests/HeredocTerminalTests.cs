using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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

    /// <summary>
    /// Summary description for HeredocTerminalTests
    /// </summary>
    [TestClass]
    public class HeredocTerminalTests : TerminalTestsBase {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestHereDocLiteral() {
            var term = new HereDocTerminal("Heredoc","<<",HereDocOptions.None);
            SetTerminal(term);
            TryMatch(@"<<BEGIN
test
BEGIN");
            Assert.IsNotNull(_token, "Failed to produce a token on valid string.");
            Assert.IsNotNull(_token.Value, "Token Value field is null - should be string.");
            Assert.IsTrue((string)_token.Value == "test" + Environment.NewLine, "Token Value is wrong, got {0} of type {1}", _token.Value, _token.Value.GetType().ToString());
        }

        [TestMethod]
        public void TestHereDocIndentedLiteral() {
            var term = new HereDocTerminal("Heredoc", "<<-", HereDocOptions.AllowIndentedEndToken);
            SetTerminal(term);
            TryMatch(@"<<-BEGIN
test
                        BEGIN");
            Assert.IsNotNull(_token, "Failed to produce a token on valid string.");
            Assert.IsNotNull(_token.Value, "Token Value field is null - should be string.");
            Assert.IsTrue((string)_token.Value == "test" + Environment.NewLine, "Token Value is wrong, got {0} of type {1}", _token.Value, _token.Value.GetType().ToString());
        }

        [TestMethod]
        public void TestHereDocLiteralError() {
            var term = new HereDocTerminal("Heredoc","<<",HereDocOptions.None);
            SetTerminal(term);
            TryMatch(@"<<BEGIN
test");
            Assert.IsNotNull(_token, "Failed to produce a token on valid string.");
            Assert.IsTrue(_token.IsError(), "Failed to detect error on invalid heredoc.");
        }

        [TestMethod]
        public void TestHereDocIndentedLiteralError() {
            var term = new HereDocTerminal("Heredoc", "<<-", HereDocOptions.AllowIndentedEndToken);
            SetTerminal(term);
            TryMatch(@"<<-BEGIN
test");
            Assert.IsNotNull(_token, "Failed to produce a token on valid string.");
            Assert.IsTrue(_token.IsError(), "Failed to detect error on invalid heredoc.");
        }

        [TestMethod]
        public void TestHereDocLiteralErrorIndented() {
            var term = new HereDocTerminal("Heredoc", "<<", HereDocOptions.None);
            SetTerminal(term);
            TryMatch(@"<<BEGIN
test
     BEGIN");
            Assert.IsNotNull(_token, "Failed to produce a token on valid string.");
            Assert.IsTrue(_token.IsError(), "Failed to detect error on invalid heredoc.");
        }
    }
}

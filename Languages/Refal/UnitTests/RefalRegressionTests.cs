using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Irony;
using Irony.Interpreter;
using Irony.Parsing;

namespace Refal.UnitTests
{
  #region Unit testing platform abstraction layer
#if NUNIT
	using NUnit.Framework;
	using TestClass = NUnit.Framework.TestFixtureAttribute;
	using TestMethod = NUnit.Framework.TestAttribute;
	using TestContext = System.Object;
#else
  using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
	#endregion

	/// <summary>
	/// Refal regression tests.
	/// Written by Alexey Yakovlev, yallie@yandex.ru.
	/// http://refal.codeplex.com
	/// </summary>
	[TestClass]
	public class RefalRegressionTests
	{
		/// <summary>
		/// Resource name = DefaultNamespace + "." + FolderName + "." + FileName
		/// Example: Irony.Samples.Refal.Samples.hello.ref
		/// </summary>
		const string BaseResourceName = "Refal.UnitTests.Sources.";

		/// <summary>
		/// Initialized by MSTest Framework
		/// </summary>
		public TestContext TestContext { get; set; }

		[TestMethod]
		public void RefalTest_Hello()
		{
			RunSampleAndCompareResults("hello.ref", "hello.txt");
		}

		[TestMethod]
		public void RefalTest_Binary()
		{
			RunSampleAndCompareResults("binary.ref", "binary.txt");
		}

		[TestMethod]
		public void RefalTest_Factorial()
		{
			RunSampleAndCompareResults("factorial.ref", "factorial.txt");
		}

		[TestMethod]
		public void RefalTest_ChangeV1()
		{
			RunSampleAndCompareResults("change-v1.ref", "change.txt");
		}

		[TestMethod]
		public void RefalTest_ChangeV2()
		{
			RunSampleAndCompareResults("change-v2.ref", "change.txt");
		}

		[TestMethod]
		public void RefalTest_Italian()
		{
			RunSampleAndCompareResults("italian.ref", "italian.txt");
		}

		[TestMethod]
		public void RefalTest_Palyndrome()
		{
			RunSampleAndCompareResults("palyndrome.ref", "palyndrome.txt");
		}

    internal class ArithInputAdapter : BufferedConsoleAdapter {
      private Queue<string> SampleInputs = new Queue<string>(new[] {
        "10+20*30-40", "Joe^2 * 5 / Markus^(Carol + 318)"
      });

      public override string ReadLine() {
        var input = SampleInputs.Count > 0 ? SampleInputs.Dequeue() : null;
        if (input != null) {
          WriteLine(input);
        }
        return input;
      }
    }

		[TestMethod]
		public void RefalTest_ArithmeticTranslator()
		{
      RunSampleAndCompareResults("arith.ref", "arith.txt", new ArithInputAdapter());
		}

		[TestMethod]
		public void RefalTest_PrettyPrintExpressions()
		{
			RunSampleAndCompareResults("pretty.ref", "pretty.txt");
		}

		[TestMethod]
		public void RefalTest_QuinePlain()
		{
			RunSampleAndCompareResults("quine-plain.ref");
		}

		[TestMethod]
		public void RefalTest_QuineSimple()
		{
			RunSampleAndCompareResults("quine-simple.ref");
		}

		[TestMethod]
		public void RefalTest_QuineXplained()
		{
			RunSampleAndCompareResults("quine-xplained.ref");
		}

		[TestMethod]
		public void RefalTest_XtrasBigint()
		{
			RunSampleAndCompareResults("xtras-bigint.ref", "xtras-bigint.txt");
		}

		[TestMethod]
		public void RefalTest_XtrasFactorial()
		{
			RunSampleAndCompareResults("xtras-factorial.ref", "xtras-factorial.txt");
		}

		[TestMethod]
		public void RefalTest_99BottlesV1()
		{
			RunSampleAndCompareResults("99-bottles-v1.ref", "99-bottles-v1.txt");
		}

		[TestMethod]
		public void RefalTest_99BottlesV2()
		{
			RunSampleAndCompareResults("99-bottles-v2.ref", "99-bottles-v2.txt");
		}

		[TestMethod]
		public void RefalTest_BrainfuckInterpreter()
		{
			RunSampleAndCompareResults("brainfuck.ref", "brainfuck.txt");
		}

		/// <summary>
		/// Load sample program from resources, run it and check its output
		/// </summary>
		void RunSampleAndCompareResults(string programResourceName, string outputResourceName, IConsoleAdapter console = null)
		{
			var grammar = new RefalGrammar();
			var parser = new Parser(grammar);
			var parseTree = parser.Parse(LoadResourceText(programResourceName));

			Assert.IsNotNull(parseTree);
			Assert.IsFalse(parseTree.HasErrors());

			string result = grammar.RunSample(new RunSampleArgs(parser.Language, null, parseTree, console));
			Assert.IsNotNull(result);
			Assert.AreEqual(LoadResourceText(outputResourceName), result);
		}

		/// <summary>
		/// Load quine program from resources, run it and compare its output with itself
		/// </summary>
		void RunSampleAndCompareResults(string programResourceName)
		{
			RunSampleAndCompareResults(programResourceName, programResourceName);
		}

		/// <summary>
		/// Load sample Refal program or output from assembly resources
		/// </summary>
		string LoadResourceText(string resourceName)
		{
			var asm = this.GetType().Assembly;

			using (var stream = asm.GetManifestResourceStream(BaseResourceName + resourceName))
			{
				Assert.IsNotNull(stream);

				using (var sr = new StreamReader(stream))
				{
					var s = sr.ReadToEnd();
					Assert.IsFalse(string.IsNullOrEmpty(s));

					s = Regex.Replace(s, @"\r\n?", Environment.NewLine);
					return s;
				}
			}
		}
	}
}

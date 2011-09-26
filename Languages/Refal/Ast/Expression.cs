using System;
using System.Collections.Generic;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using Irony.Interpreter;
using Refal.Runtime;

namespace Refal
{
	/// <summary>
	/// Expression is a sequence of symbols, macrodigits, bound variables and function calls
	/// </summary>
	public class Expression : AstNode
	{
		public IList<AstNode> Terms { get; private set; }

		public Expression()
		{
			Terms = new List<AstNode>();
		}

		public override void Init(ParsingContext context, ParseTreeNode parseNode)
		{
			base.Init(context, parseNode);

			foreach (var node in parseNode.ChildNodes)
			{
				if (node.AstNode is AstNode)
					Terms.Add(node.AstNode as AstNode);
			}
		}

		public override System.Collections.IEnumerable GetChildNodes()
		{
			foreach (var term in Terms)
				yield return term;
		}

		public bool IsEmpty
		{
			get { return Terms.Count == 0; }
		}

		protected override object DoEvaluate(ScriptThread thread)
		{
			return EvaluateExpression(thread);
		}

		internal Runtime.PassiveExpression EvaluateExpression(ScriptThread thread)
		{
			// standard prolog
			thread.CurrentNode = this;

			try
			{
				var terms = new List<object>();

				foreach (var term in Terms)
				{
					var result = term.Evaluate(thread);
					terms.Add(result);
				}

				return PassiveExpression.Build(terms.ToArray());
			}
			finally
			{
				// standard epilog
				thread.CurrentNode = Parent;
			}
		}

		public override string ToString()
		{
			return "expression";
		}
	}
}

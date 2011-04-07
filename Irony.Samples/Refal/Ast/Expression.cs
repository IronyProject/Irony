using System;
using System.Collections.Generic;
using Irony.Ast;
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

		public override void EvaluateNode(EvaluationContext context, AstMode mode)
		{
			// evaluate terms
			var initialCount = context.Data.Count;
			foreach (var term in Terms)
				term.Evaluate(context, mode);

			// build passive expression from terms
			var args = new List<object>();
			while (context.Data.Count > initialCount)
				args.Add(context.Data.Pop());

			// build expression and push onto stack
			args.Reverse();
			context.Data.Push(PassiveExpression.Build(args.ToArray()));
		}

		public override string ToString()
		{
			return "expression";
		}
	}
}

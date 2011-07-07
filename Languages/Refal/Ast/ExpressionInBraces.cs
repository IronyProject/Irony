using System;
using System.Collections.Generic;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using Irony.Interpreter;
using Refal.Runtime;

namespace Refal
{
	/// <summary>
	/// Expression or pattern in structure braces ()
	/// </summary>
	public class ExpressionInBraces : AstNode
	{
		public AstNode InnerExpression { get; private set; }

		public override void Init(ParsingContext context, ParseTreeNode parseNode)
		{
			base.Init(context, parseNode);

			foreach (var node in parseNode.ChildNodes)
			{
				if (node.AstNode is AstNode)
					InnerExpression = (node.AstNode as AstNode);
			}
		}

		public override System.Collections.IEnumerable GetChildNodes()
		{
			return InnerExpression.GetChildNodes();
		}

		public override void EvaluateNode(EvaluationContext context, AstMode mode)
		{
			context.Data.Push(new OpeningBrace());
			if (InnerExpression != null)
				InnerExpression.Evaluate(context, mode);
			context.Data.Push(new ClosingBrace());
		}

		public override string ToString()
		{
			return "(structure braces)";
		}
	}
}

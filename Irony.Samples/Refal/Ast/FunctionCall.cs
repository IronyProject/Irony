using System;
using System.Linq;
using System.Collections.Generic;
using Irony.Ast;
using Irony.Parsing;
using Irony.Interpreter;
using Refal.Runtime;

namespace Refal
{
	using IronySymbol = Irony.Parsing.Symbol;

	/// <summary>
	/// Function call
	/// </summary>
	public class FunctionCall : AstNode
	{
		public IronySymbol FunctionName { get; private set; }

		public Expression Expression { get; private set; }

		private SourceSpan? NameSpan { get; set; }

		public override void Init(ParsingContext context, ParseTreeNode parseNode)
		{
			base.Init(context, parseNode);

			foreach (var node in parseNode.ChildNodes)
			{
				if (node.AstNode is AuxiliaryNode)
				{
					var auxNode = node.AstNode as AuxiliaryNode;
					NameSpan = auxNode.Span;

					// function call can be either Identifier: <Prout e.1>
					var identifier = auxNode.ChildNodes.OfType<IdentifierNode>().FirstOrDefault();
					if (identifier != null)
					{
						FunctionName = identifier.Symbol;
						continue;
					}

					// or symbol of arithmetic operation: <- s.1 1>
					var pnode = auxNode.ChildParseNodes.Where(n => n.Term != null).FirstOrDefault();
					if (pnode != null)
					{
						FunctionName = SymbolTable.Symbols.TextToSymbol(pnode.Term.Name);
						continue;
					}

					// and nothing else
					throw new InvalidOperationException("Invalid function call");
				}
				else if (node.AstNode is Expression)
				{
					Expression = (node.AstNode as Expression);
				}
			}
		}

		public override System.Collections.IEnumerable GetChildNodes()
		{
			return Expression.GetChildNodes();
		}

		public override void EvaluateNode(EvaluationContext context, AstMode mode)
		{
			Expression.Evaluate(context, mode);

			object value;
			if (context.TryGetValue(FunctionName, out value))
			{
				ICallTarget function = value as ICallTarget;
				if (function == null)
					context.ThrowError("This identifier cannot be called: {0}", FunctionName);

				function.Call(context);
				return;
			}

			context.ThrowError("Unknown identifier: {0}", FunctionName.Text);
		}

		public override SourceLocation GetErrorAnchor()
		{
			return (NameSpan != null ? NameSpan.Value : Span).Location;
		}

		public override string ToString()
		{
			return "call " + FunctionName;
		}
	}
}

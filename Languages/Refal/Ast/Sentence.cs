// Refal5.NET interpreter
// Written by Alexey Yakovlev <yallie@yandex.ru>
// http://refal.codeplex.com

using System.Linq;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using Refal.Runtime;

namespace Refal
{
	/// <summary>
	/// Sentence is an element of a function.
	/// There are two possible forms of sentences:
	/// 1) pattern { conditions } = expression;
	/// 2) pattern conditions block;
	/// </summary>
	public class Sentence : AstNode
	{
		public Pattern Pattern { get; private set; }

		public Conditions Conditions { get; private set; }

		public Expression Expression { get; private set; }

		public Runtime.Pattern BlockPattern { get; set; }

		public PassiveExpression InputExpression { get; set; }

		public override void Init(ParsingContext context, ParseTreeNode parseNode)
		{
			base.Init(context, parseNode);
			
			foreach (var node in parseNode.ChildNodes)
			{
				if (node.AstNode is Pattern)
				{
					Pattern = node.AstNode as Pattern;
				}
				else if (node.AstNode is AuxiliaryNode)
				{
					var nodes = (node.AstNode as AuxiliaryNode).ChildNodes;
					Conditions = nodes.OfType<Conditions>().FirstOrDefault();
					Expression = nodes.OfType<Expression>().FirstOrDefault();
				}
			}

			foreach (var astNode in new AstNode[] { Pattern, Conditions, Expression })
			{
				if (astNode != null)
				{
					astNode.Parent = this;
				}
			}
		}

		public override System.Collections.IEnumerable GetChildNodes()
		{
			yield return Pattern;

			if (Conditions != null)
				yield return Conditions;

			if (Expression != null)
				yield return Expression;
		}

		protected override object DoEvaluate(ScriptThread thread)
		{
			// standard prolog
			thread.CurrentNode = this;

			try
			{
				// evaluate pattern and copy bound variables of the current block
				var patt = Pattern.Instantiate(thread);
				if (BlockPattern != null)
				{
					patt.CopyBoundVariables(BlockPattern);
				}

				// if pattern is recognized, calculate new expression and return true
				var result = patt.Match(InputExpression);
				if (result)
				{
					// store last recognized pattern as a local variable
					thread.SetLastPattern(patt);

					// matching, return expression
					if (Expression != null)
					{
						return Expression.Evaluate(thread);
					}

					// matching succeeded? it depends on conditions
					if (Conditions != null)
					{
						return Conditions.Evaluate(thread);
					}
				}

				// matching failed
				return null;
			}
			finally
			{
				// standard epilog
				thread.CurrentNode = Parent;
			}
		}

		public override string ToString()
		{
			return "match";
		}
	}
}

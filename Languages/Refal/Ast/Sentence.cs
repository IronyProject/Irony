using System;
using System.Linq;
using System.Collections.Generic;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using Irony.Interpreter;
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
		}

		public override System.Collections.IEnumerable GetChildNodes()
		{
			yield return Pattern;

			if (Conditions != null)
				yield return Conditions;

			if (Expression != null)
				yield return Expression;
		}

		public override void EvaluateNode(EvaluationContext context, AstMode mode)
		{
			// evaluate pattern and copy bound variables of the current block
			var patt = Pattern.Instantiate(context, mode);
			if (BlockPattern != null)
			{
				patt.CopyBoundVariables(BlockPattern);
			}

			// pop expression from evaluation stack
			var expr = context.Data.Pop() as Runtime.PassiveExpression;

			// if pattern is recognized, calculate new expression and return true
			var result = patt.Match(expr);
			if (result)
			{
				// store last recognized pattern as a local variable
				context.SetLastPattern(patt);

				// match succeeded, return expression
				if (Expression != null)
				{
					Expression.Evaluate(context, AstMode.Read);
					context.Data.Push(true);
					return;
				}

				// match succeeded? it depends on conditions
				if (Conditions != null)
				{
					Conditions.Evaluate(context, mode);

					// check if conditions succeeded
					result = Convert.ToBoolean(context.Data.Pop());
					if (result)
					{
						context.Data.Push(true);
						return;
					}
				}
			}

			// push expression back for the next sentence
			context.Data.Push(expr);
			context.Data.Push(false); // match failed
		}

		public override string ToString()
		{
			return "match";
		}
	}
}

// Refal5.NET interpreter
// Written by Alexey Yakovlev <yallie@yandex.ru>
// http://refal.codeplex.com

using System;
using System.Linq;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace Refal
{
	/// <summary>
	/// Where- and When-clauses.
	/// </summary>
	public class Conditions : AstNode
	{
		public Expression Expression { get; private set; }

		public Pattern Pattern { get; private set; }

		public Conditions MoreConditions { get; private set; }

		public Expression ResultExpression { get; private set; }

		public Block Block { get; private set; }

		public override void Init(ParsingContext context, ParseTreeNode parseNode)
		{
			base.Init(context, parseNode);

			foreach (var node in parseNode.ChildNodes)
			{
				if (node.AstNode is Expression)
				{
					Expression = (node.AstNode as Expression);
				}
				else if (node.AstNode is AuxiliaryNode)
				{
					var nodes = (node.AstNode as AuxiliaryNode).ChildNodes;
					Pattern = nodes.OfType<Pattern>().FirstOrDefault();
					Block = nodes.OfType<Block>().FirstOrDefault();
					MoreConditions = nodes.OfType<Conditions>().FirstOrDefault();
					ResultExpression = nodes.OfType<Expression>().FirstOrDefault();
				}

				foreach (var astNode in new AstNode[] { Expression, Pattern, Block, MoreConditions, ResultExpression })
				{
					if (astNode != null)
						astNode.Parent = this;
				}
			}

			AsString = Block != null ? "with-clause" : "where-clause";
		}

		public override System.Collections.IEnumerable GetChildNodes()
		{
			yield return Expression;

			if (Block != null)
				yield return Block;

			if (Pattern != null)
				yield return Pattern;

			if (ResultExpression != null)
				yield return ResultExpression;

			if (MoreConditions != null)
				yield return MoreConditions;
		}

		protected override object DoEvaluate(ScriptThread thread)
		{
			// standard prolog
			thread.CurrentNode = this;

			try
			{
				// evaluate expression
				var expression = Expression.EvaluateExpression(thread);

				// extract last recognized pattern (it contains bound variables)
				var lastPattern = thread.GetLastPattern();
				if (lastPattern == null)
				{
					thread.ThrowScriptError("Internal error: last recognized pattern is lost.");
				}

				// with-clause
				if (Block != null)
				{
					Block.InputExpression = expression;
					Block.BlockPattern = lastPattern;
					return Block.Evaluate(thread);
				}

				// where-clause
				if (Pattern != null)
				{
					return EvaluateWhereClause(expression, lastPattern, thread);
				}

				return false;
			}
			finally
			{
				// standard epilog
				thread.CurrentNode = Parent;
			}
		}

		bool EvaluateWhereClause(Runtime.PassiveExpression expr, Runtime.Pattern lastPattern, ScriptThread thread)
		{
			// instantiate where-clause pattern
			var patt = Pattern.Instantiate(thread);
			patt.CopyBoundVariables(lastPattern);

			// perform matching
			var result = patt.Match(expr);
			if (result)
			{
				// store last recognized pattern as a local variable
				thread.SetLastPattern(patt);

				// match succeeded, return true
				if (ResultExpression != null)
				{
					return Convert.ToBoolean(ResultExpression.Evaluate(thread));
				}

				// match succeeded? depends on more conditions
				if (MoreConditions != null)
				{
					// return true or false
					return Convert.ToBoolean(MoreConditions.Evaluate(thread));
				}
			}

			// matching failed, return false
			return false;
		}
	}
}

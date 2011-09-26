// Refal5.NET interpreter
// Written by Alexey Yakovlev <yallie@yandex.ru>
// http://refal.codeplex.com

using System;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace Refal
{
	/// <summary>
	/// Variable is a part of refal expression that can be bound to a value.
	/// Being part of a pattern is not bound to a value and is called "free variable".
	/// In an expression to the right of "=" variable is bound to a value.
	/// </summary>
	public abstract class Variable : AstNode
	{
		public virtual string Index { get; protected set; }

		public static void CreateVariableNode(ParsingContext context, ParseTreeNode parseNode)
		{
			Variable varNode = null;

			foreach (ParseTreeNode nt in parseNode.ChildNodes)
			{
				// (e | s | t)
				if (varNode == null)
				{
					switch (nt.Term.Name)
					{
						case "s":
							varNode = new SymbolVariable();
							break;

						case "e":
							varNode = new ExpressionVariable();
							break;

						case "t":
							varNode = new TermVariable();
							break;

						default:
							throw new ArgumentOutOfRangeException("Unknown variable type: " + nt.Term.Name);
					}
					continue;
				}

				if (nt.Term.Name == ".")
					continue;

				// Number | Identifier
				if (nt.AstNode is LiteralValueNode)
				{
					varNode.Index = (nt.AstNode as LiteralValueNode).Value.ToString();
				}
				else if (nt.AstNode is IdentifierNode)
				{
					varNode.Index = (nt.AstNode as IdentifierNode).Symbol;
				}
			}

			varNode.Span = parseNode.Span;
			parseNode.AstNode = varNode;
		}

		protected override object DoEvaluate(ScriptThread thread)
		{
			// standard prolog
			thread.CurrentNode = this;

			try
			{
				// get last recognized pattern
				var pattern = thread.GetLastPattern();
				if (pattern == null)
				{
					thread.ThrowScriptError("No pattern recognized!");
					return null;
				}

				// read variable from the last recognized pattern
				return pattern.GetVariable(Index);
			}
			finally
			{
				// standard epilog
				thread.CurrentNode = Parent;
			}
		}

		/// <summary>
		/// Create pattern variable
		/// </summary>
		public abstract Runtime.Variable CreateVariable();

		public override string ToString()
		{
			return Index;
		}
	}
}

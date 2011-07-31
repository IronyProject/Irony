using System;
using System.Collections.Generic;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using Irony.Interpreter;
using Refal.Runtime;

namespace Refal
{
	/// <summary>
	/// DefinedFunction is a function defined in the current compulation unit
	/// </summary>
	public class DefinedFunction : Function
	{
		public Block Block { get; private set; }

		public bool IsPublic { get; private set; }

		public override void Init(ParsingContext context, ParseTreeNode parseNode)
		{
			base.Init(context, parseNode);

			foreach (var node in parseNode.ChildNodes)
			{
				if (node.AstNode is IdentifierNode)
				{
					Name = (node.AstNode as IdentifierNode).Symbol;
				}
				else if (node.AstNode is Block)
				{
					Block = (node.AstNode as Block);
				}
				else if (node.Term is KeyTerm && node.Term.Name == "$ENTRY")
				{
					IsPublic = true;
				}
			}
		}

		public override System.Collections.IEnumerable GetChildNodes()
		{
			return Block.GetChildNodes();
		}

		public override void Call(ScriptAppInfo context)
		{
			context.PushFrame(Name, null, context.CurrentFrame); // AstNode argument
			Block.Evaluate(context, AstMode.None);
			context.PopFrame();
		}

		public override string ToString()
		{
			return (IsPublic ? "public " : "private ") + Name;
		}
	}
}

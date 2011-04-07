using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Irony.Ast;
using Irony.Parsing;

namespace Refal
{
	/// <summary>
	/// Initializes Refal literal nodes
	/// </summary>
	public static class LiteralValueNodeHelper
	{
		/// <summary>
		/// Converts identifiers to compound symbols (strings in double quotes),
		/// expands character strings (in single quotes) to arrays of characters
		/// </summary>
		public static void InitNode(ParsingContext context, ParseTreeNode parseNode)
		{
			foreach (var node in parseNode.ChildNodes)
			{
				if (node.AstNode is LiteralValueNode)
				{
					if (node.Term.Name == "Char")
					{
						var literal = node.AstNode as LiteralValueNode;
						literal.Value = literal.Value.ToString().ToCharArray();
					}

					parseNode.AstNode = node.AstNode;
				}
				else
				{
					// identifiers in expressions are treated as strings (True is same as "True")
					parseNode.AstNode = new LiteralValueNode()
					{
						Value = node.FindTokenAndGetText(),
						Span = node.Span
					};
				}
			}
		}
	}
}

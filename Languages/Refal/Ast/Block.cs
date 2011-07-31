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
	/// Block is a sequence of sentences
	/// </summary>
	public class Block : AstNode
	{
		public IList<Sentence> Sentences { get; private set; }

		public Runtime.Pattern BlockPattern { get; set; }

		public Block()
		{
			Sentences = new List<Sentence>();
		}

		public override void Init(ParsingContext context, ParseTreeNode parseNode)
		{
			base.Init(context, parseNode);

			foreach (var node in parseNode.ChildNodes)
			{
				// copy sentences to block
				if (node.AstNode is AuxiliaryNode)
				{
					var auxNode = node.AstNode as AuxiliaryNode;
					
					foreach (var s in auxNode.ChildNodes.OfType<Sentence>())
						Sentences.Add(s);
				}
			}
		}

		public override System.Collections.IEnumerable GetChildNodes()
		{
			foreach (Sentence s in Sentences)
				yield return s;
		}

		public override void EvaluateNode(ScriptAppInfo context, AstMode mode)
		{
			foreach (Sentence sentence in Sentences)
			{
				sentence.BlockPattern = BlockPattern;
				sentence.Evaluate(context, mode);

				// if some sentence is evaluated to true, then stop
				var result = context.Data.Pop();
				if (Convert.ToBoolean(result) == true)
					return;
			}

			context.ThrowError("Recognition impossible");
		}

		public override string ToString()
		{
			return "block";
		}
	}
}

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

		protected override object DoEvaluate(ScriptThread thread)
		{
			// standard prolog
			thread.CurrentNode = this;

			try
			{
				foreach (var sentence in Sentences)
				{
					sentence.BlockPattern = BlockPattern;
					if (Convert.ToBoolean(sentence.Evaluate(thread)))
						return true;
				}

				thread.ThrowScriptError("Recognition impossible");
				return false;
			}
			finally
			{
				// standard epilog
				thread.CurrentNode = Parent;
			}
		}

		public override string ToString()
		{
			return "block";
		}
	}
}

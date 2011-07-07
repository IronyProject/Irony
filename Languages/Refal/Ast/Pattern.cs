using System;
using System.Collections.Generic;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using Irony.Interpreter;
using Refal.Runtime;

namespace Refal
{
	/// <summary>
	/// Pattern is a passive expression that may contain free variables
	/// </summary>
	public class Pattern : AstNode
	{
		public IList<AstNode> Terms { get; private set; }

		public bool IsEmpty
		{
			get { return Terms.Count == 0; }
		}

		public Pattern()
		{
			Terms = new List<AstNode>();
		}

		public override void Init(ParsingContext context, ParseTreeNode parseNode)
		{
			base.Init(context, parseNode);
			
			foreach (var node in parseNode.ChildNodes)
			{
				if (node.AstNode is AstNode)
					Terms.Add(node.AstNode as AstNode);
			}
		}

		public override System.Collections.IEnumerable GetChildNodes()
		{
			foreach (var term in Terms)
				yield return term;
		}

		public override void EvaluateNode(EvaluationContext context, AstMode mode)
		{
			foreach (var term in Terms)
			{
				// in pattern, variables are never read
				mode = term is Variable ? AstMode.None : AstMode.Read;
				term.Evaluate(context, mode);
			}
		}

		private object[] EvaluateTerms(EvaluationContext context, AstMode mode)
		{
			// save initial stack position
			var initialCount = context.Data.Count;
			Evaluate(context, mode);

			// get terms from evaluation stack
			var args = new List<object>();
			while (context.Data.Count > initialCount)
				args.Add(context.Data.Pop());

			// restore original order
			args.Reverse();
			return args.ToArray();
		}
		
		public Runtime.Pattern Instantiate(EvaluationContext context, AstMode mode)
		{
			// evaluate pattern and instantiate Runtime.Pattern
			return new Runtime.Pattern(EvaluateTerms(context, mode));
		}

		public override string ToString()
		{
			return "pattern";
		}
	}
}

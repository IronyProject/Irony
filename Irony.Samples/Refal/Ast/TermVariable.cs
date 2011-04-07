using System;
using System.Collections.Generic;
using Irony.Ast;
using Irony.Parsing;
using Irony.Interpreter;
using Refal.Runtime;

namespace Refal
{
	/// <summary>
	/// Variable of form t.X that can be bound either to a symbol or to expression in a structure braces
	/// </summary>
	public class TermVariable : Variable
	{
		public override string Index
		{
			get { return base.Index; }
			protected set { base.Index = "t." + value; }
		}

		public override Runtime.Variable CreateVariable()
		{
			return new Runtime.TermVariable(Index);
		}
	}
}

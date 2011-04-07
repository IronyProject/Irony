using System;
using System.Collections.Generic;
using Irony.Ast;
using Irony.Parsing;
using Irony.Interpreter;
using Refal.Runtime;

namespace Refal
{
	/// <summary>
	/// Variable of form e.X that can be bound to any expression
	/// </summary>
	public class ExpressionVariable : Variable
	{
		public override string Index
		{
			get { return base.Index; }
			protected set { base.Index = "e." + value; }
		}

		public override Runtime.Variable CreateVariable()
		{
			return new Runtime.ExpressionVariable(Index);
		}
	}
}

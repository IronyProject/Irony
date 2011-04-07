using System;
using System.Collections.Generic;
using Irony.Ast;
using Irony.Parsing;
using Irony.Interpreter;
using Refal.Runtime;

namespace Refal
{
	/// <summary>
	/// Variable of form s.X that can be bound to single symbol
	/// </summary>
	public class SymbolVariable : Variable
	{
		public override string Index
		{
			get { return base.Index; }
			protected set { base.Index = "s." + value; }
		}

		public override Runtime.Variable CreateVariable()
		{
			return new Runtime.SymbolVariable(Index);
		}
	}
}

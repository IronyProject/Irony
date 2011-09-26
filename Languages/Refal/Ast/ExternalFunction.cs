using System;
using System.Collections.Generic;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using Irony.Interpreter;
using Refal.Runtime;

namespace Refal
{
	/// <summary>
	/// External function is a library function referenced from the current compilation unit
	/// External functions are not supported yet
	/// </summary>
	public class ExternalFunction : Function
	{
		public void SetSpan(SourceSpan sourceSpan)
		{
			Span = sourceSpan;
		}
		
		public override System.Collections.IEnumerable GetChildNodes()
		{
			yield break;
		}

		public override void Call(ScriptAppInfo context)
		{
			context.ThrowError("Calling external function is not supported");
		}

		public override string ToString()
		{
			return "extern " + Name;
		}
	}
}

using System;
using System.Collections.Generic;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using Irony.Interpreter;
using Refal.Runtime;

namespace Refal
{
	/// <summary>
	/// Base node for all functions
	/// </summary>
	public abstract class Function : AstNode, ICallTarget
	{
		public string Name { get; set; } // TODO: value.Replace("-", "__")

		public override void EvaluateNode(ScriptAppInfo context, AstMode mode)
		{
			// define function
			context.SetValue(Name, this);
		}

		public abstract void Call(ScriptAppInfo context);
	}
}

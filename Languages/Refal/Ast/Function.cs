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

		protected override object DoEvaluate(ScriptThread thread)
		{
			// standard prolog
			thread.CurrentNode = this;

			try
			{
				// define function: bind function name to the current instance
				var binding = thread.Bind(Name, BindingOptions.Write);
				binding.SetValueRef(thread, this);
				return null;
			}
			finally
			{
				// standard epilog
				thread.CurrentNode = Parent;
			}
		}

		public virtual abstract object Call(ScriptThread thread, object[] parameters);
	}
}

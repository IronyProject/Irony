using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Irony.Parsing;
using Irony.Interpreter;

namespace Refal
{
	/// <summary>
	/// Refal script thread should be able to store the last recognized pattern
	/// </summary>
	public static class ScriptThreadExtensions
	{
		/// Unique names for implicit local variables
		static string LastPatternSymbolName = Guid.NewGuid().ToString();
		static string DataStackSymbolName = Guid.NewGuid().ToString();

		/// <summary>
		/// Retrieve last evaluated pattern
		/// </summary>
		public static Refal.Runtime.Pattern GetLastPattern(this ScriptThread thread)
		{
			var binding = thread.Bind(LastPatternSymbolName, BindingOptions.Read);
			return binding.GetValueRef(thread) as Refal.Runtime.Pattern;
		}

		/// <summary>
		/// Set last evaluated pattern
		/// </summary>
		public static void SetLastPattern(this ScriptThread thread, Refal.Runtime.Pattern pattern)
		{
			var binding = thread.Bind(LastPatternSymbolName, BindingOptions.Write);
			binding.SetValueRef(thread, pattern);
		}

		/// <summary>
		/// Retrieve data stack of the given thread.
		/// </summary>
		public static Stack<object> GetDataStack(this ScriptThread thread)
		{
			var binding = thread.Bind(DataStackSymbolName, BindingOptions.Read | BindingOptions.Write);
			var stack = binding.GetValueRef(thread) as Stack<object>;
			if (stack != null)
			{
				return stack;
			}

			stack = new Stack<object>();
			binding.SetValueRef(thread, stack);
			return stack;
		}
	}
}

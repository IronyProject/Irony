using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Irony.Parsing;
using Irony.Interpreter;

namespace Refal
{
	/// <summary>
	/// Refal evaluation context should be able to store the last recognized pattern
	/// </summary>
	public static class EvaluationContextExtensions
	{
		/// <summary>
		/// Generate unique name for implicit local variable
		/// </summary>
		static string LastPatternSymbolName = Guid.NewGuid().ToString();

		/// <summary>
		/// Retrieve last evaluated pattern
		/// </summary>
		public static Refal.Runtime.Pattern GetLastPattern(this ScriptAppInfo context)
		{
			object pattern;

			if (context.TryGetValue(LastPatternSymbolName, out pattern))
				return pattern as Refal.Runtime.Pattern;

			return null;
		}

		/// <summary>
		/// Set last evaluated pattern
		/// </summary>
		public static void SetLastPattern(this ScriptAppInfo context, Refal.Runtime.Pattern pattern)
		{
			context.SetValue(LastPatternSymbolName, pattern);
		}
	}
}

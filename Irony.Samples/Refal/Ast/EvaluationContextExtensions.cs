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
		/// Convert string to Symbol using Evaluation Context's symbol table
		/// </summary>
		static Symbol GetLastPatternName(EvaluationContext context)
		{
			return SymbolTable.Symbols.TextToSymbol(LastPatternSymbolName);
		}

		/// <summary>
		/// Retrieve last evaluated pattern
		/// </summary>
		public static Refal.Runtime.Pattern GetLastPattern(this EvaluationContext context)
		{
			object pattern;

			if (context.TryGetValue(GetLastPatternName(context), out pattern))
				return pattern as Refal.Runtime.Pattern;

			return null;
		}

		/// <summary>
		/// Set last evaluated pattern
		/// </summary>
		public static void SetLastPattern(this EvaluationContext context, Refal.Runtime.Pattern pattern)
		{
			context.SetValue(GetLastPatternName(context), pattern);
		}
	}
}

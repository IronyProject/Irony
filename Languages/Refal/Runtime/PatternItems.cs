// Refal5.NET runtime
// Written by Alexey Yakovlev <yallie@yandex.ru>
// http://refal.codeplex.com

using System;

namespace Refal.Runtime
{
	/// <summary>
	/// Pattern contains pattern items of three types: symbols, braces and variables.
	/// Pattern items can match given expression
	/// </summary>
	public abstract class PatternItem
	{
		public PatternItem()
		{
		}

		// match expression at exIndex pointer, advance pointer as needed
		public abstract MatchResult Match(PassiveExpression expression, ref int exIndex, int patIndex);
	}

	/// <summary>
	/// MatchResult represents result of matching operation
	/// </summary>
	public enum MatchResult
	{
		/// <summary>
		/// Failure means than expression item don't match pattern item
		/// </summary>
		Failure,

		/// <summary>
		/// Success means that item matches unambiguously
		/// </summary>
		Success,

		/// <summary>
		/// PartialSuccess means that item matches ambiguously (can possibly find another match)
		/// </summary>
		PartialSuccess
	}

	public class Symbol : PatternItem
	{
		object Value { get; set; }

		public Symbol(object value)
		{
			Value = value;
		}

		public override MatchResult Match(PassiveExpression expression, ref int exIndex, int patIndex)
		{
			if (expression == null || exIndex >= expression.Count)
				return MatchResult.Failure;

			// symbol matches single symbol
			if (Value.Equals(expression[exIndex]))
			{
				exIndex++;
				return MatchResult.Success;
			}

			return MatchResult.Failure;
		}
	}

	public abstract class StructureBrace : PatternItem
	{
		public StructureBrace()
		{
		}

		public override MatchResult Match(PassiveExpression expression, ref int exIndex, int patIndex)
		{
			if (expression == null || exIndex >= expression.Count)
				return MatchResult.Failure;

			// opening brace <=> opening brace, closing brace <=> closing brace
			if (expression[exIndex].GetType() == GetType())
			{
				exIndex++;
				return MatchResult.Success;
			}

			return MatchResult.Failure;
		}
	}

	public class OpeningBrace : StructureBrace
	{
		public OpeningBrace()
		{
		}

		public override string ToString()
		{
			return "(";
		}
	}

	public class ClosingBrace : StructureBrace
	{
		public ClosingBrace()
		{
		}

		public override string ToString()
		{
			return ")";
		}
	}
}

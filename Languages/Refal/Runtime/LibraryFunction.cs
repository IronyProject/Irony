// Refal5.NET runtime
// Written by Alexey Yakovlev <yallie@yandex.ru>
// http://refal.codeplex.com

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using Irony.Interpreter.Ast;
using Irony.Interpreter;

namespace Refal.Runtime
{
	using Irony.Parsing;

	/// <summary>
	/// LibraryFunction is a function defined in the standard library and available to Refal program
	/// </summary>
	public class LibraryFunction : ICallTarget
	{
		public string Name { get; private set; }

		private LibraryDelegate Function { get; set; }

		delegate PassiveExpression LibraryDelegate(PassiveExpression value);

		private LibraryFunction(string n, LibraryDelegate fun)
		{
			Name = n;
			Function = fun;
		}

		public void Call(EvaluationContext context)
		{
			context.PushFrame(Name, null, context.CurrentFrame);

			var ex = Function(context.Data.Pop() as PassiveExpression);
			if (ex != null)
				context.Data.Push(ex);

			context.PopFrame();
		}

		public static LibraryFunction[] ExtractLibraryFunctions(EvaluationContext context, object instance)
		{
			if (instance == null)
				return new LibraryFunction[0];

			var list = new List<LibraryFunction>();

			MethodInfo[] methods = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
			foreach (MethodInfo method in methods)
			{
				var fun = method.CreateDelegate<LibraryDelegate>(instance, false);
				if (fun != null)
				{
					var fname = method.GetCustomAttribute<FunctionNamesAttribute>();
					var names = (fname == null) ? new string[] { method.Name } : fname.Names;
					foreach (var strName in names)
					{
						string name = context.LanguageCaseSensitive ? strName : strName.ToLowerInvariant();
						list.Add(new LibraryFunction(name, fun));
					}
				}
			}

			return list.ToArray();
		}

		public override string ToString()
		{
			return "refal function: " + Name;
		}
	}
}

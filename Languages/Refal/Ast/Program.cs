using System;
using System.Linq;
using System.Collections.Generic;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using Irony.Interpreter;
using Refal.Runtime;

namespace Refal
{
	/// <summary>
	/// Program is a list of functions
	/// </summary>
	public class Program : AstNode
	{
		public IDictionary<string, Function> Functions { get; private set; }

		public IList<Function> FunctionList { get; private set; }

		public Function EntryPoint { get; private set; }

		public Program()
		{
			Functions = new Dictionary<string, Function>();
			FunctionList = new List<Function>();
			EntryPoint = null;
		}

		public override void Init(ParsingContext context, ParseTreeNode parseNode)
		{
			base.Init(context, parseNode);

			foreach (var node in parseNode.ChildNodes)
			{
				if (node.AstNode is Function)
				{
					AddFunction(node.AstNode as Function);
				}
				else if (node.AstNode is AuxiliaryNode)
				{
					var ids = (node.AstNode as AuxiliaryNode).ChildNodes.OfType<IdentifierNode>();

					foreach (var id in ids)
					{
						ExternalFunction ef = new ExternalFunction();
						ef.SetSpan(id.Span);
						ef.Name = id.Symbol;
						AddFunction(ef);
					}
				}
			}
		}

		public override System.Collections.IEnumerable GetChildNodes()
		{
			foreach (var fun in FunctionList)
				yield return fun;
		}

		public void AddFunction(Function function)
		{
			Functions[function.Name] = function;
			FunctionList.Add(function);
			
			if (function.Name == "Go")
			{
				EntryPoint = function;
			}
		}

		public override void EvaluateNode(ScriptAppInfo context, AstMode mode)
		{
			if (EntryPoint == null)
				context.ThrowError("No entry point defined (entry point is a function named «Go»)");

			// load standard run-time library functions
			var libraryFunctions = LibraryFunction.ExtractLibraryFunctions(context, new RefalLibrary(context));
			foreach (LibraryFunction libFun in libraryFunctions)
			{
				context.SetValue(libFun.Name, libFun);
			}

			// define all functions
			foreach (Function fun in FunctionList)
			{
				fun.Evaluate(context, mode);
			}

			// call entry point with empty expression as an argument
			context.Data.Push(Runtime.PassiveExpression.Build());
			EntryPoint.Call(context);

			// discard execution results
			context.Data.Pop();
			context.ClearLastResult();
		}

		public override string ToString()
		{
			return "Refal-5 program";
		}
	}
}

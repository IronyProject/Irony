#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter;

namespace Irony.Interpreter.Ast {

  public class UnaryOperationNode : AstNode {
    public string OpSymbol;
    public AstNode Argument;
    private OperatorImplementation _lastUsed;

    public UnaryOperationNode() { }
    public override void Init(ParsingContext context, ParseTreeNode treeNode) {
      base.Init(context, treeNode); 
      OpSymbol = treeNode.ChildNodes[0].FindTokenAndGetText();
      Argument = AddChild("Arg", treeNode.ChildNodes[1]);
      base.AsString = OpSymbol + "(unary op)";
      base.ExpressionType = context.GetUnaryOperatorExpressionType(OpSymbol);
    }

    protected override object DoEvaluate(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      var arg = Argument.Evaluate(thread);
      var result = thread.Runtime.ExecuteBinaryOperator(base.ExpressionType, arg, null, ref _lastUsed);
      thread.CurrentNode = Parent; //standard epilog
      return result; 
    }

    public override void SetIsTail() {
      base.SetIsTail();
      Argument.SetIsTail();
    }

  }//class
}//namespace

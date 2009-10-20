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
using System.Linq;
using System.Text;
using Irony.Interpreter;
using Irony.Parsing;

namespace Irony.Ast {

  //A node representing expression list - for example, list of argument expressions in function call
  public class ExpressionListNode : AstNode {
     
    public override void Init(ParsingContext context, ParseTreeNode treeNode) {
      base.Init(context, treeNode);
      foreach (var child in treeNode.ChildNodes) {
          AddChild("expr", child); 
      }
      AsString = "Expression list";
    }

    public override void EvaluateNode(EvaluationContext context, AstMode mode) {
      foreach(var expr in ChildNodes) 
        expr.Evaluate(context, AstMode.Read);
      //Pop all results from data stack, collect them into one list and push the list into the stack
      var result = new ValueList();
      for (var i = 0; i < ChildNodes.Count; i++)
        result.Add(context.Data.Pop());
      context.Data.Push(result); 
    }

  }//class

}//namespace

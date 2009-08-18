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
using Irony.Ast.Interpreter;

namespace Irony.Ast {
  public class BinExprNode : AstNode {
    public AstNode Left;
    public string Op;
    public AstNode Right;


    public BinExprNode(NodeArgs args, AstNode left, string op, AstNode right) : base(args) {
      ChildNodes.Clear();
      Left = left;
      AddChild("Arg", Left);
      Op = op; 
      Right = right;
      AddChild("Arg", Right);
      //Flags |= AstNodeFlags.TypeBasedDispatch;
    }
    public BinExprNode(NodeArgs args) 
      : this(args, args.ChildNodes[0], GetContent(args.ChildNodes[1]), args.ChildNodes[2]) {  
    }

    public override object Evaluate(EvaluationContext context, AstUseMode useMode) {
      var arg1 = Left.Evaluate(context, AstUseMode.Read);
      var arg2 = Right.Evaluate(context, AstUseMode.Read);
      context.Runtime.Dispatcher.Evaluate(context, Op, arg1, arg2);
      return context.Result; 
    }
  

    public override string ToString() {
      return Op + " (binary operation)";
    }
  }//class
}//namespace

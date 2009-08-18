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
  public class AssigmentNode : AstNode {
    public VarRefNode Identifier;
    public AstNode Expression;

    public AssigmentNode(NodeArgs args)  : this(args, args.ChildNodes[0], args.ChildNodes[2]) {  }

    public AssigmentNode(NodeArgs args, AstNode lvalue, AstNode expression) : base(args) {
      ChildNodes.Clear();
      var Identifier = lvalue as VarRefNode;
      if (Identifier == null) {
        args.Context.AddError(lvalue.Location, "Expected identifier.");
        return; 
      }
      Identifier.Flags |= AstNodeFlags.AllocateSlot | AstNodeFlags.NotRValue;
      AddChild("Name", Identifier);
      Expression = expression;
      AddChild("Expr", Expression);
    }

    public override void Evaluate(EvaluationContext context) {
      Expression.Evaluate(context);
      Identifier.Evaluate(context); //writes the value into the slot
    }

  }
}

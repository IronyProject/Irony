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

  public class AnonFunctionNode : AstNode  {
    public AstNode Parameters;
    public AstNode Body;
    public FunctionBindingInfo BindingInfo;

    public AnonFunctionNode(NodeArgs args, AstNode parameters, AstNode body) : base(args) {
      ChildNodes.Clear();
      Parameters = parameters;
      AddChild("Params", Parameters);
      Body = body;
      AddChild("Body", Body);
      foreach (VarRefNode prm in Parameters.ChildNodes)
        prm.Flags |= AstNodeFlags.AllocateSlot;
    }

    protected void EvaluateOnDefine(EvaluationContext context) {
      context.Result = new Closure(context.CurrentFrame, this, BindingInfo); // Body.Evaluate);
    }


    #region IBindingTarget Members

    public FunctionBindingInfo GetBindingInfo() {
      return BindingInfo;
    }

    #endregion
  }//class
}
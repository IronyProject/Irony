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
using Irony.Parsing;
using Irony.Interpreter;

namespace Irony.Ast {

  public class AnonFunctionNode : AstNode  {
    public AstNode Parameters;
    public AstNode Body;
    public FunctionBindingInfo BindingInfo;

    public AnonFunctionNode() { }
    public override void Init(ParsingContext context, ParseTreeNode treeNode) {
      base.Init(context, treeNode); 
            
    }
    public AnonFunctionNode(AstNode parameters, AstNode body) : this() {
      ChildNodes.Clear();
/*
      Parameters = AddChild("Params", parameters);
      
      Body = body;
      AddChild("Body", Body);
      foreach (VarRefNode prm in Parameters.ChildNodes)
        prm.Flags |= AstNodeFlags.AllocateSlot;
  
 */ }

  }//class
}
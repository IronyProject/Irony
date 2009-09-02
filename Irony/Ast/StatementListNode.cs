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

  public class StatementListNode : AstNode {

    public StatementListNode() { }
     
    public override void Init(ParsingContext context, ParseTreeNode treeNode) {
      base.Init(context, treeNode);
      foreach (var child in treeNode.ChildNodes) {
        AddChild("stmt", child, false); //don't throw error if null
      }
      AsString = "Statement List";
    }

    public override void Evaluate(EvaluationContext context, AstMode mode) {
      if (ChildNodes.Count == 0) return;
      ChildNodes[ChildNodes.Count - 1].Flags |= AstNodeFlags.IsTail;
      int iniCount = context.Data.Count; 
      foreach(var stmt in ChildNodes) {
        //restore position, in case one of the statement left something (like standalone expression vs assignment) 
        context.Data.PopUntil(iniCount); 
        stmt.Evaluate(context, AstMode.Read);
      }
    }
    
  }//class

}//namespace

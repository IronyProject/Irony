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
  public class CondFormNode : AstNode {
    public AstNodeList Clauses;
    public AstNode ElseClause;

    public CondFormNode() { }

    public override void Init(ParsingContext context, ParseTreeNode treeNode) {
      base.Init(context, treeNode);
      //TODO: finish this      
    }
    public CondFormNode(AstNodeList clauses, AstNode elseClause) {
      ChildNodes.Clear();
      Clauses = clauses;
      foreach (AstNode clause in clauses) {
        clause.Role = "Arg";
        ChildNodes.Add(clause);
      }
      ElseClause = elseClause;
      if (ElseClause != null) {
        ElseClause.Role = "else";
        ChildNodes.Add(ElseClause);
      }
    }

    public override void Evaluate(EvaluationContext context, AstMode mode) {
      foreach (CondClauseNode clause in Clauses) {
        clause.Test.Evaluate(context, AstMode.None);
        var result = context.Data.Pop(); 
        if (context.Runtime.IsTrue(result)) {
          clause.Expressions.Evaluate(context, mode);
          return;
        }
      }//foreach
      if (ElseClause != null)
        ElseClause.Evaluate(context, 0);
    }
  }
}

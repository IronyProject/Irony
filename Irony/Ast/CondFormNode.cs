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
  public class CondFormNode : AstNode {
    public AstNodeList Clauses;
    public AstNode ElseClause;

    public CondFormNode(NodeArgs args, AstNodeList clauses, AstNode elseClause) : base(args) {
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

    public override void Evaluate(EvaluationContext context) {
      foreach (CondClauseNode clause in Clauses) {
        clause.Test.Evaluate(context);
        if (context.Runtime.IsTrue(context.Result)) {
          clause.Expressions.Evaluate(context);
          return;
        }
      }//foreach
      if (ElseClause != null)
        ElseClause.Evaluate(context);
    }
  }
}

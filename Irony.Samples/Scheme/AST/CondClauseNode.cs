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
using System.Text;
using Irony.Compiler;
using Irony.Runtime;

namespace Irony.Samples.Scheme {
  public class CondClauseNode : AstNode {
    public AstNode Test;
    public DatumListNode Expressions;

    public CondClauseNode(AstNodeArgs args, AstNode test, DatumListNode expressions)  : base(args) {
      SetFields(test, expressions);
    }
    private void SetFields(AstNode test, DatumListNode expressions) {
      this.Tag = "Clause";
      Test = test;
      Test.Tag = "Test";
      Expressions = expressions;
      Expressions.Tag = "Command";
      ReplaceChildNodes(Test, Expressions);
    }

    public override void Evaluate(EvaluationContext context) {
      Expressions.Evaluate(context);
    }
  }//class

}//namespace

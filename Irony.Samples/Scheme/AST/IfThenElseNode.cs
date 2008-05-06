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
  //The following class is an example of custom AST node. Notice how child's tag appears as prefix in node caption
  // in AST view in GrammarExplorer form.
  public class IfThenElseNode : AstNode {
    public AstNode Test;
    public AstNode IfTrue;
    public AstNode IfFalse;

    public IfThenElseNode(AstNodeArgs args)    : base(args) {
      SetFields(args.ChildNodes[1], args.ChildNodes[2], args.ChildNodes[3]);
    }
    public IfThenElseNode(AstNodeArgs args, AstNode test, AstNode ifTrue, AstNode ifFalse) : base(args) {
       SetFields(test, ifTrue, ifFalse);
    }
    public override void OnAstProcessing(CompilerContext context, AstProcessingPhase phase) {
      base.OnAstProcessing(context, phase);
      switch (phase) {
        case AstProcessingPhase.MarkTailCalls:
          if (IsSet(AstNodeFlags.IsTail)) {
            IfTrue.Flags |= AstNodeFlags.IsTail;
            if (IfFalse != null)
              IfFalse.Flags |= AstNodeFlags.IsTail;
          }
          break;
      }
    }


    private void SetFields(AstNode test, AstNode ifTrue, AstNode ifFalse) {
      if (ifTrue != null && ifTrue.IsEmpty()) ifTrue = null;
      if (ifFalse != null && ifFalse.IsEmpty()) ifFalse = null;
      Test = test;
      Test.Tag = "Test";
      IfTrue = ifTrue;
      if(IfTrue != null) IfTrue.Tag = "IfTrue";
      IfFalse = ifFalse;
      if (IfFalse != null) IfFalse.Tag = "IfFalse";
      ReplaceChildNodes(Test, IfTrue, IfFalse);
    }

    public override void Evaluate(EvaluationContext context) {
      Test.Evaluate(context);
      if (context.Ops.IsTrue(context.CurrentResult)) {
        if (IfTrue != null)    IfTrue.Evaluate(context);
      } else {
        if (IfFalse != null)   IfFalse.Evaluate(context);
      }
    }
  }//class

}//namespace

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
using System.Linq.Expressions; 
using System.Text;
using Irony.Interpreter;
using Irony.Parsing;

namespace Irony.Interpreter.Ast {
  public class BinaryOperationNode : AstNode {
    public AstNode Left, Right;
    public string OpSymbol;
    public ExpressionType Op;
    private OperatorImplementation _lastUsed;
    private object _constValue;
    private int _failureCount; 

    public BinaryOperationNode() { }

    public override void Init(ParsingContext context, ParseTreeNode treeNode) {
      base.Init(context, treeNode);
      Left = AddChild("Arg", treeNode.ChildNodes[0]);
      Right = AddChild("Arg", treeNode.ChildNodes[2]);
      var opToken = treeNode.ChildNodes[1].FindToken();
      OpSymbol = opToken.Text;
      Op = context.GetOperatorExpressionType(OpSymbol);
      // Set error anchor to operator, so on error (Division by zero) the explorer will point to 
      // operator node as location, not to the very beginning of the first operand.
      ErrorAnchor = opToken.Location;
      AsString = Op + "(operator)";
    }

    protected override object DoEvaluate(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      var result = EvaluateNormally(thread); // first time evaluate normally
      // If constant, save the value and switch to method that directly returns the result.
      if (IsConstant()) {
        _constValue = result;
        AsString = Op + "(operator) Const=" + _constValue;
        SetEvaluate(EvaluateConst);
      } else
        this.SetEvaluate(EvaluateFast); //set for fast evaluation in the future
      thread.CurrentNode = Parent; //standard epilog
      return result;
    }

    protected object EvaluateFast(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      var arg1 = Left.Evaluate(thread);
      var arg2 = Right.Evaluate(thread);
      //If we have _lastUsed, go straight for it; if types mismatch it will throw
      if (_lastUsed != null) {
        try {
          var res = _lastUsed.EvaluateBinary(arg1, arg2);
          thread.CurrentNode = Parent; //standard epilog
          return res;
        } catch {
          _lastUsed = null;
          _failureCount++;
          // if failed 3 times, change to method without direct try
          if (_failureCount > 3)
            SetEvaluate(EvaluateNormally);
        } //catch
      }// if _lastUsed
      // go for normal evaluation
      var result = thread.Runtime.ExecuteBinaryOperator(this.Op, arg1, arg2, ref _lastUsed);
      thread.CurrentNode = Parent; //standard epilog
      return result;
    }//method

    protected object EvaluateNormally(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      var arg1 = Left.Evaluate(thread);
      var arg2 = Right.Evaluate(thread);
      var result = thread.Runtime.ExecuteBinaryOperator(this.Op, arg1, arg2, ref _lastUsed);
      thread.CurrentNode = Parent; //standard epilog
      return result;
    }//method

    private object EvaluateConst(ScriptThread thread) {
      return _constValue; 
    }

    public override bool IsConstant() {
      return Left.IsConstant() && Right.IsConstant(); 
    }
  }//class
}//namespace

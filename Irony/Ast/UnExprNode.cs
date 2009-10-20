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

  public class UnExprNode : AstNode {
    public string Op;
    public string UnaryOp; //unary operation corresponding to "++" and "--"
    public AstNode Argument;
    public bool IsPostfix;
    public bool IsIncDec;
    public NodeEvaluate EvaluateImplRef; 

    public UnExprNode() { }
    public override void Init(ParsingContext context, ParseTreeNode treeNode) {
      base.Init(context, treeNode);
      if (!CheckNodeForOpSymbol(treeNode.ChildNodes[0], false) && ! CheckNodeForOpSymbol(treeNode.ChildNodes[1], true)) {
        string msg = string.Format("Invalid unary operator node: {0}; operator symbol not registered in Grammar.Post/PrefixUnaryOperators sets.", this);
        throw new AstException(this, msg);
      }
      int argIndex = IsPostfix? 0 : 1;
      Argument = AddChild("Arg", treeNode.ChildNodes[argIndex]);
      base.AsString = Op + "(unary" + (IsPostfix ? "postfix" : string.Empty) + ")";
      SetupEvaluationMethod();
    }

    private bool CheckNodeForOpSymbol(ParseTreeNode node, bool asPostfix) {
      Op = node.FindTokenAndGetText();
      if (string.IsNullOrEmpty(Op)) return false;
      bool ok;
      if (asPostfix)
        ok = Term.Grammar.PostfixUnaryOperators.Contains(Op);
      else 
        ok = Term.Grammar.PrefixUnaryOperators.Contains(Op);
      if (!ok) return false; 
      IsIncDec = Op == "++" || Op == "--";
      IsPostfix = asPostfix;
      return true;
    }

    private void SetupEvaluationMethod() {
      switch (Op) {
        case "+": EvaluateImplRef = EvaluatePrefixPlus; break;
        case "-": EvaluateImplRef = EvaluatePrefixMinus; break;
        case "!": EvaluateImplRef = EvaluatePrefixNot; break;
        case "++":
        case "--":
          if (IsPostfix) 
            EvaluateImplRef = EvaluatePostfixIncDec; 
          else 
            EvaluateImplRef = EvaluatePrefixIncDec;
          UnaryOp = Op == "++"? "+" : "-";
          break; 
        default:
          string msg = string.Format("UnExprNode: no implementation for unary operator '{0}'.", Op);
          throw new AstException(this, msg);
      }//switch
    }//method

    #region Evaluation methods
    public override void EvaluateNode(EvaluationContext context, AstMode mode) {
      EvaluateImplRef(context, mode); 
    }
    private void EvaluatePrefixPlus(EvaluationContext context, AstMode mode) {
      Argument.Evaluate(context, AstMode.Read);
    }
    private void EvaluatePrefixMinus(EvaluationContext context, AstMode mode) {
      context.Data.Push((byte)0);
      Argument.Evaluate(context, AstMode.Read);
      context.CallDispatcher.ExecuteBinaryOperator("-"); 
    }
    private void EvaluatePrefixNot(EvaluationContext context, AstMode mode) {
      Argument.Evaluate(context, AstMode.Read);
      var value = context.Data.Pop();
      var bValue = (bool) context.Runtime.BoolResultConverter(value);
      context.Data.Push(!bValue); 
    }
    
    //prefix op: result of operation is value AFTER inc/dec
    private void EvaluatePrefixIncDec(EvaluationContext context, AstMode mode) {
      Argument.Evaluate(context, AstMode.Read);
      context.Data.Push(1);
      context.CallDispatcher.ExecuteBinaryOperator(UnaryOp);
      var result = context.LastResult;
      Argument.Evaluate(context, AstMode.Write);
      context.Data.Push(result); 
    }

    //postfix op: result of operation is value BEFORE inc/dec
    private void EvaluatePostfixIncDec(EvaluationContext context, AstMode mode) {
      Argument.Evaluate(context, AstMode.Read);
      var result = context.LastResult;
      context.Data.Push(1);
      context.CallDispatcher.ExecuteBinaryOperator(UnaryOp);
      Argument.Evaluate(context, AstMode.Write);
      context.Data.Push(result);
    }
    #endregion

  }//class
}//namespace

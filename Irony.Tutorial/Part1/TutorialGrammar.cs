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
using Irony.Compiler.AST;
using Irony.Runtime;

namespace Irony.Tutorial.Part1 {
  //Sample expression grammar; recognizes one-line arithmetic expressions with numbers
  // for example: 
  // 5 + 2.5 * 4

  [Language("TutorialGrammar", "1.0", "Sample tutorial grammar")]
  public class CalcGrammar : Irony.Compiler.Grammar {
    public CalcGrammar() {
      // 1. Terminals
      var number = new NumberLiteral("number");

      // 2. Non-terminals
      var Expr = new NonTerminal("Expr");
      var Term = new NonTerminal("Term");
      var BinExpr = new NonTerminal("BinExpr", typeof(BinExprNode));
      var ParExpr = new NonTerminal("ParExpr");
      var UnExpr = new NonTerminal("UnExpr", typeof(UnExprNode));
      var UnOp = new NonTerminal("UnOp");
      var BinOp = new NonTerminal("BinOp");

      // 3. BNF rules
      Expr.Rule = Term | UnExpr | BinExpr;
      Term.Rule = number | ParExpr;
      ParExpr.Rule = "(" + Expr + ")";
      UnExpr.Rule = UnOp + Term;
      UnOp.Rule = Symbol("+") | "-";
      BinExpr.Rule =  Expr + BinOp + Expr;
      BinOp.Rule = Symbol("+") | "-" | "*" | "/" | "**";
      this.Root = Expr;       // Set grammar root

      // 4. Operators precedence
      RegisterOperators(1, "+", "-");
      RegisterOperators(2, "*", "/");
      RegisterOperators(3, Associativity.Right, "**");

      RegisterPunctuation( "(", ")" );

    }
  }
}//namespace



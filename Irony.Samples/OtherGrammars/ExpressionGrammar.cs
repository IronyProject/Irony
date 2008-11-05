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

namespace Irony.Samples {
  //Sample expression grammar - recognizes arithmetic expressions with numbers and variables
  //
  // Expr  -> n | v | Expr BinOp Expr | UnOp Expr | (  Expr )
  // BinOp -> + | - | * | / | **
  // UnOp  -> -
  // ExprLine -> Expr EOF

  [Language("ExpressionGrammar", "1.0", "Arithmetic expressions grammar")]
  public class ExpressionGrammar : Irony.Compiler.Grammar {

    public ExpressionGrammar() {
      // 1. Terminals
      Terminal n = new NumberLiteral("number");
      Terminal v = new IdentifierTerminal("variable");

      // 2. Non-terminals
      NonTerminal Expr = new NonTerminal("Expr");
      NonTerminal BinOp = new NonTerminal("BinOp");
      NonTerminal UnOp = new NonTerminal("UnOp");
      NonTerminal ExprLine = new NonTerminal("ExprLine");

      // 3. BNF rules
      Expr.Rule = n | v | Expr + BinOp + Expr | UnOp + Expr | "(" + Expr + ")";
      BinOp.Rule = Symbol("+") | "-" | "*" | "/" | "**";
      UnOp.Rule = "-";            // Irony provides default conversion here
      ExprLine.Rule = Expr + Eof; //Eof is predefined terminal
      this.Root = ExprLine;       // Set grammar root

      // 4. Operators precedence
      RegisterOperators(1, "+", "-");
      RegisterOperators(2, "*", "/");
      RegisterOperators(3, Associativity.Right, "**");

      RegisterPunctuation( "(", ")" );

    }
  }
}//namespace

/* example of use:
  ExpressionGrammar grammar = new ExpressionGrammar();
  LanguageCompiler compiler = new LanguageCompiler(grammar);

  string expr = "a + b * 3.14 / (2.0 * x + y**z**2)";
  AstNode rootNode = compiler.Parse(expr);
  //rootNode now contains an AST node that is the root of the syntax tree.
*/ 


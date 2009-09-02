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
using Irony.Parsing;
using Irony.Ast;

namespace Irony.Tutorial.Part2 {
  // The grammar is an extension of expression grammar in Part 1. 
  // This grammar recognizes programs that contain simple expressions and assignments involving variables,
  // for ex:
  // x = 3 + 4
  // y = x * 2 + 1
  // The result of calculation is the result of last expression or assignment (value of "y" in this case).

  [Language("MiniPython2", "2.0", "Multi-line expression evaluator: variables, assignments")]
  public class MiniPython2 : Irony.Parsing.Grammar {
    public MiniPython2() {

      // 1. Terminals
      var number = new NumberLiteral("number");
      var identifier = new IdentifierTerminal("identifier");
      var comment = new CommentTerminal("comment", "#", "\n", "\r");
      //comment must to be added to NonGrammarTerminals list; it is not used directly in grammar rules,
      // so we add it to this list to let Scanner know that it is also a valid terminal. 
      base.NonGrammarTerminals.Add(comment);

      // 2. Non-terminals
      var Expr = new NonTerminal("Expr");
      var Term = new NonTerminal("Term");
      var BinExpr = new NonTerminal("BinExpr", typeof(BinExprNode));
      var ParExpr = new NonTerminal("ParExpr");
      var UnExpr = new NonTerminal("UnExpr", typeof(UnExprNode));
      var UnOp = new NonTerminal("UnOp");
      var BinOp = new NonTerminal("BinOp", "operator");
      var AssignmentStmt = new NonTerminal("AssignmentStmt", typeof(AssigmentNode));
      var Statement = new NonTerminal("Statement");
      var ProgramLine = new NonTerminal("ProgramLine");
      var Program = new NonTerminal("Program", typeof(StatementListNode));

      // 3. BNF rules
      Expr.Rule = Term | UnExpr | BinExpr;
      Term.Rule = number | ParExpr | identifier;
      ParExpr.Rule = "(" + Expr + ")";
      UnExpr.Rule = UnOp + Term;
      UnOp.Rule = Symbol("+") | "-";
      BinExpr.Rule = Expr + BinOp + Expr;
      BinOp.Rule = Symbol("+") | "-" | "*" | "/" | "**";
      AssignmentStmt.Rule = identifier + "=" + Expr;
      Statement.Rule = AssignmentStmt | Expr | Empty;
      ProgramLine.Rule = Statement + NewLine;
      Program.Rule = MakeStarRule(Program, ProgramLine);
      this.Root = Program;       // Set grammar root

      // 4. Operators precedence
      RegisterOperators(1, "+", "-");
      RegisterOperators(2, "*", "/");
      RegisterOperators(3, Associativity.Right, "**");

      RegisterPunctuation("(", ")");
      MarkTransient(Term, Expr, Statement, BinOp, ProgramLine, ParExpr);

      //automatically add NewLine before EOF so that our BNF rules work correctly when there's no final line break in source
      this.LanguageFlags = LanguageFlags.CreateAst | LanguageFlags.NewLineBeforeEOF | LanguageFlags.SupportsInterpreter;



    }
  }
}//namespace



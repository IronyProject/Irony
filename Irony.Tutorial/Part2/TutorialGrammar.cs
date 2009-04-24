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
using Irony.CompilerServices;
using Irony.Scripting.Ast;

namespace Irony.Tutorial.Part2 {
  // The grammar is an extension of expression grammar in Part 1. 
  // This grammar recognizes programs that contain simple expressions and assignments involving variables
  // for ex:
  // x = 3 + 4
  // y = x * 2 + 1
  //  the result of calculation is the result of last expression or assignment (value of "y" in this case).

  [Language("TutorialGrammar", "2.0", "Sample tutorial grammar")]
  public class CalcGrammar : Irony.CompilerServices.Grammar {
    public CalcGrammar() {
      // 1. Terminals
      var number = new NumberLiteral("number");
      var identifier = new IdentifierTerminal("identifier");

      // 2. Non-terminals
      var Variable = new NonTerminal("Variable", typeof(VarRefNode));
      var Expr = new NonTerminal("Expr");
      var Term = new NonTerminal("Term");
      var BinExpr = new NonTerminal("BinExpr", typeof(BinExprNode));
      var ParExpr = new NonTerminal("ParExpr");
      var UnExpr = new NonTerminal("UnExpr", typeof(UnExprNode));
      var UnOp = new NonTerminal("UnOp");
      var BinOp = new NonTerminal("BinOp");
      var AssignmentStmt = new NonTerminal("AssignmentStmt", typeof(AssigmentNode));
      var Statement = new NonTerminal("Statement");
      var ProgramLine = new NonTerminal("ProgramLine");
      var Program = new NonTerminal("Program", typeof(StatementListNode));

      // 3. BNF rules
      Variable.Rule = identifier;
      Expr.Rule = Term | UnExpr | BinExpr;
      Term.Rule = number | ParExpr | Variable;
      ParExpr.Rule = "(" + Expr + ")";
      UnExpr.Rule = UnOp + Term;
      UnOp.Rule = Symbol("+") | "-";
      BinExpr.Rule =  Expr + BinOp + Expr;
      BinOp.Rule = Symbol("+") | "-" | "*" | "/" | "**";
      AssignmentStmt.Rule = Variable + "=" + Expr;
      Statement.Rule = AssignmentStmt | Expr | Empty;
      ProgramLine.Rule = Statement + NewLine;
      Program.Rule = MakeStarRule(Program, ProgramLine); 
      this.Root = Program;       // Set grammar root

      // 4. Operators precedence
      RegisterOperators(1, "+", "-");
      RegisterOperators(2, "*", "/");
      RegisterOperators(3, Associativity.Right, "**");

      RegisterPunctuation( "(", ")");
      RegisterPunctuation(NewLine); //remove all newLines - important, extra new lines in output tree can mess up calc result

      //automatically add newLine before EOF so that our grammar works
      this.LanguageFlags = LanguageFlags.NewLineBeforeEOF | LanguageFlags.SupportsInterpreter | LanguageFlags.AutoDetectTransient; 

    }
  }
}//namespace



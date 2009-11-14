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

namespace Irony.Samples {
  // This grammar describes programs that consist of simple expressions and assignments
  // for ex:
  // x = 3
  // y = -x + 5
  //  the result of calculation is the result of last expression or assignment.
  //  Irony's default  runtime provides expression evaluation. 
  //  supports inc/dec operators (++,--), both prefix and postfix,
  //  and combined assignment operators like +=, -=, etc.

  [Language("ExpressionEvaluator", "1.0", "Multi-line expression evaluator")]
  public class ExpressionEvaluatorGrammar : Irony.Parsing.Grammar {
    public ExpressionEvaluatorGrammar() : base(false) { //false means case insensitive
      this.GrammarComments =
        "Simple expression evaluator. Case-insensitive. Supports big integers and float data types, variables, assignments, " +
        "arithmetic operations, augmented assignments (+=, -=), inc/dec (++,--)" ;
      // 1. Terminals
      var number = new NumberLiteral("number");
      //Let's allow big integers (with unlimited number of digits):
      number.DefaultIntTypes = new TypeCode[] { TypeCode.Int32, TypeCode.Int64, NumberLiteral.TypeCodeBigInt };
      var identifier = new IdentifierTerminal("identifier");
      var comment = new CommentTerminal("comment", "#", "\n", "\r"); 
      //comment must be added to NonGrammarTerminals list; it is not used directly in grammar rules,
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
      var PostFixExpr = new NonTerminal("PostFixExpr", typeof(UnExprNode));
      var PostFixOp = new NonTerminal("PostFixOp");
      var AssignmentStmt = new NonTerminal("AssignmentStmt", typeof(AssigmentNode));
      var AssignmentOp = new NonTerminal("AssignmentOp", "assignment operator");
      var Statement = new NonTerminal("Statement");
      var ProgramLine = new NonTerminal("ProgramLine");
      var Program = new NonTerminal("Program", typeof(StatementListNode));

      // 3. BNF rules
      Expr.Rule = Term | UnExpr | BinExpr | PostFixExpr;
      Term.Rule = number | ParExpr | identifier;
      ParExpr.Rule = "(" + Expr + ")";
      UnExpr.Rule = UnOp + Term;
      UnOp.Rule = ToTerm("+") | "-" | "++" | "--";
      BinExpr.Rule = Expr + BinOp + Expr;
      BinOp.Rule = ToTerm("+") | "-" | "*" | "/" | "**";
      PostFixExpr.Rule = Term + PostFixOp;
      PostFixOp.Rule = ToTerm("++") | "--";
      AssignmentStmt.Rule = identifier + AssignmentOp + Expr;
      AssignmentOp.Rule = ToTerm("=") | "+=" | "-=" | "*=" | "/=";
      Statement.Rule = AssignmentStmt | Expr | Empty;
      ProgramLine.Rule = Statement + NewLine;
      Program.Rule = MakeStarRule(Program, ProgramLine);
      this.Root = Program;       // Set grammar root

      // 4. Operators precedence
      RegisterOperators(1, "+", "-");
      RegisterOperators(2, "*", "/");
      RegisterOperators(3, Associativity.Right, "**");

      // 5. Punctuation and transient terms
      RegisterPunctuation("(", ")");
      RegisterBracePair("(", ")");
      MarkTransient(Term, Expr, Statement, BinOp, UnOp, PostFixOp, AssignmentOp, ProgramLine, ParExpr);
      // The following makes error messages a little cleaner (try evaluating expr 'x y' with and without this line to see the difference)
      MarkNotReported("++", "--"); 

      //automatically add NewLine before EOF so that our BNF rules work correctly when there's no final line break in source
      this.LanguageFlags = LanguageFlags.CreateAst | LanguageFlags.NewLineBeforeEOF | LanguageFlags.CanRunSample; 

      //Console
      ConsoleTitle = "Irony Expression Evaluator";
      ConsoleGreeting =
@"Irony Sample Console for Expression Evaluator 

  Supports variable assignments, arithmetic operators (+, -, *, /),
  augmented assignments (+=, -=, etc), prefix/postfix operators ++,--. 
  Supports big integer arithmetics.

Press Ctrl-C to exit the program at any time.
";
      ConsolePrompt = "?";
      ConsolePromptMoreInput = "?"; 

    }
  }
}//namespace



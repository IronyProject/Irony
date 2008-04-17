using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Compiler;

namespace Irony.Samples
{

	/// <summary>
	/// This class defines the Grammar for the GwBASIC language.
  /// Based on grammar by Daniel Flower.
	/// </summary>
	public class GWBasicGrammar : Grammar
	{

		public GWBasicGrammar()
		{

			#region Initialisation

			// BASIC is not case sensitive... 
			this.CaseSensitive = false;

			// Add a custom filter to remove blank lines and make sure lines are numbered correctly.
			this.TokenFilters.Add(new CodeOutlineFilter(false));
      //TODO: add line numbers validation to LINES node

			// Define the Terminals
			Terminal number = new NumberLiteral("NUMBER");
      IdentifierTerminal variable = new IdentifierTerminal("Identifier", "$%!", string.Empty);
			Terminal stringLiteral = new StringLiteral("STRING", "\"", ScanFlags.None);
      //Important: do not add comment term to base.NonGrammarTerminals list - we do use this terminal in grammar rules
      Terminal comment = new CommentTerminal("Comment", "REM", "\n");
      Terminal comma = Symbol(",", "comma");
      NonTerminal semi_opt = new NonTerminal("semi_opt");
      semi_opt.Rule = Empty | ";";

			// Define the non-terminals
      NonTerminal PROGRAM = new NonTerminal("PROGRAM");
			NonTerminal LINE = new NonTerminal("LINE");
			NonTerminal STATEMENT_LIST = new NonTerminal("STATEMENT_LIST");
			NonTerminal STATEMENT = new NonTerminal("STATEMENT");
      NonTerminal COMMAND = new NonTerminal("COMMAND"); 
      NonTerminal PRINT_STMT = new NonTerminal("PRINT_STMT");
      NonTerminal PRINT_LIST = new NonTerminal("PRINT_LIST");
      NonTerminal PRINT_ARG = new NonTerminal("PRINT_ARG");
      NonTerminal INPUT_STMT = new NonTerminal("INPUT_STMT");
			NonTerminal IF_STMT = new NonTerminal("IF_STMT"); 
      NonTerminal ELSE_CLAUSE_OPT = new NonTerminal("ELSE_CLAUSE_OPT", typeof(AstNode)); 
			NonTerminal EXPR = new NonTerminal("EXPRESSION");
			NonTerminal EXPR_LIST = new NonTerminal("EXPRESSION_LIST");
			NonTerminal BINARY_OP = new NonTerminal("BINARY_OP");
      NonTerminal BINARY_EXPR = new NonTerminal("BINARY_EXPR");
      NonTerminal UNARY_EXPR = new NonTerminal("UNARY_EXPR");
      NonTerminal SIGN = new NonTerminal("SIGN");
      NonTerminal BRANCH_STMT = new NonTerminal("BRANCH_STMT");
			NonTerminal ASSIGN_STMT = new NonTerminal("ASSIGN_STMT");
			NonTerminal FOR_STMT = new NonTerminal("FOR_STMT");
      NonTerminal STEP_OPT = new NonTerminal("STEP_OPT");  
      NonTerminal NEXT_STMT = new NonTerminal("NEXT_STMT");
			NonTerminal LOCATE_STMT = new NonTerminal("LOCATE_STMT");
			NonTerminal WHILE_STMT = new NonTerminal("WHILE_STMT");
			NonTerminal WEND_STMT = new NonTerminal("WEND_STMT");
			NonTerminal SWAP_STMT = new NonTerminal("SWAP_STMT");
			NonTerminal GLOBAL_FUNCTION_EXPR = new NonTerminal("GLOBAL_FUNCTION_EXPR");
      NonTerminal ARG_LIST = new NonTerminal("ARG_LIST");
      NonTerminal FUNC_NAME = new NonTerminal("FUNC_NAME");
      NonTerminal COMMENT_STMT = new NonTerminal("COMMENT_STMT");
      NonTerminal GLOBAL_VAR_EXPR = new NonTerminal("GLOBAL_VAR_EXPR");

			// set the PROGRAM to be the root node of BASIC programs.
      this.Root = PROGRAM;

			#endregion

			#region Grammar declaration
      // A program is a bunch of lines
      PROGRAM.Rule = MakePlusRule(PROGRAM, null, LINE);

			// "Lines" is recursively defined as "Lines" followed by a line, or just a single line.
			//LINES.Rule = MakePlusRule(LINES, null, LINE);

			// A line can be an empty line, or it's a number followed by a statement list ended by a new-line.
			LINE.Rule = NewLine  | number + NewLine | number + STATEMENT_LIST + NewLine;

			// A statement list is 1 or more statements separated by the ':' character
			STATEMENT_LIST.Rule = MakePlusRule(STATEMENT_LIST, Symbol(":"), STATEMENT);

			// A statement can be one of a number of types
			STATEMENT.Rule = EXPR | ASSIGN_STMT | PRINT_STMT | INPUT_STMT | IF_STMT | COMMENT_STMT  
									   | BRANCH_STMT | COMMAND | FOR_STMT | NEXT_STMT | LOCATE_STMT | SWAP_STMT 
                     | WHILE_STMT | WEND_STMT;
			// The different statements are defined here
			PRINT_STMT.Rule = "print" + PRINT_LIST;
      PRINT_LIST.Rule = MakeStarRule(PRINT_LIST, null, PRINT_ARG);
      PRINT_ARG.Rule = EXPR + semi_opt; 
      INPUT_STMT.Rule = "input" + EXPR_LIST + variable;
			IF_STMT.Rule = "if" + EXPR + "then" + STATEMENT_LIST + ELSE_CLAUSE_OPT;
      ELSE_CLAUSE_OPT.Rule = Empty | "else" + STATEMENT_LIST;
			BRANCH_STMT.Rule = "goto" + number | "gosub" + number | "return";
			ASSIGN_STMT.Rule = variable + "=" + EXPR;
			LOCATE_STMT.Rule = "locate" + EXPR + comma + EXPR;
			SWAP_STMT.Rule = "swap" + EXPR + comma + EXPR;
      COMMAND.Rule = Symbol("end") | "cls";
      COMMENT_STMT.Rule = comment; 

			// An expression is a number, or a variable, a string, or the result of a binary comparison.
      EXPR.Rule = number | variable | stringLiteral | BINARY_EXPR 
				| GLOBAL_VAR_EXPR | GLOBAL_FUNCTION_EXPR | "(" + EXPR + ")" | UNARY_EXPR;
      BINARY_EXPR.Rule = EXPR + BINARY_OP + EXPR;
      UNARY_EXPR.Rule = SIGN + EXPR;
      SIGN.Rule = Symbol("-") | "+";

      BINARY_OP.Rule = Symbol("+") | "-" | "*" | "/" | "=" | "<=" | ">=" | "<" | ">" | "<>" | "and" | "or";
      //let's do operator precedence right here
      RegisterOperators(50, "*", "/");
      RegisterOperators(40, "+", "-");
      RegisterOperators(30, "=", "<=", ">=", "<", ">", "<>");
      RegisterOperators(20, "and", "or");

			EXPR_LIST.Rule = MakeStarRule(EXPR_LIST, null, EXPR);

      FOR_STMT.Rule = "for" + ASSIGN_STMT + "to" + EXPR + STEP_OPT;
      STEP_OPT.Rule = Empty | "step" + EXPR;
			NEXT_STMT.Rule = "next" + variable | "next";
			WHILE_STMT.Rule = "while" + EXPR;
			WEND_STMT.Rule = "wend";

      //TODO: check number of arguments for particular function in node constructor
      GLOBAL_FUNCTION_EXPR.Rule = FUNC_NAME + "(" + ARG_LIST + ")";
      FUNC_NAME.Rule = Symbol("len") | "left$" | "mid$" | "right$" | "abs" | "asc" | "chr$" | "csrlin$"
                     | "cvi" | "cvs" | "cvd" | "exp" | "fix" | "log" | "pos" | "sgn" | "sin" | "cos" | "tan"
                     | "instr" | "space$" | "spc" | "sqr" | "str$" | "string$" | "val" | "cint";
      ARG_LIST.Rule = MakePlusRule(ARG_LIST, comma, EXPR);
			GLOBAL_VAR_EXPR.Rule = Symbol("rnd") | "timer" | "inkey$" | "csrlin";

      RegisterPunctuation("(", ")", ",");
			#endregion

    }//constructor

	}//class
}//namespace

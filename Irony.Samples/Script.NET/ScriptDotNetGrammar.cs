/*
 * Created by SharpDevelop.
 * User: Petro Protsyk
 * Date: 11.02.2008
 * Update: 15.02.2008
 * Time: 20:58
 * 
 */

using System;
using Irony.Parsing;

namespace Irony.Samples.ScriptNET
{
  /// <summary>
  /// This class represents Irony Grammar for Script.NET
  /// </summary>
  [Language("Script.NET", "1.0", "Script.NET grammar")]
  public class ScriptdotnetGrammar : Grammar
  {
    public ScriptdotnetGrammar()
    {
      #region 1. Terminals
      NumberLiteral n = TerminalFactory.CreateCSharpNumber("number");

      IdentifierTerminal v = new IdentifierTerminal("Identifier");
 /*     v.AddReservedWords("true", "false", "null", "if", "else",
                         "while", "for", "foreach", "in",
                         "switch", "case", "default", "break",
                         "continue", "return", "function", "is",
                         "pre", "post", "invariant", "new"); */

      StringLiteral s = new StringLiteral("String", "'", StringFlags.AllowsDoubledQuote);

      Terminal dot = ToTerm(".", "dot");
      Terminal less = ToTerm("<");
      Terminal greater = ToTerm(">");
      Terminal arrow = ToTerm("->");
      Terminal LSb = ToTerm("[");
      Terminal RSb = ToTerm("]");
      Terminal LCb = ToTerm("(");
      Terminal RCb = ToTerm(")");
      Terminal RFb = ToTerm("}");
      Terminal LFb = ToTerm("{");
      Terminal LMb = ToTerm("<[");
      Terminal RMb = ToTerm("]>");
      Terminal comma = ToTerm(",");
      Terminal semicolon = ToTerm(";");
      Terminal colon = ToTerm(":");

      #endregion

      #region 2. Non-terminals
      #region 2.1 Expressions
      NonTerminal Expr = new NonTerminal("Expr");
      NonTerminal BinOp = new NonTerminal("BinOp");
      NonTerminal LUnOp = new NonTerminal("LUnOp");
      NonTerminal RUnOp = new NonTerminal("RUnOp");

      NonTerminal ArrayConstructor = new NonTerminal("ArrayConstructor");
      NonTerminal MObjectConstructor = new NonTerminal("MObjectConstructor");
      NonTerminal MObjectList = new NonTerminal("MObjectList");
      #endregion

      #region 2.2 QualifiedName
      //Expression List:  expr1, expr2, expr3, ..
      NonTerminal ExprList = new NonTerminal("ExprList");
      //A name in form: a.b.c().d[1,2].e ....
      NonTerminal NewStmt = new NonTerminal("NewStmt");
      NonTerminal NewArrStmt = new NonTerminal("NewArrStmt");
      NonTerminal QualifiedName = new NonTerminal("QualifiedName");
      NonTerminal GenericsPostfix = new NonTerminal("GenericsPostfix");
      NonTerminal ArrayExpression = new NonTerminal("ArrayExpression");
      NonTerminal FunctionExpression = new NonTerminal("FunctionExpression");
      #endregion

      #region 2.3 Statement
      NonTerminal Condition = new NonTerminal("Condition");

      NonTerminal Statement = new NonTerminal("Statement");
      NonTerminal Statements = new NonTerminal("Statements");

      //Block
      NonTerminal CompoundStatement = new NonTerminal("CompoundStatement");
      #endregion

      #region 2.4 Program and Functions
      NonTerminal Prog = new NonTerminal("Prog");
      NonTerminal Element = new NonTerminal("Element");
      NonTerminal FuncDef = new NonTerminal("FuncDef");
      NonTerminal FuncContract = new NonTerminal("FuncContract");
      NonTerminal ParameterListOpt = new NonTerminal("ParamaterListOpt");
      NonTerminal SwitchStatements = new NonTerminal("SwitchStatements");
      #endregion

      #endregion

      #region 3. BNF rules
      #region 3.1 Expressions
      Expr.Rule = ToTerm("true")
                  | "false"
                  | "null"
                  | s
                  | n
                  | QualifiedName
                  // The following is needed: to parse "A<B ..." either as comparison or as beginning of GenericsPostfix
                  | QualifiedName + less + Expr 
                  | QualifiedName + less + QualifiedName + greater 
                  | NewStmt
                  | NewArrStmt
                  | QualifiedName + ":=" + Expr
                  | ArrayExpression
                  | FunctionExpression
                  | ArrayConstructor
                  | MObjectConstructor
                  | Expr + BinOp + Expr
                  | LUnOp + Expr
                  | Expr + RUnOp
                  | LMb + Element.Star() + RMb
                  | LCb + Expr + RCb
                  ;
      ExprList.Rule = MakePlusRule(ExprList, comma, Expr);
      NewStmt.Rule = "new" + QualifiedName + GenericsPostfix.Q() + LCb + ExprList.Q() + RCb;
      NewArrStmt.Rule = "new" + QualifiedName + GenericsPostfix.Q() + LSb + ExprList.Q() + RSb;
      BinOp.Rule = ToTerm("+") | "-" | "*" | "/" | "%" | "^" | "&" | "|"
                  | "&&" | "||" | "==" | "!=" | greater | less 
                  | ">=" | "<=" | "is" 
                  | "="  | "+=" | "-=" 
                  | ".";

      LUnOp.Rule = ToTerm("-") | "~" | "!";
      RUnOp.Rule = ToTerm("++") | "--";

      ArrayConstructor.Rule = LSb + ExprList + RSb;
      MObjectConstructor.Rule = LSb + v + arrow + Expr + MObjectList.Star() + RSb;
      MObjectList.Rule = comma + v + arrow + Expr;
      #endregion

      #region 3.2 QualifiedName
      ArrayExpression.Rule = QualifiedName + LSb + ExprList + RSb;
      FunctionExpression.Rule = QualifiedName + LCb + ExprList.Q() + RCb;

      QualifiedName.Rule = v | QualifiedName + dot + v; 
                           

      GenericsPostfix.Rule = less + QualifiedName + greater;

      //ExprList.Rule = Expr.Plus(comma);
      #endregion

      #region 3.3 Statement
      Condition.Rule = LCb + Expr + RCb;

      Statement.Rule =  semicolon
                      | "if" + Condition + Statement
                      | "if" + Condition + Statement + PreferShiftHere() + "else" + Statement
                      | "while" + Condition + Statement
                      | "for" + LCb + Expr.Q() + semicolon + Expr.Q() + semicolon + Expr.Q() + RCb + Statement
                      | "foreach" + LCb + v + "in" + Expr + RCb + Statement
                      | "switch" + LCb + Expr + RCb + LFb + SwitchStatements + RFb
      	              | CompoundStatement
                      | Expr + semicolon
                      | "break" + semicolon
                      | "continue" + semicolon
                      | "return" + Expr + semicolon;
      Statements.Rule = MakeStarRule(Statements, null, Statement);
      CompoundStatement.Rule = LFb + Statements + RFb;


      SwitchStatements.Rule = ToTerm("case") + Expr + colon + Statements + SwitchStatements
                             | "default" + colon + Statements;

      #endregion

      #region 3.4 Prog
      Prog.Rule = Element.Star() + Eof;
      FuncContract.Rule = LSb +
                        "pre" + LCb + ExprList.Q() + RCb + semicolon +
                        "post" + LCb + ExprList.Q() + RCb + semicolon +
                        "invariant" + LCb + ExprList.Q() + RCb + semicolon +
                      RSb;

      ParameterListOpt.Rule = MakeStarRule(ParameterListOpt, comma, v);
      FuncDef.Rule = "function" + v + LCb + ParameterListOpt + RCb + FuncContract.Q() + CompoundStatement;

      Element.Rule = Statement | FuncDef;

      Terminal Comment = new CommentTerminal("Comment", "/*", "*/");
      NonGrammarTerminals.Add(Comment);
      Terminal LineComment = new CommentTerminal("LineComment", "//", "\n");
      NonGrammarTerminals.Add(LineComment);
      #endregion
      #endregion

      #region 4. Set starting symbol
      this.Root = Prog; // Set grammar root
      #endregion

      #region 5. Operators precedence
      RegisterOperators(1, "=", "+=", "-=");
      RegisterOperators(2, "+", "-");
      RegisterOperators(3, "*", "/", "%");
      RegisterOperators(4, Associativity.Right, "^");
      RegisterOperators(5, "|", "||");
      RegisterOperators(6, "&", "&&");
      RegisterOperators(7, "==", "!=", ">", "<" , ">=", "<=");
      RegisterOperators(8, "is");
      RegisterOperators(9, "~", "!", "++", "--");
      RegisterOperators(10, ".");

      //RegisterOperators(10, Associativity.Right, ".",",", ")", "(", "]", "[", "{", "}");
      //RegisterOperators(11, Associativity.Right, "else");
      #endregion

      #region 6. Punctuation symbols
      RegisterPunctuation( "(", ")", "[", "]", "{", "}", ",", ";" );
      #endregion
    }

  }
}

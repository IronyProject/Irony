using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.CompilerServices;

namespace Irony.Samples.SQL {
  // Loosely based on SQL89 grammar from Gold parser. Supports some extra TSQL constructs.
  // Does not support BETWEEN, which brings a few shift/reduce conflicts

  [Language("SQL", "89", "SQL 89 grammar")]
  public class SqlGrammar : Grammar {
    public SqlGrammar() : base(false) { //SQL is case insensitive
      //Terminals
      var comment = new CommentTerminal("comment", "/*", "*/");
      var lineComment = new CommentTerminal("line_comment", "--", "\n", "\r\n");
      NonGrammarTerminals.Add(comment);
      NonGrammarTerminals.Add(lineComment);
      var number = new NumberLiteral("number");
      var string_literal = new StringLiteral("string", "'", StringFlags.AllowsDoubledQuote);
      var name = new IdentifierTerminal("name");
      var name_ext = TerminalFactory.CreateSqlExtIdentifier("name_ext");
      var comma = Symbol(",");
      var dot = Symbol(".");
      var CREATE = Symbol("CREATE"); 
      var NULL = Symbol("NULL"); 
      var NOT = Symbol("NOT");
      var UNIQUE = Symbol("UNIQUE"); 
      var WITH = Symbol("WITH");
      var TABLE = Symbol("TABLE"); 
      var ALTER = Symbol("ALTER"); 
      var ADD = Symbol("ADD"); 
      var COLUMN = Symbol("COLUMN"); 
      var DROP = Symbol("DROP"); 
      var CONSTRAINT = Symbol("CONSTRAINT");
      var INDEX = Symbol("INDEX"); 
      var ON = Symbol("ON");
      var KEY = Symbol("KEY");
      var PRIMARY = Symbol("PRIMARY"); 
      var INSERT = Symbol("INSERT");
      var INTO = Symbol("INTO");
      var UPDATE = Symbol("UPDATE");
      var SET = Symbol("SET"); 
      var VALUES = Symbol("VALUES");
      var DELETE = Symbol("DELETE");
      var SELECT = Symbol("SELECT"); 
      var FROM = Symbol("FROM");
      var AS = Symbol("AS");
      var COUNT = Symbol("COUNT");
      var JOIN = Symbol("JOIN");
      var BY = Symbol("BY"); 

      //Non-terminals
      var Id = new NonTerminal("Id");
      var Id_simple = new NonTerminal("id_simple");  
      var stmt = new NonTerminal("stmt");
      var createTableStmt = new NonTerminal("createTableStmt");
      var createIndexStmt = new NonTerminal("createIndexStmt");
      var alterStmt = new NonTerminal("alterStmt");
      var dropTableStmt = new NonTerminal("dropTableStmt");
      var dropIndexStmt = new NonTerminal("dropIndexStmt");
      var selectStmt = new NonTerminal("selectStmt");
      var insertStmt = new NonTerminal("insertStmt");
      var updateStmt = new NonTerminal("updateStmt");
      var deleteStmt = new NonTerminal("deleteStmt");
      var fieldDef = new NonTerminal("fieldDef");
      var fieldDefList = new NonTerminal("fieldDefList");
      var nullSpecOpt = new NonTerminal("nullSpecOpt");
      var typeName = new NonTerminal("typeName"); 
      var typeSpec = new NonTerminal("typeSpec");
      var typeParamsOpt = new NonTerminal("typeParams");
      var constraintDef = new NonTerminal("constraintDef");
      var constraintListOpt = new NonTerminal("constraintListOpt");
      var constraintTypeOpt = new NonTerminal("constraintTypeOpt");
      var idlist = new NonTerminal("idlist"); 
      var idlistPar = new NonTerminal("idlistPar"); 
      var uniqueOpt = new NonTerminal("uniqueOpt");
      var orderList = new NonTerminal("orderList");
      var orderMember = new NonTerminal("orderMember"); 
      var orderDirOpt = new NonTerminal("orderDirOpt");
      var withClauseOpt = new NonTerminal("withClauseOpt");
      var alterCmd = new NonTerminal("alterCmd");
      var insertData = new NonTerminal("insertData"); 
      var intoOpt = new NonTerminal("intoOpt");
      var assignList = new NonTerminal("assignList");
      var whereClauseOpt = new NonTerminal("whereClauseOpt");
      var assignment = new NonTerminal("assignment");
      var expression = new NonTerminal("expression");
      var exprList = new NonTerminal("exprList");
      var selRestrOpt = new NonTerminal("selRestrOpt");
      var selList = new NonTerminal("selList");
      var intoClauseOpt = new NonTerminal("intoClauseOpt");
      var fromClauseOpt = new NonTerminal("fromClauseOpt");
      var groupClauseOpt = new NonTerminal("groupClauseOpt");
      var havingClauseOpt = new NonTerminal("havingClauseOpt");
      var orderClauseOpt = new NonTerminal("orderClauseOpt");
      var columnItemList = new NonTerminal("columnItemList");
      var columnItem = new NonTerminal("columnItem");
      var columnSource = new NonTerminal("columnSource");
      var asOpt = new NonTerminal("asOpt");
      var aliasOpt = new NonTerminal("aliasOpt");
      var aggregate = new NonTerminal("aggregate");
      var aggregateArg = new NonTerminal("aggregateArg");
      var aggregateName = new NonTerminal("aggregateName");
      var tuple = new NonTerminal("tuple");
      var joinChainOpt = new NonTerminal("joinChainOpt");
      var joinKindOpt = new NonTerminal("joinKindOpt");
      var term = new NonTerminal("term");
      var unExpr = new NonTerminal("unExpr");
      var unOp = new NonTerminal("unOp");
      var binExpr = new NonTerminal("binExpr");
      var binOp = new NonTerminal("binOp");
      var betweenExpr = new NonTerminal("betweenExpr");
      var inExpr = new NonTerminal("inExpr");
      var parSelectStmt = new NonTerminal("parSelectStmt");
      var notOpt = new NonTerminal("notOpt");
      var funCall = new NonTerminal("funCall");
      var stmtLine = new NonTerminal("stmtLine");
      var semiOpt = new NonTerminal("semiOpt");
      var stmtList = new NonTerminal("stmtList");
      var funArgs = new NonTerminal("funArgs");
      var inStmt = new NonTerminal("inStmt");


      //BNF Rules
      this.Root = stmtList;
      stmtLine.Rule = stmt + semiOpt;
      semiOpt.Rule = Empty | ";";
      stmtList.Rule = MakePlusRule(stmtList, stmtLine);

      //ID
      Id_simple.Rule = name | name_ext;
      Id.Rule = MakePlusRule(Id, dot, Id_simple);

      stmt.Rule = createTableStmt | createIndexStmt | alterStmt 
                | dropTableStmt | dropIndexStmt 
                | selectStmt | insertStmt | updateStmt | deleteStmt
                | "GO" ;
      //Create table
      createTableStmt.Rule = CREATE + TABLE + Id + "(" + fieldDefList + ")" + constraintListOpt;
      fieldDefList.Rule = MakePlusRule(fieldDefList, comma, fieldDef);
      fieldDef.Rule = Id + typeName + typeParamsOpt + nullSpecOpt;
      nullSpecOpt.Rule = NULL | NOT + NULL | Empty;
      typeName.Rule = Symbol("BIT") | "DATE" | "TIME" | "TIMESTAMP" | "DECIMAL" | "REAL" | "FLOAT" | "SMALLINT" | "INTEGER"
                                   | "INTERVAL" | "CHARACTER"
                                   // MS SQL types:  
                                   | "DATETIME" | "INT" | "DOUBLE" | "CHAR" | "NCHAR" | "VARCHAR" | "NVARCHAR"
                                   | "IMAGE" | "TEXT" | "NTEXT";
      typeParamsOpt.Rule = "(" + number + ")" | "(" + number + comma + number + ")" | Empty;
      constraintDef.Rule = CONSTRAINT + Id + constraintTypeOpt;
      constraintListOpt.Rule = MakeStarRule(constraintListOpt, constraintDef );
      constraintTypeOpt.Rule = PRIMARY + KEY + idlistPar | UNIQUE + idlistPar | NOT + NULL + idlistPar
                             | "Foreign" + KEY + idlistPar + "References" + Id + idlistPar;
      idlistPar.Rule = "(" + idlist + ")";
      idlist.Rule = MakePlusRule(idlist, comma, Id); 

      //Create Index
      createIndexStmt.Rule = CREATE + uniqueOpt + INDEX + Id + ON + Id + orderList + withClauseOpt;
      uniqueOpt.Rule = Empty | UNIQUE;
      orderList.Rule = MakePlusRule(orderList, comma, orderMember);
      orderMember.Rule = Id + orderDirOpt;
      orderDirOpt.Rule = Empty | "ASC" | "DESC";
      withClauseOpt.Rule = Empty | WITH + PRIMARY | WITH + "Disallow" + NULL | WITH + "Ignore" + NULL;

      //Alter 
      alterStmt.Rule = ALTER + TABLE + Id + alterCmd;
      alterCmd.Rule = ADD + COLUMN  + fieldDefList + constraintListOpt 
                    | ADD + constraintDef
                    | DROP + COLUMN + Id
                    | DROP + CONSTRAINT + Id;

      //Drop stmts
      dropTableStmt.Rule = DROP + TABLE + Id;
      dropIndexStmt.Rule = DROP + INDEX + Id + ON + Id; 

      //Insert stmt
      insertStmt.Rule = INSERT + intoOpt + Id + idlistPar + insertData;
      insertData.Rule = selectStmt | VALUES + "(" + exprList + ")"; 
      intoOpt.Rule = Empty | INTO; //Into is optional in MSSQL

      //Update stmt
      updateStmt.Rule = UPDATE + Id + SET + assignList + whereClauseOpt;
      assignList.Rule = MakePlusRule(assignList, comma, assignment);
      assignment.Rule = Id + "=" + expression;

      //Delete stmt
      deleteStmt.Rule = DELETE + FROM + Id + whereClauseOpt;

      //Select stmt
      selectStmt.Rule = SELECT + selRestrOpt + selList + intoClauseOpt + fromClauseOpt + whereClauseOpt +
                        groupClauseOpt + havingClauseOpt + orderClauseOpt;
      selRestrOpt.Rule = Empty | "ALL" | "DISTINCT";
      selList.Rule = columnItemList | "*";
      columnItemList.Rule = MakePlusRule(columnItemList, comma, columnItem);
      columnItem.Rule = columnSource + aliasOpt;
      aliasOpt.Rule = Empty | asOpt + Id; 
      asOpt.Rule = Empty | AS;
      columnSource.Rule = aggregate | Id;
      aggregate.Rule = aggregateName + "(" + aggregateArg + ")";
      aggregateArg.Rule = expression | "*"; 
      aggregateName.Rule = COUNT | "Avg" | "Min" | "Max" | "StDev" | "StDevP" | "Sum" | "Var" | "VarP";
      intoClauseOpt.Rule = Empty | INTO + Id;
      fromClauseOpt.Rule = Empty | FROM + idlist + joinChainOpt; 
      joinChainOpt.Rule = Empty | joinKindOpt + JOIN + idlist + ON + Id + "=" + Id;
      joinKindOpt.Rule = Empty | "INNER" | "LEFT" | "RIGHT";
      whereClauseOpt.Rule = Empty | "WHERE" + expression;
      groupClauseOpt.Rule = Empty | "GROUP" + BY + idlist;
      havingClauseOpt.Rule = Empty | "HAVING" + expression; 
      orderClauseOpt.Rule = Empty | "ORDER" + BY + orderList;
 
      //Expression
      exprList.Rule = MakePlusRule(exprList, comma, expression);
      expression.Rule = term | unExpr | binExpr;// | betweenExpr; //-- BETWEEN doesn't work - yet; brings a few parsing conflicts 
      term.Rule = Id | string_literal | number | funCall | tuple | parSelectStmt;// | inStmt;
      tuple.Rule = "(" + exprList + ")";
      parSelectStmt.Rule = "(" + selectStmt + ")"; 
      unExpr.Rule = unOp + term;
      unOp.Rule = NOT | "+" | "-" | "~"; 
      binExpr.Rule = expression + binOp + expression;
      binOp.Rule = Symbol("+") | "-" | "*" | "/" | "%" //arithmetic
                 | "&" | "|" | "^"                     //bit
                 | "=" | ">" | "<" | ">=" | "<=" | "<>" | "!=" | "!<" | "!>"
                 | "AND" | "OR" | "LIKE" | NOT + "LIKE" | "IN" | NOT + "IN" ; 
      betweenExpr.Rule = expression + notOpt + "BETWEEN" + expression + "AND" + expression;
      notOpt.Rule = Empty | NOT;
      //funCall covers some psedo-operators and special forms like ANY(...), SOME(...), ALL(...), EXISTS(...), IN(...)
      funCall.Rule = name + "(" + funArgs  + ")";
      funArgs.Rule = selectStmt | exprList;
      inStmt.Rule = expression + "IN" + "(" + exprList + ")";

      //Operators
      RegisterOperators(10, "*", "/", "%"); 
      RegisterOperators(9, "+", "-");
      RegisterOperators(8, "=" , ">" , "<" , ">=" , "<=" , "<>" , "!=" , "!<" , "!>");
      RegisterOperators(7, "^", "&", "|");
      RegisterOperators(6, "NOT"); 
      RegisterOperators(5, "AND");
      RegisterOperators(4, "OR", "LIKE", "IN");

      RegisterPunctuation(",", "(", ")");
      RegisterPunctuation(semiOpt);
      base.MarkTransient(stmt, Id_simple, term, asOpt, aliasOpt, stmtLine, binOp, expression);

      /*
      AddKeywords("SELECT", "CREATE", "ALTER", "UPDATE", "INSERT", "DELETE", "FROM", "WHERE", "GROUP", "ORDER", "BY", 
                  "INNER", "LEFT", "RIGHT", "JOIN", "ON"); 
      */
    }//constructor

  }//class
}//namespace

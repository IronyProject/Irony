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

namespace Irony.Samples.Ruby {

  // Note that this is a sample grammar, not real Ruby production-quality grammar. 
  // It is loosely based on original grammar from Matz whose purpose (as it seems) was to explain language syntax to programmers, 
  // not for directly building parsers.  
  // See Grammar Errors tab in GrammarExplorer for remaining conflicts.

  public class RubyGrammar : Grammar {

    public RubyGrammar() {

      #region Terminals
      //String Literals with single and double-quote start/end symbols
      StringLiteral STRING = new StringLiteral("STRING", BnfFlags.StringIgnoreCase);
      STRING.StartEndSymbolTable.Add("\"", StringOptions.None);
      STRING.StartEndSymbolTable.Add("'", StringOptions.None);
      Terminal HereDoc = new Terminal("HereDoc"); //-- implement me!
      Terminal RegExLiteral = new Terminal("RegExLiteral"); //-- implement me!
      IdentifierTerminal IDENTIFIER = new IdentifierTerminal("identifier", "_!?", "_$@");
      //                                                name     extraChars      extraFirstChars 
      IDENTIFIER.Alias = "variable";
      //we need to isolate reserved words to avoid ambiguities in grammar 
      IDENTIFIER.AddReservedWords("do", "end", "def", "class", 
                                       "if", "case", "return", "yield", "while", "until");  //and some others...
      Terminal Number = new NumberTerminal("Number", "number");
      Terminal Comment = new CommentTerminal("Comment", "#", "\n");
      ExtraTerminals.Add(Comment); //add comment explicitly to this list as it is not reachable from Root

      //some conveniency variables
      Terminal Pipe = Symbol("|");
      Terminal dot = Symbol(".");
      Terminal comma = Symbol(",");
      #endregion

      #region NonTerminals 
      //NT variables names match element names in original grammar 
      NonTerminal PROGRAM = new NonTerminal("PROGRAM");
      NonTerminal COMPSTMT = new NonTerminal("COMPSTMT");
      NonTerminal STMT = new NonTerminal("STMT");
      NonTerminal BLOCK = new NonTerminal("BLOCK");
      NonTerminal EXPR = new NonTerminal("EXPR");
      //NonTerminal CALL = new NonTerminal("CALL");
      NonTerminal COMMAND = new NonTerminal("COMMAND");
      NonTerminal FUNCTION = new NonTerminal("FUNCTION");
      NonTerminal ARG = EXPR;// new NonTerminal("ARG");
      NonTerminal PRIMARY = new NonTerminal("PRIMARY", "operand");
      NonTerminal WHEN_ARGS = new NonTerminal("WHEN_ARGS");
      NonTerminal THEN = new NonTerminal("THEN");
      NonTerminal BLOCK_BEGIN = new NonTerminal("BLOCK_BEGIN");
      NonTerminal BLOCK_END = new NonTerminal("BLOCK_END");
      NonTerminal BLOCK_VAR = new NonTerminal("BLOCK_VAR");
//      NonTerminal MLHS_ITEM = new NonTerminal("MLHS_ITEM");
      NonTerminal LHS = new NonTerminal("LHS");
      NonTerminal MRHS = new NonTerminal("MRHS");
//      NonTerminal MLHS = MRHS; // new NonTerminal("MLHS");
      NonTerminal CALL_ARGS = new NonTerminal("CALL_ARGS");
      NonTerminal CALL_ARGS_P = new NonTerminal("CALL_ARGS_P");
      NonTerminal AMP_ARG = new NonTerminal("AMP_ARG");
      NonTerminal STAR_ARG = new NonTerminal("STAR_ARG");
      NonTerminal ARGS = new NonTerminal("ARGS");
      NonTerminal ARGDECL = new NonTerminal("ARGDECL");
      NonTerminal ARGLIST = new NonTerminal("ARGLIST");
//      NonTerminal SINGLETON = new NonTerminal("SINGLETON");
      NonTerminal ASSOCS = new NonTerminal("ASSOCS");
      NonTerminal ASSOC = new NonTerminal("ASSOC");
//      NonTerminal VARIABLE = new NonTerminal("VARIABLE");  --merged into IDENTIFIER
      NonTerminal LITERAL = new NonTerminal("LITERAL", "value");
      NonTerminal TERM = new NonTerminal("TERM");
      NonTerminal DO = new NonTerminal("DO");
//      NonTerminal VARNAME = new NonTerminal("VARNAME");   // note 1
      NonTerminal GLOBAL = new NonTerminal("GLOBAL");
      NonTerminal RETURN_STMT = new NonTerminal("RETURN_STMT");
      NonTerminal YIELD_STMT = new NonTerminal("YIELD_STMT");
      NonTerminal DEFINEDQ_STMT = new NonTerminal("DEFINEDQ_STMT");
      NonTerminal FUNCTION_STMT = new NonTerminal("FUNCTION_STMT");
      NonTerminal IF_STMT = new NonTerminal("IF_STMT");
      NonTerminal UNLESS_STMT = new NonTerminal("UNLESS_STMT");
      NonTerminal WHILE_STMT = new NonTerminal("WHILE_STMT");
      NonTerminal UNTIL_STMT = new NonTerminal("UNTIL_STMT");
      NonTerminal CASE_STMT = new NonTerminal("CASE_STMT");
      NonTerminal FOR_STMT = new NonTerminal("FOR_STMT");
      NonTerminal BLOCK_STMT = new NonTerminal("BLOCK_STMT");
      NonTerminal CLASS_DEF = new NonTerminal("CLASS_DEF");
      NonTerminal BASE_REF = new NonTerminal("BASE_REF");
      NonTerminal MODULE = new NonTerminal("MODULE_STMT");
      NonTerminal DEFFUNC_STMT = new NonTerminal("DEFFUNC_STMT");
      NonTerminal DEFSING_STMT = new NonTerminal("DEFSING_STMT");
      NonTerminal SINGLETON = new NonTerminal("SINGLETON");
      NonTerminal END = new NonTerminal("end");

      NonTerminal SYMBOL = new NonTerminal("SYMBOL");
      //Not in original grammar
      NonTerminal FNAME = new NonTerminal("FNAME");
      BLOCK_BEGIN.Rule = Symbol("do") | "{";
      BLOCK_END.Rule = Symbol("end") | "}";
      NonTerminal OPERATION = new NonTerminal("OPERATION");
      //      Terminal VARNAME = IDENTIFIER;
      NonTerminal AUG_ASGN = new NonTerminal("AUG_ASGN");
      NonTerminal BINOP = new NonTerminal("BINOP", "operator");
      NonTerminal UNOP = new NonTerminal("UNOP");
      NonTerminal DELIM = new NonTerminal("DELIM");

      #endregion

      #region Rules 
      //Set grammar root
      this.Root = PROGRAM;

      //PROGRAM         : COMPSTMT
      PROGRAM.Rule = COMPSTMT; // +Grammar.Eof;
      //COMPSTMT        : STMT (TERM EXPR)* [TERM] 
      COMPSTMT.Rule = NewLine.Q() + STMT.Plus(TERM) + TERM.Q();   

      /* STMT   : CALL do [`|' [BLOCK_VAR] `|'] COMPSTMT end
                | undef FNAME
                | alias FNAME FNAME
                | STMT if EXPR
                | STMT while EXPR
                | STMT unless EXPR
                | STMT until EXPR
                | `BEGIN' `{' COMPSTMT `}'
                | `"end"' `{' COMPSTMT `}'
                | LHS `=' COMMAND [do [`|' [BLOCK_VAR] `|'] COMPSTMT end]
                | EXPR    */
      STMT.Rule =     FUNCTION
                      | COMMAND + BLOCK.Q()  
                      | "undef" + FNAME | "alias" + FNAME + FNAME 
                      | STMT + (Symbol("if")|"while"|"unless"|"until") + EXPR 
                      | Symbol("BEGIN") + "{" + COMPSTMT + "}"  
                     // | Symbol("end") + BLOCK_BEGIN + COMPSTMT + BLOCK_END    // don't quite get it
                   //   | LHS + "=" + COMMAND + BLOCK.Q()
                      | LHS + "=" + EXPR  //changed this 
                      | LHS + AUG_ASGN + EXPR
                      | EXPR; 
      BLOCK.Rule = "do" + WithQ(Pipe + BLOCK_VAR.Q() + Pipe) + COMPSTMT + "end";

      /* EXPR   : MLHS `=' MRHS
                | return CALL_ARGS
                | yield CALL_ARGS
                | EXPR and EXPR
                | EXPR or EXPR
                | not EXPR
                | COMMAND
                | `!' COMMAND
                | ARG   */
      //this one is completely changed, for better or worse...
      EXPR.Rule = //  MRHS + "=" + EXPR | //changed to EXPR   
                     //  LHS + "=" + EXPR  //changed this 
                    // | LHS + AUG_ASGN + EXPR
                      EXPR + BINOP + EXPR   
                     | UNOP + EXPR
                     //| "(" + EXPR + ")"
                      | EXPR + "?" + EXPR + ":" + EXPR   //added this to cover "?" operator
                     | "defined?" + ARG
                     | PRIMARY
                      ;
      ARG = EXPR;
      // CALL   : FUNCTION | COMMAND
     // CALL.Expression = FUNCTION | COMMAND; //expression embedded directly into STMT

      /* COMMAND         : OPERATION CALL_ARGS
                | PRIMARY `.' OPERATION CALL_ARGS
                | PRIMARY `::' OPERATION CALL_ARGS
                | super CALL_ARGS   */
      COMMAND.Rule =  OPERATION + CALL_ARGS
                         | PRIMARY + DELIM + OPERATION + CALL_ARGS
                         | "super" + CALL_ARGS;
      OPERATION.Rule =  IDENTIFIER;
      DELIM.Rule = dot | "::";

      /* FUNCTION   : OPERATION [`(' [CALL_ARGS] `)']
                | PRIMARY `.' OPERATION `(' [CALL_ARGS] `)'
                | PRIMARY `::' OPERATION `(' [CALL_ARGS] `)'
                | PRIMARY `.' OPERATION
                | PRIMARY `::' OPERATION
                | super `(' [CALL_ARGS] `)'
                | super  */
      FUNCTION.Rule = OPERATION + CALL_ARGS_P
                   | PRIMARY + DELIM + OPERATION + CALL_ARGS_P.Q() 
                   | "super" + CALL_ARGS_P;
      CALL_ARGS_P.Rule = "(" + CALL_ARGS.Q() + ")";        
      /*  ARG   : LHS `=' ARG
                | LHS OP_ASGN ARG
                | ARG `..' ARG
                | ARG `...' ARG
                | ARG `+' ARG
                | ARG `-' ARG
                | ARG `*' ARG
                | ARG `/' ARG
                | ARG `%' ARG
                | ARG `**' ARG
                | `+' ARG
                | `-' ARG
                | ARG `|' ARG
                | ARG `^' ARG
                | ARG `&' ARG
                | ARG `<=>' ARG
                | ARG `>' ARG
                | ARG `>=' ARG
                | ARG `<' ARG
                | ARG `<=' ARG
                | ARG `==' ARG
                | ARG `===' ARG
                | ARG `!=' ARG
                | ARG `=~' ARG
                | ARG `!~' ARG
                | `!' ARG
                | `~' ARG
                | ARG `<<' ARG
                | ARG `>>' ARG
                | ARG `&&' ARG
                | ARG `||' ARG
                | defined? ARG
                | PRIMARY   */

    /*  ARG.Expression = LHS + "=" + EXPR  //changed this 
                     | LHS + AUG_ASGN + EXPR   
                     | ARG + BINOP + ARG  //moved to EXPR 
                     | UNOP + ARG
                     | "defined?" + ARG
                     | PRIMARY
                     ; */
      AUG_ASGN.Rule = Symbol("+=") | "-=" | "*=" | "/=" | "%=" | "**=" | "&=" | "|=" | "^=" | "<<=" | ">>=" | "&&=" | "||=";

      BINOP.Rule = Symbol("..") | "..." | "+" | "-" | "*" | "/" | "%" | "**" | "|" | "^" | "&"
                         | "<=>" | ">" | ">=" | "<" | "<=" | "==" | "===" | "!=" | "=~" | "!~" | "<<" | ">>" | "&&" | "||"
                         | "and" | "or";  //added these two here
      UNOP.Rule = Symbol("+") | "-" | "!" | "~";
       /*PRIMARY:    */
      /*        `(' COMPSTMT `)'
                | LITERAL
                | VARIABLE
                | PRIMARY `::' IDENTIFIER
                | `::' IDENTIFIER
                | PRIMARY `[' [ARGS] `]'  
                | `[' [ARGS [`,']] `]'
                | `{' [(ARGS|ASSOCS) [`,']] `}'
                | return [`(' [CALL_ARGS] `)']
                | yield [`(' [CALL_ARGS] `)']
                | defined? `(' ARG `)' 
                | FUNCTION
                | FUNCTION `{' [`|' [BLOCK_VAR] `|'] COMPSTMT `}'     
                | if EXPR THEN
                  COMPSTMT
                  (elsif EXPR THEN COMPSTMT)*
                  [else COMPSTMT]
                  end
                | unless EXPR THEN
                  COMPSTMT
                  [else COMPSTMT]
                  end
                | while EXPR DO COMPSTMT end
                | until EXPR DO COMPSTMT end
                | case COMPSTMT
                  (when WHEN_ARGS THEN COMPSTMT)+
                  [else COMPSTMT]
                  end
                | for BLOCK_VAR in EXPR DO
                  COMPSTMT
                  end
                | begin
                  COMPSTMT
                  [rescue [ARGS] DO COMPSTMT]+
                  [else COMPSTMT]
                  [ensure COMPSTMT]
                  end
                | class IDENTIFIER [`<' IDENTIFIER]
                  COMPSTMT
                  end"=
                | module IDENTIFIER
                  COMPSTMT
                  end
                | def FNAME ARGDECL
                  COMPSTMT
                  end
                | def SINGLETON (`.'|`::') FNAME ARGDECL
                  COMPSTMT
                  end */
      PRIMARY.Rule = 
       // "(" + COMPSTMT + ")" |   //-- removed this to fix ambiguity
        LITERAL 
        | LHS  //note 1.
        | "[" + WithQ(ARGS + comma.Q()) + "]"
        | "{" + WithQ( (ARGS|ASSOC) + comma.Q() ) + "}"
        | RETURN_STMT | YIELD_STMT | DEFINEDQ_STMT | FUNCTION_STMT | IF_STMT | UNLESS_STMT | WHILE_STMT 
        | UNTIL_STMT | CASE_STMT | FOR_STMT | BLOCK_STMT | CLASS_DEF | MODULE | DEFFUNC_STMT | DEFSING_STMT;
         // LHS.Expression = VARIABLE | PRIMARY + "[" + ARGS.Q() + "]" | PRIMARY + "." + IDENTIFIER;                           

      RETURN_STMT.Rule = "return" + EXPR;// CALL_ARGS_P.Q(); //changed this
      YIELD_STMT.Rule = "yield" + CALL_ARGS_P.Q();
      DEFINEDQ_STMT.Rule = Symbol("defined?") + "(" + ARG + ")";
      FUNCTION_STMT.Rule = FUNCTION + WithQ("{" + WithQ("|" + BLOCK_VAR.Q() + "|") + COMPSTMT + "}");
      IF_STMT.Rule = "if" + EXPR + THEN + COMPSTMT + WithStar("elsif" + EXPR + THEN + COMPSTMT) + WithQ("else" + COMPSTMT) + END;
      UNLESS_STMT.Rule = "unless" + EXPR + THEN + COMPSTMT + "else" + COMPSTMT + END;
      WHILE_STMT.Rule = "while" + EXPR + DO + COMPSTMT + END;
      UNTIL_STMT.Rule = "until" + EXPR + DO + COMPSTMT + END;
      CASE_STMT.Rule = "case" + COMPSTMT + WithPlus("when" + WHEN_ARGS + THEN + COMPSTMT) 
                                 + WithQ("else" + COMPSTMT) + END;
      FOR_STMT.Rule = "for" + BLOCK_VAR + "in" + EXPR + DO + COMPSTMT + END;
      BLOCK_STMT.Rule = "begin" + COMPSTMT + WithPlus("rescue" + ARGS.Q() + DO + COMPSTMT) 
                                 + WithQ("else" + COMPSTMT) + WithQ("ensure" + COMPSTMT) + END;
      CLASS_DEF.Rule = "class" + IDENTIFIER + BASE_REF.Q() + COMPSTMT + END;
      BASE_REF.Rule = "<" + IDENTIFIER; 
      MODULE.Rule = "module" + IDENTIFIER + COMPSTMT + END;
      DEFFUNC_STMT.Rule = "def" + FNAME + ARGDECL.Q() + COMPSTMT + END;
      DEFSING_STMT.Rule = "def" + SINGLETON + (dot|"::") + FNAME + ARGDECL.Q() + COMPSTMT + END;
      END.Rule = "end"; // TERM.Q() + "end";
      //  SINGLETON : VARIABLE | `(' EXPR `)' 
      SINGLETON.Rule = IDENTIFIER | "(" + EXPR + ")"; 
      // WHEN_ARGS       : ARGS [`,' `*' ARG]  | `*' ARG
      WHEN_ARGS.Rule = ARGS + WithQ(comma + "*" + ARG) | "*" + ARG;
      // THEN   : TERM | then | TERM then
      THEN.Rule = TERM | "then" | TERM + "then";
      // DO     : TERM | do | TERM do
      DO.Rule = TERM | "do" | TERM + "do";
      //  BLOCK_VAR       : LHS | MLHS
//      BLOCK_VAR.Expression = LHS | MLHS;   // -- ambiguous, changing to the following:
      BLOCK_VAR.Rule = IDENTIFIER | "(" + IDENTIFIER.Plus(comma) + ")";
      //  MLHS  : MLHS_ITEM `,' [MLHS_ITEM (`,' MLHS_ITEM)*] [`*' [LHS]]  | `*' LHS   
//      MLHS.Expression = MLHS_ITEM.Plus(",") + WithQ("*" + LHS.Q()) | "*" + LHS;  --ambiguous
      //MLHS.Expression = PRIMARY.Plus(",") + WithQ("*" + LHS.Q()) | "*" + LHS;
      //  MLHS_ITEM  : LHS | '(' MLHS ')'  
      //MLHS_ITEM.Expression = LHS | "(" + MLHS + ")";  //--ambiguous!!! using PRIMARY
      //MLHS_ITEM = PRIMARY;

      /* LHS    : VARIABLE
                | PRIMARY `[' [ARGS] `]'
                | PRIMARY `.' IDENTIFIER  */
     // LHS.Expression = IDENTIFIER | PRIMARY + "[" + ARGS.Q() + "]" | PRIMARY + dot + IDENTIFIER;
      LHS.Rule = OPERATION 
                     | PRIMARY + "[" + ARGS.Q() + "]"
                     | "(" + EXPR + ")";
      //   MRHS : ARGS [`,' `*' ARG] | `*' ARG    
      MRHS.Rule = ARGS + WithQ(comma + "*" + ARG) | "*" + ARG;
      /* CALL_ARGS   : ARGS
                | ARGS [`,' ASSOCS] [`,' `*' ARG] [`,' `&' ARG]
                | ASSOCS [`,' `*' ARG] [`,' `&' ARG]
                | `*' ARG [`,' `&' ARG]
                | `&' ARG
                | COMMAND    */
      CALL_ARGS.Rule = // ARGS |  //removed this - it is covered by next expression
                             ARGS + WithQ(comma + ASSOCS) + STAR_ARG.Q() + AMP_ARG.Q()  
                           | ASSOCS + STAR_ARG.Q() + AMP_ARG.Q()
                           | "*" + ARG + AMP_ARG.Q()
                           | "&" + ARG
                           | COMMAND; 
      AMP_ARG.Rule = comma + "&" + ARG;
      STAR_ARG.Rule = comma + "*" + ARG;
      //  ARGS            : ARG (`,' ARG)*
      ARGS.Rule = ARG.Plus(comma);
      // ARGDECL         : `(' ARGLIST `)'  | ARGLIST TERM 
      ARGDECL.Rule = "(" + ARGLIST + ")" | ARGLIST + TERM;
      /*   ARGLIST         : IDENTIFIER(`,'IDENTIFIER)*[`,'`*'[IDENTIFIER]][`,'`&'IDENTIFIER]
                | `*'IDENTIFIER[`,'`&'IDENTIFIER]
                | [`&'IDENTIFIER]    */
      ARGLIST.Rule = IDENTIFIER.Plus(comma) + WithQ(comma + "*" + IDENTIFIER.Q()) + WithQ(comma + "&" + IDENTIFIER)
                           | "*" + IDENTIFIER + WithQ(comma + "&" + IDENTIFIER)
                           | "&" + IDENTIFIER; 
      // ASSOCS : ASSOC (`,' ASSOC)*
      ASSOCS.Rule = ASSOC.Plus(comma);
      //ASSOC : ARG `=>' ARG
      ASSOC.Rule = ARG + "=>" + ARG;
      //  VARIABLE : VARNAME | nil | self    -- variable is merged into IDENTIFIER
      //VARIABLE.Expression = IDENTIFIER | "nil" | "self";
      // LITERAL : numeric | SYMBOL | STRING | STRING2 | HERE_DOC | REGEXP
      LITERAL.Rule = Number | SYMBOL | STRING | HereDoc | RegExLiteral;
      SYMBOL.Rule = Symbol(":") + IDENTIFIER; // (FNAME | VARNAME); //note 1.
      /*  FNAME           : IDENTIFIER | `..' | `|' | `^' | `&'
                | `<=>' | `==' | `===' | `=~'
                | `>' | `>=' | `<' | `<='
                | `+' | `-' | `*' | `/' | `%' | `**'
                | `<<' | `>>' | `~'
                | `+@' | `-@' | `[]' | `[]='  */
      FNAME.Rule = IDENTIFIER | ".." | "|" | "^" | "&" | "<=>" | "==" | "===" | "=~"
                | ">" | ">=" | "<" | "<="  | "+" | "-" | "*" | "/" | "%" | "**"
                | "<<" | ">>" | "~" | "+@" | "-@" | "[]" | "[]=";
      // TERM : `;' | `\n'
      TERM.Rule = NewLine | ";";  //NewLine is produced by token filter
      #endregion

      //error handling
      EXPR.ErrorRule = SyntaxError;
      DEFFUNC_STMT.ErrorRule = "def" + SyntaxError + COMPSTMT + END;

      #region misc: Operators, TokenFilters, etc
      //Register operators - not sure if precedence is assigned correctly
      RegisterOperators(100, Associativity.Right, "**");
      RegisterOperators( 90, "<<", ">>");
      RegisterOperators( 80, "*", "/", "%");
      RegisterOperators( 70, "+", "-");
      RegisterOperators( 60, "&", "&&", "and");
      RegisterOperators( 50, "|", "||", "or", "^");
      RegisterOperators( 40, ">", ">=", "<", "<=", "?");  
      RegisterOperators( 30, "<=>" , "==" , "===" , "!=" , "=~" , "!~");
      RegisterOperators( 20, "..", "...");

      RegisterPunctuation("(", ")", "," );

      CodeOutlineFilter filter = new CodeOutlineFilter(false);
      TokenFilters.Add(filter);
      #endregion


    }//constructor


  }//class
}//namespace

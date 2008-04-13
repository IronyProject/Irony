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

namespace Irony.Samples.Python {
  // Sample Python reduced grammar
  // Note that this is a sample grammar, not real Python production-quality grammar. 
  // It is coded by manual translation of grammar rules in provided in Python_auth_svn.txt file.
  // It was tweaked in many places to resolve LALR conflicts, sometimes almost beyond recognition of original expressions.
  // See Grammar Errors tab in GrammarExplorer for remaining conflicts.

  //Note that unlike Ruby sample grammar, Python grammar does not use "non-grammar" operator precedence facility 
  // to define proper parsing order of arithmetic expressions. Instead, it expresses operator precedence directly 
  // in grammar by defining multiple expression elements with different operator-combination rules. As a result, 
  // Python AST appears much more convoluted - many simple nodes are stacked into one another multiple times. 
  // We did this purely for illustration purposes. Python grammar (and AST trees) can be compacted 
  //  using operator precedence and associativity.

  //TODO: (13 Feb 2008) - the grammar is broken, AST tree shows some "extra" symbols; this is result of some latest
  // refactoring
  public class PythonGrammar : Grammar {

    public PythonGrammar() {
      #region Declare Terminals 
      ConstantTerminal Constants = new ConstantTerminal("Constants");
      Constants.Add("True", true);
      Constants.Add("False", false);
      IdentifierTerminal Identifier = new IdentifierTerminal("Identifier");
      Terminal Comment = new CommentTerminal("Comment", "#", "\n");
      ExtraTerminals.Add(Comment);

      Terminal comma = Symbol(",", "comma");
      //commaQ is optional trailing comma in lists; it causes several conflicts in this grammar
      // so we get rid of it (by assigning it Empty value)
      //NonTerminal commaQ = comma.Q();  //this causes several conflicts
      BnfTerm commaQ = Empty; 
      Terminal dot = Symbol(".", "dot");
      Terminal LBr   = Symbol("[");
      Terminal RBr   = Symbol("]");
      Terminal bQuote   = Symbol("`");
      Terminal ellipsis   = Symbol("...");
      Terminal colon = Symbol(":");
      Terminal NAME = Identifier;
      Terminal NEWLINE = Grammar.NewLine;
      Terminal INDENT = Grammar.Indent;
      Terminal DEDENT = Grammar.Dedent;
      Terminal semicolon = Symbol(";");
      Terminal EOF = Grammar.Eof;
      Terminal NUMBER = TerminalFactory.CreatePythonNumber("NUMBER");
      #endregion

      #region Declare NonTerminals
      StringLiteral STRING = TerminalFactory.CreatePythonString("String");
      NonTerminal single_input = new NonTerminal("single_input");
      NonTerminal file_input = new NonTerminal("file_input");
      NonTerminal eval_input = new NonTerminal("eval_input");
      NonTerminal decorator = new NonTerminal("decorator");
      NonTerminal funcdef = new NonTerminal("funcdef");
      NonTerminal parameters = new NonTerminal("parameters");
      NonTerminal varargslist = new NonTerminal("varargslist");
      NonTerminal vararg = new NonTerminal("vararg");
      NonTerminal fpdef = new NonTerminal("fpdef");
      NonTerminal fpdef_ext = new NonTerminal("fpdef_ext");
      NonTerminal fplist = new NonTerminal("fplist");
      NonTerminal stmt = new NonTerminal("stmt");
      NonTerminal simple_stmt = new NonTerminal("simple_stmt");
      NonTerminal small_stmt = new NonTerminal("small_stmt");
      NonTerminal expr_stmt = new NonTerminal("expr_stmt");
      NonTerminal yield_or_testlist = new NonTerminal("yield_or_testlist");
      NonTerminal augassign = new NonTerminal("augassign");
      NonTerminal print_stmt = new NonTerminal("print_stmt");
      NonTerminal del_stmt = new NonTerminal("del_stmt");
      NonTerminal pass_stmt = new NonTerminal("pass_stmt");
      NonTerminal flow_stmt = new NonTerminal("flow_stmt");
      NonTerminal break_stmt = new NonTerminal("break_stmt");
      NonTerminal continue_stmt = new NonTerminal("continue_stmt");
      NonTerminal return_stmt = new NonTerminal("return_stmt");
      NonTerminal yield_stmt = new NonTerminal("yield_stmt");
      NonTerminal raise_stmt = new NonTerminal("raise_stmt");
      NonTerminal import_stmt = new NonTerminal("import_stmt");
      NonTerminal import_name = new NonTerminal("import_name");
      NonTerminal import_from = new NonTerminal("import_from");
      NonTerminal import_as_name = new NonTerminal("import_as_name");
      NonTerminal dotted_as_name = new NonTerminal("dotted_as_name");
      NonTerminal import_as_names = new NonTerminal("import_as_names");
      NonTerminal dotted_as_names = new NonTerminal("dotted_as_names");
      NonTerminal dotted_name = new NonTerminal("dotted_name");
      NonTerminal global_stmt = new NonTerminal("global_stmt");
      NonTerminal exec_stmt = new NonTerminal("exec_stmt");
      NonTerminal assert_stmt = new NonTerminal("assert_stmt");
      NonTerminal compound_stmt = new NonTerminal("compound_stmt");
      NonTerminal if_stmt = new NonTerminal("if_stmt");
      NonTerminal else_clause = new NonTerminal("else_clause");
      NonTerminal while_stmt = new NonTerminal("while_stmt");
      NonTerminal for_stmt = new NonTerminal("for_stmt");
      NonTerminal try_stmt = new NonTerminal("try_stmt");
      NonTerminal finally_block = new NonTerminal("finally_block");
      NonTerminal with_stmt = new NonTerminal("with_stmt");
      NonTerminal with_var = new NonTerminal("with_var");
      NonTerminal except_clause = new NonTerminal("except_clause");
      NonTerminal suite = new NonTerminal("suite");
      NonTerminal testlist_safe = new NonTerminal("testlist_safe");
      NonTerminal old_test = new NonTerminal("old_test");
      NonTerminal old_lambdef = new NonTerminal("old_lambdef");
      NonTerminal test = new NonTerminal("test");
      NonTerminal testlist = new NonTerminal("testlist");
      NonTerminal testlist1 = new NonTerminal("testlist1");
      NonTerminal or_test = new NonTerminal("or_test");
      NonTerminal and_test = new NonTerminal("and_test");
      NonTerminal not_test = new NonTerminal("not_test");
      NonTerminal comparison = new NonTerminal("comparison");
      NonTerminal comp_op = new NonTerminal("comp_op");
      NonTerminal expr = new NonTerminal("expr");
      NonTerminal xor_expr = new NonTerminal("xor_expr");
      NonTerminal and_expr = new NonTerminal("and_expr");
      NonTerminal shift_expr = new NonTerminal("shift_expr");
      NonTerminal arith_expr = new NonTerminal("arith_expr");
      NonTerminal shift_op = new NonTerminal("shift_op");
      NonTerminal sum_op = new NonTerminal("sum_op");
      NonTerminal mul_op = new NonTerminal("mul_op");

      NonTerminal term = new NonTerminal("term");
      NonTerminal factor = new NonTerminal("factor");
      NonTerminal power = new NonTerminal("power");
      NonTerminal atom = new NonTerminal("atom");
      NonTerminal listmaker = new NonTerminal("listmaker");
      NonTerminal testlist_gexp = new NonTerminal("testlist_gexp");
      NonTerminal lambdef = new NonTerminal("lambdef");
      NonTerminal trailer = new NonTerminal("trailer");
      NonTerminal subscriptlist = new NonTerminal("subscriptlist");
      NonTerminal subscript = new NonTerminal("subscript");
      NonTerminal sliceop = new NonTerminal("sliceop");
      NonTerminal exprlist = new NonTerminal("exprlist");
      NonTerminal dictmaker = new NonTerminal("dictmaker");
      NonTerminal dict_elem = new NonTerminal("dict_elem");
      NonTerminal classdef = new NonTerminal("classdef");
      NonTerminal arglist = new NonTerminal("arglist");
      NonTerminal argument = new NonTerminal("argument");
      NonTerminal list_iter = new NonTerminal("list_iter");
      NonTerminal list_for = new NonTerminal("list_for");
      NonTerminal list_if = new NonTerminal("list_if");
      NonTerminal gen_iter = new NonTerminal("gen_iter");
      NonTerminal gen_for = new NonTerminal("gen_for");
      NonTerminal gen_if = new NonTerminal("gen_if");
      NonTerminal encoding_decl = new NonTerminal("encoding_decl");
      NonTerminal yield_expr = new NonTerminal("yield_expr");
      #endregion

      #region RULES
      // The commented rules before each statement are original grammar rules
      //  copied from the grammar file. 
      //Set grammar root
      base.Root = file_input;
      //file_input: (NEWLINE | stmt)* ENDMARKER
      file_input.Rule = NEWLINE.Q() + stmt.Star();// +EOF; //EOF is added by default as a lookahead

      //single_input: NEWLINE | simple_stmt | compound_stmt NEWLINE
      single_input.Rule = NEWLINE | simple_stmt | compound_stmt + NEWLINE;
      //eval_input: testlist NEWLINE* ENDMARKER
      eval_input.Rule = NEWLINE.Q() + WithStar(testlist + NEWLINE); // +EOF;  //changed this

      //decorators: decorator+
      //decorator: '@' dotted_name [ '(' [arglist] ')' ] NEWLINE
      decorator.Rule = "@" + dotted_name + WithQ("(" + arglist.Q() + ")") + NEWLINE;
      //funcdef: [decorators] 'def' NAME parameters ':' suite
      funcdef.Rule = decorator.Star() + "def" + NAME + parameters + ":" + suite;
      // parameters: '(' [varargslist] ')'
      parameters.Rule = "(" + varargslist.Q() + ")";


      /* varargslist: ((fpdef ['=' test] ',')*
              ('*' NAME [',' '**' NAME] | '**' NAME) |
                fpdef ['=' test] (',' fpdef ['=' test])* [','])  */
      fpdef_ext.Rule = fpdef + WithQ("=" + test);
/*      varargslist.Expression = WithStar(fpdef_ext + comma) +
                    WithQ("*" + NAME + WithQ (comma + "**" + NAME) | "**" + NAME) |
                    fpdef_ext.Plus(comma) + commaQ; */  // ambiguous
      varargslist.Rule = vararg.Plus(comma) + commaQ;
      vararg.Rule = fpdef_ext | "*" + NAME | "**" + NAME; // added this to grammar
      // fpdef: NAME | '(' fplist ')'
      fpdef.Rule = NAME | "(" + fplist + ")";
      //fplist: fpdef (',' fpdef)* [',']
      fplist.Rule = fpdef.Plus(comma) + commaQ;

      //stmt: simple_stmt | compound_stmt
      stmt.Rule = simple_stmt | compound_stmt;
      //simple_stmt: small_stmt (';' small_stmt)* [';'] NEWLINE
      simple_stmt.Rule = small_stmt.Plus(semicolon) + semicolon.Q() + NEWLINE;
      /* small_stmt: (expr_stmt | print_stmt  | del_stmt | pass_stmt | flow_stmt |
             import_stmt | global_stmt | exec_stmt | assert_stmt)   */      
      small_stmt.Rule = expr_stmt | print_stmt  | del_stmt | pass_stmt | flow_stmt |
                   import_stmt | global_stmt | exec_stmt | assert_stmt;
      /* expr_stmt: testlist (augassign (yield_expr|testlist) |
                     ('=' (yield_expr|testlist))*)    */
      //Note!: the following is a less strict expression, it allows augassign to appear multiple times
      //  in non-first position; the after-parse analysis should catch this
      expr_stmt.Rule = testlist + WithStar( (augassign|"=") + yield_or_testlist);
      yield_or_testlist.Rule = yield_expr | testlist;
      /* augassign: ('+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '|=' | '^=' |
            '<<=' | '>>=' | '**=' | '//=') */
      augassign.Rule = Symbol("+=") | "-=" | "*=" | "/=" | "%=" | "&=" | "|=" | "^=" |
                  "<<=" | ">>=" | "**=" | "//=";
      //# For normal assignments, additional restrictions enforced by the interpreter
      /*  print_stmt: 'print' ( [ test (',' test)* [','] ] |
                      '>>' test [ (',' test)+ [','] ] )     */
      print_stmt.Rule = "print" + (Empty | testlist | ">>" + testlist);  //modified slightly using testlist
      //del_stmt: 'del' exprlist
      del_stmt.Rule = "del" + exprlist;
      //pass_stmt: 'pass'
      pass_stmt.Rule = "pass";
      //flow_stmt: break_stmt | continue_stmt | return_stmt | raise_stmt | yield_stmt
      flow_stmt.Rule = break_stmt | continue_stmt | return_stmt | raise_stmt | yield_stmt;
      //break_stmt: 'break'
      break_stmt.Rule = "break";
      // continue_stmt: 'continue'
      continue_stmt.Rule = "continue";
      // return_stmt: 'return' [testlist]
      return_stmt.Rule = "return" + testlist.Q();
      // yield_stmt: yield_expr
      yield_stmt.Rule = yield_expr;
      // raise_stmt: 'raise' [test [',' test [',' test]]]
      raise_stmt.Rule = "raise" + WithQ( test + WithQ("," + test + WithQ("," + test)));
      // import_stmt: import_name | import_from
      import_stmt.Rule = import_name | import_from;
      // import_name: 'import' dotted_as_names
      import_name.Rule = "import" + dotted_as_names;
      // import_from: ('from' ('.'* dotted_name | '.'+)
      //        'import' ('*' | '(' import_as_names ')' | import_as_names))
      // import_from.Expression = Symbol("from") + (dot.Star() + dotted_name | dot.Plus()) +   //ambiguious
      import_from.Rule = Symbol("from") + dot.Star() + (dotted_name | dot) +
                    "import" + (Symbol("*") | "(" + import_as_names + ")" | import_as_names);
      // import_as_name: NAME ['as' NAME]
      import_as_name.Rule = NAME + WithQ("as" + NAME);
      // dotted_as_name: dotted_name ['as' NAME]
      dotted_as_name.Rule = dotted_name + WithQ("as" + NAME);
      // import_as_names: import_as_name (',' import_as_name)* [',']
      import_as_names.Rule = import_as_name.Plus(comma) + commaQ;
      // dotted_as_names: dotted_as_name (',' dotted_as_name)*
      dotted_as_names.Rule = dotted_as_name.Plus(comma);
      // dotted_name: NAME ('.' NAME)*
      dotted_name.Rule = NAME.Plus(dot);
      // global_stmt: 'global' NAME (',' NAME)*
      global_stmt.Rule = "global" + NAME.Plus(comma);
      // exec_stmt: 'exec' expr ['in' test [',' test]]
      exec_stmt.Rule = "exec" + expr + WithQ("in" + test.Plus(comma));
      // assert_stmt: 'assert' test [',' test]
      assert_stmt.Rule = "assert" + test.Plus(comma);

      // compound_stmt: if_stmt | while_stmt | for_stmt | try_stmt | with_stmt | funcdef | classdef
      compound_stmt.Rule = if_stmt | while_stmt | for_stmt | try_stmt | with_stmt | funcdef | classdef;
      // if_stmt: 'if' test ':' suite ('elif' test ':' suite)* ['else' ':' suite]
      if_stmt.Rule = "if" + test + ":" + suite +
                    WithStar("elif" + test + ":" + suite) + else_clause.Q();
      else_clause.Rule = "else" + colon + suite;
      // while_stmt: 'while' test ':' suite ['else' ':' suite]
      while_stmt.Rule = "while" + test + ":" + suite + else_clause.Q();
      // for_stmt: 'for' exprlist 'in' testlist ':' suite ['else' ':' suite]
      for_stmt.Rule = "for" + exprlist + "in" + testlist + ":" + suite + else_clause.Q();
/* try_stmt: ('try' ':' suite
           ((except_clause ':' suite)+
	    ['else' ':' suite]
	    ['finally' ':' suite] |
	   'finally' ':' suite))   */
      try_stmt.Rule = "try" + colon + suite + 
            (  (except_clause + ":" + suite)+ else_clause.Q() + finally_block.Q() | finally_block   );
      finally_block.Rule = "finally" + colon + suite;
      // with_stmt: 'with' test [ with_var ] ':' suite
      with_stmt.Rule = "with" + test + with_var.Q() + ":" + suite;
      // with_var: 'as' expr
      with_var.Rule = "as" + expr;
      // NB compile.c makes sure that the default except clause is last
      // except_clause: 'except' [test [('as' | ',') test]]
      except_clause.Rule = "except" + WithQ(test + WithQ( (Symbol("as") | ",") + test));
      // suite: simple_stmt | NEWLINE INDENT stmt+ DEDENT
      suite.Rule = simple_stmt | NEWLINE  + INDENT + stmt.Plus() + DEDENT;

      //# Backward compatibility cruft to support:
      //# [ x for x in lambda: True, lambda: False if x() ]
      //# even while also allowing:
      //# lambda x: 5 if x else 2
      //# (But not a mix of the two)

      // testlist_safe: old_test [(',' old_test)+ [',']]
      testlist_safe.Rule = old_test.Plus(comma) + commaQ;
      // old_test: or_test | old_lambdef
      old_test.Rule = or_test | old_lambdef;
      // old_lambdef: 'lambda' [varargslist] ':' old_test
      old_lambdef.Rule = "lambda" + varargslist.Q() + ":" + old_test;

      // test: or_test ['if' or_test 'else' test] | lambdef
      test.Rule = or_test + WithQ("if" + or_test + "else" + test) | lambdef;
      // or_test: and_test ('or' and_test)*
      or_test.Rule = and_test + WithStar("or" + and_test);
      // and_test: not_test ('and' not_test)*
      and_test.Rule = not_test + WithStar("and" + not_test);
      // not_test: 'not' not_test | comparison
      not_test.Rule = "not" + not_test | comparison;
      // comparison: expr (comp_op expr)*
      comparison.Rule = expr + WithStar(comp_op + expr);
      // comp_op: '<'|'>'|'=='|'>='|'<='|'<>'|'!='|'in'|'not' 'in'|'is'|'is' 'not'
      comp_op.Rule = Symbol("<")|">"|"=="|">="|"<="|"<>"|"!="|"in"|
                           Symbol("not") + "in"|"is"|Symbol("is") + "not";
      // expr: xor_expr ('|' xor_expr)*
      expr.Rule = xor_expr.Plus(Symbol("|"));
      // xor_expr: and_expr ('^' and_expr)*
      xor_expr.Rule = and_expr.Plus(Symbol("^"));
      // and_expr: shift_expr ('&' shift_expr)*
      and_expr.Rule = shift_expr.Plus(Symbol("&"));
      // shift_expr: arith_expr (('<<'|'>>') arith_expr)*
      shift_expr.Rule = arith_expr.Plus(shift_op); // 
      shift_op.Rule = Symbol("<<")|">>";
      // arith_expr: term (('+'|'-') term)*
      arith_expr.Rule = term.Plus(sum_op);
      sum_op.Rule = Symbol("+") | "-";
      // term: factor (('*'|'/'|'%'|'//') factor)*
      term.Rule = factor.Plus(mul_op);
      mul_op.Rule = Symbol("*")|"/"|"%"|"//";
      // factor: ('+'|'-'|'~') factor | power
      factor.Rule = (Symbol("+")|"-"|"~") + factor | power;
      // power: atom trailer* ['**' factor]
      power.Rule = atom + trailer.Star() + WithQ("**" + factor);
      /* atom: ('(' [yield_expr|testlist_gexp] ')' |
          '[' [listmaker] ']' |
          '{' [dictmaker] '}' |
          '`' testlist1 '`' |
          NAME | NUMBER | STRING+)  */
      atom.Rule = "(" + WithQ(yield_expr|testlist_gexp) + ")" |
             "[" + listmaker.Q() + "]" |
             "{" + dictmaker.Q() + "}" |
             "`" + testlist1 + "`" |
             NAME | NUMBER | STRING; //.Plus();  //removed "+" - seems strange at least
      // listmaker: test ( list_for | (',' test)* [','] )
      //  listmaker.Expression = test + ( list_for | WithStar("," + test) + commaQ ); // ambigouous
//      listmaker.Expression = test + list_for.Q() | testlist;                             // modified version
      listmaker.Rule = test + list_for.Q() + testlist.Q() + commaQ;                             // modified version
      // testlist_gexp: test ( gen_for | (',' test)* [','] )
      //   testlist_gexp.Expression = test + ( gen_for | test.Star(comma) + commaQ ); // ambiguous
      testlist_gexp.Rule = test + gen_for | test.Plus(comma) + commaQ;          // modified version
      // lambdef: 'lambda' [varargslist] ':' test
      lambdef.Rule = "lambda" + varargslist.Q() + ":" + test;
      // trailer: '(' [arglist] ')' | '[' subscriptlist ']' | '.' NAME
      trailer.Rule = "(" + arglist.Q() + ")" | "[" + subscriptlist + "]" | "." + NAME;
      // subscriptlist: subscript (',' subscript)* [',']
      subscriptlist.Rule = subscript.Plus(comma) + commaQ;
      // subscript: '.' '.' '.' | test | [test] ':' [test] [sliceop]
      subscript.Rule = "..." | test | test.Q() + ":" + test.Q() + sliceop.Q();
      // sliceop: ':' [test]
      sliceop.Rule = ":" + test.Q();
      // exprlist: expr (',' expr)* [',']
      exprlist.Rule = expr.Plus(comma)  + commaQ;
      // testlist: test (',' test)* [',']
      testlist.Rule = test.Plus(comma) + commaQ;
      // dictmaker: test ':' test (',' test ':' test)* [',']
      dictmaker.Rule = dict_elem.Plus(comma) + commaQ;
      dict_elem.Rule = test + ":" + test;

      // classdef: 'class' NAME ['(' [testlist] ')'] ':' suite
      classdef.Rule = "class" + NAME + WithQ("(" + testlist.Q() + ")") + ":" + suite;

      // arglist: (argument ',')* (argument [',']| '*' test [',' '**' test] | '**' test)
      arglist.Rule = WithStar(argument + comma) + 
               (argument + commaQ | "*" + test + WithQ(comma + "**" + test) | "**" + test);

      // argument: test [gen_for] | test '=' test  # Really [keyword '='] test
      argument.Rule = test + gen_for.Q() | test + "=" + test;  //# Really [keyword "="] test

      // list_iter: list_for | list_if
      list_iter.Rule = list_for | list_if;
      // list_for: 'for' exprlist 'in' testlist_safe [list_iter]
      list_for.Rule = "for" + exprlist + "in" + testlist_safe + list_iter.Q();
      // list_if: 'if' old_test [list_iter]
      list_if.Rule = "if" + old_test + list_iter.Q();

      // gen_iter: gen_for | gen_if
      gen_iter.Rule = gen_for | gen_if;
      // gen_for: 'for' exprlist 'in' or_test [gen_iter]
      gen_for.Rule = "for" + exprlist + "in" + or_test + gen_iter.Q();
      // gen_if: 'if' old_test [gen_iter]
      gen_if.Rule = "if" + old_test + gen_iter.Q();

      // testlist1: test (',' test)*
      testlist1.Rule = test.Plus(comma);

      // # not used in grammar, but may appear in "node" passed from Parser to Compiler
      // encoding_decl: NAME
      encoding_decl.Rule = NAME;

      // yield_expr: 'yield' [testlist]
      yield_expr.Rule = "yield" + testlist.Q();
      #endregion

      RegisterPunctuation( "(", ")", ",", ":" );

      TokenFilters.Add(new CodeOutlineFilter(true));

    }//constructor

  }//class
}//namespace


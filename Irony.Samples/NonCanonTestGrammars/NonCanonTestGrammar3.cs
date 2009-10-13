using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Irony.Samples {
  //A test case for non-canonical parser
  // This grammar is identical to grammar #1, only with WrapTail hints added, so it can be successfully parsed by 
  // NLALR parser. Parser builder automatically transforms the grammar by creating extra wrapping non-terminals 
  // at points identified by hints, so these new non-terminals may be used as non-canonical lookaheads. 
  [Language("N-C Test #3", "", "Original with hints, NLALR-parsable")]
  public class NonCanonTestGrammar3 : Grammar {
    public NonCanonTestGrammar3() {
      this.GrammarComments = "Original version with hints for NLALR.\r\n" +
        "See file _about_noncanon_grammars.txt in source grammar's folder for information about these grammars.";
      this.ParseMethod = ParseMethod.Nlalr; 

      var Identifier = new IdentifierTerminal("Id");

      var Statement = new NonTerminal("Statement");
      var StatementList = new NonTerminal("StatementList"); 

      var FieldDef = new NonTerminal("FieldDef");
      var FieldMod = new NonTerminal("FieldMod");
      var FieldMods = new NonTerminal("FieldMods");
      var PropDef = new NonTerminal("PropDef");
      var PropMod = new NonTerminal("PropMod");
      var PropMods = new NonTerminal("PropMods");
      var MethodDef = new NonTerminal("MethodDef");
      var MethodMod = new NonTerminal("MethodMod");
      var MethodMods = new NonTerminal("MethodMods");

      StatementList.Rule = MakePlusRule(StatementList, Statement);
      Statement.Rule = FieldDef | PropDef | MethodDef;
      FieldDef.Rule = FieldMods + WrapTail() + Identifier + Identifier + ";";
      PropDef.Rule = PropMods + WrapTail() + Identifier + Identifier + "{" + "}";
      MethodDef.Rule = MethodMods + WrapTail() + Identifier + Identifier + "(" + ")" + ";";
      FieldMods.Rule = MakeStarRule(FieldMods, FieldMod);
      PropMods.Rule = MakeStarRule(PropMods, PropMod);
      MethodMods.Rule = MakeStarRule(MethodMods, MethodMod);
      FieldMod.Rule = ToTerm("new") | "public" | "protected" | "internal" | "private" | "static" | "readonly" | "volatile";
      MethodMod.Rule = ToTerm("new") | "public" | "protected" | "internal" | "private" | "static" | "virtual" | "sealed" |
        "override" | "abstract" | "extern";
      PropMod.Rule = ToTerm("new") | "public" | "protected" | "internal" | "private" | "static" | "virtual" | "sealed" |
        "override";

      this.Root = StatementList;
      this.MarkTransient(Statement); 
     

    }
  }
}

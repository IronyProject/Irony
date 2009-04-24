using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.CompilerServices;

namespace Irony.Samples {
  //A test case for non-canonical parser
  // This is original grammar for a simple language mimicking c# properties, fields and methods definitions. 
  // This is straightforward grammar for this language, and parser builder finds obvious reduce-reduce conflicts
  // when it tries to analyze the grammar. This grammar cannot be parsed by NLALR parser as well, because it 
  // does not have non-terminals that can potentially be used as non-canonical lookaheads to resolve conflicts.
  // Grammar 2 has such non-terminals, so it can be parsed using NLALR. Grammar 3 is identical to this (#1) grammar,
  // only hints are added to expresssions instructing parser to create non-terminals (wrap tails) to resolve conflicts. 
  // And finally grammar #4 is a bit more complex variation of original grammar #1, with added hints, to show that 
  // this technique works in more complex cases. 
  [Language("N-C Test #1", "", "Original, non-parsable by LALR or NLALR")]
  public class NonCanonTestGrammar1 : Grammar {
    public NonCanonTestGrammar1() {
      this.GrammarComments = "Original grammar with conflicts.\r\n" +
                        "See file _about_noncanon_grammars.txt in source grammar's folder for information about these grammars.";
      this.ParseMethod = ParseMethod.Lalr; 

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
      FieldDef.Rule = FieldMods + Identifier + Identifier + ";";
      PropDef.Rule = PropMods + Identifier + Identifier + "{" + "}";
      MethodDef.Rule = MethodMods + Identifier + Identifier + "(" + ")" + ";";
      FieldMods.Rule = MakeStarRule(FieldMods, FieldMod);
      PropMods.Rule = MakeStarRule(PropMods, PropMod);
      MethodMods.Rule = MakeStarRule(MethodMods, MethodMod);
      FieldMod.Rule = Symbol("new") | "public" | "protected" | "internal" | "private" | "static" | "readonly" | "volatile";
      MethodMod.Rule = Symbol("new") | "public" | "protected" | "internal" | "private" | "static" | "virtual" | "sealed" |
        "override" | "abstract" | "extern";
      PropMod.Rule = Symbol("new") | "public" | "protected" | "internal" | "private" | "static" | "virtual" | "sealed" |
        "override";

      this.Root = StatementList;
     

    }
  }
}

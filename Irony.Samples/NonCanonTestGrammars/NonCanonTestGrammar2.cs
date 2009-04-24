using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.CompilerServices;

namespace Irony.Samples {
  //A test case for non-canonical parser
  // This grammar defines the same language as grammar #1, but structured differently, so that it can 
  // be parsed by NLALR parser. It still has conflicts when tried with LALR. This grammar restructuring 
  // for NLALR can be done automatically by parser builder if grammar author provides WrapTail() hints
  // to point to places where to introduce extra non-terminals. This is what is done in grammar #3.
  [Language("N-C Test #2", "", "Restructured, NLALR-parsable.")]
  public class NonCanonTestGrammar2 : Grammar {
    public NonCanonTestGrammar2() {
      this.GrammarComments = "Grammar adjusted by hand for NLALR.\r\n" + 
        "See file _about_noncanon_grammars.txt in source grammar's folder for information about these grammars.";
      this.ParseMethod = ParseMethod.Nlalr; 
      
      var Identifier = new IdentifierTerminal("Id");
      var Statement = new NonTerminal("Statement");
      var StatementList = new NonTerminal("StatementList"); 

      var FieldDef = new NonTerminal("FieldDef");
      var FieldMod = new NonTerminal("FieldMod");
      var FieldMods = new NonTerminal("FieldMods");
      var FieldTail = new NonTerminal("FieldTail");
      var PropDef = new NonTerminal("PropDef");
      var PropMod = new NonTerminal("PropMod");
      var PropMods = new NonTerminal("PropMods");
      var PropTail = new NonTerminal("PropTail");
      var MethodDef = new NonTerminal("MethodDef");
      var MethodMod = new NonTerminal("MethodMod");
      var MethodMods = new NonTerminal("MethodMods");
      var MethodTail = new NonTerminal("MethodTail");

      StatementList.Rule = MakePlusRule(StatementList, Statement);
      Statement.Rule = FieldDef | PropDef | MethodDef;
      FieldDef.Rule = FieldMods + FieldTail;
      FieldTail.Rule = Identifier + Identifier + ";";
      PropDef.Rule = PropMods + PropTail;
      PropTail.Rule = Identifier + Identifier + "{" + "}";
      MethodDef.Rule = MethodMods + MethodTail;
      MethodTail.Rule = Identifier + Identifier + "(" + ")" + ";";
      FieldMods.Rule = MakeStarRule(FieldMods, FieldMod);
      PropMods.Rule = MakeStarRule(PropMods, PropMod);
      MethodMods.Rule = MakeStarRule(MethodMods, MethodMod);
      FieldMod.Rule = Symbol("new") | "public" | "protected" | "internal" | "private" | "static" | "readonly" | "volatile";
      MethodMod.Rule = Symbol("new") | "public" | "protected" | "internal" | "private" | "static" | "virtual" | "sealed" |
        "override" | "abstract" | "extern";
      PropMod.Rule = Symbol("new") | "public" | "protected" | "internal" | "private" | "static" | "virtual" | "sealed" |
        "override"; // | "abstract" | "extern"; - just to make it different from MethodMod

      this.Root = StatementList;
     

    }
  }
}

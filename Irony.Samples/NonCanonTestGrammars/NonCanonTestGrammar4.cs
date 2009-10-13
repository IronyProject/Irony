using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Irony.Samples {
  //A test case for non-canonical parser
  //This grammar is a little more complex variation of grammar #1, with extra Header non-terminals 
  // covering Modifiers and the first identifier ("Type"). 
  // The purpose of this grammar is to show that tail wrapping still works in this more sophisticated case. 
  // As a result, the wrapping of this type identifer resolves original conflict, but brings new 
  // conflicts for these new tail wrapper. We then have to make another round of tail wrapping 
  // to resolve these. 
  [Language("N-C Test #4", "", "More complex case with hints, NLALR-parsable.")]
  public class NonCanonTestGrammar4 : Grammar {
    public NonCanonTestGrammar4() {
      this.GrammarComments = "More complex case with hints for NLALR.\r\n" +
        "See file _about_noncanon_grammars.txt in source grammar's folder for information about these grammars.";
      this.ParseMethod = ParseMethod.Nlalr; 

      var Identifier = new IdentifierTerminal("Id");

      var Statement = new NonTerminal("Statement");
      var StatementList = new NonTerminal("StatementList"); 

      var FieldDef = new NonTerminal("FieldDef");
      var FieldMod = new NonTerminal("FieldMod");
      var FieldMods = new NonTerminal("FieldMods");
      var FieldHeader = new NonTerminal("FieldHeader");
      var PropDef = new NonTerminal("PropDef");
      var PropMod = new NonTerminal("PropMod");
      var PropMods = new NonTerminal("PropMods");
      var PropHeader = new NonTerminal("PropHeader");
      var MethodDef = new NonTerminal("MethodDef");
      var MethodMod = new NonTerminal("MethodMod");
      var MethodMods = new NonTerminal("MethodMods");
      var MethodHeader = new NonTerminal("MethodHeader");

      StatementList.Rule = MakePlusRule(StatementList, Statement);
      Statement.Rule = FieldDef | PropDef | MethodDef;
      FieldDef.Rule = FieldHeader + WrapTail() + Identifier + ";";
      FieldHeader.Rule = FieldMods + WrapTail() + Identifier;
      PropDef.Rule = PropHeader + WrapTail() +  Identifier + "{" + "}";
      PropHeader.Rule = PropMods + WrapTail() + Identifier;
      MethodDef.Rule = MethodHeader + WrapTail() + Identifier + "(" + ")" + ";";
      MethodHeader.Rule = MethodMods + WrapTail() + Identifier;
      FieldMods.Rule = MakeStarRule(FieldMods, FieldMod);
      PropMods.Rule = MakeStarRule(PropMods, PropMod);
      MethodMods.Rule = MakeStarRule(MethodMods, MethodMod);
      FieldMod.Rule = ToTerm("new") | "public" | "protected" | "internal" | "private" | "static" | "readonly" | "volatile";
      MethodMod.Rule = ToTerm("new") | "public" | "protected" | "internal" | "private" | "static" | "virtual" | "sealed" |
        "override" | "abstract" | "extern";
      PropMod.Rule = ToTerm("new") | "public" | "protected" | "internal" | "private" | "static" | "virtual" | "sealed" |
        "override"; // | "abstract" | "extern"; - just to make it different from MethodMod

      this.Root = StatementList;
      this.MarkTransient(Statement); 
     

    }
  }
}

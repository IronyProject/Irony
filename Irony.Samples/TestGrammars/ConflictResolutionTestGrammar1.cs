using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;

namespace Irony.Samples {

  [Language("Grammar1 with conflicts", "1.1", "First sample grammar with conflicts resolution")]
  public class ConflictResolutionTestGrammar1 : Grammar {
    public ConflictResolutionTestGrammar1() : base(true) {
      var name = new IdentifierTerminal("id");

      var definition = new NonTerminal("definition");
      var fieldDef = new NonTerminal("fieldDef");
      var propDef = new NonTerminal("propDef");
      var fieldModifier = new NonTerminal("fieldModifier");
      var propModifier = new NonTerminal("propModifier");

      definition.Rule = fieldDef | propDef;
      fieldDef.Rule = fieldModifier + name + name + ";";
      propDef.Rule = propModifier + name + name + "{" + "}";
      fieldModifier.Rule = ToTerm("public") + ReduceIf(";").ComesBefore("{") | "private" + ReduceIf(";").ComesBefore("{") | "readonly";
      propModifier.Rule = ToTerm("public") | "private" | "override";

      Root = definition;
    } 
  }
}

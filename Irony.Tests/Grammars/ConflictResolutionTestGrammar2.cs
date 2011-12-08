using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;

namespace Irony.Tests.Grammars {

  [Language("Grammar with conflicts #2", "1.1", "Sample grammar with conflicts resolution")]
  public class ConflictResolutionTestGrammar2 : Grammar {
    public ConflictResolutionTestGrammar2() : base(true) {
      var name = new IdentifierTerminal("id");

      var definition = new NonTerminal("definition");
      var fieldDef = new NonTerminal("fieldDef");
      var propDef = new NonTerminal("propDef");
      var fieldModifier = new NonTerminal("fieldModifier");
      var propModifier = new NonTerminal("propModifier");

      definition.Rule = fieldDef | propDef;
      fieldDef.Rule = fieldModifier + name + name + ";";
      propDef.Rule = propModifier + name + name + "{" + "}";
      fieldModifier.Rule = ToTerm("public") + ReduceIf(";", comesBefore: "{") | "private" + ReduceIf(";", comesBefore: "{") | "readonly";
      propModifier.Rule = ToTerm("public") | "private" | "override";

      Root = definition;
    } 
  }
}

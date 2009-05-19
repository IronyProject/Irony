using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.CompilerServices;

namespace Irony.Samples {

  [Language("Newick", "1.0", "Scientific format to represent trees")]
  public class NewickGrammar : Grammar {
    public NewickGrammar() {
      this.GrammarComments = "See http://en.wikipedia.org/wiki/Newick_format for more info.";

      var distance = new NumberLiteral("distance", NumberFlags.AllowSign);
      var name = new IdentifierTerminal("name", Strings.DecimalDigits, Strings.DecimalDigits + "-");

      var Tree = new NonTerminal("Tree");
      var SubTree = new NonTerminal("SubTree");
      var Leaf = new NonTerminal("Leaf");
      var Internal = new NonTerminal("Internal");
      var BranchList = new NonTerminal("BranchList");
      var Branch = new NonTerminal("Branch");
      var Length = new NonTerminal("Length");
      var Name = new NonTerminal("Name");

      Tree.Rule = SubTree + ";";
      SubTree.Rule = Leaf | Internal;
      Leaf.Rule = Empty | name;
      Internal.Rule = "(" + BranchList + ")" + Name;
      BranchList.Rule = Branch | Branch + "," + BranchList;
      Branch.Rule = SubTree + Length;
      Length.Rule = Empty | ":" + distance;
      Name.Rule = Empty | name;

      this.Root = Tree;       // Set grammar root
      RegisterPunctuation("(", ")");

    }//constructor

  }//class
}

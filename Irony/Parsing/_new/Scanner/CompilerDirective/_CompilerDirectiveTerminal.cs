using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing.New {

  public class CompilerDirectiveTerminal : Terminal {
    public string Prefix;
    public Dictionary<string, CompilerDirectiveBase> Directives = new Dictionary<string,CompilerDirectiveBase>();


    public CompilerDirectiveTerminal(string name, string prefix = "#") : base(name, TokenCategory.Directive) {
      Prefix = prefix; 
    }

    public void RegisterDirective(CompilerDirectiveBase directive) {
      var keywords = directive.GetKeywords(); 
      foreach(var keyw in keywords)
        Directives.Add(keyw, directive);
    }

    public override void Init(GrammarData grammarData) {
      base.Init(grammarData);
    }
    public override IList<string> GetFirsts() {
      return new string[] {Prefix};
    }

    public override IEnumerable<SourceSegment> Scan(ParsingContext context, TextSegment segment) {
      return null; 
    } 
  }//class

}

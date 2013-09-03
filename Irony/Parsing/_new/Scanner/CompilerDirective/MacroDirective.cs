using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing.New {

  //c-style macro
  public class MacroDirective : CompilerDirectiveBase {
    public string Keyword;

    public MacroDirective(string keyword = "define")  : base() {
      Keyword = keyword;
    } 
 
    public override IEnumerable<string> GetKeywords() {
      return new string[] {Keyword};
    }


    public override void Init() {
    }

    public override IEnumerable<SourceSegment> Scan(ParsingContext context, TextSegment segment) {
       return null;
    } 
  }//class

}

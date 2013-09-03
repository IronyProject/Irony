using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing.New {

  public enum IfSubType {
    If,
    Else,
    Elif,
    EndIf
  }

  //#if directive handler
  public class IfDirective : CompilerDirectiveBase {
    public Dictionary<string, IfSubType> KeywordMap = new Dictionary<string, IfSubType>();

    public IfDirective() {  }

    public void MapKeyword(string keyword, IfSubType subType) {
      KeywordMap.Add(keyword, subType); 
    }

    public override IEnumerable<string> GetKeywords() {
      return KeywordMap.Keys;
    }

    public override void Init() {
    }

    public override IEnumerable<SourceSegment> Scan(ParsingContext context, TextSegment segment) {
      return null;
    }
  }//class

}

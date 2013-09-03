using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {
  using Irony.Parsing.Construction;
  using Irony.Parsing.New;

  [Flags]
  public enum TerminalRestrictions {
    None = 0,
    FistLine = 1,
    FistColumn = 1 << 1,
    FistOnLine = 1 << 2,
    LastOnLine = 1 << 3, // KeyTerm only; mainly for use by '\' symbol that signals line continuation

  }

  public enum TerminalFlags {
    // use if needed; for linebreaks, when used optional stmt terminator, along with optional ";"
    //more info:  http://inimino.org/~inimino/blog/javascript_semicolons
    // see also _notes.txt file
    SkipIfNotExpected = 1 << 20,
  }

  public partial class Terminal : ITextSegmentScanner {
    
    public TerminalRestrictions Restriction = TerminalRestrictions.None; 

    public virtual IEnumerable<SourceSegment> Scan(ParsingContext context, TextSegment segment){
      var token = TryMatch(context, segment);
      if (token != null) {
        yield return token;
      }
    }

  }//class

  //make it inherit from SourceSegment
  public partial class Token : SourceSegment {
  }

  public partial class LanguageData {

  }
} //ns


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing.New {

  public abstract class CompilerDirectiveBase {
    public abstract IEnumerable<string> GetKeywords();

    public abstract IEnumerable<SourceSegment> Scan(ParsingContext context, TextSegment segment);

    public virtual void Init() {
    }


  } //class
}

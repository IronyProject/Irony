using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.CompilerServices {

  [Serializable]
  public struct SourceLocation {
    public int Position;
    public int Line;
    public int Column;
    public SourceLocation(int position, int line, int column) {
      Position = position;
      Line = line;
      Column = column;
    }
    public override string ToString() {
      return Line + ", " + Column;
    }
    public static int Compare(SourceLocation x, SourceLocation y) {
      if (x.Position < y.Position) return -1;
      if (x.Position == y.Position) return 0;
      return 1;
    }
  }//SourceLocation

  public struct SourceSpan {
    public readonly SourceLocation Start;
    public readonly int Length;
    public SourceSpan(SourceLocation start, int length) {
      Start = start;
      Length = length;
    }
    public int EndPos {
      get { return Start.Position + Length; }
    }
  }


}//namespace

#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {

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
    //Line/col are zero-based internally
    public override string ToString() {
      return "(" + Line.ToString() + ":" + Column.ToString() + ")";
    }
    //Line and Column displayed to user should be 1-based
    public string ToUiString() {
      return "(" + (Line + 1).ToString() + ":" + (Column + 1).ToString() + ")";
    }
    public static int Compare(SourceLocation x, SourceLocation y) {
      if (x.Position < y.Position) return -1;
      if (x.Position == y.Position) return 0;
      return 1;
    }
    public static SourceLocation Empty {
      get { return _empty; }
    } static SourceLocation _empty = new SourceLocation();  

  }//SourceLocation

  public struct SourceSpan {
    public readonly SourceLocation Location;
    public readonly int Length;
    public SourceSpan(SourceLocation location, int length) {
      Location = location;
      Length = length;
    }
    public int EndPosition {
      get { return Location.Position + Length; }
    }
  }


}//namespace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Runtime {
  public class Unassigned {
    private Unassigned() {}

    public override string ToString() {
      return "(unassigned)";
    }
    public static object Value = new Unassigned();
  }
}

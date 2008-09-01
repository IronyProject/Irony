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

    //This array is used for initializing parameters/local variables arrays, see EvaluationContext.CreateCallArgs method
    public static object[] ArrayOfUnassigned = CreateArray();
    public const int InitialSize = 256;

    private static object[] CreateArray() {
      ResizeArrayTo(InitialSize);
      return ArrayOfUnassigned;
    }
    public static void ResizeArrayTo(int newSize) {
      lock (typeof(Unassigned)) {
        //check if we still need to resize it - other thread may have already done the job while this thread was waiting for the lock
        if (ArrayOfUnassigned != null && newSize < ArrayOfUnassigned.Length) return; 
        object[] tmp = new object[newSize];
        for (int i = 0; i < tmp.Length; i++)
          tmp[i] = Unassigned.Value;
        ArrayOfUnassigned = tmp;
      } //lock
    }


  }
}

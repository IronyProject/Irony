using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Irony.Interpreter {

  public class ScopeBase {
    public ScopeInfo Info;
    protected object[] Values;

    public ScopeBase(ScopeInfo scopeInfo) : this(scopeInfo, null) {}
    public ScopeBase(ScopeInfo scopeInfo, object[] values) {
      Info = scopeInfo;
      Values = values; 
      if (Values == null)
        Values = new object[scopeInfo.ValuesCount];
    }

    public SlotInfo AddSlot(string name) {
      var slot = Info.AddSlot(name, SlotType.Value);
      if (slot.Index >= Values.Length)
        Resize(Values.Length + 4);
      return slot; 
    }


    public object[] GetValues() {
      return Values; 
    }

    public object GetValue(int index) {
      try {
        var tmp = Values;
        return tmp[index]; //this may throw null-reference exception, if resizing is happening at the same time
      } catch (NullReferenceException) {
        Thread.Sleep(0); // Silverlight does not have Yield
        // Thread.Yield(); // maybe SpinWait.SpinOnce?
        return GetValue(index); //repeat attempt
      }
    }//method

    public void SetValue(int index, object value) {
      try {
        var tmp = Values;
        tmp[index] = value; //this may throw null-reference exception, if resizing is happening at the same time
        //Now check that Locals is the same as tmp - if not, then resizing happened in the middle, 
        // so repeat assignment to make sure the value is in resized array.
        if (tmp != Values)
          SetValue(index, value); // do it again
      } catch (NullReferenceException) {
        Thread.Sleep(0); //For SL
        //Thread.Yield();
        SetValue(index, value); //repeat it again
      }
    }//method

    protected void Resize(int newSize) {
      object[] tmp = null;
      while (tmp == null)
        tmp = Interlocked.Exchange(ref Values, null);
      Array.Resize(ref tmp, newSize);
      Interlocked.Exchange(ref Values, tmp);
    }

    public IDictionary<string, object> AsDictionary() {
      return new ScopeValuesDictionary(this);
    }

  }//class


}

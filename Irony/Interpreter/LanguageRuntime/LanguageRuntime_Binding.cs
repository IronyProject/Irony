using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Interpreter {

  public partial class LanguageRuntime {
    public virtual Binding BindVariable(ScriptThread thread, string symbol, BindingPurpose purpose) {
      switch (purpose) {
        case BindingPurpose.Invoke:
        case BindingPurpose.Read:
          var currFrame = thread.CurrentScope;
          do {
            var slot = currFrame.Info.GetSlot(symbol);
            if (slot != null)
              return new Binding(slot);
            currFrame = currFrame.Parent;
          } while (currFrame != null);
          return null;
        case BindingPurpose.Write:
          var existingSlot = thread.CurrentScope.Info.GetSlot(symbol);
          if (existingSlot != null)
            return new Binding(existingSlot);
          var newSlot = thread.CurrentScope.AddSlot(symbol);
          return new Binding(newSlot);
      }//switch
      return null;
    }//method

  }//class
}

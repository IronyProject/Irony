using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Interpreter {

  //Used by LanguageRuntime.BindVariable to specify the purpose of variable binding
  public enum BindingPurpose {
    Read,
    Write,
    Invoke,
  }

  public enum BindingResult {
    Slot,
    Namespace,
    ExternalClass,
    ExternalMethod,

  }

  public class Binding {
    public BindingResult Result;
    public object Data;

    public Binding(SlotInfo slot) {
      Result = BindingResult.Slot;
      Data = slot; 
    }
  }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Interpreter {

  public delegate object ValueGetter(ScriptThread thread);
  public delegate void ValueSetter(ScriptThread thread, object value);

  public abstract class ValueAccessorBase {
    //method references
    public ValueGetter GetValue;
    public ValueSetter SetValue;
    public bool IsConstant;
  }

  public class SlotAccessorBase : ValueAccessorBase {
    public string VariableName;
    public SlotInfo Slot;
    public int SlotIndex; //same as Slot.Index
    public int ModuleIndex;
    public ScopeInfo TargetScopeInfo; //Used for searching the scope of a variable in enclosing scope.
    public SlotAccessorBase(string variableName) {
      VariableName = variableName;
    }
  }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace Irony.Interpreter {

  //Implements fast write access to a variable (local/global var or parameter) in local scope or in any enclosing scope
  public class SlotWriter : SlotAccessorBase {

    public SlotWriter(string variableName) : base(variableName) {
      this.SetValue = SetValueFirstTime; 
    }

    public void SetValueFirstTime(ScriptThread thread, object value) {
      lock (this) {
        var binding = thread.Runtime.BindVariable(thread, VariableName, BindingPurpose.Write);
        SetupMethodRef(binding.Data as SlotInfo, thread);
        SetValue(thread, value);
      }
    }

    protected virtual void SetupMethodRef(SlotInfo slot, ScriptThread thread) {
      Slot = slot;
      SlotIndex = slot.Index;
      TargetScopeInfo = slot.Scope;
      //Current scope
      if (thread.CurrentScope.Info == slot.Scope) {
        if (slot.Type == SlotType.Value)
          SetValue = SetLocal;
        else
          SetValue = SetParameter;
      } else   // Check module scope
      if (slot.Scope == thread.CurrentScope.Info.ParentModuleInfo) {
        ModuleIndex = thread.CurrentScope.Info.ParentModuleInfo.Index;
        SetValue = SetModuleVar;
      } else 
        thread.ThrowScriptError("Invalid slot for SlotWriter ({0}): target must be either local or global.", slot.Name);
    }

    //Specific method implementations
    private void SetModuleVar(ScriptThread thread, object value) {
      thread.App.ModuleScopes[ModuleIndex].SetValue(SlotIndex, value);
    }

    private void SetLocal(ScriptThread thread, object value) {
      thread.CurrentScope.SetValue(SlotIndex, value);
    }

    private void SetParameter(ScriptThread thread, object value) {
      thread.CurrentScope.SetParameter(SlotIndex, value);
    }
  }//class

}

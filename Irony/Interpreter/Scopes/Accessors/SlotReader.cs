using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace Irony.Interpreter {

  //Implements fast access to a variable (local/global var or parameter) in local scope or in any enclosing scope
  public class SlotReader : SlotAccessorBase {
    BindingPurpose _readPurpose;

    public SlotReader(string variableName) : this(variableName, false) { }

    public SlotReader(string variableName, bool forInvoke) : base(variableName) {
      base.GetValue = GetValueFirstTime;
      _readPurpose = forInvoke ? BindingPurpose.Read : BindingPurpose.Invoke;
    }

    public object GetValueFirstTime(ScriptThread thread) {
      lock (this) {
        var binding = thread.Runtime.BindVariable(thread, VariableName, _readPurpose);
        if (binding == null)
          thread.ThrowScriptError("Unknown variable [{0}].", VariableName);
        SetupMethodRef(binding.Data as SlotInfo, thread);
      }//lock
      return GetValue(thread);
    }//method

    protected virtual void SetupMethodRef(SlotInfo slot, ScriptThread thread) {
      Slot = slot;
      SlotIndex = slot.Index;
      TargetScopeInfo = slot.Scope;
      var currScope = thread.CurrentScope;
      //Check current scope
      if (currScope.Info == slot.Scope) {
        if (slot.Type == SlotType.Value)
          GetValue = GetLocal;
        else
          GetValue = GetParameter;
        return;
      }
      // Check module scope
      if (slot.Scope == currScope.Info.ParentModuleInfo) {
        ModuleIndex = currScope.Info.ParentModuleInfo.Index;
        GetValue = GetModuleVar;
        return;
      }
      //it is enclosing scope
      GetValue = GetParentScopeVariable;
    }

    //Specific method implementations
    private object GetModuleVar(ScriptThread thread) {
      return thread.App.ModuleScopes[ModuleIndex].GetValue(SlotIndex);
    }

    private object GetLocal(ScriptThread thread) {
      return thread.CurrentScope.GetValue(SlotIndex);
    }

    private object GetParameter(ScriptThread thread) {
      return thread.CurrentScope.GetParameter(SlotIndex);
    }

    private object GetParentScopeVariable(ScriptThread thread) {
      var current = thread.CurrentScope.Parent;
      do {
        if (current.Info == TargetScopeInfo)
          return Slot.Type == SlotType.Value ? current.GetValue(SlotIndex) : current.GetParameter(SlotIndex);
        current = current.Parent;
      } while (current != null);
      ThrowNotFound(thread);
      return null;
    }

    private void ThrowNotFound(ScriptThread thread) {
      thread.ThrowScriptError("Variable [{0}] not defined.", VariableName);
    }

  }//class SlotReader

}

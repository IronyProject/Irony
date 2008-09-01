using System;
using System.Collections.Generic;
using System.Text;
using Irony.Compiler;


namespace Irony.Runtime {

  public class Frame {
    public string MethodName; //for debugging purposes
    public Frame Parent; //Lexical parent - not the same as the caller
    public Frame Caller;
    public AstNode Node;
    public object[] Locals; //includes parameters and local variables

    public Frame(string methodName, AstNode node, Frame caller, Frame parent, object[] args) {
      MethodName = methodName;
      Node = node; 
      Caller = caller;
      Parent = parent;
      Locals = args; 
      //When allocating an array for parameters, we reserve extra 8 elements for locals - this should be enough in most cases
      // If not, we resize the array
      int argCount = args.Length;
      int localCount = node.Scope.Slots.Count;
      if (localCount > args.Length) {
        Array.Resize(ref Locals, localCount);
        for (int i = argCount + 1; i < localCount; i++)
          Locals[i] = Unassigned.Value;
      }
    }

    public int ScopeLevel {
      get { return Node.Scope.Level; }
    }

    public object GetValue(SlotInfo slot) {
      Frame targetFrame = GetFrame(slot.Scope.Level);
      object result = targetFrame.Locals[slot.Index];
      if (result == Unassigned.Value)
        throw new RuntimeException("Access to unassigned variable " + slot.Name);
      return result; 
    }

    public void SetValue(SlotInfo slot, object value) {
      Frame f = GetFrame(slot.Scope.Level);
      f.Locals[slot.Index] = value;
    }

    public Frame GetFrame(int scopeLevel) {
      Frame result = this;
      while (result.ScopeLevel != scopeLevel)
        result = result.Parent;
      return result;
    }//method

  }//class

}//namespace

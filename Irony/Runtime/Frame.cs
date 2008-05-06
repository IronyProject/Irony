using System;
using System.Collections.Generic;
using System.Text;
using Irony.Compiler;

namespace Irony.Runtime {

  public class ValueList : List<object> {
    public ValueList() { }
    public ValueList(int capacity) : base(capacity) { }
    public void Expand(int count) {
      while (base.Count < count)
        Add(Unassigned.Value);
    }
  }

  public class Frame {
    public Frame Parent; //Lexical parent - not the same as the caller
    public Frame Caller;
    public AstNode Node;
    public ValueList Locals;

    public Frame(AstNode node, Frame caller, Frame parent, ValueList args) {
      Node = node; 
      Caller = caller;
      Parent = parent;
      Locals = args;
      Locals.Expand(Node.Scope.Slots.Count);
    }
    public short ScopeLevel {
      get { return Node.Scope.Level; }
    }

    public object GetValue(LexicalAddress address) {
      Frame targetFrame = GetFrame(address);
      object result = targetFrame.Locals[address.SlotIndex];
      if (result == Unassigned.Value)
        throw new RuntimeException("Access to unassigned variable " + address.Name);
      return result; 
    }

    public void SetValue(LexicalAddress address, object value) {
      Frame f = GetFrame(address);
      f.Locals[address.SlotIndex] = value;
    }

    public Frame GetFrame(LexicalAddress address) {
      Frame result = this;
      while(result.ScopeLevel != address.ScopeLevel)
        result = result.Parent;
      return result; 
    }//method

  }//class

}//namespace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Runtime;

namespace Irony.Compiler {

  public class SlotInfo {
    public string Name;
    public Type Type; //for use in typed languages or for type inference
    public short Index;
    public SlotInfo(string name, Type type, short index) {
      Name = name; 
      Type = type;
      Index = index;
    }
  }

  public class SlotInfoTable : Dictionary<string, SlotInfo> {}
  public class Scope {
    public AstNode Node;
    public Scope Parent; //lexical parent, or container!
    public short Level;
    public readonly SlotInfoTable Slots = new SlotInfoTable();
    public Scope(AstNode node, Scope parent) {
      Node = node;
      Parent = parent;
      Level = (short)(parent == null ? 0 : parent.Level + 1);
    }
    public LexicalAddress GetAddress(string name) {
      SlotInfo slot;
      if (Slots.TryGetValue(name, out slot))
        return new LexicalAddress(Level, slot.Index, name);
      else if (Parent != null) 
        return Parent.GetAddress(name);
      else   
        return new LexicalAddress(-1, 0, name);
    }//method

    public SlotInfo CreateSlot(string name) {
      SlotInfo slot = new SlotInfo(name, null, (short)Slots.Count);
      Slots.Add(name, slot);
      return slot;
    }
    public SlotInfo GetSlot(short index) {
      foreach (SlotInfo slot in Slots.Values)
        if (slot.Index == index)
          return slot;
      return null;
    }
  }//class

}//namespace

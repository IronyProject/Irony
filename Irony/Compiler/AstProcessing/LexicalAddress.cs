using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Compiler {
  public struct LexicalAddress {
    public short ScopeLevel;
    public short SlotIndex;
    public string Name; //not actually used but makes debugging whole lot easier
    public LexicalAddress(short scopeLevel, short slotIndex, string name) {
      ScopeLevel = scopeLevel;
      SlotIndex = slotIndex;
      Name = name;       
    }
    public bool IsEmpty() {
      return ScopeLevel == -1;
    }
  }


}

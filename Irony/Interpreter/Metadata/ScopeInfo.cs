using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace Irony.Interpreter {

  /// <summary>Describes all variables (locals and parameters) defined in a scope of a function or module. </summary>
  /// <remarks>ScopeInfo is metadata, it does not contain variable values - Scope is a container for values.</remarks>
  // Note that all access to SlotTable is done through "lock" operator, so it's thread safe
  public class ScopeInfo {
    public readonly ModuleInfo ParentModuleInfo; 
    public readonly ScopeInfo ParentScopeInfo; //Lexical parent
    public int ValuesCount, ParametersCount;
    public AstNode Node;

    private SlotInfoDictionary _slotTable;
    private object _lockObject = new object(); 

    public ScopeInfo(ModuleInfo parentModule, AstNode node, bool caseSensitive) {
      ParentModuleInfo = parentModule; 
      Node = node;
      _slotTable = new SlotInfoDictionary(caseSensitive);
    }

    public SlotInfo AddSlot(string name, SlotType type) {
      lock (_lockObject) {
        var index = type == SlotType.Value ? ValuesCount++ : ParametersCount++;
        var slot = new SlotInfo(this, type, name, index);
        _slotTable.Add(name, slot);
        return slot;
      }
    }

    //Returns null if slot not found.
    public SlotInfo GetSlot(string name) {
      lock (_lockObject) {
        SlotInfo slot;
        _slotTable.TryGetValue(name, out slot);
        return slot;
      }
    }

    public IList<SlotInfo> GetSlots() {
      lock (_lockObject) {
        return new List<SlotInfo>(_slotTable.Values);
      }
    }

    public IList<string> GetNames() {
      lock (_lockObject) {
        return new List<string>(_slotTable.Keys);
      }
    }

    public int GetSlotCount() {
      lock (_lockObject) {
        return _slotTable.Count;
      }
    }
  }//class

}

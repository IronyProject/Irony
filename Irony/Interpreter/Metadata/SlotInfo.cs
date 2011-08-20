using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Interpreter {
  
  public enum SlotType {
    Value,     //local or property value
    Parameter, //function parameter
    Function,
    Closure,
  }

  /// <summary> Describes a variable. </summary>
  public class SlotInfo {
    public readonly ScopeInfo Scope;
    public readonly SlotType Type;
    public readonly string Name;
    public readonly int Index;
    internal SlotInfo(ScopeInfo scope, SlotType type, string name, int index) {
      Scope = scope;
      Type = type;
      Name = name;
      Index = index;
    }
  }

  public class SlotInfoDictionary : Dictionary<string, SlotInfo> {
    public SlotInfoDictionary(bool caseSensitive)
      : base(32, caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase) { }
  }

}

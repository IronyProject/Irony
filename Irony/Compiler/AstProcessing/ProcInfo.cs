using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Irony.Runtime;

namespace Irony.Compiler {
  [Flags]
  public enum ProcFlags {
    IsExternal     = 0x01,   // is interop method
    IsImported     = 0x02,   // same language but imported from another module
    IsClosure      = 0x04,   // 

    HasParamArray  = 0x100,      // The last argument is a param array
    TypeBasedDispatch   = 0x200, //uses dynamic dispatch based on types
  }

  public class ProcInfo {
    public ProcFlags Flags;
    public Type[] ParamTypes;
    public Type returnType;
    public IInvokeTarget Target;
    public MethodInfo ClrMethodInfo; //MethodInfo for .NET based method

    public bool IsSet(ProcFlags flag) {
      return (Flags & flag) != 0;
    }
  }//ProcInfo

}


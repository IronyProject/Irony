using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace Irony.Interpreter {

  public class ModuleInfoList : List<ModuleInfo> { }

  public class ModuleInfo {
    public readonly string Name;
    public readonly string FileName;
    public readonly ScopeInfo ScopeInfo; //scope for module variables
    public readonly ImportSpecList Imports = new ImportSpecList();

    public ModuleInfo(string name, string fileName, ScopeInfo scopeInfo) {
      Name = name;
      FileName = fileName;
      ScopeInfo = scopeInfo;
    }

    //Used for imported modules
    public Binding BindToExport(BindingRequest request) {
      return null; 
    }

  }
}

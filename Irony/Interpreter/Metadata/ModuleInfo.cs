using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace Irony.Interpreter {

  public class ModuleInfoList : List<ModuleInfo> {}

  public class ModuleInfo : ScopeInfo {
    public readonly string Name;
    public readonly string FileName; 
    public int Index; // the index of module's scope in EvaluationContext.ModuleScopes array
    public readonly ModuleInfoList Imports = new ModuleInfoList();

    public ModuleInfo(string name, string fileName, bool languageCaseSensitive) : base(null, null, languageCaseSensitive) {
      Name = name;
      FileName = fileName; 
    }


  }
}

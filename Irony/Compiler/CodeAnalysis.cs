using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Compiler {

  public enum CodeAnalysisPhase {
    Init,
    AssignScopes,
    Allocate,    //Allocating local variables
    Binding,     //Binding variable references to variable locations
    Optimization,
    MarkTailCalls,
  }

  public class CodeAnalysisArgs {
    public readonly CompilerContext Context;
    public CodeAnalysisPhase Phase;
    public bool SkipChildren;
    public CodeAnalysisArgs(CompilerContext context) {
      Context = context;
      Phase = CodeAnalysisPhase.Init;
    }
  }



}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Compiler {
  [Flags]
  public enum OptimizationOptions {
    
  }

  public class LanguageOptions {
    public OptimizationOptions Optimization;
    public bool TailRecursive = true; 

    public bool OptimizationOptionIsSet(OptimizationOptions option) {
      return (Optimization & option) != 0;
    }
      
  }

}//namespace

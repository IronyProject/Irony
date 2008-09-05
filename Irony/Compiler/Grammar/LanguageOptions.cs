using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Compiler {

  [Flags]
  public enum LanguageOptions {
    None = 0,
    TailRecursive = 0x01,
    AutoNewLine = 0x02,

    Default = AutoNewLine,
  }

}//namespace

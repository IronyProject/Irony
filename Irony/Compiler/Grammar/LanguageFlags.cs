using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Compiler {

  [Flags]
  public enum LanguageFlags {
    None = 0,

    TailRecursive = 0x01,
    AutoNewLine = 0x02,
    SupportsInterpreter = 0x04,
    SupportsConsole = 0x08,

    Default = AutoNewLine,
  }



}//namespace

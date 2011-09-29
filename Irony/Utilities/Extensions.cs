using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {
  public static class ParsingEnumExtensions {

    public static bool IsSet(this TermFlags enumValue, TermFlags flag) {
      return (enumValue & flag) != 0;
    }
    public static bool IsSet(this LanguageFlags enumValue, LanguageFlags flag) {
      return (enumValue & flag) != 0;
    }
  }//class

}

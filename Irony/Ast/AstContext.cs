using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing; 

namespace Irony.Ast {
  public class AstContext {
    public readonly LanguageData Language;
    public Dictionary<object, object> Values = new Dictionary<object, object>();
    public LogMessageList Messages; 

    public AstContext(LanguageData language) {
      Language = language;
    }

    public void AddMessage(ErrorLevel level, SourceLocation location, string message, params object[] args) {
      if (args != null && args.Length > 0)
        message = string.Format(message, args);
      Messages.Add(new LogMessage(level, location, message, null));
    }

  }//class
}//ns

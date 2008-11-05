using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Compiler {

  [AttributeUsage(AttributeTargets.Class)]
  public class LanguageAttribute : Attribute {
    public LanguageAttribute() : this(null) { }
    public LanguageAttribute(string languageName) : this(languageName, "1.0", string.Empty) { }

    public LanguageAttribute(string languageName, string version, string description) {
      _languageName = languageName;
      _version = version;
      _description = description;
    }
    
    public string LanguageName {
      get { return _languageName; }
    } string _languageName;

    public string Version {
      get { return _version; }
    } string _version;

    public string Description {
      get { return _description; }
    } string _description; 

  }//class
}//namespace

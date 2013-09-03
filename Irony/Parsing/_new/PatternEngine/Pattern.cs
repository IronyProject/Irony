using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irony.Parsing._new {

  public class Pattern : BnfTerm {
    public BnfExpression Rule; 

    public Pattern(string name, BnfExpression rule = null) : base(name)  {
      Rule = rule; 
    }

  }//class

  public class ListPattern : BnfTerm {
    public BnfExpression ElementRule;
    public string[] Terminators;
    public string[] Separators; 

    public ListPattern(string name, string separator = null, BnfExpression elementRule = null) : base(name) {
      if (separator != null)
        Separators = new string[] { separator };
      ElementRule = elementRule;
    }
  } //class


}//ns

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {

  public enum CompilerDirective {
    Define,
    Undefine,
    IfDefined,
    IfUndefined,
    Else,
    ElseIf,
    EndIf,
    Include,
    Warning,
    Error,
    Line,
    Pragma,
    Region,
    EndRegion,
    CustomStart = 100,
  }

  public class MacroDefinition {
    public string Symbol;
    public StringList Parameters;
    public string Definition;
    public bool IsFunctional {
      get { return Parameters.Count > 0; }
    }
  }//class

  public class MacroDefinitionTable : Dictionary<string, MacroDefinition> { }

  public class DirectiveStack : Stack<string> { }

  public class Preprocessor {
    public StringSet GlobalSymbols = new StringSet();
    public StringSet CurrentSymbols = new StringSet();
    public MacroDefinitionTable Macros = new MacroDefinitionTable();
    public DirectiveStack ActiveDirectives = new DirectiveStack();
    public virtual bool ProcessDirective(ParsingContext context, string directive, StringList args) {
      return true;
    }//method

  }//class


}//namespace

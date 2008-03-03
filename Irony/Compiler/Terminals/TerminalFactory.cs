using System;
using System.Collections.Generic;
using System.Text;

namespace Irony.Compiler {
  public static class TerminalFactory {

    public static StringLiteral CreateCSharpString(string name) {
      StringLiteral term = new StringLiteral(name, BnfFlags.None);
      term.AddStartEnd("\"", StringOptions.EnableUEscapes | StringOptions.EnableXEscapes | StringOptions.EnableOctalEscapes);
      term.AddPrefix("@", StringOptions.DisableEscape | StringOptions.AllowLineBreak | StringOptions.DoubleStartEndAsSingle);
      return term;
    }
    public static StringLiteral CreateCSharpChar(string name) {
      StringLiteral term = new StringLiteral(name, BnfFlags.None);
      term.AddStartEnd("'", StringOptions.IsChar);
      return term;
    }

    public static StringLiteral CreateVbString(string name) {
      StringLiteral term = new StringLiteral(name, BnfFlags.StringIgnoreCase);
      term.AddStartEnd("\"", StringOptions.DisableEscape | StringOptions.DoubleStartEndAsSingle);
      term.AddSuffix("$", StringOptions.None);
      term.AddSuffix("c", StringOptions.IsChar);
      return term;
    }

    public static StringLiteral CreatePythonString(string name) {
      StringLiteral term = new StringLiteral(name, BnfFlags.StringIgnoreCase);
      StringOptions allEscapes = StringOptions.EnableUEscapes | StringOptions.EnableXEscapes | StringOptions.EnableOctalEscapes;
      term.AddStartEnd("'", allEscapes);
      term.AddStartEnd("'''",allEscapes | StringOptions.AllowLineBreak );
      term.AddStartEnd("\"", allEscapes);
      term.AddStartEnd("\"\"\"", allEscapes | StringOptions.AllowLineBreak);

      term.AddPrefix("u", allEscapes);
      term.AddPrefix("r", StringOptions.DisableEscape);
      term.AddPrefix("ur", StringOptions.DisableEscape);
 
      return term;
    }

    public static NumberTerminal CreateCSharpNumber(string name) {
      NumberTerminal term = new NumberTerminal(name, BnfFlags.NumberIgnoreCase);
      term.AddPrefix("0x", 16);
      term.AddSuffix("u", TypeCode.UInt32);
      term.AddSuffix("l", TypeCode.Int64);
      term.AddSuffix("ul", TypeCode.UInt64);
      term.AddSuffix("f", TypeCode.Single);
      term.AddSuffix("m", TypeCode.Decimal);
      return term;
    }
    public static NumberTerminal CreateVbNumber(string name) {
      NumberTerminal term = new NumberTerminal(name, BnfFlags.NumberIgnoreCase);
      term.AddPrefix("&H", 16);
      term.AddPrefix("&O", 8);
      term.AddSuffix("S", TypeCode.Int16);
      term.AddSuffix("I", TypeCode.Int32);
      term.AddSuffix("%", TypeCode.Int32);
      term.AddSuffix("L", TypeCode.Int64);
      term.AddSuffix("&", TypeCode.Int64);
      term.AddSuffix("D", TypeCode.Decimal);
      term.AddSuffix("@", TypeCode.Decimal);
      term.AddSuffix("F", TypeCode.Single);
      term.AddSuffix("!", TypeCode.Single);
      term.AddSuffix("R", TypeCode.Double);
      term.AddSuffix("#", TypeCode.Double);
      term.AddSuffix("US", TypeCode.UInt16);
      term.AddSuffix("UI", TypeCode.UInt32);
      term.AddSuffix("UL", TypeCode.UInt64);
      return term;
    }
    public static NumberTerminal CreatePythonNumber(string name) {
      NumberTerminal term = new NumberTerminal(name, BnfFlags.NumberAllowBigInts | BnfFlags.NumberIgnoreCase);
      term.AddPrefix("0x", 16);
      term.AddPrefix("0", 8);
      term.AddSuffix("L", TypeCode.Int64);
      return term;
    }


  }//class
}//namespace

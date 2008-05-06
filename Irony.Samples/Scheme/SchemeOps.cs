using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Runtime;

namespace Irony.Samples.Scheme {

  public class Pair {
    public object Car;
    public object Cdr;
    public Pair(object car, object cdr) {
      Car = car;
      Cdr = cdr;
    }
  }

  public class SchemeOps : Irony.Runtime.LanguageOps {
    public override bool IsTrue(object value) {
      return value != null;
    }
    public override object NullObject {
      get { return null; }
    }

    public override MethodRefTarget GetGlobalFunction(string name) {
      MethodRef method = null;
      switch (name) {
        case "+": method = OpPlus; break;
        case "-": method = OpMinus; break;
        case "*": method = OpMul; break;
        case "/": method = OpDiv; break;
        case "<": method = OpLess; break;
        case ">": method = OpGreater; break;
        case "<=": method = OpLessOrEqual; break;
        case ">=": method = OpGreaterOrEqual; break;
        case "=": method = OpEqual; break;
        case "!=": method = OpNonEqual; break;

        case "cons": method = ConsImpl; break;
        case "car": method = CarImpl; break;
        case "cdr": method = CdrImpl; break;
        case "list": method = ListImpl; break;
        case "null?": method = NullQImpl; break;

        case "display": method = DisplayImpl; break;
        case "newline": method = NewLineImpl; break;
        default: return null;
      }
      return new MethodRefTarget(method);
    }

    private object OpPlus(ValueList args) {
      return (int)args[0] + (int)args[1];
    }
    private object OpMinus(ValueList args) {
      return (int)args[0] - (int)args[1];
    }
    private object OpMul(ValueList args) {
      return (int)args[0] * (int)args[1];
    }
    private object OpDiv(ValueList args) {
      return (int)args[0] / (int)args[1];
    }
    private object OpLess(ValueList args) {
      return BoolToObject((int)args[0] < (int)args[1]);
    }
    private object OpGreater(ValueList args) {
      return BoolToObject((int)args[0] > (int)args[1]);
    }
    private object OpLessOrEqual(ValueList args) {
      return BoolToObject((int)args[0] <= (int)args[1]);
    }
    private object OpGreaterOrEqual(ValueList args) {
      return BoolToObject((int)args[0] >= (int)args[1]);
    }
    private object OpEqual(ValueList args) {
      return BoolToObject(OpEqualHelper(args));
    }
    private object OpNonEqual(ValueList args) {
      return BoolToObject(!OpEqualHelper(args));
    }
    private bool OpEqualHelper(ValueList args) {
      object arg0 = args[0];
      object arg1 = args[1];
      if (arg0 == null || arg1 == null) return arg0 == null && arg1 == null;
      if (arg0.GetType() != arg1.GetType()) return false;
      return arg0.ToString() == arg1.ToString();
    }

    private object ConsImpl(ValueList args) {
      Check(args.Count == 2, "Invalid number of arguments - expected 2.");
      return new Pair(args[0], args[1]); 
    }
    private object CarImpl(ValueList args) {
      return GetPair(args).Car;
    }
    private object CdrImpl(ValueList args) {
      return GetPair(args).Cdr;
    }
    private Pair GetPair(ValueList args) {
      Check(args.Count == 1, "Invalid number of arguments - expected 1.");
      Check(args[0] != null, "Invalid argument - cannot be null.");
      Pair result = args[0] as Pair;
      Check(result != null, "Invalid argument type - expected a pair");
      return result;    
    }
    private object ListImpl(ValueList args) {
      Pair p = null;
      for(int i = args.Count - 1; i >= 0; i--) {
        p = new Pair(args[i], p);
      }
      return p;
    }
    private object NullQImpl(ValueList args) {
      Check(args.Count == 1, "Invalid number of arguments - expected 1.");
      return BoolToObject(args[0] == NullObject);
    }

    private object DisplayImpl(ValueList args) {
      foreach (object v in args) {
        if (v == null) continue; 
        OnConsoleWrite(v.ToString());
      }
      return null;
    }
    
    private object NewLineImpl(ValueList args) {
      OnConsoleWrite(Environment.NewLine);
      return null;
    }

    private object BoolToObject(bool value) {
      return value ? (object) 1 : null;
    }


  }//class

}

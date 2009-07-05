using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Scripting.Runtime;

namespace Irony.Samples.Scheme {

  public class Pair {
    public object Car;
    public object Cdr;
    public Pair(object car, object cdr) {
      Car = car;
      Cdr = cdr;
    }
    public override string ToString() {
      return "(" + Car + "." + Cdr + ")";
    }
  }//class Pair

  public sealed class SchemeRuntime : LanguageRuntime {
    public override bool IsTrue(object value) {
      return value != null;
    }
    public override object NullObject {
      get { return null; }
    }

    public static readonly object TrueValue = (object)1;
    public static readonly object FalseValue = null; 

    public override void Init() {
      // In Scheme, the "false" value is nil, while all other values are considered "true" when used in conditional expressions.
      // On the other hand, in Irony runtime library the comparison methods return CLR bool type by default. 
      // Irony can change the return values in these methods if we provide a custom result converter before the base.Init 
      // method is called.
      base.BoolResultConverter = BoolObjectToSchemeBool;
      base.Init();
      //In Scheme, the equality operator is "=", not "==" as in other languages. Irony runtime creates 
      // dispatcher for "==" operator. So let's put this dispatcher under the key "="
      CallDispatcher eqDisp = GetDispatcher("==");
      base.CallDispatchers.Add("=", eqDisp);
      //Register Scheme runtime library methods
      InitRuntimeLibrary();
    }

    public void InitRuntimeLibrary() {
      //create standard Scheme functions
      AddFunction("cons", 2, ConsImpl);
      AddFunction("car", 1, CarImpl);
      AddFunction("cdr", 1, CdrImpl);
      AddFunction("list", 1, ListImpl, FunctionFlags.HasParamArray | FunctionFlags.IsExternal);
      AddFunction("null?", 1, NullQImpl);
      AddFunction("display", 1, DisplayImpl, FunctionFlags.HasParamArray | FunctionFlags.IsExternal);
      AddFunction("newline", 0, NewLineImpl);
    }
    //Type converter used as result converter in comparison operations
    public object BoolObjectToSchemeBool(object value) {
      return (bool)value ? TrueValue : FalseValue;
    }

    #region Scheme Runtime library implementations
    private void ConsImpl(EvaluationContext context) {
      context.CurrentResult = new Pair(context.CallArgs[0], context.CallArgs[1]);
    }
    private void CarImpl(EvaluationContext context) {
      context.CurrentResult = GetPair(context).Car;
    }
    private void CdrImpl(EvaluationContext context) {
      context.CurrentResult = GetPair(context).Cdr;
    }
    private Pair GetPair(EvaluationContext context) {
      Pair result = context.CallArgs[0] as Pair;
      Check(result != null, "Invalid argument type - expected a pair");
      return result; 
    }
    private void ListImpl(EvaluationContext context) {
      object[] data = (object[])context.CallArgs[0];
      Pair p = null;
      foreach (object v in data) {
        p = new Pair(v, p);
      }
      context.CurrentResult = p;
    }
    private void NullQImpl(EvaluationContext context) {
      context.CurrentResult = (BoolToSchemeObject(context.CallArgs[0] == NullObject));
    }

    //almost same as BooObjectToSchemeObject, only with typed argument to avoid extra boxing
    // this typed version is used internally by other library functions.
    private object BoolToSchemeObject(bool value) {
      return value ? TrueValue : FalseValue;
    }
    private void DisplayImpl(EvaluationContext context) {
      object[] data = (object[])context.CallArgs[0];
      foreach (object v in data) {
        if (v == null) continue;
        OnConsoleWrite(v.ToString());
      }
      context.CurrentResult = null;
    }

    private void NewLineImpl(EvaluationContext context) {
      OnConsoleWrite(Environment.NewLine);
    }
    #endregion 

/*
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

*/

  }//class

}

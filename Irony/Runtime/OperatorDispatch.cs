using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Compiler;

namespace Irony.Runtime {
  using BigInteger = Microsoft.Scripting.Math.BigInteger;
  using Complex = Microsoft.Scripting.Math.Complex64;
 

  public delegate object ArgConverter(object arg);
  public delegate object OperatorMethod(object arg1, object arg2);
  public delegate object OperatorExceptionHandler(Exception exception);

  public class DispatchTable : Dictionary<TypePair, DispatchRecord> {
    public DispatchTable() : base(new TypePairComparer()) { }
  }

  public struct TypePair {
    public Type Arg1Type;
    public Type Arg2Type;
    public int HashCode;
    public TypePair(Type arg1Type, Type arg2Type) {
      Arg1Type = arg1Type;
      Arg2Type = arg2Type;
      int h1 = (arg1Type == null ? 0 : arg1Type.GetHashCode());
      int h2 = (arg2Type == null ? 0 : arg2Type.GetHashCode());
      //shift is for assymetry
      HashCode = unchecked( (h1 >> 1) + h2 ); 
    }//OpKey

    public override int GetHashCode() {
      return HashCode;
    }
    public override string ToString() {
      return "(" + Arg1Type + "," + Arg2Type + ")";
    }
  }//class

  public class TypePairComparer : IEqualityComparer<TypePair> {
    #region IEqualityComparer<OpKey> Members

    public bool Equals(TypePair x, TypePair y) {
      return x.Arg1Type == y.Arg1Type && x.Arg2Type == y.Arg2Type;
    }

    public int GetHashCode(TypePair obj) {
      return obj.HashCode;
    }

    #endregion
  }

  public sealed class DispatchRecord {
    public readonly OperatorMethod MethodRef;  //publicly available pointer to the actual method - one of ExecuteConvXXX 
    public readonly TypePair Key;  //just for easier debugging
    public readonly Type CommonType;
    public readonly OperatorMethod Implementation; //internal implementation with same-type args, provided from outside
    ArgConverter Arg1Converter;
    ArgConverter Arg2Converter;

    public DispatchRecord(TypePair key, Type commonType, ArgConverter arg1Converter, ArgConverter arg2Converter,
           OperatorMethod implementation) {
      Key = key;
      CommonType = commonType;
      Arg1Converter = arg1Converter;
      Arg2Converter = arg2Converter;
      Implementation = implementation;
      if (Arg1Converter == null && arg2Converter == null)
        MethodRef = ExecuteConvNone;
      else if (Arg1Converter != null && arg2Converter == null)
        MethodRef = ExecuteConvLeft;
      else if (Arg1Converter == null && arg2Converter != null)
        MethodRef = ExecuteConvRight;
      else // if (Arg1Converter != null && arg2Converter != null)
        MethodRef = ExecuteConvBoth;
    }

    private object ExecuteConvNone(object arg1, object arg2) {
      return Implementation(arg1, arg2);
    }
    private object ExecuteConvLeft(object arg1, object arg2) {
      return Implementation(Arg1Converter(arg1), arg2);
    }
    private object ExecuteConvRight(object arg1, object arg2) {
      return Implementation(arg1, Arg2Converter(arg2));
    }
    private object ExecuteConvBoth(object arg1, object arg2) {
      return Implementation(Arg1Converter(arg1), Arg2Converter(arg2));
    }

  }//class


  public class OperatorDispatcherTable : Dictionary<string, OperatorDispatcher> { }
  //one instance per operator; for ex., "+" operator would have its own dispatcher
  public class OperatorDispatcher : IInvokeTarget {
    public readonly string OperatorSymbol;
    public readonly OperatorDispatchManager Manager;
    public readonly DispatchTable Targets = new DispatchTable(); //table of DispatchRecord objects, indexed by type pairs

    public OperatorDispatcher(OperatorDispatchManager manager, string opSymbol) {
      OperatorSymbol = opSymbol;
      Manager = manager;
    }
    public void Invoke(EvaluationContext context, object arg1, object arg2) {
      context.CurrentResult = Dispatch(arg1, arg2);
    }
    public object Dispatch(object arg1, object arg2) {
      DispatchRecord target = null;
      try {
        Type arg1Type = (arg1 == null ? null : arg1.GetType());
        Type arg2Type = (arg2 == null ? null : arg2.GetType());
        TypePair key = new TypePair( arg1Type, arg2Type);
        if (!Targets.TryGetValue(key, out target))
          target = Manager.CreateDispatchRecord(this, key);
        object result = target.MethodRef(arg1, arg2);
        return result;
      } catch (OverflowException ex) {
        return Manager.HandleOverflow(ex, this, target, arg1, arg2);
      } catch (Exception ex) {
        return Manager.HandleException(ex, this, target, arg1, arg2);
      }
    }
    internal void AddDispatchRecord(DispatchRecord record) {
      Targets[record.Key] = record;
    }


    #region IInvokeTarget Members

    public void Invoke(EvaluationContext context, ValueList args) {
      if (args.Count > 1) 
        Invoke(context, args[0], args[1]);
      else
        Invoke(context, args[0], null);
    }

    #endregion
  }//class

  public class ConverterTable : Dictionary<TypePair, ArgConverter> {
    public void Add(Type type1, Type type2, ArgConverter converter) {
      TypePair key = new TypePair(type1, type2);
      this[key] = converter;
    }
  }
  
  public class OperatorDispatchManager {
    public readonly OperatorDispatcherTable Dispatchers = new OperatorDispatcherTable();
    public readonly List<Type> Types = new List<Type>();
    public readonly ConverterTable Converters = new ConverterTable();
    public readonly StringSet SupportedOperators = new StringSet();

    public OperatorDispatchManager() {
      //Must be in the order of checking types for common types in GetCommonType method; 
      // so double should be before single which in turn is before all int types
      Types.AddRange(new Type[] {
        typeof(string), typeof(Complex), typeof(Double), typeof(Single), typeof(Decimal), 
        typeof(BigInteger), 
        typeof(UInt64), typeof(Int64), typeof(UInt32), typeof(Int32), typeof(UInt16), typeof(Int16), typeof(byte), typeof(sbyte), typeof(bool) 
      });
      InitConvertersTable();
      InitImplementations();
    }

    public OperatorDispatcher GetDispatcher(string operatorSymbol) {
      if (!SupportedOperators.Contains(operatorSymbol))
        //TODO: decide on exact type of the exception
        throw new CompilerException("Operator " + operatorSymbol + " not supported by language runtime.");
      OperatorDispatcher dispatcher;
      if (!Dispatchers.TryGetValue(operatorSymbol, out dispatcher)) {
        dispatcher = new OperatorDispatcher(this, operatorSymbol);
        Dispatchers.Add(operatorSymbol, dispatcher);
      }
      return dispatcher;
    }

    public virtual DispatchRecord CreateDispatchRecord(OperatorDispatcher dispatcher, TypePair forKey) {
      Type commonType = GetCommonType(dispatcher.OperatorSymbol, forKey.Arg1Type, forKey.Arg2Type);
      if (commonType == null) return null;
      ArgConverter arg1Converter = GetConverter(forKey.Arg1Type, commonType);
      ArgConverter arg2Converter = GetConverter(forKey.Arg2Type, commonType);
      //Get base method for the operator and common type 
      TypePair baseKey = new TypePair(commonType, commonType);
      DispatchRecord rec;
      if (!dispatcher.Targets.TryGetValue(baseKey, out rec))
        throw new RuntimeException("Operator not defined for type " + commonType);
      OperatorMethod opMethod = rec.Implementation; 
      rec = new DispatchRecord(forKey, commonType, arg1Converter, arg2Converter, opMethod);
      dispatcher.Targets[forKey] = rec; 
      return rec; 
    }
    protected virtual bool UseDefaultImplementationFor(string opName, Type type) {
      return Types.Contains(type);
    }
    protected virtual Type GetCommonType(string opName, Type type1, Type type2) {
      // Check that both types use default operator handling - they are not custom classes with possibly overridden operators
      if (!UseDefaultImplementationFor(opName, type1) || type2 != null && !UseDefaultImplementationFor(opName, type2))  return null;
      //Find which one is first in our list
      foreach (Type t in Types)
        if (type1 == t || type2 == t) return t;
      return null;
    }

    //This is just a sketch
    //TODO: implement customizable behavior, using dictionary type->type, to specify to which  type to switch in case of overflow
    public virtual object HandleOverflow(Exception ex, OperatorDispatcher dispatcher, DispatchRecord failedTarget, object arg1, object arg2) {
      //get the common type and decide what to do...
      Type newType = null;
      switch (failedTarget.CommonType.Name) {
        case "Byte": case "SByte":
        case "Int16": case "UInt16":
        case "Int32": case "UInt32":
          newType = typeof(Int64);
          break;
        case "Int64": case "UInt64":
          newType = typeof(BigInteger);
          break; 
        case "Single":
          newType = typeof(double);
          break;
      }
      if (newType == null) 
        throw ex;
      arg1 = Convert.ChangeType(arg1, newType);
      arg2 = Convert.ChangeType(arg2, newType);
      return dispatcher.Dispatch(arg1, arg2);
    }
    public virtual object HandleException(Exception ex, OperatorDispatcher dispatcher, DispatchRecord failedTarget, object arg1, object arg2) {
      throw ex; 
    }

    protected virtual ArgConverter GetConverter(Type fromType, Type toType) {
      if (fromType == toType) return null;
      TypePair key = new TypePair(fromType, toType);
      ArgConverter result;
      if (Converters.TryGetValue(key, out result)) return result;
      string err = string.Format("Cannot convert from %1 to %2.", fromType, toType);
      throw new RuntimeException(err);
    }

    #region Converters

    //Add most-often used converters, not necessarily all of them; the rest can be created on as-needed basis in CreateConverter
    protected virtual void InitConvertersTable() {
      //types:  typeof(string), typeof(Complex), typeof(Double), typeof(Single), typeof(Decimal), typeof(BigInteger), 
      //        typeof(UInt64), typeof(Int64), typeof(UInt32), typeof(Int32), typeof(UInt16), typeof(Int16), typeof(byte), typeof(sbyte), typeof(bool) 
      //any to string
      Type T = typeof(string);
      foreach (Type t in this.Types)
        if (t != T)
          Converters.Add(t, T, ConvertAnyToString);
      //Complex
      T = typeof(Complex);
      foreach (Type t in this.Types)
        if (t != T && t != typeof(string))
          Converters.Add(t, T, ConvertAnyToComplex);
      //->Double
      Converters.Add(typeof(sbyte), typeof(double), value => (double)(sbyte)value);
      Converters.Add(typeof(byte), typeof(double), value => (double)(byte)value);
      Converters.Add(typeof(Int16), typeof(double), value => (double)(Int16)value);
      Converters.Add(typeof(UInt16), typeof(double), value => (double)(UInt16)value);
      Converters.Add(typeof(Int32), typeof(double), value => (double)(Int32)value); 
      Converters.Add(typeof(UInt32), typeof(double), value => (double)(UInt32)value);
      Converters.Add(typeof(Int64), typeof(double), value => (double)(Int64)value);
      Converters.Add(typeof(UInt64), typeof(double), value => (double)(UInt64)value);
      Converters.Add(typeof(Single), typeof(double), value => (double)(Single)value);
      Converters.Add(typeof(BigInteger), typeof(double), value => ((BigInteger)value).ToDouble(null));
      //->Single
      Converters.Add(typeof(sbyte), typeof(Single), value => (Single)(sbyte)value);
      Converters.Add(typeof(byte), typeof(Single), value => (Single)(byte)value);
      Converters.Add(typeof(Int16), typeof(Single), value => (Single)(Int16)value);
      Converters.Add(typeof(UInt16), typeof(Single), value => (Single)(UInt16)value);
      Converters.Add(typeof(Int32), typeof(Single), value => (Single)(Int32)value);
      Converters.Add(typeof(UInt32), typeof(Single), value => (Single)(UInt32)value);
      Converters.Add(typeof(Int64), typeof(Single), value => (Single)(Int64)value);
      Converters.Add(typeof(UInt64), typeof(Single), value => (Single)(UInt64)value);
      Converters.Add(typeof(BigInteger), typeof(Single), value => (Single)((BigInteger)value).ToDouble(null));
      //->BigInteger
      Converters.Add(typeof(sbyte), typeof(BigInteger), ConvertAnyIntToBigInteger);
      Converters.Add(typeof(byte), typeof(BigInteger), ConvertAnyIntToBigInteger);
      Converters.Add(typeof(Int16), typeof(BigInteger), ConvertAnyIntToBigInteger);
      Converters.Add(typeof(UInt16), typeof(BigInteger), ConvertAnyIntToBigInteger);
      Converters.Add(typeof(Int32), typeof(BigInteger), ConvertAnyIntToBigInteger);
      Converters.Add(typeof(UInt32), typeof(BigInteger), ConvertAnyIntToBigInteger);
      Converters.Add(typeof(Int64), typeof(BigInteger), ConvertAnyIntToBigInteger);
      Converters.Add(typeof(UInt64), typeof(BigInteger), ConvertAnyIntToBigInteger);
      //->UInt64
      Converters.Add(typeof(sbyte), typeof(UInt64), value => (UInt64)(sbyte)value);
      Converters.Add(typeof(byte), typeof(UInt64), value => (UInt64)(byte)value);
      Converters.Add(typeof(Int16), typeof(UInt64), value => (UInt64)(Int16)value);
      Converters.Add(typeof(UInt16), typeof(UInt64), value => (UInt64)(UInt16)value);
      Converters.Add(typeof(Int32), typeof(UInt64), value => (UInt64)(Int32)value);
      Converters.Add(typeof(UInt32), typeof(UInt64), value => (UInt64)(UInt32)value);
      Converters.Add(typeof(Int64), typeof(UInt64), value => (UInt64)(Int64)value);
      //->Int64
      Converters.Add(typeof(sbyte), typeof(Int64), value => (Int64)(sbyte)value);
      Converters.Add(typeof(byte), typeof(Int64), value => (Int64)(byte)value);
      Converters.Add(typeof(Int16), typeof(Int64), value => (Int64)(Int16)value);
      Converters.Add(typeof(UInt16), typeof(Int64), value => (Int64)(UInt16)value);
      Converters.Add(typeof(Int32), typeof(Int64), value => (Int64)(Int32)value);
      Converters.Add(typeof(UInt32), typeof(Int64), value => (Int64)(UInt32)value);
      //->UInt32
      Converters.Add(typeof(sbyte), typeof(UInt32), value => (UInt32)(sbyte)value);
      Converters.Add(typeof(byte), typeof(UInt32), value => (UInt32)(byte)value);
      Converters.Add(typeof(Int16), typeof(UInt32), value => (UInt32)(Int16)value);
      Converters.Add(typeof(UInt16), typeof(UInt32), value => (UInt32)(UInt16)value);
      Converters.Add(typeof(Int32), typeof(UInt32), value => (UInt32)(Int32)value);
      //->Int32
      Converters.Add(typeof(sbyte), typeof(Int32), value => (Int32)(sbyte)value);
      Converters.Add(typeof(byte), typeof(Int32), value => (Int32)(byte)value);
      Converters.Add(typeof(Int16), typeof(Int32), value => (Int32)(Int16)value);
      Converters.Add(typeof(UInt16), typeof(Int32), value => (Int32)(UInt16)value);
      //->UInt16
      Converters.Add(typeof(sbyte), typeof(UInt16), value => (UInt16)(sbyte)value);
      Converters.Add(typeof(byte), typeof(UInt16), value => (UInt16)(byte)value);
      Converters.Add(typeof(Int16), typeof(UInt16), value => (UInt16)(Int16)value);
      //->Int16
      Converters.Add(typeof(sbyte), typeof(Int16), value => (Int16)(sbyte)value);
      Converters.Add(typeof(byte), typeof(Int16), value => (Int16)(byte)value);
      //->byte
      Converters.Add(typeof(sbyte), typeof(byte), value => (byte)(sbyte)value);
    }

    private object ConvertAnyToString(object value) {
      return value == null ? string.Empty : value.ToString();
    }

    private object ConvertAnyToComplex(object value) {
      double d = Convert.ToDouble(value);
      return new Complex(d);
    }
    private object ConvertAnyIntToBigInteger(object value) {
      long l = Convert.ToInt64(value);
      return BigInteger.Create(l);
    }

    #endregion

    #region Implementations
    private void InitImplementations() {
      SupportedOperators.AddRange(new string[] { "+", "-", "*", "/", "&", "|", "^" });
      //note that arithmetics on byte, sbyte, int16, uint16 are performed in Int32 format, so the result is always Int32
      // we don't force the result back to original type - I don't think it's necessary
      // For each operator, we add a series of implementation methods for same-type operands. They are saved as DispatchRecords in 
      // operator dispatchers. This happens at initialization time. Dispatch records for mismatched argument types (ex: int + double)
      // are created on-the-fly at execution time. 
      OperatorDispatcher disp;

      disp = GetDispatcher("+");
      AddImplementation(disp, typeof(sbyte), (x, y) => (sbyte)x + (sbyte)y);
      AddImplementation(disp, typeof(byte),   (x, y) => (byte)x + (byte)y);  
      AddImplementation(disp, typeof(Int16),  (x, y) => (Int16)x + (Int16)y);
      AddImplementation(disp, typeof(UInt16), (x, y) => (UInt16)x + (UInt16)y);
      AddImplementation(disp, typeof(Int32),  (x, y) => checked ((Int32)x + (Int32)y));
      AddImplementation(disp, typeof(UInt32), (x, y) => checked((UInt32)x + (UInt32)y));
      AddImplementation(disp, typeof(Int64),  (x, y) => checked((Int64)x + (Int64)y));
      AddImplementation(disp, typeof(UInt64), (x, y) => checked((UInt64)x + (UInt64)y));
      AddImplementation(disp, typeof(Single), (x, y) => (Single)x + (Single)y);
      AddImplementation(disp, typeof(double), (x, y) => (double)x + (double)y);
      AddImplementation(disp, typeof(BigInteger), (x, y) => (BigInteger)x + (BigInteger)y);
      AddImplementation(disp, typeof(Complex), (x, y) => (Complex)x + (Complex)y);
      AddImplementation(disp, typeof(string), (x, y) => (string)x + (string)y);

      disp = GetDispatcher("-");
      AddImplementation(disp, typeof(sbyte), (x, y) => (sbyte)x - (sbyte)y);
      AddImplementation(disp, typeof(byte), (x, y) => (byte)x - (byte)y);
      AddImplementation(disp, typeof(Int16), (x, y) => (Int16)x - (Int16)y);
      AddImplementation(disp, typeof(UInt16), (x, y) => (UInt16)x - (UInt16)y);
      AddImplementation(disp, typeof(Int32), (x, y) => checked((Int32)x - (Int32)y));
      AddImplementation(disp, typeof(UInt32), (x, y) => checked((UInt32)x - (UInt32)y));
      AddImplementation(disp, typeof(Int64), (x, y) => checked((Int64)x - (Int64)y));
      AddImplementation(disp, typeof(UInt64), (x, y) => checked((UInt64)x - (UInt64)y));
      AddImplementation(disp, typeof(Single), (x, y) => (Single)x - (Single)y);
      AddImplementation(disp, typeof(double), (x, y) => (double)x - (double)y);
      AddImplementation(disp, typeof(BigInteger), (x, y) => (BigInteger)x - (BigInteger)y);
      AddImplementation(disp, typeof(Complex), (x, y) => (Complex)x - (Complex)y);

      disp = GetDispatcher("*");
      AddImplementation(disp, typeof(sbyte), (x, y) => (sbyte)x * (sbyte)y);
      AddImplementation(disp, typeof(byte), (x, y) => (byte)x * (byte)y);
      AddImplementation(disp, typeof(Int16), (x, y) => checked((Int16)x * (Int16)y));
      AddImplementation(disp, typeof(UInt16), (x, y) => checked((UInt16)x * (UInt16)y));
      AddImplementation(disp, typeof(Int32), (x, y) => checked((Int32)x * (Int32)y));
      AddImplementation(disp, typeof(UInt32), (x, y) => checked((UInt32)x * (UInt32)y));
      AddImplementation(disp, typeof(Int64), (x, y) => checked((Int64)x * (Int64)y));
      AddImplementation(disp, typeof(UInt64), (x, y) => checked((UInt64)x * (UInt64)y));
      AddImplementation(disp, typeof(Single), (x, y) => (Single)x * (Single)y);
      AddImplementation(disp, typeof(double), (x, y) => (double)x * (double)y);
      AddImplementation(disp, typeof(BigInteger), (x, y) => (BigInteger)x * (BigInteger)y);
      AddImplementation(disp, typeof(Complex), (x, y) => (Complex)x * (Complex)y);

      disp = GetDispatcher("/");
      AddImplementation(disp, typeof(sbyte), (x, y) => (sbyte)x / (sbyte)y);
      AddImplementation(disp, typeof(byte), (x, y) => (byte)x / (byte)y);
      AddImplementation(disp, typeof(Int16), (x, y) => checked((Int16)x / (Int16)y));
      AddImplementation(disp, typeof(UInt16), (x, y) => checked((UInt16)x / (UInt16)y));
      AddImplementation(disp, typeof(Int32), (x, y) => checked((Int32)x / (Int32)y));
      AddImplementation(disp, typeof(UInt32), (x, y) => checked((UInt32)x / (UInt32)y));
      AddImplementation(disp, typeof(Int64), (x, y) => checked((Int64)x / (Int64)y));
      AddImplementation(disp, typeof(UInt64), (x, y) => checked((UInt64)x / (UInt64)y));
      AddImplementation(disp, typeof(Single), (x, y) => (Single)x / (Single)y);
      AddImplementation(disp, typeof(double), (x, y) => (double)x / (double)y);
      AddImplementation(disp, typeof(BigInteger), (x, y) => (BigInteger)x / (BigInteger)y);
      AddImplementation(disp, typeof(Complex), (x, y) => (Complex)x / (Complex)y);

      disp = GetDispatcher("&");
      AddImplementation(disp, typeof(bool), (x, y) => (bool)x & (bool)y);
      AddImplementation(disp, typeof(sbyte), (x, y) => (sbyte)x & (sbyte)y);
      AddImplementation(disp, typeof(byte), (x, y) => (byte)x & (byte)y);
      AddImplementation(disp, typeof(Int16), (x, y) => (Int16)x & (Int16)y);
      AddImplementation(disp, typeof(UInt16), (x, y) => (UInt16)x & (UInt16)y);
      AddImplementation(disp, typeof(Int32), (x, y) => (Int32)x & (Int32)y);
      AddImplementation(disp, typeof(UInt32), (x, y) => (UInt32)x & (UInt32)y);
      AddImplementation(disp, typeof(Int64), (x, y) => (Int64)x & (Int64)y);
      AddImplementation(disp, typeof(UInt64), (x, y) => (UInt64)x & (UInt64)y);

      disp = GetDispatcher("|");
      AddImplementation(disp, typeof(bool), (x, y) => (bool)x | (bool)y);
      AddImplementation(disp, typeof(sbyte), (x, y) => (sbyte)x | (sbyte)y);
      AddImplementation(disp, typeof(byte), (x, y) => (byte)x | (byte)y);
      AddImplementation(disp, typeof(Int16), (x, y) => (Int16)x | (Int16)y);
      AddImplementation(disp, typeof(UInt16), (x, y) => (UInt16)x | (UInt16)y);
      AddImplementation(disp, typeof(Int32), (x, y) => (Int32)x | (Int32)y);
      AddImplementation(disp, typeof(UInt32), (x, y) => (UInt32)x | (UInt32)y);
      AddImplementation(disp, typeof(Int64), (x, y) => (Int64)x | (Int64)y);
      AddImplementation(disp, typeof(UInt64), (x, y) => (UInt64)x | (UInt64)y);

      disp = GetDispatcher("^"); //XOR
      AddImplementation(disp, typeof(bool), (x, y) => (bool)x ^ (bool)y);
      AddImplementation(disp, typeof(sbyte), (x, y) => (sbyte)x ^ (sbyte)y);
      AddImplementation(disp, typeof(byte), (x, y) => (byte)x ^ (byte)y);
      AddImplementation(disp, typeof(Int16), (x, y) => (Int16)x ^ (Int16)y);
      AddImplementation(disp, typeof(UInt16), (x, y) => (UInt16)x ^ (UInt16)y);
      AddImplementation(disp, typeof(Int32), (x, y) => (Int32)x ^ (Int32)y);
      AddImplementation(disp, typeof(UInt32), (x, y) => (UInt32)x ^ (UInt32)y);
      AddImplementation(disp, typeof(Int64), (x, y) => (Int64)x ^ (Int64)y);
      AddImplementation(disp, typeof(UInt64), (x, y) => (UInt64)x ^ (UInt64)y);

      //Note that && and || are special forms, not binary operators

    }
    //Add DispatchRecord to the dispatcher identified by operator symbol. The record is for "base" operator implementation, without arg conversions
    private void AddImplementation(OperatorDispatcher dispatcher, Type commonType, OperatorMethod method) {
      TypePair key = new TypePair(commonType, commonType);
      DispatchRecord rec = new DispatchRecord(key, commonType, null, null, method);
      dispatcher.AddDispatchRecord(rec);
    }
    #endregion

  }//class


}//namespace

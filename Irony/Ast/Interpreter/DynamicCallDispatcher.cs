#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Irony.Ast.Interpreter {

  #region DispatchKey class
  /// <summary>
  /// The struct is used as a key for the dictionary of dispatch records. Contains types of arguments for a method or operator
  /// implementation.
  /// </summary>
  public struct DispatchKey : IEquatable<DispatchKey> {
    public string OpSymbol;
    public Type Arg1Type;
    public Type Arg2Type;
    public int HashCode;
    private DispatchKey(string opSymbol, Type arg1Type, Type arg2Type) {
      OpSymbol = opSymbol;
      Arg1Type = arg1Type;
      Arg2Type = arg2Type;
      int h1 = (arg1Type == null ? 0 : arg1Type.GetHashCode());
      int h2 = (arg2Type == null ? 0 : arg2Type.GetHashCode());
      //shift is for assymetry
      HashCode = unchecked((h1 << 1) + h2 + opSymbol.GetHashCode());
    }//OpKey

    public override int GetHashCode() {
      return HashCode;
    }
    public override string ToString() {
      return "(" + OpSymbol + " " + Arg1Type + " " + Arg2Type + ")";
    }

    public static DispatchKey CreateFromTypes(string opSymbol, Type arg1Type, Type arg2Type) {
      return new DispatchKey(opSymbol, arg1Type, arg2Type);
    }
    public static DispatchKey CreateFromArgs(string opSymbol, object arg1, object arg2) {
      return new DispatchKey(opSymbol, (arg1 == null ? null : arg1.GetType()), (arg2 == null ? null : arg2.GetType()));
    }

    #region IEquatable<DispatchKey> Members
    public bool Equals(DispatchKey other) {
      return HashCode == other.HashCode && OpSymbol == other.OpSymbol && Arg1Type == other.Arg1Type && Arg2Type == other.Arg2Type;
    }
    #endregion
  }//class
  #endregion 

  #region TypeConverter
  public delegate object TypeConverter(object arg);
  public class TypeConverterTable : Dictionary<DispatchKey, TypeConverter> {
    public void Add(Type fromType, Type toType, TypeConverter converter) {
      DispatchKey key = DispatchKey.CreateFromTypes(string.Empty, fromType, toType);
      this[key] = converter;
    }
  }
  #endregion 

  #region DispatchRecord, DispatchRecordTable classes
  public delegate object BinaryCallTarget(object arg1, object arg2);

  ///<summary>
  ///The DispatchRecord class represents an implementation of an operator or method with specific argument types.
  ///</summary>
  ///<remarks>
  /// The DispatchRecord holds 4 method execution components, which are simply delegate references: 
  /// converters for both arguments, implementation method and converter for the result. 
  /// Each operator/method implementation (CallDispatch object)contains a dictionary of DispatchRecord objects,
  /// one for each arg type pairs. 
  ///</remarks>
  public sealed class DispatchRecord {
    public BinaryCallTarget Evaluate;  //A reference to the actual method - one of EvaluateConvXXX 
    public readonly DispatchKey Key;
    public readonly Type CommonType;
    internal TypeConverter Arg1Converter;
    internal TypeConverter Arg2Converter;
    internal TypeConverter ResultConverter;
    // no-conversion operator implementation. 
    // It has to be public so it can be accessible for creating records with conversion from "no-conversion" records
    public readonly BinaryCallTarget Implementation;

    public DispatchRecord(DispatchKey key, Type commonType, TypeConverter arg1Converter, TypeConverter arg2Converter,
             TypeConverter resultConverter, BinaryCallTarget implementation) {
      Key = key;
      CommonType = commonType;
      Arg1Converter = arg1Converter;
      Arg2Converter = arg2Converter;
      ResultConverter = resultConverter;
      Implementation = implementation;
      SetupEvaluationMethod();
    }

    public void SetupEvaluationMethod() {
      if (ResultConverter == null) {
        if (Arg1Converter == null && Arg2Converter == null)
          Evaluate = EvaluateConvNone;
        else if (Arg1Converter != null && Arg2Converter == null)
          Evaluate = EvaluateConvLeft;
        else if (Arg1Converter == null && Arg2Converter != null)
          Evaluate = EvaluateConvRight;
        else // if (Arg1Converter != null && arg2Converter != null)
          Evaluate = EvaluateConvBoth;
      } else {
        //with result converter
        if (Arg1Converter == null && Arg2Converter == null)
          Evaluate = EvaluateConvNoneConvResult;
        else if (Arg1Converter != null && Arg2Converter == null)
          Evaluate = EvaluateConvLeftConvResult;
        else if (Arg1Converter == null && Arg2Converter != null)
          Evaluate = EvaluateConvRightConvResult;
        else // if (Arg1Converter != null && Arg2Converter != null)
          Evaluate = EvaluateConvBothConvResult;
      }
    }

    private object EvaluateConvNone(object arg1, object arg2) {
      return Implementation(arg1, arg2);
    }
    private object EvaluateConvLeft(object arg1, object arg2) {
      return Implementation(Arg1Converter(arg1), arg2);
    }
    private object EvaluateConvRight(object arg1, object arg2) {
      return Implementation(arg1, Arg2Converter(arg2));
    }
    private object EvaluateConvBoth(object arg1, object arg2) {
      return Implementation(Arg1Converter(arg1), Arg2Converter(arg2));
    }

    private object EvaluateConvNoneConvResult(object arg1, object arg2) {
      return ResultConverter(Implementation(arg1, arg2));
    }
    private object EvaluateConvLeftConvResult(object arg1, object arg2) {
      return ResultConverter(Implementation(Arg1Converter(arg1), arg2));
    }
    private object EvaluateConvRightConvResult(object arg1, object arg2) {
      return ResultConverter(Implementation(arg1, Arg2Converter(arg2)));
    }
    private object EvaluateConvBothConvResult(object arg1, object arg2) {
      return ResultConverter(Implementation(Arg1Converter(arg1), Arg2Converter(arg2)));
    }
  }//class

  public class DispatchRecordTable : Dictionary<DispatchKey, DispatchRecord> { }
  #endregion 


  #region DynamicCallDispatcher
  /// <summary>
  /// The DynamicCallDispatcher is responsible for fast dispatching a function call to the implementation based on argument types
  /// </summary>
  public class DynamicCallDispatcher {
    public readonly LanguageRuntime Runtime;
    //A table of DispatchRecord objects, indexed by type pairs; 
    // declare it volatile  to tip compiler about multi-threaded access - not sure if it is needed since we use interlocked 
    private volatile DispatchRecordTable DispatchRecords = new DispatchRecordTable();

    public DynamicCallDispatcher(LanguageRuntime runtime) {
      this.Runtime = runtime;
    }
    public void Evaluate(EvaluationContext context, string opSymbol, object arg1, object arg2) {
      DispatchRecord record = null;
      try {
        DispatchKey key = DispatchKey.CreateFromArgs(opSymbol, arg1, arg2);
        if (!DispatchRecords.TryGetValue(key, out record))
          record = this.Runtime.CreateDispatchRecord(this, key);
        context.Result = record.Evaluate(arg1, arg2);
      } catch (OverflowException ex) {
        if (this.Runtime.HandleOverflow(ex, this, record, context))
        Evaluate(context, opSymbol, arg1, arg2); //recursively call self again, with new arg types 
      } catch (Exception ex) {
        if (!this.Runtime.HandleException(ex, this, record,context))
          throw;
      }
    }

    private void EvaluateSafe(EvaluationContext context) {
    }
    internal void Add(DispatchRecord record) {
      lock (this) {
        DispatchRecords[record.Key] = record; 
      }
    }

    internal DispatchRecord GetRecord(DispatchKey key) {
      DispatchRecord result;
      if (DispatchRecords.TryGetValue(key, out result)) return result;
      return null;
    }
    public void SetResultConverter(TypeConverter converter) {
      foreach (DispatchRecord rec in DispatchRecords.Values) {
        rec.ResultConverter = converter;
        rec.SetupEvaluationMethod();
      }
    }

  }//class
  #endregion

}//namespace

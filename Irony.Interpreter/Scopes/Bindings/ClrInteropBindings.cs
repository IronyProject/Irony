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
using System.Reflection;
using Irony.Interpreter.Ast;

namespace Irony.Interpreter {

  public class ClrNamespaceBinding : Binding {
    public string Namespace;
    public ClrNamespaceBinding(string ns) : base(BindingTarget.ClrNamespace, BindingOptions.Read) {
      Namespace = ns;
      this.IsConstant = true; 
    }
  }

  public class ClrMemberBinding : Binding, ICallTarget {
    public string Name;
    public Type Type;
    public object Instance;
    BindingFlags _invokeFlags;
    PropertyInfo _propertyInfo;
    FieldInfo _fieldInfo;

    public ClrMemberBinding(BindingOptions options, MemberInfo[] members, object instance)   : base(BindingTarget.ClrMember, options) {
      var m0 = members[0];
      Name = m0.Name;
      Type = m0.DeclaringType;
      Instance = instance;
      _invokeFlags = BindingFlags.Public;
      if (Instance == null)
        _invokeFlags |= BindingFlags.Static;
      else
        _invokeFlags |= BindingFlags.Instance;
      switch (m0.MemberType) {
        case MemberTypes.Method:
          _invokeFlags |= BindingFlags.InvokeMethod;
          this.GetValueRef = GetCallable;
          break; 
        case MemberTypes.Property:
          _propertyInfo = m0 as PropertyInfo;
          this.GetValueRef = GetPropertyValue;
          this.SetValueRef = SetPropertyValue; 
          break;
        case MemberTypes.Field:
          _fieldInfo = m0 as FieldInfo;
          this.GetValueRef = GetFieldValue;
          this.SetValueRef = SetFieldValue;
          break;
      }
      base.IsConstant = true;
    }

    private object GetPropertyValue(ScriptThread thread) {
      var result = _propertyInfo.GetValue(Instance, null);
      return result; 
    }
    private void SetPropertyValue(ScriptThread thread, object value) {
      _propertyInfo.SetValue(Instance, value, null);
    }
    private object GetFieldValue(ScriptThread thread) {
      var result = _fieldInfo.GetValue(Instance);
      return result;
    }
    private void SetFieldValue(ScriptThread thread, object value) {
      _fieldInfo.SetValue(Instance, value);
    }


    private object GetCallable(ScriptThread thread) {
      return this; 
    }

    #region ICalllable.Call implementation
    public object Call(ScriptThread thread, object[] parameters) {
      // TODO: fix this. Currently doing it slow by easy way, through reflection
      if (parameters != null && parameters.Length == 0)
        parameters = null; 
      var result = Type.InvokeMember(Name, _invokeFlags, null, Instance, parameters);
      return result;
    }
    #endregion

  }

  public class ClrTypeBinding : Binding {
    public Type Type;
    public ClrTypeBinding(Type type) : base(BindingTarget.ClrType, BindingOptions.Read) {
      Type = type;
      this.IsConstant = true; 
    }
  }



}

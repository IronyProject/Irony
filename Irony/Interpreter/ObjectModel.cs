using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Interpreter { 

  public class MethodImp {
    public StackFrame OwnerFrame; 
    public void Execute(EvaluationContext context, object target, int argCount) { }
  }
  public class MethodTable : Dictionary<string, MethodImp> { }

  //Base classes
  public class ScriptObjectBase {
    public readonly MetaObjectBase MetaObject; 
  }
  public abstract class MetaObjectBase {
    public virtual MethodImp FindMethod(EvaluationContext context, object target, string methodName, int argCount) {
      throw new NotImplementedException("FindMethod"); 
    }
    public virtual void Execute(EvaluationContext context, object target, string methodName, int argCount) {
      var method = FindMethod(context, target, methodName, argCount);
      method.Execute(context, target, argCount); 
    }
    public virtual MethodImp MethodNotFound(object target, string methodName) {
      throw new Exception(methodName + " not found");
    }
    public virtual MethodImp MethodNotCallable(object target, string methodName) {
      throw new Exception(methodName + " is not callable");
    }
  }

  public class MetaObjectTable : Dictionary<Type, MetaObjectBase> { }

  //Scripting languages
  public class ScriptObject : ScriptObjectBase {
    public ValueSet Fields = new ValueSet(); 
  }
  //Vmt for class-based object model
  public class ClassMetaObject : MetaObjectBase {
    public ClassMetaObject Base;
    public MethodTable Methods = new MethodTable(); 
    public override MethodImp FindMethod(EvaluationContext context, object target, string methodName, int argCount) {
      MethodImp method;
      var scriptObj = (ScriptObject)target;
      object methodObj;
      if (scriptObj.Fields.TryGetValue(methodName, out methodObj)) {
        method = methodObj as MethodImp;
        if (method != null) return method;
        return MethodNotCallable(target, methodName); 
      } 
      if (Methods.TryGetValue(methodName, out method))
        return method;
      if (Base != null)
        return Base.FindMethod(context, target, methodName, argCount);
      return MethodNotFound(target, methodName);
    }

  }//ClassMetaObject class

  //MetaObject for prototype-based object model
  public class ProtoMetaObject : MetaObjectBase {
    private string _protoFieldName = "__prototype"; 
    public override MethodImp FindMethod(EvaluationContext context, object target, string methodName, int argCount) {
      MethodImp method;
      var scriptObj = (ScriptObject)target;
      object methodObj;
      if (scriptObj.Fields.TryGetValue(methodName, out methodObj)) {
        method = methodObj as MethodImp;
        if (method != null) return method;
        return MethodNotCallable(target, methodName);
      }
      object protoObj;
      if (scriptObj.Fields.TryGetValue(_protoFieldName, out protoObj))
        return context.CallDispatcher.FindMethod(protoObj, methodName, argCount);
      return MethodNotFound(target, methodName);
    }
  }//ProtoVmt class

  //MetaObject for primitive objects like int, double, etc
  public class NativeMetaObject : MetaObjectBase {
  }

  //CLR object
  public class ClrObject : ScriptObjectBase {
    public object Target;
  }//class

  public class ClrMetaObject : MetaObjectBase {
  }//class
  
}//namespace

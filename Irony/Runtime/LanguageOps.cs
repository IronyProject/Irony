using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Runtime {

  public delegate object MethodRef(ValueList args);
  public class MethodRefTarget : IInvokeTarget {
    MethodRef _target;
    public MethodRefTarget(MethodRef target) {
      _target = target;
    }
    public void Invoke(EvaluationContext context, ValueList args) {
      context.CurrentResult = _target(args);
    }

  }

  public class ConsoleWriteEventArgs : EventArgs {
    public string Text;
    public ConsoleWriteEventArgs(string text) {
      Text = text;
    }
  }

  public class LanguageOps {
    public virtual bool IsTrue(object value) {
      return value != NullObject;
    }

    public virtual object NullObject {
      get { return null; }
    }

    public virtual MethodRefTarget GetGlobalFunction(string name) {
      return null;
    }

    public event EventHandler<ConsoleWriteEventArgs> ConsoleWrite;
    protected void OnConsoleWrite(string text) {
      if (ConsoleWrite != null) {
        ConsoleWriteEventArgs args = new ConsoleWriteEventArgs(text);
        ConsoleWrite(this, args);
      }
    }

    //Temporarily put it here
    public static void Check(bool condition, string message, params object[] args) {
      if (condition) return;
      if (args != null)
        message = string.Format(message, args);
      RuntimeException exc = new RuntimeException(message);
      throw exc;
    }

  }//class

}//namespace


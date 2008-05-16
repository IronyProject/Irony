using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Compiler;

namespace Irony.Runtime {

  public delegate object FunctionRef(ValueList args);
  public class FunctionRefWrapper : IInvokeTarget {
    FunctionRef _function;
    public FunctionRefWrapper(FunctionRef function) {
      _function = function;
    }
    public void Invoke(EvaluationContext context, ValueList args) {
      context.CurrentResult = _function(args);
    }

  }

  public class ConsoleWriteEventArgs : EventArgs {
    public string Text;
    public ConsoleWriteEventArgs(string text) {
      Text = text;
    }
  }

  //Note: mark the derived language-specific class as sealed - important for JIT optimizations
  // details here: http://www.codeproject.com/KB/dotnet/JITOptimizations.aspx
  public class LanguageOps {
    public virtual bool IsTrue(object value) {
      return value != NullObject;
    }

    public virtual object NullObject {
      get { return null; }
    }

    public virtual FunctionRefWrapper GetGlobalFunction(string name, AstNodeList parameters) {
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


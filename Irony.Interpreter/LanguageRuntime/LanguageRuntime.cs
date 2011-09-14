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
using System.Linq.Expressions;
using System.Text;
using System.Numerics;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace Irony.Interpreter { 

  public class ConsoleWriteEventArgs : EventArgs {
    public string Text;
    public ConsoleWriteEventArgs(string text) {
      Text = text;
    }
  }


  //Note: mark the derived language-specific class as sealed - important for JIT optimizations
  // details here: http://www.codeproject.com/KB/dotnet/JITOptimizations.aspx
  public partial class LanguageRuntime {
    public readonly LanguageData Language;
    //Converter of the result for comparison operation; converts bool value to values
    // specific for the language
    public UnaryOperatorMethod BoolResultConverter = null;
    //An unassigned reserved object for a language implementation
    public NoneClass NoneValue { get; protected set; }
    
    //Built-in entities
    public ImportSpecList BuiltIns = new ImportSpecList(); 
    
    public LanguageRuntime(LanguageData language) {
      Language = language;
      NoneValue = NoneClass.Value;
      Init();
    }

    public virtual void Init() {
      InitOperatorImplementations();
    }

    public virtual bool IsTrue(object value) {
      if (value is bool)
        return (bool)value;
      if (value is int)
        return ((int)value != 0);
      if(value == NoneValue) 
        return false;
      return value != null; 
    }

    #region ConsoleWrite event
    public event EventHandler<ConsoleWriteEventArgs> ConsoleWrite;
    protected void OnConsoleWrite(ScriptThread thread, string text) {
      if (ConsoleWrite != null) {
        ConsoleWriteEventArgs args = new ConsoleWriteEventArgs(text);
        ConsoleWrite(this, args);
      }
      thread.App.Write(text); 
    }
    #endregion


    internal protected void ThrowError(string message, params object[] args) {
      if (args != null && args.Length > 0)
        message = string.Format(message, args);
      throw new Exception(message);
    }

    internal protected void ThrowScriptError(string message, params object[] args) {
      if (args != null && args.Length > 0)
        message = string.Format(message, args);
      throw new ScriptException(message);
    }
  

  }//class

}//namespace


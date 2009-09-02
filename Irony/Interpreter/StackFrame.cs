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
using System.Text;
using Irony.Parsing;
using Irony.Ast;

namespace Irony.Interpreter { 

  public class ValueSet : Dictionary<string, object> { }

  public class StackFrame {
    public string MethodName; //for debugging purposes
    public StackFrame Parent; //Lexical parent - not the same as the caller
    public StackFrame Caller;
    public AstNode Node;
    public ValueSet Values; //global values for top frame; parameters and local variables for method frame

    public StackFrame(ValueSet globals) {
      Values = globals; 
    }

    public StackFrame(MethodImp method, StackFrame caller, StackFrame parent, DataStack args, int argCount) {
      Caller = caller;
      //Parent = parent;
    }

    public object GetValue(string name) {
      return null; 
    }

    public void SetValue(string name, object value) {
    }

    public StackFrame GetFrame(int scopeLevel) {
      StackFrame result = this;
      return result;
    }//method

  }//class

}//namespace

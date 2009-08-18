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

namespace Irony.Ast.Interpreter {
  public enum JumpType {
    None = 0,
    Break,
    Continue,
    Return,
    Goto,
    Exception,
  }

  public class EvaluationContext  {
    public LanguageRuntime Runtime;

    public StackFrame CurrentFrame;
    public object Result;

    //The following are not used yet
    public JumpType Jump = JumpType.None;
    public AstNode GotoTarget;

    public Closure Tail;

    //contains call args for a function call; it is passed to the new frame when the call is made. 
    // CallArgs are created by the caller based on number of arguments in the call.
    // Additionally, we reserve extra space for local variables so that this array can be used directly as local variables 
    // space in a new frame. 
    public object[] CallArgs; 

    public EvaluationContext(LanguageRuntime runtime, StackFrame topFrame) {
      Runtime = runtime;
      CurrentFrame = topFrame;
    }

    public void PushFrame(string methodName, AstNode node, StackFrame parent) {
      CurrentFrame = new StackFrame(methodName, node, CurrentFrame, parent, null);
    }
    public void PopFrame() {
      CurrentFrame = CurrentFrame.Caller;
    }


  }//class

}

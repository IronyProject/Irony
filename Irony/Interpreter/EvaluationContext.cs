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
using System.Threading;
using Irony.Parsing;
using Irony.Ast;

namespace Irony.Interpreter { 
  public enum JumpType {
    None = 0,
    Break,
    Continue,
    Return,
    Goto,
    Exception,
  }

  public partial class EvaluationContext  {
    public readonly int ThreadId; 
    public LanguageRuntime Runtime;
    public DataStack Data;
    public DynamicCallDispatcher CallDispatcher;
    public JumpType Jump = JumpType.None;
    public AstNode GotoTarget;
    public Closure Tail;
    public StackFrame CurrentFrame;

    public EvaluationContext(LanguageRuntime runtime) : this(runtime, null) { }
    public EvaluationContext(LanguageRuntime runtime, StackFrame topFrame) {
      Runtime = runtime;
      CallDispatcher = new DynamicCallDispatcher(this);
      ThreadId = Thread.CurrentThread.ManagedThreadId;
      CurrentFrame = topFrame;
      if (CurrentFrame == null)
        CurrentFrame = new StackFrame(new ValueSet());
      Data = new DataStack();
      Data.Init(runtime.Unassinged);//set LastPushedItem to unassigned
    }

    public object LastResult {
      get { return Data.LastPushedItem; }
    }

    public void PushFrame(string methodName, AstNode node, StackFrame parent) {
      //CurrentFrame = new StackFrame(methodName, node, CurrentFrame, parent, null);
    }
    public void PopFrame() {
      CurrentFrame = CurrentFrame.Caller;
    }



  }//class

}

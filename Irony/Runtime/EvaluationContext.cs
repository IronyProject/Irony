using System;
using System.Collections.Generic;
using System.Text;
using Irony.Compiler;

namespace Irony.Runtime {
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

    public Frame CurrentFrame;
    public object CurrentResult;
    //Two slots reserved for arguments of binary and unary operators
    public object Arg1;
    public object Arg2;

    //The following are not used yet
    public JumpType Jump = JumpType.None;
    public AstNode GotoTarget;

    public Closure Tail;

    //contains call args for a function call; it is passed to the new frame when the call is made. 
    // CallArgs are created by the caller based on number of arguments in the call.
    // Additionally, we reserve extra space for local variables so tha this array can be used directly as local variables 
    // space in a new frame. 
    public object[] CallArgs; 

    public EvaluationContext(LanguageRuntime runtime, AstNode rootNode) {
      Runtime = runtime;
      CallArgs = CreateCallArgs(rootNode.Scope.Slots.Count);
      PushFrame("root", rootNode, null);
    }
    // We use Array.Copy as a fast way to initialize local data with Unassigned value
    //  "When copying elements between arrays of the same types, array.Copy performs a single range check before the transfer 
    //    followed by a ultrafast memmove byte transfer." (from http://www.codeproject.com/KB/dotnet/arrays.aspx)
    public object[] CreateCallArgs(int argCount) {
      int count = argCount + LocalsPreallocateCount;
      if (count > Unassigned.ArrayOfUnassigned.Length)
        Unassigned.ResizeArrayTo(count);
      object[] args = new object[count];
      Array.Copy(Unassigned.ArrayOfUnassigned, args, count);
      return args; 
    }

    public void PushFrame(string methodName, AstNode node, Frame parent) {
      CurrentFrame = new Frame(methodName, node, CurrentFrame, parent, CallArgs);
    }
    public void PopFrame() {
      CurrentFrame = CurrentFrame.Caller;
    }

    //Used exclusively for debugging
    public StackTrace CallStack {
      get { return new StackTrace(this); }
    }

    public static int LocalsPreallocateCount = 8;
  }//class

}

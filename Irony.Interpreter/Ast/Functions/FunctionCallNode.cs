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
using Irony.Interpreter;
using Irony.Parsing;

namespace Irony.Interpreter.Ast {

  //A node representing function call
  public class FunctionCallNode : AstNode {
    AstNode TargetRef;
    AstNode Arguments;
    string _targetName;
     
    public override void Init(ParsingContext context, ParseTreeNode treeNode) {
      base.Init(context, treeNode);
      TargetRef = AddChild("Target", treeNode.ChildNodes[0]);
      TargetRef.UseType = NodeUseType.CallTarget;
      _targetName = treeNode.ChildNodes[0].FindTokenAndGetText(); 
      Arguments = AddChild("Args", treeNode.ChildNodes[1]);
      AsString = "Call " + _targetName;
    }

    protected override object DoEvaluate(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      var languageTailRecursive = thread.Runtime.Language.Grammar.LanguageFlags.HasFlag(LanguageFlags.TailRecursive);
      lock (this.LockObject) {
        if (languageTailRecursive) {
          var isTail = Flags.HasFlag(AstNodeFlags.IsTail);
          if (isTail)
            this.Evaluate = EvaluateTail;
          else 
           this.Evaluate = EvaluateWithTailCheck;
        } else 
          this.Evaluate = EvaluateNoTail; 
      }//lock
      //Actually evaluate
      var result = Evaluate(thread);
      thread.CurrentNode = Parent; //standard epilog
      return result;
    }

    // Evaluation for non-tail languages
    private object EvaluateNoTail(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      var target = TargetRef.Evaluate(thread);
      var iCall = target as ICallTarget;
      if (iCall == null)
        thread.ThrowScriptError(Resources.ErrVarIsNotCallable, _targetName);
      var args = (object[])Arguments.Evaluate(thread);
      object result = iCall.Call(thread, args);
      thread.CurrentNode = Parent; //standard epilog
      return result;
    }

    //Evaluation for tailed languages
    private object EvaluateTail(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      var target = TargetRef.Evaluate(thread);
      var iCall = target as ICallTarget;
      if (iCall == null)
        thread.ThrowScriptError(Resources.ErrVarIsNotCallable, _targetName);
      var args = (object[])Arguments.Evaluate(thread);
      thread.Tail = iCall;
      thread.TailArgs = args;
      thread.CurrentNode = Parent; //standard epilog
      return null;
    }

    private object EvaluateWithTailCheck(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      var target = TargetRef.Evaluate(thread);
      var iCall = target as ICallTarget;
      if (iCall == null)
        thread.ThrowScriptError(Resources.ErrVarIsNotCallable, _targetName);
      var args = (object[])Arguments.Evaluate(thread);
      object result = null;
      result = iCall.Call(thread, args);
      //Note that after invoking tail we can get another tail. 
      // So we need to keep calling tails while they are there.
      while (thread.Tail != null) {
        var tail = thread.Tail;
        var tailArgs = thread.TailArgs;
        thread.Tail = null;
        thread.TailArgs = null;
        result = tail.Call(thread, tailArgs);
      }
      thread.CurrentNode = Parent; //standard epilog
      return result; 
    }

  }//class

}//namespace

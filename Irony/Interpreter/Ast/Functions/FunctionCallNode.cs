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
    bool _isTail; 
     
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
          this.Evaluate = EvaluateWithTail;
          _isTail = Flags.HasFlag(AstNodeFlags.IsTail);
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
      object result = iCall.Call(thread, thread.CurrentScope, args);
      thread.CurrentNode = Parent; //standard epilog
      return result;
    }

    //Evaluation for tailed languages
    private object EvaluateWithTail(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      var target = TargetRef.Evaluate(thread);
      var iCall = target as ICallTarget;
      if (iCall == null)
        thread.ThrowScriptError(Resources.ErrVarIsNotCallable, _targetName);
      var args = (object[])Arguments.Evaluate(thread);
      object result = null;
      if (_isTail) {
          thread.Tail = new Closure(thread.CurrentScope, iCall, args);
      } else {
          result = iCall.Call(thread, thread.CurrentScope, args);
          if (thread.Tail != null)
            result = InvokeTail(thread);
      }
      thread.CurrentNode = Parent; //standard epilog
      return result; 
    }

    //Note that after invoking tail we can get another tail. 
    // So we need to keep calling tails while they are there.
    private object InvokeTail(ScriptThread thread) {
      object result = null; 
      while (thread.Tail != null) {
        var tail = thread.Tail;
        thread.Tail = null;
        result = tail.Method.Call(thread, tail.Scope, tail.Arguments);
      }
      return result; 
    }

  }//class

}//namespace

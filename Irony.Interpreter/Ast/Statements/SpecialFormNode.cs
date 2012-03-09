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

using Irony.Ast;
using Irony.Parsing;

namespace Irony.Interpreter.Ast {

  //A node representing special form - a statement handled by a SpecialForm method provided by Runtime.
  public class SpecialFormNode : AstNode {
    string _formName; 
    SpecialForm _specialForm;
    AstNode[] _specialFormArgs;

    public override void Init(AstContext context, ParseTreeNode treeNode) {
      base.Init(context, treeNode);
      var nodes = treeNode.GetMappedChildNodes();
      TargetRef = AddChild("Target", nodes[0]);
      TargetRef.UseType = NodeUseType.CallTarget;
      _targetName = nodes[0].FindTokenAndGetText();
      Arguments = AddChild("Args", nodes[1]);
      AsString = "Call " + _targetName;
      context.Language.
    }

    protected override object DoEvaluate(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      SetupEvaluateMethod(thread); 
      var result = Evaluate(thread);
      thread.CurrentNode = Parent; //standard epilog
      return result;
    }

    private void SetupEvaluateMethod(ScriptThread thread) {
      var languageTailRecursive = thread.Runtime.Language.Grammar.LanguageFlags.IsSet(LanguageFlags.TailRecursive);
      lock (this.LockObject) {
        _specialForm = null; 
        var bnd = thread.Bind(_formName, BindingRequestFlags.Invoke);
        if (bnd != null)
          _specialForm = bnd.GetValueRef(thread) as SpecialForm;
        IBindingSource src; 
        var target = thread.Runtime.BuiltIns.TryGetValue(_formName, out src);
        if (target is SpecialForm) {
          _specialForm = target as SpecialForm;
          _specialFormArgs = Arguments.ChildNodes.ToArray();
          this.Evaluate = EvaluateSpecialForm;
        } else {
          if (languageTailRecursive) {
            var isTail = Flags.IsSet(AstNodeFlags.IsTail);
            if (isTail)
              this.Evaluate = EvaluateTail;
            else
              this.Evaluate = EvaluateWithTailCheck;
          } else
            this.Evaluate = EvaluateNoTail;
        } 
      }//lock 
    }

    // Evaluation for special forms
    private object EvaluateSpecialForm(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      var result = _specialForm(thread, _specialFormArgs); 
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

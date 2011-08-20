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

  //A node representing function definition
  public class FunctionDefNode : AstNode, ICallTarget {
    AstNode _nameNode;
    AstNode _parameterList;
    AstNode _body;
     
    public override void Init(ParsingContext context, ParseTreeNode treeNode) {
      base.Init(context, treeNode);
      this.Flags |= AstNodeFlags.IsScope;
      //child #0 is usually a keyword like "def"
      _nameNode = AddChild("Name", treeNode.ChildNodes[1]);
      _parameterList = AddChild("Parameters", treeNode.ChildNodes[2]);
      _body = AddChild("Body", treeNode.ChildNodes[3]);
      AsString = "<Function " + _nameNode.AsString + ">";
      _body.SetIsTail(); //this will be propagated to the last statement
    }

    protected override object DoEvaluate(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      lock (LockObject) {
        DependentScopeInfo = new ScopeInfo(Module, this, thread.App.Info.LanguageCaseSensitive);
        // In the first evaluation the parameter list will add parameter's SlotInfo objects to Scope.ScopeInfo
        thread.PushScope(this.DependentScopeInfo, thread.CurrentScope, null);
        _parameterList.Evaluate(thread);
        thread.PopScope(); 
        //Now save the call target in current scope - call target is "this" object
        _nameNode.SetValue(thread, this);
        SetEvaluate(EvaluateAfter);
      }
      thread.CurrentNode = Parent; //standard epilog
      return this;
    }

    private object EvaluateAfter(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      _nameNode.SetValue(thread, this); // "this" implements ICallable - which is a target for the call
      thread.CurrentNode = Parent; //standard epilog
      return this;
    }

    #region ICallTarget Members
    public object Call(ScriptThread thread, Scope parentScope, object[] parameters) {
      var caller = thread.CurrentNode;
      thread.CurrentNode = this;  // prolog - not quite standard; we need to save/restore the caller as current node
      var newScope = thread.PushScope(DependentScopeInfo, thread.CurrentScope, parameters);
      var result = _body.Evaluate(thread);
      thread.PopScope();
      thread.CurrentNode = caller; //epilog
      return result; 
    }
    #endregion

    public override void SetIsTail() {
      //ignore this call, do not mark this node as tail, it is meaningless
    }
  }//class

}//namespace

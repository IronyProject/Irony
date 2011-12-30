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

  //A node representing function definition
  public class FunctionDefNode : AstNode {
    public AstNode NameNode;
    public AstNode Parameters;
    public AstNode Body;


    public override void Init(AstContext context, ParseTreeNode treeNode) {
      base.Init(context, treeNode);
      //child #0 is usually a keyword like "def"
      NameNode = AddChild("Name", treeNode.MappedChildNodes[1]);
      Parameters = AddChild("Parameters", treeNode.MappedChildNodes[2]);
      Body = AddChild("Body", treeNode.MappedChildNodes[3]);
      AsString = "<Function " + NameNode.AsString + ">";
      Body.SetIsTail(); //this will be propagated to the last statement
    }

    public override void Reset() {
      DependentScopeInfo = null; 
      base.Reset();
    }

    protected override object DoEvaluate(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      lock (LockObject) {
        if (DependentScopeInfo == null) {
          var langCaseSensitive = thread.App.Language.Grammar.CaseSensitive;
          DependentScopeInfo = new ScopeInfo(this, langCaseSensitive);
        }
        // In the first evaluation the parameter list will add parameter's SlotInfo objects to Scope.ScopeInfo
        thread.PushScope(DependentScopeInfo, null);
        Parameters.Evaluate(thread);
        thread.PopScope();
        //Set Evaluate method and invoke it later
        this.Evaluate = EvaluateAfter;
      }
      var result = Evaluate(thread);
      thread.CurrentNode = Parent; //standard epilog
      return result;
    }

    private object EvaluateAfter(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      var closure = new Closure(thread.CurrentScope, this);
      if (NameNode != null)
        NameNode.SetValue(thread, closure); 
      thread.CurrentNode = Parent; //standard epilog
      return closure;
    }

    public override void SetIsTail() {
      //ignore this call, do not mark this node as tail, it is meaningless
    }
  }//class

}//namespace

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
using System.Xml;
using Irony.Parsing;
using Irony.Interpreter;

namespace Irony.Interpreter.Ast {

  public class IdentifierNode : AstNode {
    public string Symbol;
    private ValueAccessorBase _reader;
    private ValueAccessorBase _writer; 

    public IdentifierNode() { }

    public override void Init(ParsingContext context, ParseTreeNode treeNode) {
      base.Init(context, treeNode);
      Symbol = treeNode.Token.ValueString;
      AsString = Symbol; 
    }

    //Executed only once, on the first call
    protected override object DoEvaluate(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      _reader = new SlotReader(Symbol);
      this.SetEvaluate(EvaluateReader);
      var result = this.Evaluate(thread);
      thread.CurrentNode = Parent; //standard epilog
      return result; 
    }

    private object EvaluateReader(ScriptThread thread) {
      thread.CurrentNode = this;  //standard prolog
      var result = _reader.GetValue(thread);
      if (result == null)
        thread.ThrowScriptError("Use of unassigned value ({0}).", Symbol);
      thread.CurrentNode = Parent;  //standard epilog
      return result;
    }

    protected internal override void SetValue(ScriptThread thread, object value) {
      thread.CurrentNode = this;  //standard prolog
      if (_writer == null)
        _writer = new SlotWriter(Symbol);
      _writer.SetValue(thread, value);
      thread.CurrentNode = Parent;  //standard epilog
    }

  }//class
}//namespace

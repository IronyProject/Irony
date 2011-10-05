using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing; 

namespace Irony.Interpreter.Ast {
  public class DotOperationNode : AstNode {
    AstNode _left;
    string _memberName;
    Binding _leftBinding;

    public override void Init(ParsingContext context, ParseTreeNode treeNode) {
      base.Init(context, treeNode);
      _left = AddChild("Target", treeNode.FirstChild);
      _memberName = treeNode.LastChild.FindTokenAndGetText();
      AsString = "." + _memberName;

    }

    protected override object DoEvaluate(ScriptThread thread) {
      var target = _left.Evaluate(thread);

      switch (_leftBinding.TargetInfo.Type) {
        case BindingTargetType.Slot:
          this.Evaluate = EvaluateSlotRef; 
          break; 
        case BindingTargetType.ClrInterop:
          break; 
      }
      return null; 
    }

    private object EvaluateSlotRef(ScriptThread thread) {
      var left = _leftBinding.GetValueRef(thread); 

    }


  }//class
}//namespace

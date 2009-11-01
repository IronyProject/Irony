using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter;

namespace Irony.Ast {
  public class LiteralValueNode : AstNode {
    public object Value; 

    public override void Init(ParsingContext context, ParseTreeNode treeNode) {
      base.Init(context, treeNode); 
      Value = treeNode.Token.Value;
      AsString = Value == null ? "null" : Value.ToString(); 
    }

    public override void EvaluateNode(EvaluationContext context, AstMode mode) {
      switch (mode) {
        case AstMode.Read: context.Data.Push(Value); break;
        //Write call might happen for expressions like "5++"; when system tries to save the incremented value 
        // in the "variable"; simply pop the value from data stack
        case AstMode.Write: context.Data.Pop(); break;  
      }
    }
  
  }//class
}

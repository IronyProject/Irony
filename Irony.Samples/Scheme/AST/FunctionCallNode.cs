using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Compiler;
using Irony.Runtime;

namespace Irony.Samples.Scheme {
  public delegate void ExternalMethod(string name, EvaluationContext context);

  public class FunctionCallNode : AstNode {
    public string Name;
    public AstNodeList Arguments;

    public LexicalAddress Address;
    //might be a global function, or local function that does not use external variables, so it does not need closure
    public IInvokeTarget FixedTarget; 

    public FunctionCallNode(AstNodeArgs args)  : base(args) {
      SetFields(args.GetContent(0), args.ChildNodes[1].ChildNodes);
    }

    public FunctionCallNode(AstNodeArgs args, string name, AstNodeList arguments) : base(args) {
      SetFields(name, arguments);
    }//constructor

    private void SetFields(string name, AstNodeList arguments) {
      Name = name; 
      Arguments = arguments;
      ReplaceChildNodes(arguments);
      foreach (AstNode arg in Arguments)
        arg.Tag = "Arg";
    }
    public override void OnAstProcessing(CompilerContext context, AstProcessingPhase phase) {
      base.OnAstProcessing(context, phase);
      switch (phase) {
        case AstProcessingPhase.Linking:
          Address = Scope.GetAddress(Name);
          if (Address.IsEmpty()) {
            FixedTarget = context.Ops.GetGlobalFunction(Name, Arguments);
            if (FixedTarget == null)
              ReportError(context, "Method not found: {0}", Name);
          }
          break;
      }//switch
    }

    private void ReportError(CompilerContext context, string message, params object[] args) {
      if (args != null && args.Length > 0)
        message = String.Format(message, args);
      message += "(" + this.Location.ToString() + ")";
      context.AddError(this.Location, message);
    }

    public override void Evaluate(EvaluationContext context) {
      try {
        //First evaluate all args
        ValueList values = new ValueList();
        foreach (AstNode arg in Arguments) {
          arg.Evaluate(context);
          values.Add(context.CurrentResult);
        }
        if (FixedTarget != null) {
          FixedTarget.Invoke(context, values);
          return;
        } 
        IInvokeTarget target = context.CurrentFrame.GetValue(Address) as IInvokeTarget;
        //Check for tail call
        if (IsSet(AstNodeFlags.IsTail)) {
          context.Tail = target;
          context.TailArgs = values;
          return;
        } else {
          context.Tail = null;
          target.Invoke(context, values);
          //check for remaining tail
          while (context.Tail != null) {
            target = context.Tail;
            context.Tail = null;
            target.Invoke(context, context.TailArgs);
          }
          context.TailArgs = null; //clear args
        }
      } catch (RuntimeException exc) {
        exc.Location = this.Location;
        throw;
      }
    }//method

    public override string ToString() {
      string result =string.IsNullOrEmpty(Tag)? string.Empty : Tag + ": ";
      result += "call " + Name;
      return result; 
    }

  }//class
}

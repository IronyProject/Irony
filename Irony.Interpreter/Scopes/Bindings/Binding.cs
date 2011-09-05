using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Irony.Interpreter.Ast;

namespace Irony.Interpreter {

  [Flags]
  public enum BindingOptions {
    Read = 0x01,
    Write = 0x02,
    Invoke = 0x04,
    ExistingOrNew = 0x10,
    NewOnly = 0x20,  // for new variable, for ex, in JavaScript "var x..." - introduces x as new variable
  }

  public class BindingRequest {
    public ScriptThread Thread;
    public AstNode FromNode;
    public ModuleInfo FromModule; 
    public BindingOptions Options;
    public string Symbol;
    public ScopeInfo FromScopeInfo;
    public bool IgnoreCase;
    public BindingRequest(ScriptThread thread, AstNode fromNode, string symbol, BindingOptions options) {
      Thread = thread;
      FromNode = fromNode;
      FromModule = thread.App.Info.GetModule(fromNode.ModuleNode);
      Symbol = symbol;
      Options = options;
      FromScopeInfo = thread.CurrentScope.Info;
      IgnoreCase = !thread.Runtime.Language.Grammar.CaseSensitive;
    }
  }

  public enum BindingTarget {
    Slot,
    ClrNamespace,
    ClrMember,
    ClrType,
  }

  // Binding is a link between a variable in the script (for ex, IdentifierNode) and a value storage  - 
  // a slot in local or module-level Scope. Binding to internal variables is supported by SlotBinding class. 
  // Alternatively a symbol can be bound to external CLR entity in imported namespace - class, function, property, etc.
  // Binding is produced by Runtime.Bind method and allows read/write operations through GetValueRef and SetValueRef methods. 
  public class Binding {
    public BindingTarget TargetType;
    public BindingOptions Options; 
    public EvaluateMethod GetValueRef;     // ref to Getter method implementation
    public ValueSetterMethod SetValueRef;  // ref to Setter method implementation
    public bool IsConstant { get; protected set; }
    public Binding(BindingTarget target, BindingOptions options) {
      TargetType = target;
      Options = options; 
    }

  }

}

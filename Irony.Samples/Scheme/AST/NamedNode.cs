using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Compiler;
using Irony.Runtime;

namespace Irony.Samples.Scheme {
  public class NamedNode : AstNode {
    public string Name; 
    public LexicalAddress Address;
    public NamedNode(AstNodeArgs args) : base(args) {
      Name = args.GetContent(0);
    }
    public NamedNode(AstNodeArgs args, AstNode idNode) : base(args) {
      Name = idNode.GetContent();
    }
    public NamedNode(AstNodeArgs args, string name) : base(args) {
      Name = name;
    }
    public override void OnAstProcessing(CompilerContext context, AstProcessingPhase phase) {
      base.OnAstProcessing(context, phase);
      switch (phase) {
        case AstProcessingPhase.Linking:
          if (Name != null)
            Address = Scope.GetAddress(Name);
          break;

      }
    } 

    public override string ToString() {
      return Name;
    }

  }//class

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Compiler {
  public enum AstProcessingPhase {
    CreatingScopes = 10,
    Allocating = 20, //Allocating local variables
    Linking = 30,    //Linking variable references to variables
    MarkTailCalls = 40,
  }


  public class AstProcessor {

    public void DoDefaultProcessing(AstNode astRoot, CompilerContext context) {
      Scope rootScope = new Scope(astRoot, null);
      astRoot.Scope = rootScope;
      RunPhases(astRoot, context, AstProcessingPhase.CreatingScopes, AstProcessingPhase.Allocating,
           AstProcessingPhase.Linking, AstProcessingPhase.MarkTailCalls);
    }
    public void RunPhases(AstNode astRoot, CompilerContext context, params AstProcessingPhase[] phases) {
      IEnumerable<AstNode> allNodes = astRoot.GetAll();
      foreach (AstProcessingPhase phase in phases) {
        foreach (AstNode node in allNodes)
          node.OnAstProcessing(context, phase);
      }//foreach phase
    }//method
  
  }

}

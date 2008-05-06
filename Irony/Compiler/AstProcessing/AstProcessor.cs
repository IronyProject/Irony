using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Compiler {
  public enum AstProcessingPhase {
    CreatingScopes = 5,
    Allocating = 10, //Allocating local variables
    Linking = 20,    //Linking variable references to variables
  }


  public class AstProcessor {
    public void AssignScopes(AstNode astRoot, CompilerContext context) {
    }

    public void ProcessAst(AstNode astRoot, CompilerContext context) {
      AssignScopes(astRoot, context);
      IEnumerable<AstNode> allNodes = astRoot.GetAll();
      Scope rootScope = new Scope(astRoot, null);
      astRoot.Scope = rootScope;
      foreach (AstNode node in allNodes)
        node.OnAstProcessing(context, AstProcessingPhase.CreatingScopes);
      foreach (AstNode node in allNodes) 
        node.OnAstProcessing(context, AstProcessingPhase.Allocating);
      foreach (AstNode node in allNodes)
        node.OnAstProcessing(context, AstProcessingPhase.Linking);
      
    }
  
  }

}

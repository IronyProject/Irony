using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Irony.Ast {

  public class AstBuilder<TAstNode> {

    public Type DefaultNodeType;
    public Type DefaultLiteralNodeType; //default node type for literals
    public Type DefaultIdentifierNodeType; //default node type for identifiers

    public AstBuilder(LanguageData language) {
    }

    public TAstNode BuildAstBottomUp(ParseTree parseTree) {
    }
  }//class

  /* OLD code from CoreParser
      //Note that we create AST objects for parse nodes only when we pop the node from the stack (when it is a child being added to to its parent). 
      // So only when we form a parent node, we run thru children in the stack top and check/create their AST nodes.
      // This is done to provide correct initialization of List nodes (created with Plus or Star operation). 
      // We create a parse tree node for a list non-terminal very early, when we encounter its first element. We push the newly created list node into
      // the stack. At this moment it is too early to create the AST node for the list. We should wait until all child nodes are parsed and accumulated
      // in the stack. Only then, when list construction is finished, we can create AST node and provide it with all list elements.  
      private void CheckCreateAstNode(ParseTreeNode parseNode) {
        try {
          //Check preconditions
          if (!_grammar.LanguageFlags.IsSet(LanguageFlags.CreateAst))
            return; 
          if (parseNode.AstNode != null || parseNode.Term.Flags.IsSet(TermFlags.IsTransient) 
              || parseNode.Term.Flags.IsSet(TermFlags.NoAstNode)) return;  
          if (Context.Status != ParserStatus.Parsing || Context.HasErrors) return; 
          //Prepare mapped child node list
          CheckCreateMappedChildNodeList(parseNode); 
          //Actually create node
          _grammar.CreateAstNode(Context, parseNode);
          if (parseNode.AstNode != null)
            parseNode.Term.OnAstNodeCreated(parseNode);
        } catch (Exception ex) {
          Context.AddParserMessage(ParserErrorLevel.Error, parseNode.Span.Location, Resources.ErrFailedCreateNode, parseNode.Term.Name, ex.Message); 
        }
      }

      private bool CheckCreateMappedChildNodeList(ParseTreeNode parseTreeNode) {
        var term = parseTreeNode.Term;
        if (term.AstPartsMap == null) return false; 
        parseTreeNode.MappedChildNodes = new ParseTreeNodeList();
        foreach (var index in term.AstPartsMap)
          parseTreeNode.MappedChildNodes.Add(parseTreeNode.ChildNodes[index]);
        return true; 
      }

  
   */

}

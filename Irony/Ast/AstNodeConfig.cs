using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Irony.Ast {

  public delegate TNode AstNodeCreatorMethod<TContext, TNode>(TContext context, ParseTreeNode parseNode, TNode[] childNodes);
  public delegate void AstNodeCreatedHandler<TContext, TNode>(TContext context, ParseTreeNode parseNode, TNode node);

  public class AstNodeConfig {
    public Type NodeType;
    // An optional map (selector, filter) of child AST nodes. This facility provides a way to adjust the "map" of child nodes in various languages to 
    // the structure of a standard AST nodes (that can be shared betweeen languages). 
    // ParseTreeNode object has two properties containing list nodes: ChildNodes and MappedChildNodes.
    //  If term.Ast.PartsMap is null, these two child node lists are identical and contain all child nodes. 
    // If AstParts is not null, then MappedChildNodes will contain child nodes identified by indexes in the map. 
    // For example, if we set  
    //           term.AstConfig.ChildMap = new int[] {1, 4, 2}; 
    // then MappedChildNodes will contain 3 child nodes, which are under indexes 1, 4, 2 in ChildNodes list.
    // The mapping is performed in CoreParser.cs, method CheckCreateMappedChildNodeList.
    public int[] ChildMap;

    public object Data; // any custom data

    // We store untyped refs for creator and createdhandler methods, but provide strongly-typed get/set methods.
    // This way we can use AstNodeConfig class in Grammar class without knowing in advance the AST node base types. 
    // At the time language creator writes the grammar, he would know the AST node types and so he can use strongly-typed
    // methods to attach his custom handlers.
    private object _creator; // a custom method for creating AST nodes
    private object _created; //a handler invoked after AST node is created. 

    public void AttachCreator<TContext, TNode>(AstNodeCreatorMethod<TContext, TNode> method) {
      _creator = method;
    }

    public void AttachNodeCreatedHandler<TContext, TNode>(AstNodeCreatedHandler<TContext, TNode> method) {
      _created = method;
    }

    public AstNodeCreatorMethod<TContext, TNode> GetCreator<TContext, TNode>() {
      if (_creator == null) return null;
      return (AstNodeCreatorMethod<TContext, TNode>) _creator;
    }
    public AstNodeCreatedHandler<TContext, TNode> GetCreatedHandler<TContext, TNode>() {
      if (_created == null) return null;
      return (AstNodeCreatedHandler<TContext, TNode>)_created;
    }
      
  }//AstNodeConfig class
}

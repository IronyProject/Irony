using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Irony.Parsing;
using System.Reflection.Emit;

namespace Irony.Ast {

  public class AstBuilder {
    public AstContext Context; 

    public AstBuilder(AstContext context) {
      Context = context; 
    }

    public virtual void BuildAst(ParseTree parseTree) {
      Context.Messages = parseTree.ParserMessages; 
      BuildAst(parseTree.Root);
    }

    public virtual void BuildAst(ParseTreeNode parseNode) {
      var term = parseNode.Term;
      if (term.Flags.IsSet(TermFlags.NoAstNode) || parseNode.AstNode != null) return; 
      //children first
      var processChildren = !parseNode.Term.Flags.IsSet(TermFlags.AstDelayChildren) && parseNode.ChildNodes.Count > 0;
      if (processChildren) {
        var mappedChildNodes = parseNode.MappedChildNodes;
        for (int i = 0; i < mappedChildNodes.Count; i++)
          BuildAst(mappedChildNodes[i]);
      }
      //create the node
      //First check the custom creator delegate
      if (term.AstNodeCreator != null) {
        term.AstNodeCreator(Context, parseNode);
        // We assume that Node creator method creates node and initializes it, so parser does not need to call 
        // IAstNodeInit.Init() method on node object. But we do call AstNodeCreated custom event on term.
        term.OnAstNodeCreated(parseNode);
        return; 
      }
      //No custom creator. We create node from AstNodeType. We use compiled delegate for this which we create on the fly.
      if (term.DefaultAstNodeCreator == null) {
        var nodeType = term.AstNodeType ?? Context.Language.Grammar.DefaultNodeType;
        term.DefaultAstNodeCreator = CompileDefaultNodeCreator(nodeType);
      }
      //Invoke the creator
      parseNode.AstNode = term.DefaultAstNodeCreator();
      //Initialize node
      var iInit = parseNode.AstNode as IAstNodeInit;
      if (iInit != null)
        iInit.Init(Context, parseNode);
      //Invoke the event on term
      term.OnAstNodeCreated(parseNode);
    }//method

    //Contributed by William Horner (wmh)
    private DefaultAstNodeCreator CompileDefaultNodeCreator(Type nodeType) {
      ConstructorInfo constr = nodeType.GetConstructor(Type.EmptyTypes);
      DynamicMethod method = new DynamicMethod("CreateAstNode", nodeType, Type.EmptyTypes);
      ILGenerator il = method.GetILGenerator();
      il.Emit(OpCodes.Newobj, constr);
      il.Emit(OpCodes.Ret);
      var result  = (DefaultAstNodeCreator) method.CreateDelegate(typeof(DefaultAstNodeCreator));
      return result; 
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

  
   */

}

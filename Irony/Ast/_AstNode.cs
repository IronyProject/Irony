#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.Xml;
using System.IO;
using Irony.Parsing;
using Irony.Ast.Interpreter;

namespace Irony.Ast {

  #region NodeArgs
  // This class is a container for information used by the NodeCreator delegate and default node constructor
  // Using this struct simplifies signatures of custom AST nodes and it allows to easily add parameters in the future
  // if such need arises without breaking the existing code. 
  public class NodeArgs {
    public readonly CompilerContext Context;
    public readonly BnfTerm Term;
    public readonly SourceSpan Span;
    public readonly AstNodeList ChildNodes;

    public NodeArgs(CompilerContext context, BnfTerm term, SourceSpan span, AstNodeList childNodes) {
      Context = context;
      Term = term;
      Span = span; 
      ChildNodes = childNodes;
    }
  }//struct
  #endregion

  [Flags]
  public enum AstNodeFlags {
    None = 0x0,
    IsTail          = 0x01,     //the node is in tail position
    //Identifiers flags
    AllocateSlot    = 0x02,     //the identifier node should allocate slot for a variable
    SuppressNotDefined = 0x04,  //suppress "variable not defined" message during binding; this flag is set in Id node of function call - 
                                // to allow for external functions
    NotRValue        = 0x08,    // used in identifiers as indicators that this use is not RValue, so identifier does not mark the slot as 
    IsLValue         = 0x10,           // used in identifiers as indicators that this use is LValue assignment
    UsesOuterScope   = 0x020,   //the function uses values in outer(parent) scope(s), so it may need closure 

  }


  public class AstNodeList : List<AstNode> { }

  public class NodeAttributeDictionary : Dictionary<string, object> { }

  //Base AST node class
  public class AstNode : IAstNodeInit, IInterpretedNode {
    public AstNode() {
    }
    public AstNode(NodeArgs args) {
    }

    #region IAstNodeInit Members
    public virtual void Init(CompilerContext context, ParseTreeNode treeNode) {
      this.Term = treeNode.Term;
      Span = treeNode.Span;
      foreach (ParseTreeNode childInfo in treeNode.ChildNodes)
        AddChild(null, childInfo.AstNode as AstNode);
    }

    public void SetParent(AstNode parent) {
      Parent = parent;
    }
    #endregion

    #region properties Parent, Term, Span, Location, ChildNodes, Scope, Role, Flags, Attributes
    public AstNode Parent;
    public BnfTerm Term; 
    public SourceSpan Span; 
    public SourceLocation Location {
      get { return Span.Location; }
    }
    // Role is a free-form string used as prefix in ToString() representation of the node. 
    // Node's parent can set it to "property name" or role of the child node in parent's node context. 
    public string Role;
    //Flags
    public AstNodeFlags Flags;
    public bool IsSet(AstNodeFlags flag) {
      return (Flags & flag) != 0;
    }

    //List of child nodes
    public AstNodeList  ChildNodes = new AstNodeList();
    public void AddChild(string role, AstNode child) {
      if (child == null) return;
      child.Role = role;
      child.SetParent(this); 
      ChildNodes.Add(child);
    }

    public NodeAttributeDictionary Attributes {
      get {
        if (_attributes == null)
          _attributes = new NodeAttributeDictionary();
        return _attributes;
      }
    } NodeAttributeDictionary _attributes;

    //TODO: finish this
    public static string GetContent(AstNode node) {
      return string.Empty;
    }

    #endregion

    
    public override string ToString() {
      string result = string.Empty; 
      if (!string.IsNullOrEmpty(Role))
        result = Role + ": ";
      if (Term != null)
        result += Term.Name;
      return result;

    }

    #region Visitors, Iterators
    //the first primitive Visitor facility
    public virtual void AcceptVisitor(IAstVisitor visitor) {
      visitor.BeginVisit(this);
      if (ChildNodes.Count > 0)
        foreach(AstNode node in ChildNodes)
          node.AcceptVisitor(visitor);
      visitor.EndVisit(this);
    }

    //Node traversal 
    public IEnumerable<AstNode> GetAll() {
      AstNodeList result = new AstNodeList();
      AddAll(result);
      return result; 
    }
    private void AddAll(AstNodeList list) {
      list.Add(this);
      foreach (AstNode child in this.ChildNodes)
        if (child != null) 
          child.AddAll(list);
    }
    #endregion


    #region IInterpretedNode.Evaluate evaluation: Evaluate
    public virtual void Evaluate(EvaluationContext context) {
    }
    #endregion

    #region ChildNodes manipulations
    public virtual bool IsEmpty() {
      return ChildNodes.Count == 0;
    }
    #endregion


    #region IInterpretedNode Members
    public virtual object Evaluate(EvaluationContext context, AstUseMode useMode) {
      return null; 
    }
    #endregion
  }//class

}//namespace

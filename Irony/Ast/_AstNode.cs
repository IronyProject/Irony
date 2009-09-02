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
using Irony.Interpreter;

namespace Irony.Ast {

  #region NodeArgs
  // This class is a container for information used by the NodeCreator delegate
  public class NodeArgs_ {
    public readonly ParsingContext Context;
    public readonly BnfTerm Term;
    public readonly SourceSpan Span;
    public readonly AstNodeList ChildNodes;

    public NodeArgs_(ParsingContext context, BnfTerm term, SourceSpan span, AstNodeList childNodes) {
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
  public class AstNode : IAstNodeInit, IBrowsableAstNode, IInterpretedAstNode {
    public AstNode() {
    }


    #region IAstNodeInit Members
    public virtual void Init(ParsingContext context, ParseTreeNode treeNode) {
      this.Term = treeNode.Term;
      Span = treeNode.Span;
      treeNode.AstNode = this;
      AsString = (Term == null ? this.GetType().Name : Term.Name); 
    }
    #endregion

    #region IInterpretedAstNode Members
    public virtual void Evaluate(EvaluationContext context, AstMode mode) {

    }
    #endregion

    #region IBrowsableAstNode Members
    public virtual System.Collections.IEnumerable GetChildNodes() {
      return ChildNodes;
    }
    public SourceLocation Location {
      get { return Span.Location; }
    }
    #endregion

    #region properties Parent, Term, Span, Caption, Role, Flags, ChildNodes, Attributes
    public AstNode Parent;
    public BnfTerm Term; 
    public SourceSpan Span;
    public AstNodeFlags Flags;
    // Role is a free-form string used as prefix in ToString() representation of the node. 
    // Node's parent can set it to "property name" or role of the child node in parent's node context. 
    public string Role;
    // node.ToString() returns 'Role: AsString', which is used for showing node in AST tree. 
    protected string AsString;

    //List of child nodes
    public AstNodeList  ChildNodes = new AstNodeList();

    public NodeAttributeDictionary Attributes {
      get {
        if (_attributes == null)
          _attributes = new NodeAttributeDictionary();
        return _attributes;
      }
    } NodeAttributeDictionary _attributes;
    #endregion


    #region Utility methods: AddChild, IsEmpty, SetParent, IsSet, Location...
    public AstNode AddChild(string role, ParseTreeNode childParseNode) {
      return AddChild(role, childParseNode, true); 
    }
    public AstNode AddChild(string role, ParseTreeNode childParseNode, bool throwIfNull) {
      var child = (AstNode) childParseNode.AstNode;
      if (child == null) {
        if (throwIfNull) {
          var msg = string.Format("Child AST node #{0} is null. Parent: {1}, Role: {2}", 
              ChildNodes.Count + 1, this.Term.Name, role);
          throw new AstException(this, msg); 
        } else 
          return null;
      }
      child.Role = role;
      child.SetParent(this); 
      ChildNodes.Add(child);
      return child; 
    }
    public bool IsEmpty() {
      return ChildNodes.Count == 0;
    }
    public void SetParent(AstNode parent) {
      Parent = parent;
    }
    public bool IsSet(AstNodeFlags flag) {
      return (Flags & flag) != 0;
    }
    #endregion

    
    public override string ToString() {
      return string.IsNullOrEmpty(Role) ? AsString : Role + ": " + AsString; 
    }

    protected void InvalidAstMode(string mode) {
      throw new Exception(string.Format("Invalid AstMode value in call to Evaluate method. Node: {0}, mode: {1}.", 
        this.ToString(), mode));
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

  }//class

}//namespace

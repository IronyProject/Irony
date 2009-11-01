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

  public delegate void NodeEvaluate(EvaluationContext context, AstMode mode); 

  [Flags]
  public enum AstNodeFlags {
    None = 0x0,
    IsTail          = 0x01,     //the node is in tail position
  }

  public class AstNodeList : List<AstNode> { }

  //Base AST node class
  public class AstNode : IAstNodeInit, IBrowsableAstNode, IInterpretedAstNode {
    public AstNode() {
    }


    #region IAstNodeInit Members
    public virtual void Init(ParsingContext context, ParseTreeNode treeNode) {
      this.Term = treeNode.Term;
      Span = treeNode.Span;
      ErrorAnchor = this.Location;
      treeNode.AstNode = this;
      AsString = (Term == null ? this.GetType().Name : Term.Name);
    }
    #endregion

    #region IInterpretedAstNode Members
    //Important: do not override this method! - override EvaluateNode instead!
    // You should have strong reasons to override it - for example, if you want to change 
    // exception handling implementation in base method. Otherwise, put all derived functionality
    // in EvaluateNode
    public virtual void Evaluate(EvaluationContext context, AstMode mode) {
      try {
        EvaluateNode(context, mode); 
      } catch (RuntimeException) {
        throw;
      } catch (Exception ex) {
        var rex = new RuntimeException(ex.Message, ex, this.GetErrorAnchor()); 
        throw rex;
      }
    }

    public const string KeyErrorLocation = "_error_location";
    public const string KeyErrorNode = "_error_node";

    public SourceLocation GetErrorAnchor() {
      return ErrorAnchor;
    }
    #endregion

    public virtual void EvaluateNode(EvaluationContext context, AstMode mode) {
    }

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
    //Used for pointing to error location. For most nodes it would be the location of the node itself.
    // One exception is BinExprNode: when we get "Division by zero" error evaluating 
    //  x = (5 + 3) / (2 - 2)
    // it is better to point to "/" as error location, rather than the first "(" - which is the start 
    // location of binary expression. 
    protected SourceLocation ErrorAnchor;
    // Role is a free-form string used as prefix in ToString() representation of the node. 
    // Node's parent can set it to "property name" or role of the child node in parent's node context. 
    public string Role;
    // node.ToString() returns 'Role: AsString', which is used for showing node in AST tree. 
    public string AsString { get; protected set; }

    //List of child nodes
    public readonly AstNodeList  ChildNodes = new AstNodeList();

    #endregion


    #region Utility methods: AddChild, SetParent, FlagIsSet ...
    protected AstNode AddChild(string role, ParseTreeNode childParseNode) {
      var child = (AstNode)childParseNode.AstNode;
      child.Role = role;
      child.SetParent(this);
      ChildNodes.Add(child);
      return child;
    }

    public void SetParent(AstNode parent) {
      Parent = parent;
    }
    
    public bool FlagIsSet(AstNodeFlags flag) {
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

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
using Irony.Runtime;

namespace Irony.Compiler {

  public class AstNodeList : List<AstNode> {}
  public class AttributeDictionary : Dictionary<string, object> { }

  //Base AST node class
  public class AstNode {
    public AstNode(AstNodeArgs args) {
      Term = args.Term;
      Span = args.Span;
      if (args.ChildNodes == null || args.ChildNodes.Count == 0) return;
      //add child nodes, skipping nulls and punctuation symbols
      foreach (AstNode child in args.ChildNodes) {
        if (child != null && !child.Term.IsSet(TermOptions.IsPunctuation)) 
          AddChild(child);
      }//foreach
    }

    #region properties Term, Span, ChildNodes, Parent, Scope, Tag, Flags, Attributes
    public readonly BnfTerm Term;
    public readonly SourceSpan Span;
    public AstNode Parent;
    public readonly AstNodeList ChildNodes = new AstNodeList();
    //Most of the time, Scope is the scope that owns this node - the scope in which it is defined; this scope belongs to one of 
    // node parents. Only for AnonFunctionNode we have a scope that is defined by the node itself - the scope that contain function's local
    // variables
    public Scope Scope;
    // Tag is a free-form string used as prefix in ToString() representation of the node. 
    // Node's parent can set it to "property name" or role of the child node in parent's node context. 
    public string Tag;  
    
    public AstNodeFlags Flags;
    public bool IsSet(AstNodeFlags flag) {
      return (Flags & flag) != 0;
    }

    public SourceLocation Location {
      get { return Span.Start; }
    }
    public AttributeDictionary Attributes {
      get {
        if (_attributes == null)
          _attributes = new AttributeDictionary();
        return _attributes;
      }
    } AttributeDictionary _attributes;

    #endregion
    
    public override string ToString() {
      string result = string.Empty; 
      if (!string.IsNullOrEmpty(Tag))
        result = Tag + ": ";
      result += Term.Name;
      if (ChildNodes.Count == 0)
        result += "(Empty)";
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
        child.AddAll(list);
    }
    #endregion

    #region AstProcessing
    public virtual void OnAstProcessing(CompilerContext context, AstProcessingPhase phase) {
      switch (phase) {
        case AstProcessingPhase.CreatingScopes:
          if (Parent != null)
            this.Scope = Parent.Scope;
          break;
      }//switch
    }
    #endregion

    public virtual void Evaluate(EvaluationContext context) {
      if (ChildNodes.Count == 0) return;
      foreach (AstNode child in ChildNodes)
        child.Evaluate(context);
    }

    #region ChildNodes manipulations
    public virtual bool IsEmpty() {
      return ChildNodes.Count == 0;
    }

    public void ReplaceChildNodes(params AstNode[] nodes) {
      ChildNodes.Clear();
      foreach (AstNode node in nodes)
        AddChild(node);
    }
    public void ReplaceChildNodes(AstNodeList nodeList) {
      ChildNodes.Clear();
      foreach (AstNode node in nodeList)
        AddChild(node);
    }
    public void AddChild(AstNode child) {
      if (child == null) return;
      child.Parent = this;
      ChildNodes.Add(child);
    }
    //Finds the first token with non-null value, among this node plus all its children
    public string GetContent() {
      foreach (AstNode node in GetAll()) {
        Token tkn = node as Token;
        if (tkn != null && tkn.Value != null)
          return tkn.ValueString;
      }
      return null;
    }
    #endregion

  }//class

}//namespace

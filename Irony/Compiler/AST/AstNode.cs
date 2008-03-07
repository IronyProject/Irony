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

namespace Irony.Compiler {

  public class AstNodeList : List<AstNode> {}
  public class AttributeDictionary : Dictionary<string, object> { }

  //Base AST node class
  public class AstNode {
    public AstNode(CompilerContext context, BnfTerm term, SourceLocation location, AstNodeList childNodes) {
      Term = term;
      Location = location;
      if (childNodes == null) return;
      //add child nodes, skipping nulls and punctuation symbols
      foreach (AstNode child in childNodes) {
        if (child != null && !child.Term.IsSet(TermOptions.IsPunctuation)) {
          ChildNodes.Add(child);
          child.Parent = this;
        }
      }//foreach
    }

    #region properties Term, Location, ChildNodes, Parent, CodeDomObject, Tag, Attributes
    public readonly BnfTerm Term;
    public readonly SourceLocation Location;
    public readonly AstNodeList ChildNodes = new AstNodeList();
    
    public AstNode Parent  {
      get {return _parent;}
      set {_parent = value;}
    } AstNode  _parent;

    public CodeObject CodeDomObject {
      get { return _codeDomObject; }
      set { _codeDomObject = value; }
    }  CodeObject _codeDomObject;

    // Tag is a free-form string used as prefix in ToString() representation of the node. 
    // Node's parent can set it to "property name" or role of the child node in parent's node context. 
    public string Tag  {
      get {return _tag;}
      set {_tag = value;}
    } string  _tag;

    public AttributeDictionary Attributes {
      get {
        if (_attributes == null)
          _attributes = new AttributeDictionary();
        return _attributes;
      }
    } AttributeDictionary _attributes;

    #endregion
    
    public override string ToString() {
      if (string.IsNullOrEmpty(_tag))
        return Term.Name;
      else
        return Tag + ":" + Term.Name;
    }

    //the first primitive Visitor facility
    public virtual void AcceptVisitor(IAstVisitor visitor) {
      visitor.BeginVisit(this);
      if (ChildNodes.Count > 0)
        foreach(AstNode node in ChildNodes)
          node.AcceptVisitor(visitor);
      visitor.EndVisit(this);
    }

  }//class

}//namespace

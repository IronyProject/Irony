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

namespace Irony.Compiler {

  public delegate AstNode NodeCreatorMethod(NodeArgs args);

  public class NonTerminalList : List<NonTerminal> { }

  public class NonTerminal : BnfTerm {

    #region constructors
    public NonTerminal(string name)  : base(name) {
    }
    public NonTerminal(string name, NodeCreatorMethod nodeCreator)  : base(name) {
      NodeCreator = nodeCreator;
    }
    public NonTerminal(string name, string alias)
      : base(name, alias) {
    }
    public NonTerminal(string name, Type nodeType) : this(name) { 
      base.NodeType = nodeType;
    }
    public NonTerminal(Type nodeType) : this(nodeType.Name) {
      base.NodeType = nodeType;
    }
    public NonTerminal(string name, BnfExpression expression)
      : this(name) { 
      Rule = expression;
    }
    #endregion

    #region properties/fields: Rule, ErrorRule 
    public BnfExpression Rule; 
    //Separate property for specifying error expressions. This allows putting all such expressions in a separate section
    // in grammar for all non-terminals. However you can still put error expressions in the main Rule property, just like
    // in YACC
    public BnfExpression ErrorRule;

    #endregion

    #region events and delegates: NodeCreator, NodeCreated
    public NodeCreatorMethod NodeCreator;
    public event EventHandler<NodeCreatedEventArgs> NodeCreated;

    protected internal void OnNodeCreated(AstNode node) {
      if (NodeCreated == null) return;
      NodeCreatedEventArgs args = new NodeCreatedEventArgs(node);
      NodeCreated(this, args);
    }
    #endregion

    #region overrids: ToString
    public override string ToString() {
      string result = Name;
      if (string.IsNullOrEmpty(Name))
        result = "(unnamed)";
      return result; 
    }
    #endregion

  }//class


}//namespace

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

  public delegate AstNode NodeCreatorMethod(AstNodeArgs args);
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
    public NonTerminal(string name, BnfExpression expression) : this(name) { 
      _rule = expression;
    }
    #endregion

    #region properties: Rule, ErrorRule, Productions, ErrorProductions, Firsts
    public BnfExpression Rule {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _rule; }
      set {_rule = value; }
    }  BnfExpression _rule;

    //Separate property for specifying error expressions. This allows putting all such expressions in a separate section
    // in grammar for all non-terminals. However you can still put error expressions in the main Rule property, just like
    // in YACC
    public BnfExpression ErrorRule {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _errorRule; }
      set {_errorRule = value;}
    } BnfExpression  _errorRule;

    //The following fields are used by GrammarDataBuilder and Parser
    public readonly ProductionList Productions = new ProductionList();
    public readonly StringSet Firsts = new StringSet();
    public readonly NonTerminalList PropagateFirstsTo = new NonTerminalList();
    #endregion

    #region events and delegates: NodeCreator, NodeCreated
    public NodeCreatorMethod NodeCreator;
    public event EventHandler<NodeCreatedEventArgs> NodeCreated;

    protected internal AstNode InvokeNodeCreator(AstNodeArgs args) {
      if (NodeCreator == null) return null;
      return NodeCreator(args);
    }
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

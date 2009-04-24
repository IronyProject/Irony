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
using System.Reflection; 

namespace Irony.CompilerServices {

  public delegate void AstNodeCreator(CompilerContext context, ParseTreeNode parseNode);

  public class NonTerminalList : List<NonTerminal> { }
  public class NonTerminalSet : HashSet<NonTerminal> {}

  public class NonTerminal : BnfTerm {

    #region constructors
    public NonTerminal(string name)  : base(name, null) { //by default display name is null
    }
    public NonTerminal(string name, AstNodeCreator nodeCreator)  : base(name) {
      NodeCreator = nodeCreator;
    }
    public NonTerminal(string name, string displayName) : base(name, displayName) {
    }
    public NonTerminal(string name, Type nodeType) : this(name) { 
      NodeType = nodeType;
    }
    public NonTerminal(Type nodeType) : this(nodeType.Name) {
      NodeType = nodeType;
    }
    public NonTerminal(string name, BnfExpression expression)
      : this(name) { 
      Rule = expression;
    }
    #endregion

    #region properties/fields: NodeType, Rule, ErrorRule 
    public Type NodeType;
    
    public BnfExpression Rule; 
    //Separate property for specifying error expressions. This allows putting all such expressions in a separate section
    // in grammar for all non-terminals. However you can still put error expressions in the main Rule property, just like
    // in YACC
    public BnfExpression ErrorRule;

    #endregion

    #region events and delegates: NodeCreator, NodeCreated
    public AstNodeCreator NodeCreator;
    public event EventHandler<NodeCreatedEventArgs> NodeCreated;

    protected internal void OnNodeCreated(object node) {
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

    #region data used by Parser builder
    public readonly ProductionList Productions = new ProductionList();
    public readonly BnfTermSet Firsts = new BnfTermSet();
    public readonly BnfTermSet DirectFirsts = new BnfTermSet();
    internal int _tailCount; //for generating unique names for tails

    internal int _lastChecked; //used in computation of Firsts
    internal int _lastChanged;

    #endregion

  }//class


}//namespace

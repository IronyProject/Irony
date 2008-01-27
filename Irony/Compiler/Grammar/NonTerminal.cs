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

  //Node creator method delegate. A non-terminal may have a custom node-creation method associated with it. 
  public delegate AstNode NodeCreatorMethod(CompilerContext context, ActionRecord reduceAction,
                                      SourceLocation location, AstNodeList childNodes); 
  //Class representing Non-Terminal syntactic element in BNF forms. 
  public class NonTerminal : BnfElement {

    #region constructors
    public NonTerminal(string name) : base(name) { 
    }
    public NonTerminal(string name, string alias) : base(name, alias) {
    }
    public NonTerminal(string name, bool isList)
      : this(name) {
      _isList = isList;
    }
    public NonTerminal(string name, Type nodeType) : this(name) { 
      _nodeType = nodeType;
    }
    public NonTerminal(Type nodeType) : this(nodeType.Name) {
      _nodeType = nodeType;
    }
    public NonTerminal(string name, BnfExpression expression) : this(name) { 
      _rule = expression;
    }
    public NonTerminal(string name, Type nodeType, NodeCreatorMethod nodeCreator)   : this(name, nodeType) {
      this.NodeCreator = nodeCreator;
    }
    #endregion

    #region properties: NodeType, Rule, ErrorRule, Productions, ErrorProductions, Firsts
    public Type NodeType  {
      get {return _nodeType;}
      set {_nodeType = value;}
    } Type  _nodeType;

    public BnfExpression Rule {
      get { return _rule; }
      set {_rule = value; }
    }  BnfExpression _rule;

    //Separate property for specifying error expressions. This allows putting all such expressions in a separate section
    // in grammar for all non-terminals. However you can still put error expressions in the main Rule property, just like
    // in YACC
    public BnfExpression ErrorRule  {
      get {return _errorRule;}
      set {_errorRule = value;}
    } BnfExpression  _errorRule;

    //The following fields are used by GrammarDataBuilder and Parser
    public readonly ProductionList Productions = new ProductionList();
    public readonly KeyList Firsts = new KeyList();
    public readonly NonTerminalList PropagateFirstsTo = new NonTerminalList();

    public bool IsList {
      get { return _isList; }
      set { _isList = value; }
    } bool _isList;

    //A custom node creation method
    public NodeCreatorMethod NodeCreator;
    #endregion

    public override string ToString() {
      if (string.IsNullOrEmpty(Name))
        return "(unnamed)";
      else 
        return Name;
    }

  }//class

  public class NonTerminalList : List<NonTerminal> { }

}//namespace

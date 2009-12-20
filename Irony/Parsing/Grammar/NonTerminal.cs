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

namespace Irony.Parsing {


  public class NonTerminalList : List<NonTerminal> { }
  public class NonTerminalSet : HashSet<NonTerminal> {}

  public class NonTerminal : BnfTerm {

    #region constructors
    public NonTerminal(string name)  : base(name, null) { }  //by default display name is null
    public NonTerminal(string name, string errorAlias) : base(name, errorAlias) { }
    public NonTerminal(string name, string errorAlias, Type nodeType) : base(name, errorAlias, nodeType ) { }
    public NonTerminal(string name, string errorAlias,  AstNodeCreator nodeCreator) : base(name, errorAlias, nodeCreator) {}
    public NonTerminal(string name, Type nodeType) : base(name, null, nodeType) { }
    public NonTerminal(string name, AstNodeCreator nodeCreator) : base(name, null, nodeCreator) { }
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

    #region overrids: ToString
    public override string ToString() {
      string result = Name;
      if (string.IsNullOrEmpty(Name))
        result = Resources.LabelUnnamed;
      return result; 
    }
    #endregion

    #region data used by Parser builder
    public readonly ProductionList Productions = new ProductionList();
    #endregion

  }//class


}//namespace

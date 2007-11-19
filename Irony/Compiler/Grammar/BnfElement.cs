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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Irony.Compiler {

  public class BnfElementList : List<BnfElement> { }

  //Basic Backus-Naur Form element. BNF elements may be of two kinds: Terminals and NonTerminals. 
  public abstract class BnfElement : IBnfExpression, IComparable  {
    public BnfElement(string name) {
      _name = name;
      _key = name + "\b"; //to guarantee against erroneous match of element's key with token value
                          // so Identifier terminal doesn't match 'identifier' string 
                          // in the input stream
    }
    public override string ToString() {
      return "[" + _name + "]";
    }

    #region properties: Name
    public string Name {
      get { return _name; }
      internal set { _name = value; }
    } string  _name;

    //The Key is used in matching 
    public virtual string Key {
      get {return _key; }
      protected set { _key = value; }
    } string _key;

    public virtual bool Nullable {
      get { return _nullable; }
      set { _nullable = value; }
    }   bool _nullable;

    #endregion

    #region Kleene operators: Q(), Plus(), Star()
    //We cache values of some multiplicator functions, so that subsequent calls to the same function
    // return the same non-terminal
    NonTerminal _q, _plus, _star;
    
    public NonTerminal Q() {
      if (_q != null) return _q;
      _q = new NonTerminal(this.Name + "?");
      _q.Expression = this | Grammar.Empty;
      return _q;
    }
    public NonTerminal Plus() {
      if (_plus != null) return _plus;
      _plus = new NonTerminal(this.Name + "+", true);
      _plus.Expression = this | _plus + this;
      return _plus;
    }
    public NonTerminal Star() {
      if (_star != null) return _star;
      _star = new NonTerminal(this.Name + "*", true);
      _star.Expression = _star + this | Grammar.Empty;
      return _star;
    }

    public NonTerminal Plus(BnfElement delimiter) {
      if (delimiter == null) return Plus();
      NonTerminal list = new NonTerminal(this.Name + "_list", true);
      list.Expression = this | list + delimiter + this;
      return list;
    }

    public NonTerminal Star(BnfElement delimiter) {
      if (delimiter == null) return Star();
      NonTerminal list = new NonTerminal(this.Name + "_list?", true);
      list.Expression = this.Plus(delimiter).Q();
      return list;
    }
    public NonTerminal Plus(string delimiter) {
      return Plus(SymbolTerminal.GetSymbol(delimiter));
    }
    public NonTerminal Star(string delimiter) {
      return Star(SymbolTerminal.GetSymbol(delimiter));
    }
    #endregion

    #region static methods and operations; operators: +, |, implicit, -
    public static BnfExpression operator +(BnfElement elem, IBnfExpression iexpr) {
      return BnfExpression.Op_Plus(elem, iexpr);
    }
    public static BnfExpression operator +(BnfElement elem1, string symbol2) {
      return BnfExpression.Op_Plus(elem1, symbol2);
    }
    public static BnfExpression operator +( string symbol1, BnfElement elem2) {
      return BnfExpression.Op_Plus(symbol1, elem2);
    }

    //Alternative 
    public static BnfExpression operator |(BnfElement elem, IBnfExpression iexpr) {
      return BnfExpression.Op_Pipe(elem, iexpr);
    }
    public static BnfExpression operator |(BnfElement elem1, string symbol2) {
      return BnfExpression.Op_Pipe(elem1, symbol2);
    }
    public static BnfExpression operator |(string symbol1, BnfElement elem2) {
      return BnfExpression.Op_Pipe(symbol1, elem2);
    }
    
    #endregion

    #region ISyntacticExpression Members

    BnfExpressionType IBnfExpression.ExpressionType {
      get { return BnfExpressionType.Element; }
    }

    #endregion


    #region IComparable Members
    // Terminals are sorted by string representation just for pretty printing only
    public int CompareTo(object obj) {
      return string.Compare(this.ToString(), obj.ToString());
    }

    #endregion
  }//class



}//namespace

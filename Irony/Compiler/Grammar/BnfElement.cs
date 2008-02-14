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

  //Basic Backus-Naur Form element. 
  public class BnfElement  {
    public BnfElement(string name) : this(name, null) { }
    public BnfElement(string name, string alias) {
      _name = name;
      _alias = alias;
      _key = name + "\b"; //to guarantee against erroneous match of element's key with token value
                          // so Identifier terminal doesn't match 'identifier' string 
                          // in the input stream
    }
    public virtual void Init(Grammar grammar) {
      _grammar = grammar;
    }

    public override string ToString() {
      return "[" + _name + "]";
    }


    #region properties: Name
    public string Name {
      get { return _name; }
      internal set { _name = value; }
    } string  _name;
    
    //Alias is used in error reporting, e.g. "Syntax error, expected <list-of-aliases>". 
    public string Alias  {
      get {return _alias;}
      set {_alias = value;}
    } string  _alias;

    //The Key is used in matching 
    public virtual string Key {
      get {return _key; }
      set { _key = value; }
    } string _key;

    public virtual bool Nullable {
      get { return _nullable; }
      set { _nullable = value; }
    }   bool _nullable;

    protected Grammar Grammar {
      get { return _grammar; }
    } Grammar _grammar;

    public virtual Type NodeType {
      get { return _nodeType; }
      set { _nodeType = value; }
    } Type _nodeType;

    public BnfFlags Flags {
      get { return _flags; }
      set { _flags = value; }
    } BnfFlags _flags;

    public bool IsFlagSet(BnfFlags flag) {
      return (_flags & flag) != 0;
    }
    public void SetFlag(BnfFlags flag) {
      SetFlag(flag, true);
    }
    public void SetFlag(BnfFlags flag, bool value) {
      if (value)
        _flags |= flag;
      else
        _flags &= ~flag;
    }

    #endregion

    #region Kleene operators: Q(), Plus(), Star()
    //We cache values of some multiplicator functions, so that subsequent calls to the same function
    // return the same non-terminal
    NonTerminal _q, _plus, _star;
    
    public NonTerminal Q() {
      if (_q != null) return _q;
      _q = new NonTerminal(this.Name + "?");
      _q.Rule = this | Grammar.Empty;
      return _q;
    }
    public NonTerminal Plus() {
      if (_plus != null) return _plus;
      _plus = new NonTerminal(this.Name + "+");
      _plus.SetFlag(BnfFlags.IsList);
      _plus.Rule = this | _plus + this;
      return _plus;
    }
    public NonTerminal Star() {
      if (_star != null) return _star;
      _star = new NonTerminal(this.Name + "*");
      _star.SetFlag(BnfFlags.IsList);
      _star.Rule = _star + this | Grammar.Empty;
      return _star;
    }

    public NonTerminal Plus(BnfElement delimiter) {
      if (delimiter == null) return Plus();
      NonTerminal list = new NonTerminal(this.Name + "_list");
      list.SetFlag(BnfFlags.IsList);
      list.Rule = this | list + delimiter + this;
      return list;
    }
    public NonTerminal Star(BnfElement delimiter) {
      if (delimiter == null) return Star();
      NonTerminal list = new NonTerminal(this.Name + "_list?");
      list.SetFlag(BnfFlags.IsList);
      list.Rule = Grammar.Empty | list + delimiter + this;
      return list;
    }
    public NonTerminal Plus(string delimiter) {
      return Plus(SymbolTerminal.GetSymbol(delimiter));
    }
    public NonTerminal Star(string delimiter) {
      return Star(SymbolTerminal.GetSymbol(delimiter));
    }
    #endregion

    #region Operators: +, |, implicit
    public static BnfExpression operator +(BnfElement elem1, BnfElement elem2) {
      return Op_Plus(elem1, elem2);
    }
    public static BnfExpression operator +(BnfElement elem1, string symbol2) {
      return Op_Plus(elem1, SymbolTerminal.GetSymbol(symbol2));
    }
    public static BnfExpression operator +( string symbol1, BnfElement elem2) {
      return Op_Plus(SymbolTerminal.GetSymbol(symbol1), elem2);
    }

    //Alternative 
    public static BnfExpression operator |(BnfElement elem1, BnfElement elem2) {
      return Op_Pipe(elem1, elem2);
    }
    public static BnfExpression operator |(BnfElement elem1, string symbol2) {
      return Op_Pipe(elem1, SymbolTerminal.GetSymbol(symbol2));
    }
    public static BnfExpression operator |(string symbol1, BnfElement elem2) {
      return Op_Pipe(SymbolTerminal.GetSymbol(symbol1), elem2);
    }

    //BNF operations implementation -----------------------
    // Plus/sequence
    internal static BnfExpression Op_Plus(BnfElement elem1, BnfElement elem2) {
      //Check elem1 and see if we can use it as result, simply adding elem2 as operand
      BnfExpression expr1 = elem1 as BnfExpression;
      if (expr1 == null || expr1.Data.Count > 1) //either not expression at all, or Pipe-type expression (count > 1)
        expr1 = new BnfExpression(elem1);
      if (elem2 != Grammar.Empty)
        expr1.Data[expr1.Data.Count - 1].Add(elem2);
      return expr1;
    }

    //Pipe/Alternative
    internal static BnfExpression Op_Pipe(BnfElement elem1, BnfElement elem2) {
      //Check elem1 and see if we can use it as result, simply adding elem2 as operand
      BnfExpression expr1 = elem1 as BnfExpression;
      if (expr1 == null) //either not expression at all, or Pipe-type expression (count > 1)
        expr1 = new BnfExpression(elem1);
      //Check elem2; if it is an expression and is simple sequence (Data.Count == 1) then add this sequence directly to expr1
      BnfExpression expr2 = elem2 as BnfExpression;
      //1. elem2 is a simple expression
      if (expr2 != null && expr2.Data.Count == 1) { // if it is simple sequence (plus operation), add it directly
        expr1.Data.Add(expr2.Data[0]);
        return expr1;
      }
      //2. elem2 is not a simple expression
      expr1.Data.Add(new BnfElementList()); //add a list for a new OR element (new "plus" sequence)
      if (elem2 != Grammar.Empty)
        expr1.Data[expr1.Data.Count - 1].Add(elem2); // and put  elem2 there if it is not Empty pseudo-element
      return expr1;
    }

    #endregion



  }//class



}//namespace

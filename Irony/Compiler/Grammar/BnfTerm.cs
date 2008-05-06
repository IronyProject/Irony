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

  public class BnfTermList : List<BnfTerm> { }

  //Basic Backus-Naur Form element. 
  public class BnfTerm  {
    public BnfTerm(string name) : this(name, name) { }
    public BnfTerm(string name, string displayName) {
      Name = name;
      DisplayName = displayName;
      Key = Name + "\b"; //to guarantee against erroneous match of term's key with token value
                          // so Identifier terminal doesn't match 'identifier' string 
                          // in the input stream
    }
    public virtual void Init(Grammar grammar) {
      Grammar = grammar;
    }

    public override string ToString() {
      return "[" + Name + "]";
    }


    #region properties: Name, DisplayName, Key, Options
    public string Name;    
    //DisplayName is used in error reporting, e.g. "Syntax error, expected <list-of-aliases>". 
    public string DisplayName;
    //The Key is used in matching 
    public string Key;
    public TermOptions Options;
    public Type NodeType;
    protected Grammar Grammar;
    public bool Nullable;

    [System.Diagnostics.DebuggerStepThrough]
    public bool IsSet(TermOptions option) {
      return (Options & option) != 0;
    }
    [System.Diagnostics.DebuggerStepThrough]
    public void SetOption(TermOptions option) {
      SetOption(option, true);
    }
    [System.Diagnostics.DebuggerStepThrough]
    public void SetOption(TermOptions option, bool value) {
      if (value)
        Options |= option;
      else
        Options &= ~option;
    }

    #endregion


    #region Kleene operators: Q(), Plus(), Star()
    // We cache Plus() and Star() lists (non-terminals) and return the same instance for repeated calls; 
    // we don't do it for Q(), because it returns expression, not NonTerminal, and expression would better be not shared
    NonTerminal _plus, _star; 
    public BnfExpression Q() {
      BnfExpression q = Grammar.Empty | this;
      q.Name = this.Name + "?";
      return q; 
    }
    public NonTerminal Plus() {
      if (_plus != null) return _plus;
      string name = this.Name + "+";
      _plus = new NonTerminal(name);
      _plus.SetOption(TermOptions.IsList);
      _plus.Rule = this | _plus + this;
      return _plus;
    }
    public NonTerminal Star() {
      if (_star != null) return _star;
      string name = this.Name + "*";
      _star = new NonTerminal(name);
      _star.SetOption(TermOptions.IsList);
      _star.Rule = Grammar.Empty | _star + this;
      return _star;
    }
    #endregion

    #region Operators: +, |, implicit
    public static BnfExpression operator +(BnfTerm term1, BnfTerm term2) {
      return Op_Plus(term1, term2);
    }
    public static BnfExpression operator +(BnfTerm term1, string symbol2) {
      return Op_Plus(term1, SymbolTerminal.GetSymbol(symbol2));
    }
    public static BnfExpression operator +( string symbol1, BnfTerm term2) {
      return Op_Plus(SymbolTerminal.GetSymbol(symbol1), term2);
    }

    //Alternative 
    public static BnfExpression operator |(BnfTerm term1, BnfTerm term2) {
      return Op_Pipe(term1, term2);
    }
    public static BnfExpression operator |(BnfTerm term1, string symbol2) {
      return Op_Pipe(term1, SymbolTerminal.GetSymbol(symbol2));
    }
    public static BnfExpression operator |(string symbol1, BnfTerm term2) {
      return Op_Pipe(SymbolTerminal.GetSymbol(symbol1), term2);
    }

    //BNF operations implementation -----------------------
    // Plus/sequence
    internal static BnfExpression Op_Plus(BnfTerm term1, BnfTerm term2) {
      //Check term1 and see if we can use it as result, simply adding term2 as operand
      BnfExpression expr1 = term1 as BnfExpression;
      if (expr1 == null || expr1.Data.Count > 1) //either not expression at all, or Pipe-type expression (count > 1)
        expr1 = new BnfExpression(term1);
      expr1.Data[expr1.Data.Count - 1].Add(term2);
      return expr1;
    }

    //Pipe/Alternative
    internal static BnfExpression Op_Pipe(BnfTerm term1, BnfTerm term2) {
      //Check term1 and see if we can use it as result, simply adding term2 as operand
      BnfExpression expr1 = term1 as BnfExpression;
      if (expr1 == null) //either not expression at all, or Pipe-type expression (count > 1)
        expr1 = new BnfExpression(term1);
      //Check term2; if it is an expression and is simple sequence (Data.Count == 1) then add this sequence directly to expr1
      BnfExpression expr2 = term2 as BnfExpression;
      //1. term2 is a simple expression
      if (expr2 != null && expr2.Data.Count == 1) { // if it is simple sequence (plus operation), add it directly
        expr1.Data.Add(expr2.Data[0]);
        return expr1;
      }
      //2. term2 is not a simple expression
      expr1.Data.Add(new BnfTermList()); //add a list for a new OR element (new "plus" sequence)
      expr1.Data[expr1.Data.Count - 1].Add(term2); // and put  term2 there if it is not Empty pseudo-element
      return expr1;
    }

    #endregion

  }//class

}//namespace


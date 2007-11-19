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
using System.Diagnostics;

namespace Irony.Compiler {

  public enum BnfExpressionType {
    Alternative,
    Sequence,
    Element
  }
  
  public interface IBnfExpression {
    BnfExpressionType ExpressionType { get;}
  }
  
  public class BnfExpressionList : List<IBnfExpression> { }


  public class BnfExpression : IBnfExpression {

    public BnfExpression(BnfElement element) {
      _expressionType = BnfExpressionType.Sequence;
      Operands.Add(element);
    }
    
    public BnfExpression(BnfExpressionType expressionType, params IBnfExpression[] operands) { 
      _expressionType = expressionType;
      Operands.AddRange(operands);
    }

    #region properties: ExpressionType, Operands
    public BnfExpressionType ExpressionType  {
      get {return _expressionType;}
    } BnfExpressionType  _expressionType;

    public BnfExpressionList Operands = new BnfExpressionList();
    #endregion


    #region overrides: ToString()
    public override string ToString() {
      if (_toString != null) return _toString;
      try {
        switch (this.ExpressionType) {
          case BnfExpressionType.Alternative: 
            _toString = OperandsToString("|");
            break;
          case BnfExpressionType.Sequence:
            _toString = OperandsToString("");
            break;
        }//case
        return _toString; 
      } catch(Exception e) {
        return "(error: " + e.Message + ")";
      }
    } private string _toString;

    private string OperandsToString(string separator) {
      string[] tmp = new string[Operands.Count];      for(int i = 0; i < tmp.Length; i++)
        tmp[i] = Operands[i].ToString();
      string result = string.Join(separator, tmp);
      return result;
    }
    #endregion

    #region static methods and operations; operators: +, |, implicit
    //Sequence
    public static BnfExpression operator +(BnfExpression expr1, IBnfExpression iexpr2) {
      return Op_Plus(expr1, iexpr2);
    }
    public static BnfExpression operator +(BnfExpression expr, string symbol) {
      return Op_Plus(expr, SymbolTerminal.GetSymbol(symbol));
    }

    //Alternative 
    public static BnfExpression operator |(BnfExpression expr1, IBnfExpression iexpr2) {
      return Op_Pipe(expr1, iexpr2);
    }
    public static BnfExpression operator |(BnfExpression expr, string symbol) {
      return Op_Pipe(expr, SymbolTerminal.GetSymbol(symbol));
    }

    //implementations -----------------------
    // Plus/sequence
    public static BnfExpression Op_Plus(IBnfExpression iexpr1, IBnfExpression iexpr2) {
      if (iexpr1.ExpressionType == BnfExpressionType.Sequence) {
        BnfExpression expr1 = iexpr1 as BnfExpression;  
        expr1.Operands.Add(iexpr2);
        return expr1;
      }
      return new BnfExpression(BnfExpressionType.Sequence, iexpr1, iexpr2);
    }

    public static BnfExpression Op_Plus(IBnfExpression iexpr1, string symbol2) {
      return new BnfExpression(BnfExpressionType.Sequence, iexpr1, SymbolTerminal.GetSymbol(symbol2));
    }

    public static BnfExpression Op_Plus(string symbol1, IBnfExpression iexpr2) {
      return new BnfExpression(BnfExpressionType.Sequence, SymbolTerminal.GetSymbol(symbol1), iexpr2 );
    }

    //Pipe/Alternative
    public static BnfExpression Op_Pipe(IBnfExpression iexpr1, IBnfExpression iexpr2) {
      if (iexpr1.ExpressionType == BnfExpressionType.Alternative ) {
        BnfExpression expr1 = iexpr1 as BnfExpression;
        expr1.Operands.Add(iexpr2);
        return expr1;
      }
      return new BnfExpression(BnfExpressionType.Alternative, iexpr1, iexpr2);
    }

    public static BnfExpression Op_Pipe(IBnfExpression iexpr1, string symbol2) {
      return new BnfExpression(BnfExpressionType.Alternative, iexpr1, SymbolTerminal.GetSymbol(symbol2));
    }

    public static BnfExpression Op_Pipe(string symbol1, IBnfExpression iexpr2) {
      return new BnfExpression(BnfExpressionType.Alternative, SymbolTerminal.GetSymbol(symbol1), iexpr2);
    }

    //Implicit conversion
    public static implicit operator BnfExpression(string symbol) {
      return new BnfExpression(SymbolTerminal.GetSymbol(symbol));
    }
 
    public static implicit operator BnfExpression(BnfElement elem) {
      return new BnfExpression(elem);
    }

    #endregion


  }//class

}//namespace

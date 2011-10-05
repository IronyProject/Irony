using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Irony.Parsing {

    public static class CustomExpressionTypes {
      public const ExpressionType NotAnExpression = (ExpressionType)(-1);
      //public const ExpressionType PropertyGet = (ExpressionType)200;
      //public const ExpressionType PropertySet = (ExpressionType)201;
    }



    public class OperatorInfo {
      public string Symbol;
      public ExpressionType ExpressionType;
      public int Precedence;
      public Associativity Associativity;
    }

    public class OperatorInfoDictionary : Dictionary<string, OperatorInfo> {
      public OperatorInfoDictionary(bool caseSensitive) : base(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase) {}

      public void Add(string symbol, ExpressionType expressionType,    int precedence, Associativity associativity = Associativity.Left) {
        var info = new OperatorInfo() { Symbol = symbol, ExpressionType = expressionType, 
            Precedence = precedence, Associativity = associativity };
        this[symbol] = info;
      }
    }//class

}

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
using System.Linq;
using System.Text;

namespace Irony.Compiler {

  public enum HintType {
    /// <summary>
    /// Instructs the parser to go for shift action in case of shift/reduce conflicts. Use Grammar.PreferShiftHere() method to create
    /// the hint of this type and insert it right before the symbol to be shifted. Shift is default preferred action for shift-reduce
    /// conflicts, so effectively this hint suppresses the warning message  reported in Grammar errors list
    /// </summary>
    PreferShift,
    /// <summary>
    /// Instructs the parser to opt for reduce operation on particular production in case of conflicts. Use Grammar.ReduceThis() method
    /// to create a hint of this type, and place it at the end of the production to be preferred.
    /// </summary>
    ReduceThis,
    /// <summary>
    /// Currently ignored by Parser, may be used in the future to set specific precedence value of the following terminal operator.
    /// One example where it can be used is setting higher precedence value for unary + or - operators. This hint would override 
    /// precedence set for these operators for cases when they are used as unary operators. (YACC has this feature).
    /// </summary>
    Precedence,
    /// <summary>
    /// Provided for custom parsers. 
    /// </summary>
    Custom
  }

  public class GrammarHintList : List<GrammarHint> { }

  //Hints are additional instructions for parser added inside BNF expressions.
  // Hint refers to specific position inside the expression (production), so hints are associated with LR0Item object 
  // (which is Production + position inside production). 
  // One example is a conflict-resolution hint produced by the Grammar.PreferShiftHere() method. It tells parser to perform
  // shift in case of a shift/reduce conflict. It is in fact the default action of LALR parser, so the hint simply suppresses the error 
  // message about the shift/reduce conflict in the grammar.
  public class GrammarHint : BnfTerm {
    public readonly HintType HintType;
    public int Position;
    public readonly object Data;

    public GrammarHint(HintType hintType) : base("hint_" + hintType.ToString()) {
      HintType = hintType;
    }
    public GrammarHint(HintType hintType, object data) : this(hintType) {
      Data = data; 
    }
  }//class

}

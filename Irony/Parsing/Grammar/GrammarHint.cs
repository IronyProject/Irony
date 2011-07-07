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

namespace Irony.Parsing {

  public enum HintType {
    /// <summary>
    /// Instruction to resolve conflict to shift
    /// </summary>
    ResolveToShift,
    /// <summary>
    /// Instruction to resolve conflict to reduce
    /// </summary>
    ResolveToReduce,
    /// <summary>
    /// Instruction to resolve the conflict using special code in grammar in OnResolvingConflict method.
    /// </summary>
    ResolveInCode,
    /// <summary>
    /// Currently ignored by Parser, may be used in the future to set specific precedence value of the following terminal operator.
    /// One example where it can be used is setting higher precedence value for unary + or - operators. This hint would override 
    /// precedence set for these operators for cases when they are used as unary operators. (YACC has this feature).
    /// </summary>
    Precedence,
    /// <summary>
    /// Provided for all custom hints that derived solutions may introduce 
    /// </summary>
    Custom
  }


  public class GrammarHintList : List<GrammarHint> {
#if SILVERLIGHT
    public delegate bool HintPredicate(GrammarHint hint);
    public GrammarHint Find(HintPredicate match) {
      foreach(var hint in this)
        if (match(hint)) return hint; 
      return null; 
    }
#endif 
  }

  //Hints are additional instructions for parser added inside BNF expressions.
  // Hint refers to specific position inside the expression (production), so hints are associated with LR0Item object 
  // One example is a conflict-resolution hint produced by the Grammar.PreferShiftHere() method. It tells parser to perform
  // shift in case of a shift/reduce conflict. It is in fact the default action of LALR parser, so the hint simply suppresses the error 
  // message about the shift/reduce conflict in the grammar.
  public class GrammarHint : BnfTerm {
    public readonly HintType HintType;
    public readonly object Data;

    public GrammarHint(HintType hintType, object data) : base("HINT") {
      HintType = hintType;
      Data = data; 
    }
  }//class

}

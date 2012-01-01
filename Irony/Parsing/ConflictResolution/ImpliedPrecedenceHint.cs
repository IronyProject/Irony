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

  public class ImpliedPrecedenceHint : GrammarHint {
    //GrammarHint inherits Precedence and Associativity members from BnfTerm; we'll use them to store implied values for this hint

    public ImpliedPrecedenceHint(int precedence, Associativity associativity) {
      Precedence = precedence;
      Associativity = associativity; 
    }

    public override void Apply(LanguageData language, Construction.LRItem owner) {
      var curr = owner.Core.Current;
      var currTerm = curr as Terminal;
      if (currTerm != null) {
        currTerm.ParserInputPreview += Terminal_ParseNodeCreated; 
        return; 
      }
      var currNonTerm = curr as NonTerminal;
      if (currNonTerm != null)
        currNonTerm.Reduced += NonTerminal_Reduced;

    }

    void Terminal_ParseNodeCreated(object sender, ParsingEventArgs e) {
      e.Context.CurrentParserInput.Associativity = Associativity; 
      e.Context.CurrentParserInput.Precedence = Precedence;
    }

    void NonTerminal_Reduced(object sender, ReducedEventArgs e) {
      e.ResultNode.Associativity = Associativity;
      e.ResultNode.Precedence = Precedence; 
    }

  
  }//class
}

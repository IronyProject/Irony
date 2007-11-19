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
using System.Text.RegularExpressions;

namespace Irony.Compiler {

  //Note: this class was not tested at all
  public class RegexBasedTerminal : Terminal {
    public RegexBasedTerminal(string pattern) : base ("RegEx:{" + pattern + "}") {
      _expression = new Regex(pattern);
    }

    public Regex Expression {
      get { return _expression; }
    } Regex  _expression;

    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      Match m = _expression.Match(source.Text, source.Position);
      if (!m.Success) 
        return null;
      source.Position += m.Length + 1;
      string text = source.GetLexeme();
      return new Token(this, source.TokenStart, text);
    }

  }//class




}//namespace

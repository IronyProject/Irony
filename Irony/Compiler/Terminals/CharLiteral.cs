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

namespace Irony.Compiler {
  //Need some work; not tested at all 
  public class CharLiteral : StringLiteral {
    public CharLiteral() : this("CharLiteral") { }
    public CharLiteral(string name)
      : base(name) {
      StartSymbol = "'";
      EndSymbol = "'";
    }
    public CharLiteral(string name, string startSymbol, string endSymbol)  : base(name, startSymbol, endSymbol) {
    }

    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      Token tkn = base.TryMatch(context, source);
      if (tkn == null) return null;
      string sv = (string)tkn.Value;
      if (!string.IsNullOrEmpty(sv))
        tkn.Value = sv[0];
      return tkn;
    }

  }//class

}

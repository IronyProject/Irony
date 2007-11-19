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
  //This terminal allows to declare a set of constants in the input language
  public class ConstantsTable : Dictionary<string, object> { }
  public class ConstantSetTerminal : Terminal {
    public ConstantSetTerminal() : this("Constants") { }
    public ConstantSetTerminal(string name)
      : base(name) {
    }
    public readonly ConstantsTable Table = new ConstantsTable();
    public void Add(string lexeme, object value) {
      this.Table[lexeme] = value;
    }
    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      int start = source.Position;
      string buffer = source.Text;
      foreach (string symbol in Table.Keys) {
        if (start + symbol.Length > buffer.Length) continue;
        if (buffer.IndexOf(symbol, start, symbol.Length) != start) continue;
        Token tkn = new Token(this, source.TokenStart, symbol, Table[symbol]);
        source.Position += symbol.Length;
        return tkn;
      }
      return null;
    }
    public override IList<string> GetPrefixes() {
      string[] array = new string[Table.Count];
      Table.Keys.CopyTo(array, 0);
      return array;
    }

  }//class  



}

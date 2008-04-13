#region License
/* **********************************************************************************
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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using NUnit.Framework.Constraints;
using Irony.Compiler;

namespace Irony.Tests {
  public class TerminalTestsBase {
    protected CompilerContext _context;
    protected Terminal _terminal;
    protected Token _token;

    [SetUp]
    public void Setup() {
      _context = CompilerContext.CreateDummy();
    }
    protected void InitTerminal() {
      _terminal.Init(_context.Compiler.Grammar);
    }
    //Utilities
    public void TryMatch(string input) {
      SourceFile source = new SourceFile(input, "test");
      _token = _terminal.TryMatch(_context, source);
    }
    public void CheckType(Type type) {
      Assert.IsNotNull(_token, "TryMatch returned null, while token was expected.");
      Type vtype = _token.Value.GetType();
      Assert.That(vtype == type, "Invalid target type, expected " + type.ToString() + ", found:  " + vtype);
    }

  }//class
}//namespace

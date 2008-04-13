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

  // The purpose of this class is to provide a container for information shared 
  // between parser, scanner and token filters.
  // Developers can extend this class to add language-specific properties or methods.
  public class CompilerContext {
    public readonly LanguageCompiler Compiler;
    public readonly SyntaxErrorList Errors = new SyntaxErrorList();
    public readonly Dictionary<string, object> Values = new Dictionary<string, object>();

    public CompilerContext(LanguageCompiler compiler) {
      this.Compiler = compiler;
    }
    public void AddError(SourceLocation location, string message, ParserState state) {
      if (Errors.Count < 20) //just for now, 20 is max
        Errors.Add(new SyntaxError(location, message, state));
    }
    public void AddError(SourceLocation location, string message) {
      this.AddError(location, message, null);
    }

    //Used in unit tests
    public static CompilerContext CreateDummy() {
      CompilerContext ctx = new CompilerContext(LanguageCompiler.CreateDummy());
      return ctx; 
    }
  }//class

}

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
using Irony.Runtime;

namespace Irony.Compiler {

  public enum CompilerOptions {
    GrammarDebugging = 0x01,
    CollectTokens = 0x02, //Collect all tokens in CompilerContext.Tokens collection
    MatchBraces = 0x04,   //link all open/closing brace tokens
    AnalyzeCode = 0x10,   //run code analysis; effective only in Module mode
  }
  public enum CompileMode {
    Module,       //default, continuous input file
    Line,         // support for VS integration mode, syntax highlighting
    //ConsoleInput, //line-by-line from console
  }

  // The purpose of this class is to provide a container for information shared 
  // between parser, scanner and token filters.
  // Developers can extend this class to add language-specific properties or methods.
  public class CompilerContext {
    public CompilerOptions Options;
    public CompileMode Mode = CompileMode.Module;
    public readonly LanguageCompiler Compiler;
    public readonly SyntaxErrorList Errors = new SyntaxErrorList();
    public readonly Dictionary<string, object> Values = new Dictionary<string, object>();
    public readonly LanguageRuntime Runtime;
    public int MaxErrors = 20;
    //Tokens consumed by parser are added to this collection, but only if CompilerOptions.RetainTokens flag is set
    public readonly TokenList Tokens = new TokenList(); 

    #region constructors and factory methods
    public CompilerContext(LanguageCompiler compiler) {
      this.Compiler = compiler;
      this.Runtime = compiler.Grammar.CreateRuntime();
#if DEBUG
      Options |= CompilerOptions.GrammarDebugging;
#endif
    }
    //Used in unit tests
    public static CompilerContext CreateDummy() {
      CompilerContext ctx = new CompilerContext(LanguageCompiler.CreateDummy());
      return ctx;
    }
    #endregion

    public bool OptionIsSet(CompilerOptions option) {
      return (Options & option) != 0;
    }

    #region Error handling
    public Token CreateErrorToken(SourceLocation location, string content) {
      return Token.Create(Grammar.SyntaxError, this, location, content);
    }
    public Token CreateErrorTokenAndReportError(SourceLocation location, string content, string message, params object[] args) {
      ReportError(location, message, args);
      Token result = Token.Create(Grammar.SyntaxError, this, location, content);
      return result; 
    }
    public void ReportError(SourceLocation location, string message, params object[] args) {
      if (Errors.Count >= MaxErrors) return;
      if (args != null && args.Length > 0)
        message = string.Format(message, args);
      Errors.Add(new SyntaxError(location, message));
    }
    #endregion

  }//class

}

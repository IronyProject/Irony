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
using System.Runtime.InteropServices;

namespace Irony.Compiler {

  public enum CompilerOptions {
    GrammarDebugging = 0x01,
    CollectTokens = 0x02, //Collect all tokens in CompilerContext.Tokens collection
    MatchBraces = 0x04,   //link all open/closing brace tokens
    AnalyzeCode = 0x10,   //run code analysis; effective only in Module mode
  }
  public enum CompileMode {
    Module,       //default, continuous input file
    VsLineScan,         // line-by-line scanning in VS integration for syntax highlighting
    //ConsoleInput, //line-by-line from console
  }
  // A struct used for packing/unpacking ScannerState int value; used for VS integration.
  [StructLayout(LayoutKind.Explicit)]
  public struct VsScannerStateMap {
    [FieldOffset(0)]
    public int Value;
    [FieldOffset(0)]
    public byte TokenKind;   //Registered kind of the multi-line token
    [FieldOffset(1)]
    public byte Data1;
    [FieldOffset(2)]
    public short Data2;
  }//struct


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
    //State variable used in line scanning mode for VS integration; when Terminal produces incomplete token, it sets 
    // this state to non-zero value; this value identifies this terminal as the one who will continue scanning when
    // it resumes, and the terminal's internal state when there may be several types of multi-line tokens for one terminal.
    // For ex., there maybe several types of string literal like in Python. 
    public VsScannerStateMap ScannerState; 

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

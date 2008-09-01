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
using System.Diagnostics;

namespace Irony.Compiler {

  public enum CodeAnalysisPhase {
    Init,
    AssignScopes,
    Allocate,    //Allocating local variables
    Binding,     //Binding variable references to variable locations - transforming refs into lex addresses
    Optimization,
    MarkTailCalls,
  }

  public class CodeAnalysisArgs {
    public readonly CompilerContext Context;
    public CodeAnalysisPhase Phase;
    public bool SkipChildren;
    public CodeAnalysisArgs(CompilerContext context) {
      Context = context;
      Phase = CodeAnalysisPhase.Init;
    }
  }

  
  public class LanguageCompiler {
    public LanguageCompiler(Grammar grammar) {
      Grammar = grammar;
      Options = grammar.Options;
      grammar.Prepare();
      ScannerControlData scannerData = new ScannerControlData(grammar);
      Scanner = new Scanner(scannerData);
      Parser = new Lalr.Parser(Grammar); 
    }
    //Used in unit tests
    public static LanguageCompiler CreateDummy() {
      LanguageCompiler compiler = new LanguageCompiler(new Grammar());
      return compiler;
    }

    public readonly Grammar Grammar;
    public readonly LanguageOptions Options; 
    public readonly Scanner Scanner;
    public readonly IParser Parser;

    //TODO - remove this
    public Lalr.Parser LalrParser { get { return Parser as Lalr.Parser; } }

    public long CompileTime  {
      [DebuggerStepThrough]
      get {return _compileTime;}
    } long  _compileTime;

    public CompilerContext Context  {
      [DebuggerStepThrough]
      get { return _context; }
    } CompilerContext  _context;

    public AstNode Parse(string source) {
      return Parse(new CompilerContext(this), new SourceFile(source, "Source"));
    }

    public AstNode Parse(CompilerContext context, SourceFile source) {
      _context = context;
      int start = Environment.TickCount;
      Scanner.Prepare(context, source);
      IEnumerable<Token> tokenStream = Scanner.BeginScan();
      //chain all token filters
      foreach (TokenFilter filter in Grammar.TokenFilters) {
        tokenStream = filter.BeginFiltering(context, tokenStream);
      }
      //finally, parser takes token stream and produces root Ast node
      AstNode rootNode = Parser.Parse(context, tokenStream);
      _compileTime = Environment.TickCount - start;
      return rootNode;
    }//method

    public void AnalyzeCode(AstNode astRoot, CompilerContext context) {
      RunAnalysisPhases(astRoot, context,
           CodeAnalysisPhase.Init, CodeAnalysisPhase.AssignScopes, CodeAnalysisPhase.Allocate,
           CodeAnalysisPhase.Binding, CodeAnalysisPhase.MarkTailCalls, CodeAnalysisPhase.Optimization);
    }

    private void RunAnalysisPhases(AstNode astRoot, CompilerContext context, params CodeAnalysisPhase[] phases) {
      CodeAnalysisArgs args = new CodeAnalysisArgs(context);
      foreach (CodeAnalysisPhase phase in phases) {
        switch (phase) {
          case CodeAnalysisPhase.AssignScopes:
            astRoot.Scope = new Scope(astRoot, null);
            break;
          
          case CodeAnalysisPhase.MarkTailCalls:
            if (!Options.TailRecursive) continue;//foreach
            astRoot.Flags |= AstNodeFlags.IsTail;
            break;
        }//switch
        args.Phase = phase;
        astRoot.OnCodeAnalysis(args);
      }//foreach phase
    }//method
  

  }//class

}//namespace

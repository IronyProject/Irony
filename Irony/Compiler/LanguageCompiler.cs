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

  public class LanguageCompiler {
    public LanguageCompiler(Grammar grammar) {
      Grammar = grammar;
      grammar.Init();
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
    public readonly Scanner Scanner;
    public readonly IParser Parser;

    //TODO - remove this
    public Lalr.Parser LalrParser { get { return Parser as Lalr.Parser; } }

    public long CompileTime  {
      [DebuggerStepThrough]
      get {return _compileTime;}
    } long  _compileTime;

    public AstNode Parse(string source) {
      return Parse(new CompilerContext(this), new SourceFile(source, "Source"));
    }

    public AstNode Parse(CompilerContext context, SourceFile source) {
      Scanner.Prepare(context, source);
      context.Tokens.Clear(); 
      //If we need to match braces then we need to ensure that we have BraceMatchFilter
      if (context.OptionIsSet(CompilerOptions.MatchBraces)) 
        EnsureBraceMatchFilter(); 
      IEnumerable<Token> tokenStream = Scanner.BeginScan();
      //chain all token filters
      foreach (TokenFilter filter in Grammar.TokenFilters) {
        tokenStream = filter.BeginFiltering(context, tokenStream);
      }
      //finally, parser takes token stream and produces root Ast node
      int start = Environment.TickCount;
      AstNode rootNode = Parser.Parse(context, tokenStream);
      _compileTime = Environment.TickCount - start;
      if (context.Errors.Count > 0)
        context.Errors.Sort(SyntaxErrorList.ByLocation);
      if (rootNode != null && context.OptionIsSet(CompilerOptions.AnalyzeCode))
        AnalyzeCode(rootNode, context); 
      return rootNode;
    }//method

    private void EnsureBraceMatchFilter() {
      foreach (TokenFilter filter in Grammar.TokenFilters)
        if (filter is BraceMatchFilter) return;
      Grammar.TokenFilters.Add(new BraceMatchFilter()); 
    }
    public void AnalyzeCode(AstNode astRoot, CompilerContext context) {
      RunAnalysisPhases(astRoot, context,
           CodeAnalysisPhase.Init, CodeAnalysisPhase.AssignScopes, CodeAnalysisPhase.Allocate,
           CodeAnalysisPhase.Binding, CodeAnalysisPhase.MarkTailCalls, CodeAnalysisPhase.Optimization);
      //sort errors if there are any
      if (context.Errors.Count > 0)
        context.Errors.Sort(SyntaxErrorList.ByLocation);
    }

    private void RunAnalysisPhases(AstNode astRoot, CompilerContext context, params CodeAnalysisPhase[] phases) {
      CodeAnalysisArgs args = new CodeAnalysisArgs(context);
      foreach (CodeAnalysisPhase phase in phases) {
        switch (phase) {
          case CodeAnalysisPhase.AssignScopes:
            astRoot.Scope = new Scope(astRoot, null);
            break;
          
          case CodeAnalysisPhase.MarkTailCalls:
            if (!Grammar.FlagIsSet(LanguageFlags.TailRecursive)) continue;//foreach loop - don't run the phase
            astRoot.Flags |= AstNodeFlags.IsTail;
            break;
        }//switch
        args.Phase = phase;
        astRoot.OnCodeAnalysis(args);
      }//foreach phase
    }//method
  

  }//class

}//namespace

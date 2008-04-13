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
  //Root compiler class
  public class LanguageCompiler {
    public LanguageCompiler(Grammar grammar) {
      Grammar = grammar;
      long startTime = Environment.TickCount;
      GrammarDataBuilder bld = new GrammarDataBuilder(grammar);
      bld.Build();
      InitTime = Environment.TickCount - startTime;
      Data = bld.Data;
      Parser = new Parser(Data); 
      Scanner = new Scanner(Data);
    }
    public LanguageCompiler(GrammarData data) {
      Data = data;
      Grammar = data.Grammar;
      Parser = new Parser(Data);
      Scanner = new Scanner(Data);
    }

    //Used in unit tests
    public static LanguageCompiler CreateDummy() {
      GrammarData data = new GrammarData();
      data.Grammar = new Grammar();
      LanguageCompiler compiler = new LanguageCompiler(data);
      return compiler;
    }

    public readonly Grammar Grammar;
    public readonly GrammarData Data;
    public readonly Scanner Scanner;
    public readonly Parser Parser;
    public readonly long InitTime;

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
    
  }//class

}//namespace

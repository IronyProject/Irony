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
using Irony.Compiler;

namespace Irony.Compiler {
  //Root compiler class
  public class LanguageCompiler {
    public LanguageCompiler(Grammar grammar) {
      Grammar = grammar;
      long startTime = Environment.TickCount;
      GrammarDataBuilder bld = new GrammarDataBuilder();
      Data = bld.Build(grammar);
      Parser = new Parser(Data); 
      Scanner = new Scanner(Data);
      InitTime = Environment.TickCount - startTime;
    }

    public readonly Grammar Grammar;
    public readonly GrammarData Data;
    public readonly Scanner Scanner;
    public readonly Parser Parser;
    public readonly long InitTime;

    public long CompileTime  {
      get {return _compileTime;}
    } long  _compileTime;

    public CompilerContext Context  {
      get {return _context;}
    } CompilerContext  _context;

    public AstNode Parse(string source) {
      return Parse(new CompilerContext(this), new SourceFile(source, "Source"));
    }

    public AstNode Parse(CompilerContext context, SourceFile source) {
      _context = context;
      int start = Environment.TickCount;
      IEnumerable<Token> tokenStream = Scanner.BeginScan(context, source);
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

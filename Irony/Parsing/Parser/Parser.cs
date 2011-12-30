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
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Irony.Parsing {
  //Parser class represents combination of scanner and LALR parser (CoreParser)
  public class Parser {
    public readonly LanguageData Language; 
    public readonly CoreParser CoreParser;
    public readonly Scanner Scanner;
    public ParsingContext Context { get; internal set; }
    public readonly NonTerminal Root;
    // Either language root or initial state for parsing snippets - like Ruby's expressions in strings : "result= #{x+y}"  
    internal readonly ParserState InitialState; 

    public Parser(Grammar grammar) : this (new LanguageData(grammar)) { }
    public Parser(LanguageData language) : this(language, null)  {}
    public Parser(LanguageData language, NonTerminal root) {
      Language = language;
      Context = new ParsingContext(this);
      Scanner = new Scanner(this);
      CoreParser = new CoreParser(this);
      Root = root; 
      if(Root == null) {
        Root = Language.Grammar.Root;
        InitialState = Language.ParserData.InitialState;
      } else {
        if(Root != Language.Grammar.Root && !Language.Grammar.SnippetRoots.Contains(Root))
          throw new Exception(string.Format(Resources.ErrRootNotRegistered, root.Name));
        InitialState = Language.ParserData.InitialStates[Root]; 
      }
    }

    internal void Reset() {
      Context.Reset(); 
      CoreParser.Reset();
      Scanner.Reset(); 
    }


    public ParseTree Parse(string sourceText) {
      return Parse(sourceText, "Source");
    }

    public ParseTree Parse(string sourceText, string fileName) {
      SourceLocation loc = default(SourceLocation); 
      if (Context.Status == ParserStatus.AcceptedPartial){
        var oldLoc = Context.Source.Location;
        loc = new SourceLocation(oldLoc.Position, oldLoc.Line + 1, 0);
      } else {
        Reset();
      }
      Context.Source = new SourceStream(sourceText, this.Language.Grammar.CaseSensitive, Context.TabWidth, loc); 
      Context.CurrentParseTree = new ParseTree(sourceText, fileName);
      Context.Status = ParserStatus.Parsing;
      var sw = new Stopwatch();
      sw.Start(); 
      CoreParser.Parse();
      //Set Parse status
      var parseTree = Context.CurrentParseTree;
      bool hasErrors = parseTree.HasErrors();
      if (hasErrors)
        parseTree.Status = ParseTreeStatus.Error;
      else if (Context.Status == ParserStatus.AcceptedPartial)
        parseTree.Status = ParseTreeStatus.Partial;
      else
        parseTree.Status = ParseTreeStatus.Parsed;
      //Build AST if no errors and AST flag is set
      bool createAst = Language.Grammar.LanguageFlags.IsSet(LanguageFlags.CreateAst);
      if (createAst && !hasErrors)
        Language.Grammar.BuildAst(Language, parseTree);
      //Done; record the time
      sw.Stop();
      parseTree.ParseTimeMilliseconds = sw.ElapsedMilliseconds;
      if (parseTree.ParserMessages.Count > 0)
        parseTree.ParserMessages.Sort(LogMessageList.ByLocation);
      return parseTree;
    }

    public ParseTree ScanOnly(string sourceText, string fileName) {
      Context.CurrentParseTree = new ParseTree(sourceText, fileName);
      Context.Source = new SourceStream(sourceText, Language.Grammar.CaseSensitive, Context.TabWidth);
      while (true) {
        var token = Scanner.GetToken();
        if (token == null || token.Terminal == Language.Grammar.Eof) break;
      }
      return Context.CurrentParseTree;
    }

  
  }//class
}//namespace

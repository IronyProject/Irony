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
      if (Context.Status != ParserStatus.AcceptedPartial)
        Reset();
      var createAst = Language.Grammar.LanguageFlags.IsSet(LanguageFlags.CreateAst);
      var keepLineNumbering = Context.Status == ParserStatus.AcceptedPartial;
      Context.SourceStream.SetText(sourceText, 0, keepLineNumbering);
      Context.CurrentParseTree = new ParseTree(sourceText, fileName);
      Context.Status = ParserStatus.Parsing;
      var sw = new Stopwatch();
      sw.Start(); 
      CoreParser.Parse();
      sw.Stop();
      Context.CurrentParseTree.ParseTimeMilliseconds = sw.ElapsedMilliseconds;
      UpdateParseTreeStatus(); 
      return Context.CurrentParseTree;
    }

    private void UpdateParseTreeStatus() {
      var parseTree = Context.CurrentParseTree;
      if (parseTree.ParserMessages.Count > 0)
        parseTree.ParserMessages.Sort(ParserMessageList.ByLocation);
      if (parseTree.HasErrors())
        parseTree.Status = ParseTreeStatus.Error;
      else if (Context.Status == ParserStatus.AcceptedPartial)
        parseTree.Status = ParseTreeStatus.Partial;
      else
        parseTree.Status = ParseTreeStatus.Parsed;
    }

    public ParseTree ScanOnly(string sourceText, string fileName) {
      Context.CurrentParseTree = new ParseTree(sourceText, fileName);
      Context.SourceStream.SetText(sourceText, 0, false);
      while (true) {
        var token = Scanner.GetToken();
        if (token == null || token.Terminal == Language.Grammar.Eof) break;
      }
      return Context.CurrentParseTree;
    }

  
  }//class
}//namespace

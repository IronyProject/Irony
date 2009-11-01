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
using System.Linq;
using System.Text;

namespace Irony.Parsing {
  //Parser class represents combination of scanner and LALR parser (CoreParser)
  public class Parser {
    public readonly LanguageData Language; 
    public readonly CoreParser CoreParser;
    public readonly Scanner Scanner;
    public ParsingContext Context { get; internal set; }

    public Parser(Grammar grammar) : this (new LanguageData(grammar)) { }
    public Parser(LanguageData language) {
      Language = language;
      Context = new ParsingContext(this);
      Scanner = new Scanner(this);
      CoreParser = new CoreParser(this);
    }

    internal void OnStatusChanged(ParserStatus oldStatus) {
      CoreParser.OnStatusChanged(oldStatus);
      Scanner.OnStatusChanged(oldStatus); 
    }


    [Obsolete("This method overload is deprecated. Use another overload without context parameter and access ParsingContext through Parser.Context property.")]
    public ParseTree Parse(ParsingContext context, string sourceText, string fileName) {
      return Parse(sourceText, fileName);
    }

    public ParseTree Parse(string sourceText) {
      return Parse(sourceText, "<Source>");
    }

    public ParseTree Parse(string sourceText, string fileName) {
      AutoReset();
      Context.SourceStream.SetText(sourceText, 0, Context.Status == ParserStatus.AcceptedPartial);
      Context.CurrentParseTree = new ParseTree(sourceText, fileName);
      Context.Status = ParserStatus.Parsing;
      int start = Environment.TickCount;
      CoreParser.Parse();
      Context.CurrentParseTree.ParseTime = Environment.TickCount - start;
      UpdateParseTreeStatus(); 
      return Context.CurrentParseTree;
    }

    private void AutoReset() {
      switch (Context.Status) {
        case ParserStatus.AcceptedPartial:
          break;
        default: //for everything else Initialize
          Context.Status = ParserStatus.Init;
          break; 
      }
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {
  //Parser class represents combination of scanner and LALR parser (CoreParser)
  public class Parser { 
    public readonly LanguageData Language; 
    public readonly Scanner Scanner;
    public readonly CoreParser CoreParser;

    public Parser(Grammar grammar) {
      Language = new LanguageData(grammar);
      Scanner = new Scanner(Language.ScannerData);
      CoreParser = new CoreParser(Language.ParserData, Scanner);
    }

    public Parser(LanguageData language) {
      Language = language; 
      Scanner = new Scanner(Language.ScannerData);
      CoreParser = new CoreParser(Language.ParserData, Scanner); 
    }

    public ParseTree Parse(string sourceText) {
      return Parse(new ParsingContext(this), sourceText, "<source>"); 
    }
    public ParseTree Parse(ParsingContext context, string sourceText, string fileName) {
      context.CurrentParseTree = new ParseTree(sourceText, fileName);
      Scanner.SetSource(sourceText);
      Scanner.BeginScan(context);
      CoreParser.Parse(context);
      if (context.CurrentParseTree.Errors.Count > 0)
        context.CurrentParseTree.Errors.Sort(SyntaxErrorList.ByLocation);
      return context.CurrentParseTree;
    }

    public ParseTree ScanOnly(string sourceText, string fileName) {
      var context = new ParsingContext(this);
      context.CurrentParseTree = new ParseTree(sourceText, fileName);
      Scanner.SetSource(sourceText);
      Scanner.BeginScan(context);
      while (true) {
        var token = Scanner.GetToken();
        if (token == null || token.Terminal == Language.Grammar.Eof) break;
      }
      return context.CurrentParseTree;
    }

  
  }//class
}//namespace

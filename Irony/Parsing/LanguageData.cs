using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {
  public class LanguageData {
    public readonly Grammar Grammar;
    public readonly GrammarData GrammarData; 
    public readonly ParserData ParserData;
    public readonly ScannerData ScannerData;
    public readonly GrammarErrorList Errors = new GrammarErrorList(); 
    public GrammarErrorLevel ErrorLevel = GrammarErrorLevel.NoError;

    public LanguageData(Grammar grammar) {
      Grammar = grammar;
      GrammarData = new GrammarData(this);
      ParserData = new ParserData(this);
      ScannerData = new ScannerData(this); 
    }
    public bool CanParse() {
      return ErrorLevel < GrammarErrorLevel.Error;
    }
  }//class
}//namespace

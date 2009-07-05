using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing.Construction {
  internal class LanguageDataBuilder {

    internal LanguageData Language;
    Grammar _grammar;
    private ParserStateHash _stateHash = new ParserStateHash();

    public LanguageDataBuilder(LanguageData language) {
      Language = language;
      _grammar = Language.Grammar;
    }

    public bool Build() {
      try {
        if (_grammar.Root == null)
          Language.Errors.AddAndThrow(GrammarErrorLevel.Error, null, "Root property of the grammar is not set.");
        var gbld = new GrammarDataBuilder(Language);
        gbld.Build();
        //Just in case grammar author wants to customize something...
        _grammar.OnGrammarDataConstructed(Language);
        var sbld = new ScannerDataBuilder(Language);
        sbld.Build();
        var pbld = new ParserDataBuilder(Language);
        pbld.Build();
        Validate();
        //call grammar method, a chance to tweak the automaton
        _grammar.OnParserDataConstructed(Language);
        return true;
      } catch (GrammarErrorException) {
        return false; //grammar error should be already added to Language.Errors collection
      } finally {
        Language.ErrorLevel = Language.Errors.GetMaxLevel();
      }

    }

    #region Language Data Validation
    private void Validate() {

    }//method
    #endregion

  
  }//class
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing.Construction { 
  internal class ScannerDataBuilder {
    LanguageData _language;
    Grammar _grammar;
    GrammarData _grammarData; 
    ScannerData _data; 

    internal ScannerDataBuilder(LanguageData language) {
      _language = language;
      _grammar = _language.Grammar;
      _grammarData = language.GrammarData;
    }

    internal void Build() {
      _data = _language.ScannerData;
      _data.LineTerminators = _grammar.LineTerminators.ToCharArray();
      _data.ScannerRecoverySymbols = _grammar.WhitespaceChars + _grammar.Delimiters;
      InitMultilineTerminalsList();
      BuildTerminalsLookupTable();
      InitTokenFilters();
    }

    private void InitMultilineTerminalsList() {
      foreach (var terminal in _grammarData.Terminals) {
        if (terminal.OptionIsSet(TermOptions.IsMultiline)) {
          _data.MultilineTerminals.Add(terminal);
          terminal.MultilineIndex = (byte)(_data.MultilineTerminals.Count);
        }
      }
    }

    private void BuildTerminalsLookupTable() {
      _data.TerminalsLookup.Clear();
      _data.FallbackTerminals.AddRange(_grammar.FallbackTerminals);
      foreach (Terminal term in _grammarData.Terminals) {
        IList<string> prefixes = term.GetFirsts();
        if (prefixes == null || prefixes.Count == 0) {
          if (!_data.FallbackTerminals.Contains(term))
            _data.FallbackTerminals.Add(term);
          continue; //foreach term
        }
        //Go through prefixes one-by-one
        foreach (string prefix in prefixes) {
          if (string.IsNullOrEmpty(prefix)) continue;
          //Calculate hash key for the prefix
          char hashKey = prefix[0];
          if (!_grammar.CaseSensitive)
            hashKey = char.ToLower(hashKey);
          TerminalList currentList;
          if (!_data.TerminalsLookup.TryGetValue(hashKey, out currentList)) {
            //if list does not exist yet, create it
            currentList = new TerminalList();
            _data.TerminalsLookup[hashKey] = currentList;
          }
          //add terminal to the list
          if (!currentList.Contains(term))
            currentList.Add(term);
        }
      }//foreach term
      //Sort all terminal lists by reverse priority, so that terminal with higher priority comes first in the list
      foreach (TerminalList list in _data.TerminalsLookup.Values)
        if (list.Count > 1)
          list.Sort(Terminal.ByPriorityReverse);
    }//method

    private void InitTokenFilters() {
      _data.TokenFilters.AddRange(_grammarData.Grammar.TokenFilters);
      //check if we need brace match token filter
      bool needBraceMatchFilter = false;
      foreach(var term in _grammarData.Terminals)
        if (term.OptionIsSet(TermOptions.IsBrace)) {
          needBraceMatchFilter = true;
          break; 
        }
      if (needBraceMatchFilter)
        EnsureBraceMatchFilter(_data); 
      //initialize filters
      foreach (var filter in _data.TokenFilters)
        filter.Init(_grammarData);
    }
    private static void EnsureBraceMatchFilter(ScannerData _data) {
      foreach (TokenFilter filter in _data.TokenFilters)
        if (filter is BraceMatchFilter) return;
      _data.TokenFilters.Add(new BraceMatchFilter());
    }

  }//class

}

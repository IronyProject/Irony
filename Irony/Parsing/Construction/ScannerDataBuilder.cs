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
using System.Globalization;
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
      _data.LineTerminatorsArray = _grammar.LineTerminators.ToCharArray();
      InitMultilineTerminalsList();
      BuildTerminalsLookupTable();
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
      foreach (Terminal term in _grammarData.Terminals) {
        IList<string> firsts = term.GetFirsts();
        if (firsts == null || firsts.Count == 0) {
          if (!_grammar.FallbackTerminals.Contains(term))
            _grammar.FallbackTerminals.Add(term);
          continue; //foreach term
        }
        //Go through prefixes one-by-one
        foreach (string prefix in firsts) {
          if (string.IsNullOrEmpty(prefix)) continue;
          //Calculate hash key for the prefix
          char hashKey = prefix[0];
          if(_grammar.CaseSensitive)
            AddTerminalToLookup(hashKey, term);
          else {
            AddTerminalToLookup(char.ToLower(hashKey), term);
            AddTerminalToLookup(char.ToUpper(hashKey), term);
          }//if
        }//foreach prefix in firsts
      }//foreach term

      //Now add Fallback terminals to every list, then sort lists by reverse priority
      // so that terminal with higher priority comes first in the list
      foreach(TerminalList list in _data.TerminalsLookup.Values) {
        foreach(var fterm in _grammar.FallbackTerminals)
          if (!list.Contains(fterm))
            list.Add(fterm);
        if(list.Count > 1)
          list.Sort(Terminal.ByPriorityReverse);
      }//foreach list
 
    }//method

    private void AddTerminalToLookup(char hashKey, Terminal term) {
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

  }//class

}//namespace

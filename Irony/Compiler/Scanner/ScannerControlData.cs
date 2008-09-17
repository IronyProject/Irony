using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Compiler {

  public class TerminalLookupTable : Dictionary<char, TerminalList> { }

  public class ScannerControlData {
    public readonly Grammar Grammar;
    public readonly TerminalList Terminals = new TerminalList();
    public readonly TerminalLookupTable TerminalsLookup = new TerminalLookupTable(); //hash table for fast terminal lookup by input char
    public readonly TerminalList FallbackTerminals = new TerminalList(); //terminals that have no explicit prefixes
    public readonly string ScannerRecoverySymbols = "";
    public readonly char[] LineTerminators; //used for line counting

    public ScannerControlData(Grammar grammar) {
      Grammar = grammar;
      if (!Grammar.Prepared)
        Grammar.Prepare();
      LineTerminators = grammar.LineTerminators.ToCharArray();
      ScannerRecoverySymbols = grammar.WhitespaceChars + grammar.Delimiters;
      ExtractTerminalsFromGrammar();
      BuildTerminalsLookupTable();
    }

    private void ExtractTerminalsFromGrammar() {
      Terminals.Clear();
      foreach (BnfTerm t in Grammar.AllTerms) {
        Terminal terminal = t as Terminal;
        if (terminal != null)
          Terminals.Add(terminal);
      }
      //Adjust case for Symbols for case-insensitive grammar (change keys to lowercase)
      if (!Grammar.CaseSensitive)
        AdjustCaseForSymbols();
      Terminals.Sort(Terminal.ByName);
    }
    
    private void AdjustCaseForSymbols() {
      foreach (Terminal term in Terminals)
        if (term is SymbolTerminal)
          term.Key = term.Key.ToLower();
    }

    private void BuildTerminalsLookupTable() {
      TerminalsLookup.Clear();
      FallbackTerminals.AddRange(Grammar.FallbackTerminals);
      foreach (Terminal term in Terminals) {
        IList<string> prefixes = term.GetFirsts();
        if (prefixes == null || prefixes.Count == 0) {
          if (!FallbackTerminals.Contains(term))
            FallbackTerminals.Add(term);
          continue; //foreach term
        }
        //Go through prefixes one-by-one
        foreach (string prefix in prefixes) {
          if (string.IsNullOrEmpty(prefix)) continue;
          //Calculate hash key for the prefix
          char hashKey = prefix[0];
          if (!Grammar.CaseSensitive)
            hashKey = char.ToLower(hashKey);
          TerminalList currentList;
          if (!TerminalsLookup.TryGetValue(hashKey, out currentList)) {
            //if list does not exist yet, create it
            currentList = new TerminalList();
            TerminalsLookup[hashKey] = currentList;
          }
          //add terminal to the list
          currentList.Add(term);
        }
      }//foreach term
      //Sort all terminal lists by reverse priority, so that terminal with higher priority comes first in the list
      foreach (TerminalList list in TerminalsLookup.Values)
        if (list.Count > 1)
          list.Sort(Terminal.ByPriorityReverse);
    }//method


  }


}

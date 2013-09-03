using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing.New {
  
  class TermLookupInfo {
    public Char FirstChar;
    public Char SecondChar;
    public string Prefix;
    public Terminal Terminal;
  }

  public class TerminalLookupByPrefix {
    List<Terminal> Terminals = new List<Terminal>();
    Dictionary<char, List<TermLookupInfo>> TermLookupTable =  new Dictionary<char,List<TermLookupInfo>>();

    public TerminalLookupByPrefix(LanguageData language) {
      var allTerms = language.GrammarData.Terminals;
      foreach(var term in allTerms) {
        var firsts = term.GetFirsts();
        if (firsts == null || firsts.Count == 0) continue; 
        // Check for key term (keywords and special symbols). Use only spec symbols, but exclude keywords
        var keyTerm = term as KeyTerm; 
        if (keyTerm != null && char.IsLetter(keyTerm.Text[0])) //if it starts with letter - it is keyword, so skip it
          continue; 
        AddTermLookupInfo(term, firsts); 
      } //foreach
    }//method

    private void AddTermLookupInfo(Terminal term, IList<string> firsts) {
      foreach (var prefix in firsts) {
        var termLkp = new TermLookupInfo() {Terminal = term, Prefix = prefix };
        termLkp.FirstChar = prefix[0];
        termLkp.SecondChar = prefix.Length > 1 ? prefix[1] : '\0';
        List<TermLookupInfo> lkpList;
        if (!TermLookupTable.TryGetValue(termLkp.FirstChar, out lkpList)) {
          lkpList = new List<TermLookupInfo>();
          TermLookupTable.Add(termLkp.FirstChar, lkpList);
        }
        lkpList.Add(termLkp); 
      }//foreach
    }

    internal List<TermLookupInfo> Lookup(char current) {
      List<TermLookupInfo> result; 
      if(TermLookupTable.TryGetValue(current, out result))
        return result; 
      return null; 
    }
  }//class

}//ns

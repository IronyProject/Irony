using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {
  // Sometimes language definition includes tokens that have no specific format, but are just "all text until some terminator character(s)";
  // FreeTextTerminal allows easy implementation of such language element.
  public class FreeTextLiteral : Terminal {
    public StringSet Terminators = new StringSet();
    public StringSet Firsts = new StringSet(); 
    public StringDictionary Escapes = new StringDictionary();
    private char[] _stopChars;

    public FreeTextLiteral(string name, params string[] terminators) : base(name, TokenCategory.Literal) {
      Terminators.UnionWith(terminators); 
    }//constructor

    public override IList<string> GetFirsts() {
      var result = new StringList();
      result.AddRange(Firsts);
      return result; 
    }
    public override void Init(GrammarData grammarData) {
      base.Init(grammarData);
      var stopChars = new CharHashSet();
      foreach (var key in Escapes.Keys)
        stopChars.Add(key[0]);
      foreach (var t in Terminators)
        stopChars.Add(t[0]);
      _stopChars = stopChars.ToArray();
    }

    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      string tokenText = string.Empty;
      while (true) {
        //Find next position
        var newPos = source.Text.IndexOfAny(_stopChars, source.PreviewPosition);
        if (newPos == -1) return null;
        tokenText += source.Text.Substring(source.PreviewPosition, newPos - source.PreviewPosition);
        source.PreviewPosition = newPos;
        //if it is escape, add escaped text and continue search
        if (CheckEscape(source, ref tokenText)) 
          continue;
        //check terminators
        if (CheckTerminators(source))
          break; //from while (true)        
      }
      return source.CreateToken(this, tokenText);
    }

    private bool CheckEscape(ISourceStream source, ref string tokenText) {
      foreach (var dictEntry in Escapes) {
        if (source.MatchSymbol(dictEntry.Key, false)) {
          source.PreviewPosition += dictEntry.Key.Length;
          tokenText += dictEntry.Value;
          return true; 
        }
      }//foreach
      return false; 
    }

    private bool CheckTerminators(ISourceStream source) {
      foreach(var term in Terminators)
        if (source.MatchSymbol(term, false))
          return true;
      return false; 
    }

  }//class

}//namespace 

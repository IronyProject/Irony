using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing.New {

  /*
 New terminals:
   CompilerDirective, macro definition
   SubLanguage (Linq inside c#, JavaScript inside HTML, c# inside Razor, expression in IfDirective)
      
 Phase 1: 
   Segmentation : string literals, comments, conditionals, macro definitions, special chars
 Phase 2:
   Mid-Processing : line analyzer, macro expander, sub-segment scanner, brace matcher, brace injector?
 Phase 3:
   Tokenizer: all other terminals
    
 SourceFilters:
  PrefixBasedScanner
  LineAnalyzer (with indents)
  Macro-expander
  Sub-grammar terminal
  Brace matcher / block analyzer - mostly for error recovery and intellisense
  Tokenizer (sequential scanner)

*/


  public interface IScanProcessor {
    void ProcessSource(ParsingContext context, SourceSegment segment);
  }

  public interface ITextSegmentScanner {
    IEnumerable<SourceSegment> Scan(ParsingContext context, TextSegment segment);
  }

  public class PrefixBasedScanner : ITextSegmentScanner {
    public LanguageData Language;
    TerminalLookupByPrefix _terminalLookup;

    public PrefixBasedScanner(LanguageData language) {
      Language = language;
      _terminalLookup = new TerminalLookupByPrefix(language); 
    }

    #region ISegmentScanner Members

    public IEnumerable<SourceSegment> Scan(ParsingContext context, TextSegment segment) {
      return null; 
    }//method

    #endregion

    private List<Terminal> FindTermsByPrefix(TextSegment segment) {
      return null; 
    }

  }//class

/*
  public class ScannerPipeline {
    public IEnumerable<Token> Scan(ParsingContext context, TextSegment source) {
      // 1. Initial prefix-based pre-scan
      var pScanner = new PrefixBasedScanner(context.Language);
      var segments = pScanner.Scan(context, source).ToList();
      // 2. Run filters
      foreach (var filter in context.Language.SourceFilters) {
        var newSegments = filter.Filter(context, segments);
        foreach (var segm in newSegments) {
          var tSegm = segm as TextSegment;
          if (tSegm != null && tSegm.IsScanned) {
            var subSegms = this.Scan(context, tSegm);
          } 

        }// foreach segm
      }
      //3. Return tokens
      foreach (var segm in segments) {
        var token = segm as Token;
        if (token != null) {
          yield return token;
          continue; 
        }
        //not token

      }


    } 
  }
  */
}

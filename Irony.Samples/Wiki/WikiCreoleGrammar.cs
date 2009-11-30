using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Irony.Parsing;

namespace Irony.Samples {
  //What does not work yet:
  // * Raw links, CamelCase words interpreted as links 
  // * Automatic paragraphs
  // * Linked images
  [Language("Wiki-Creole", "1.0", "Wiki/Creole markup grammar.")]
  public class WikiCreoleGrammar : Grammar {
    public WikiCreoleGrammar() {
      this.GrammarComments = 
@"A grammar for parsing Creole wiki files and transforming them into HTML 
http://www.wikicreole.org/";
      //Terminals
      var text = new WikiTextTerminal("text") { EscapeChar = '~' };
      var lineBreak = new WikiTagTerminal("lineBreak", WikiTermType.Element, @"\\", string.Empty) { OpenHtmlTag = "<br/>\n" }; 
      //Quote-like terminals
      var bold = new WikiTagTerminal("bold", WikiTermType.Format, "**", "strong"); 
      var italic = new WikiTagTerminal("italic",WikiTermType.Format, "//", "em"); 

      //Headings
      var h1 = new WikiTagTerminal("h1", WikiTermType.Heading, "=", "h1"); 
      var h2 = new WikiTagTerminal("h2", WikiTermType.Heading, "==", "h2"); 
      var h3 = new WikiTagTerminal("h3", WikiTermType.Heading, "===", "h3"); 
      var h4 = new WikiTagTerminal("h4", WikiTermType.Heading, "====", "h4"); 
      var h5 = new WikiTagTerminal("h5", WikiTermType.Heading, "=====", "h5"); 
      var h6 = new WikiTagTerminal("h6", WikiTermType.Heading, "======", "h6"); 

      //Bulletted lists
      var bl1 = new WikiTagTerminal("bl1", WikiTermType.List, "*", "li") { ContainerHtmlElementName = "ul" }; 
      var bl2 = new WikiTagTerminal("bl2", WikiTermType.List, "**", "li") { ContainerHtmlElementName = "ul" }; 
      var bl3 = new WikiTagTerminal("bl3", WikiTermType.List, "***", "li"){ ContainerHtmlElementName = "ul" }; 

      //Numbered lists
      var nl1 = new WikiTagTerminal("nl1", WikiTermType.List, "#", "li"){ ContainerHtmlElementName = "ol" }; 
      var nl2 = new WikiTagTerminal("nl2", WikiTermType.List, "##", "li"){ ContainerHtmlElementName = "ol" }; 
      var nl3 = new WikiTagTerminal("nl3", WikiTermType.List, "###", "li"){ ContainerHtmlElementName = "ol" }; 

      //Ruler
      var ruler = new WikiTagTerminal("ruler", WikiTermType.Heading, "----", "hr");

      //Image
      var image = new WikiBlockTerminal("image", WikiBlockType.Image, "{{", "}}", string.Empty);

      //Link 
      var link = new WikiBlockTerminal("link", WikiBlockType.Url, "[[", "]]", string.Empty);

      //Tables
      var tableHeading = new WikiTagTerminal("tableHeading", WikiTermType.Table, "|=", "th");
      var tableRow = new WikiTagTerminal("tableRow", WikiTermType.Table, "|", "td");
      
      //Block tags
      //TODO: implement full rules for one-line and multi-line nowiki element
      var nowiki = new WikiBlockTerminal("nowiki", WikiBlockType.EscapedText, "{{{", "}}}", "pre"); 

      //Non-terminals
      var wikiElement = new NonTerminal("wikiElement");
      var wikiText = new NonTerminal("wikiText"); 

      //Rules
      wikiElement.Rule = text | lineBreak | bold | italic 
        | h1 | h2 | h3 | h4 | h5 | h6  
        | bl1 | bl2 | bl3 
        | nl1 | nl2 | nl3 
        | ruler | image | nowiki | link  
        | tableHeading | tableRow 
        | NewLine;
      wikiText.Rule = MakeStarRule(wikiText, wikiElement); 

      this.Root =  wikiText; 
      this.WhitespaceChars = string.Empty;
      MarkTransient(wikiElement); 
      //We need to clear punctuation flag on NewLine, so it is not removed from parse tree
      NewLine.SetOption(TermOptions.IsPunctuation, false); 
      this.LanguageFlags |= LanguageFlags.DisableScannerParserLink | LanguageFlags.NewLineBeforeEOF | LanguageFlags.CanRunSample;
 
    }

    public override string RunSample(ParseTree parsedSample) {
      var converter = new WikiHtmlConverter();
      var html = converter.Convert(this, parsedSample.Tokens);
      var path = Path.Combine(Path.GetTempPath(), "@wikiSample.html");
      File.WriteAllText(path, html);
      System.Diagnostics.Process.Start(path);
      return html; 
    }

  }//class

}//namespace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Irony.Parsing;

namespace Irony.Samples {

  [Language("CodePlexWiki", "1.0", "Codeplex Wiki format grammar.")]
  public class CodeplexWikiGrammar : Grammar {
    public CodeplexWikiGrammar() {
      this.GrammarComments = 
@"A grammar for reading codeplex wiki files and transforming them into HTML 
http://codeplex.codeplex.com/wikipage?title=Wiki%20Formatting
http://wikiplex.codeplex.com
";
      //Terminals
      var text = new WikiTextTerminal("text");
      //Quote-like terminals
      var bold = new WikiTagTerminal("bold", WikiTermType.Format, "*", "b"); 
      var italic = new WikiTagTerminal("italic",WikiTermType.Format, "_", "i"); 
      var underline = new WikiTagTerminal("underline", WikiTermType.Format, "+", "u"); 
      var strike = new WikiTagTerminal("strike", WikiTermType.Format, "~~", "del"); 
      var super = new WikiTagTerminal("super", WikiTermType.Format, "^^", "sup"); 
      var sub = new WikiTagTerminal("sub", WikiTermType.Format, ",,", "sub"); 

      //Headings
      var h1 = new WikiTagTerminal("h1", WikiTermType.Heading, "!", "h1"); 
      var h2 = new WikiTagTerminal("h2", WikiTermType.Heading, "!!", "h2"); 
      var h3 = new WikiTagTerminal("h3", WikiTermType.Heading, "!!!", "h3"); 
      var h4 = new WikiTagTerminal("h4", WikiTermType.Heading, "!!!!", "h4"); 
      var h5 = new WikiTagTerminal("h5", WikiTermType.Heading, "!!!!!", "h5"); 
      var h6 = new WikiTagTerminal("h6", WikiTermType.Heading, "!!!!!!", "h6"); 

      //Ruler
      var ruler = new WikiTagTerminal("rule", WikiTermType.Heading, "----", "hr");// string.Empty) { OpenHtmlElement = "<hr/>" }; 

      //Bulletted lists
      var bl1 = new WikiTagTerminal("bl1", WikiTermType.List, "*", "li") { ContainerHtmlElementName = "ul" }; 
      var bl2 = new WikiTagTerminal("bl2", WikiTermType.List, "**", "li") { ContainerHtmlElementName = "ul" }; 
      var bl3 = new WikiTagTerminal("bl3", WikiTermType.List, "***", "li"){ ContainerHtmlElementName = "ul" }; 

      //Numbered lists
      var nl1 = new WikiTagTerminal("nl1", WikiTermType.List, "#", "li"){ ContainerHtmlElementName = "ol" }; 
      var nl2 = new WikiTagTerminal("nl2", WikiTermType.List, "##", "li"){ ContainerHtmlElementName = "ol" }; 
      var nl3 = new WikiTagTerminal("nl3", WikiTermType.List, "###", "li"){ ContainerHtmlElementName = "ol" }; 

      //Block tags
      var codeBlock = new WikiBlockTerminal("codeBlock", WikiBlockType.CodeBlock, "{{", "}}", "pre"); 
      var escapedBlock = new WikiBlockTerminal("escapedBlock", WikiBlockType.EscapedText, "{\"", "\"}", "pre"); 
      var anchor = new WikiBlockTerminal("anchor", WikiBlockType.Anchor, "{anchor:", "}", string.Empty);

      //Links
      var linkToAnchor = new WikiBlockTerminal("linkToAnchor", WikiBlockType.LinkToAnchor, "[#", "]", string.Empty);
      var url = new WikiBlockTerminal("url", WikiBlockType.Url, "[url:", "]", string.Empty);
      var fileLink = new WikiBlockTerminal("fileLink", WikiBlockType.FileLink, "[file:", "]", string.Empty);
      var image = new WikiBlockTerminal("image", WikiBlockType.Image, "[image:", "]", string.Empty);
      
      //Tables
      var tableHeading = new WikiTagTerminal("tableHeading", WikiTermType.Table, "||", "th");
      var tableRow = new WikiTagTerminal("tableRow", WikiTermType.Table, "|", "td");

      //Alignment, indents
      var leftAlignStart = new WikiTagTerminal("leftAlignStart", WikiTermType.Format, "<{", string.Empty)
                             { OpenHtmlTag = "<div style=\"text-align:left;float:left;\">"};
      var leftAlignEnd = new WikiTagTerminal("leftAlignEnd", WikiTermType.Format, "}<", string.Empty)
                             { OpenHtmlTag = "</div>"};
      var rightAlignStart = new WikiTagTerminal("rightAlignStart", WikiTermType.Format, ">{", string.Empty)
                             { OpenHtmlTag = "<div style=\"text-align:right;float:right;\">"};
      var rightAlignEnd = new WikiTagTerminal("rightAlignEnd", WikiTermType.Format, "}>", string.Empty)
                             { OpenHtmlTag = "</div>"};
      var indentOne = new WikiTagTerminal("indentOne", WikiTermType.Heading, ":", string.Empty)
                             { OpenHtmlTag = "<blockquote>", CloseHtmlTag = "</blockquote>" }; 
      var indentTwo = new WikiTagTerminal("indentTwo", WikiTermType.Heading, "::", string.Empty)
                             { OpenHtmlTag = "<blockquote><blockquote>", CloseHtmlTag = "</blockquote></blockquote>" }; 

      //Non-terminals
      var wikiElement = new NonTerminal("wikiElement");
      var wikiText = new NonTerminal("wikiText"); 

      //Rules
      wikiElement.Rule = text | bold | italic | strike | underline | super | sub 
        | h1 | h2 | h3 | h4 | h5 | h6 | ruler 
        | bl1 | bl2 | bl3 
        | nl1 | nl2 | nl3 
        | codeBlock | escapedBlock | anchor
        | linkToAnchor | url | fileLink | image
        | tableHeading | tableRow 
        | leftAlignStart | leftAlignEnd | rightAlignStart | rightAlignEnd | indentOne | indentTwo
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
      var converter = new CodeplexWikiConverter();
      var html = converter.Convert(this, parsedSample.Tokens);
      var path = Path.Combine(Path.GetTempPath(), "@wikiSample.html");
      File.WriteAllText(path, html);
      System.Diagnostics.Process.Start(path);
      return html; 
    }

  }//class

  public class CodeplexWikiConverter {
    private enum TableStatus {
      None, 
      Table, 
      Cell
    }
    internal class FlagTable : Dictionary<Terminal, bool>{}
    internal class WikiTermStack : Stack<WikiTerminalBase> { }

    StringBuilder _output;
    FlagTable _flags = new FlagTable();  
    WikiTermStack _openLists = new WikiTermStack(); 
    bool _atLineStart = true; 
    WikiTerminalBase _currentHeader = null; 
    //Table flags
    bool _insideTable = false; 
    bool _insideCell = false; 
    WikiTagTerminal _lastTableTag; 

    private int CurrentListLevel {
      get { return _openLists.Count == 0 ? 0 : _openLists.Peek().OpenTag.Length; }
    }

    private string EncodeHtml(string text) {
      return System.Web.HttpUtility.HtmlEncode(text);
    }

    public string Convert(Grammar grammar, TokenList tokens) {
      _output = new StringBuilder(8192); //8k
      _output.AppendLine("<html>"); 
      
      foreach(var token in tokens) {
        var term = token.Terminal;
        if(_atLineStart || term == grammar.Eof) {
          CheckOpeningClosingLists(token);
          CheckTableStatus(token); 
          if (term == grammar.Eof) break; 
        }
        if (term is WikiTerminalBase) 
          ProcessWikiElement(token); 
        else if(term == grammar.NewLine) {
          ProcessNewLine(token);
        } else //non-wike element and not new line 
          _output.Append(EncodeHtml(token.ValueString)); 
        _atLineStart = term == grammar.NewLine; //set for the next token
      }//foreach token

      _output.AppendLine(); 
      _output.AppendLine("</html>"); 
      return _output.ToString(); 
    }//method

    private void ProcessNewLine(Token token) {
      if (_insideTable & !_insideCell) return; //ignore it in one special case - to make output look nicer
      if (_currentHeader != null)
        _output.AppendLine(_currentHeader.CloseHtmlTag);
      else 
        _output.AppendLine("<br/>"); 
      _currentHeader = null; 
    }//method

    private void ProcessWikiElement(Token token) {
      //we check that token actually contains some chars - to allow "invisible spaces" after last table tag
      if(_lastTableTag != null && !_insideCell && token.ValueString.Trim().Length > 0) {
        _output.Append(_lastTableTag.OpenHtmlTag);
        _insideCell = true; 
      }
      var wikiTerm = token.Terminal as WikiTerminalBase; 
      switch(wikiTerm.TermType) {
        case WikiTermType.Format: 
          ProcessFormatTag(token); 
          break; 
        case WikiTermType.Heading: 
        case WikiTermType.List:
          _output.Append(wikiTerm.OpenHtmlTag);
          _currentHeader = wikiTerm;    
          break; 
        case WikiTermType.Block:
          ProcessWikiBlockTag(token); 
          break; 
        case WikiTermType.Text:
          _output.Append(EncodeHtml(token.ValueString));     
          break; 
        case WikiTermType.Table:
          if (_insideCell)
            _output.Append(_lastTableTag.CloseHtmlTag); //write out </td> or </th>
          //We do not write opening tag immediately: we need to know if it is the last table tag on the line.
          // if yes, we don't write it at all
          _lastTableTag = wikiTerm as WikiTagTerminal;
          _insideCell = false; 
          break; 
      }
    }

    private void ProcessFormatTag(Token token) {
      var term = token.Terminal as WikiTerminalBase; 
      bool value;
      bool isOn = _flags.TryGetValue(term, out value) && value;
      if (isOn)
        _output.Append(term.CloseHtmlTag);
      else 
        _output.Append(term.OpenHtmlTag); 
      _flags[term] = !isOn;    
    }

    private void ProcessWikiBlockTag(Token token) {
      var term = token.Terminal as WikiBlockTerminal;
      string template;
      string[] segments; 

      switch(term.BlockType) {
        case WikiBlockType.EscapedText:
        case WikiBlockType.CodeBlock:
          _output.Append(term.OpenHtmlTag);
          _output.Append(EncodeHtml(token.ValueString));
          _output.AppendLine(term.CloseHtmlTag);
          break;
        case WikiBlockType.Anchor:
          _output.Append("<a name=\"" + token.ValueString +"\"/>");
          break;
        case WikiBlockType.LinkToAnchor:
          _output.Append("<a href=\"#" + token.ValueString +"\">" + EncodeHtml(token.ValueString) + "</a>");
          break; 
        case WikiBlockType.Url:
        case WikiBlockType.FileLink:
          template = "<a href=\"{0}\">{1}</a>";
          segments = token.ValueString.Split('|');
          if(segments.Length > 1)
            _output.Append(string.Format(template, segments[1], segments[0]));
          else
            _output.Append(string.Format(template, segments[0], segments[0]));
          break; 
        case WikiBlockType.Image:
          segments = token.ValueString.Split('|');
          switch(segments.Length){
            case 1:
              template = "<img src=\"{0}\"/>";
              _output.Append(string.Format(template, segments[0]));
              break; 
            case 2:
              template = "<img src=\"{1}\" alt=\"{0}\" title=\"{0}\" />";
              _output.Append(string.Format(template, segments[0], segments[1]));
              break; 
            case 3:
              template = "<a href=\"{2}\"><img src=\"{1}\" alt=\"{0}\" title=\"{0}\" /></a>";
              _output.Append(string.Format(template, segments[0], segments[1], segments[2]));
              break;
          }//switch segments.Length
          break; 
      }//switch    
    }//method

    #region comments
    /* Note: we allow mix of bulleted/numbered lists, so we can have bulleted list inside numbered item:
     
      # item 1
      ** bullet 1
      ** bullet 2
      # item 2
     
     This is a bit different from codeplex rules - the bulletted list resets the numeration of items, so that "item 2" would 
     appear with number 1, not 2. While our handling seems more flexible, you can easily change the following method to 
     follow codeplex rules.  */
    #endregion 
    //Called at the start of each line (after NewLine)
    private void CheckOpeningClosingLists(Token token) {
      var nextLevel = 0;
      var wikiTerm = token.Terminal as WikiTerminalBase;
      if(wikiTerm != null && wikiTerm.TermType == WikiTermType.List)
        nextLevel = wikiTerm.OpenTag.Length;
      //for codeplex-style additionally check that the control char is the same (# vs *). If not, unwind the levels
      if (CurrentListLevel == nextLevel) return; //it is at the same level; 
      //New list begins
      if(nextLevel > CurrentListLevel) {
        _output.Append(wikiTerm.ContainerOpenHtmlTag);
        _openLists.Push(wikiTerm);
        return; 
      } 
      //One or more lists end
      while(nextLevel < CurrentListLevel) {
        var oldTerm =_openLists.Pop();
        _output.Append( oldTerm.ContainerCloseHtmlTag);
      }//while
    }//method

    //Called at the start of each line
    private void CheckTableStatus(Token token) {
      var wikiTerm = token.Terminal as WikiTerminalBase;
      bool isTableTag = wikiTerm != null && wikiTerm.TermType == WikiTermType.Table;
      if ( !_insideTable && !isTableTag) return;
      _insideCell = false; //if we are at line start, drop this flag
      _lastTableTag = null; 
      //New table begins
      if(!_insideTable && isTableTag) {
        _output.AppendLine("<table border=1>"); 
        _output.Append("<tr>");
        _insideTable = true; 
        return; 
      }
      //existing table continues
      if(_insideTable && isTableTag) {
        _output.AppendLine("</tr>");
        _output.Append("<tr>"); 
        return;
      }
      //existing table ends
      if(_insideTable && !isTableTag) {
        _output.AppendLine("</tr>"); 
        _output.AppendLine("</table>");
        _insideTable = false; 
        return;
      }
    }//method

  }//class

}//namespace

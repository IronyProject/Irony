using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Irony.Parsing {
  // Regular expression literal, like javascript literal:   /abc?/i
  // Allows optional switches
  // example:
  //  regex = /abc\\\/de/
  //  matches fragments like  "abc\/de" 
  // Note: switches are returned in token.Details field. Unlike in StringLiteral, we don't need to unescape the escaped chars,
  // (this is the job of regex engine), we only need to correctly recognize the end of expression

  [Flags]
  public enum RegexFlags {
    None = 0, 
    AllowLetterAfter = 0x01, //if not set (default) then any following letter (after legal switches) is reported as invalid switch
    CreateRegExObject = 0x02,  //if set, token.Value contains Regex object; otherwise, it contains a pattern (string)
    UniqueSwitches = 0x04,    //require unique switches
    
    Default = CreateRegExObject | UniqueSwitches,
  }

  public class RegExLiteral : Terminal {
    public class RegexSwitchTable : Dictionary<char, RegexOptions> { }
    
    public Char StartSymbol = '/';
    public Char EndSymbol='/';
    public Char EscapeSymbol='\\';
    public RegexSwitchTable Switches = new RegexSwitchTable();
    public RegexOptions DefaultOptions = RegexOptions.None;
    public RegexFlags Flags = RegexFlags.Default;

    private char[] _stopChars; 

    public RegExLiteral(string name) : base(name, TokenCategory.Literal) {
      Switches.Add('i', RegexOptions.IgnoreCase);
      Switches.Add('g', RegexOptions.None); //not sure what to do with this flag? anybody, any advice?
      Switches.Add('m', RegexOptions.Multiline); 
    }

    public RegExLiteral(string name, char startEndSymbol, char escapeSymbol) : base(name) {
      StartSymbol = startEndSymbol;
      EndSymbol = startEndSymbol;
      EscapeSymbol = escapeSymbol;
    }//constructor

    public override void Init(GrammarData grammarData) {
      base.Init(grammarData);
      _stopChars = new char[] { EndSymbol, '\r', '\n' };
    }
    public override IList<string> GetFirsts() {
      var result = new StringList();
      result.Add(StartSymbol.ToString());
      return result; 
    }

    public override Token TryMatch(ParsingContext context, ISourceStream source) {
      while (true) {
        //Find next position
        var newPos = source.Text.IndexOfAny(_stopChars, source.PreviewPosition + 1);
        //we either didn't find it
        if (newPos == -1)
          return source.CreateErrorToken("No end symbol for regex literal."); 
        source.PreviewPosition = newPos;
        if (source.PreviewChar != EndSymbol)
          //we hit CR or LF, this is an error
          return source.CreateErrorToken("No end symbol for regex literal."); 
        if (!CheckEscaped(source)) 
          break;
      }
      source.PreviewPosition++; //move after end symbol
      //save pattern length, we will need it
      var patternLen = source.PreviewPosition - source.Location.Position - 2; //exclude start and end symbol
      //read switches and turn them into options
      RegexOptions options = RegexOptions.None;
      var switches = string.Empty;
      while(ReadSwitch(source, ref options)) {
        if (FlagIsSet(RegexFlags.UniqueSwitches) && switches.Contains(source.PreviewChar))
          return source.CreateErrorToken("Duplicate switch '{0}' for regular expression", source.PreviewChar); 
        switches += source.PreviewChar.ToString();
        source.PreviewPosition++; 
      }
      //check following symbol
      if (!FlagIsSet(RegexFlags.AllowLetterAfter)) {
        var currChar = source.PreviewChar;
        if (char.IsLetter(currChar) || currChar == '_')
          return source.CreateErrorToken("Invalid switch '{0}' for regular expression", currChar); 
      }
      var token = source.CreateToken(this);
      //we have token, now what's left is to set its Value field. It is either pattern itself, or Regex instance
      string pattern = token.Text.Substring(1, patternLen); //exclude start and end symbol
      object value = pattern; 
      if (FlagIsSet(RegexFlags.CreateRegExObject)) {
        value = new Regex(pattern, options);
      }
      token.Value = value; 
      token.Details = switches; //save switches in token.Details
      return token; 
    }

    private bool CheckEscaped(ISourceStream source) {
      var savePos = source.PreviewPosition;
      bool escaped = false;
      source.PreviewPosition--; 
      while (source.PreviewChar == EscapeSymbol){
        escaped = !escaped;
        source.PreviewPosition--;
      }
      source.PreviewPosition = savePos;
      return escaped;
    }
    private bool ReadSwitch(ISourceStream source, ref RegexOptions options) {
      RegexOptions option;
      var result = Switches.TryGetValue(source.PreviewChar, out option);
      if (result)
        options |= option;
      return result; 
    }

    public bool FlagIsSet(RegexFlags flag) {
      return (Flags & flag) != 0;
    }

  }//class

}//namespace 

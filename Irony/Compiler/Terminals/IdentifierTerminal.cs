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
using System.Text;
using System.Globalization;

namespace Irony.Compiler {
  #region notes
  //Identifier terminal. Matches alpha-numeric sequences that usually represent identifiers and keywords.
  // c#: @ prefix signals to not interpret as a keyword; allows \u escapes
  // 

  #endregion
  
  public class UnicodeCategoryList : List<UnicodeCategory> { }

  public class IdentifierTerminal : CompoundTerminalBase {

    //TODO: c# allows unicode escapes in identifiers, even in the first char; thus the "\" symbol should 
    // be included in GetFirsts() list. IdentifierTerminal must provide for this!

    //Note that extraChars, extraFirstChars are used to form AllFirstChars and AllChars fields, which in turn 
    // are used in QuickParse. Only if QuickParse fails, the process switches to full version with checking every
    // char's category
    public IdentifierTerminal(string name, string extraChars, string extraFirstChars)  : base(name) {
      AllFirstChars = TextUtils.AllLatinLetters + extraFirstChars;
      AllChars = TextUtils.AllLatinLetters + TextUtils.DecimalDigits + extraChars;
      MatchMode = TokenMatchMode.ByValueThenByType;
    }
    public IdentifierTerminal(string name)  : this(name, "_", "_") {  }

    #region properties: ExtraChars, ExtraFirstChars
    //Used in QuickParse only!
    public string AllChars;
    public string AllFirstChars;
    private string _terminators;
    private StringDictionary _keywordHash;

    //The following list must include only words that are reserved and are not general identifiers (variables)
    public readonly KeyList Keywords = new KeyList();
    public readonly UnicodeCategoryList StartCharCategories = new UnicodeCategoryList(); //categories of first char
    public readonly UnicodeCategoryList CharCategories = new UnicodeCategoryList();      //categories of all other chars
    public readonly UnicodeCategoryList CharsToRemoveCategories = new UnicodeCategoryList(); //categories of chars to remove from final id, usually formatting category
    #endregion

    public void AddKeywords(params string[] keywords) {
      Keywords.AddRange(keywords);
    }

    public void AddKeywordList(string keywordList) {
      string[] arr = keywordList.Split(' ', ',', ';', '\n', '\r', '\t');
      foreach (string kw in arr) {
        string trimmed = kw.Trim();
        if (!string.IsNullOrEmpty(trimmed))
          Keywords.Add(trimmed);

      }
    }

    #region overrides
    public override void Init(Grammar grammar) {
      base.Init(grammar);
      _terminators = grammar.WhitespaceChars + grammar.Delimiters;
      //build a hash table of keywords
      _keywordHash = new StringDictionary();
      foreach (string keyw in Keywords) {
        if (grammar.CaseSensitive) 
          _keywordHash.Add(keyw, string.Empty);
        else 
          _keywordHash.Add(keyw.ToLower(), string.Empty);
      }//foreach
      if (this.StartCharCategories.Count > 0 && !grammar.FallbackTerminals.Contains(this))
        grammar.FallbackTerminals.Add(this);
    }

    //Override to assign IsKeyword flag to keyword tokens
    protected override Token CreateToken(CompilerContext context, ISourceStream source, ScanDetails details) {
      if (details.IsSet(ScanFlags.IncludePrefix) && !string.IsNullOrEmpty(details.Prefix))
        details.Value = details.Prefix + details.Body;
      Token token = base.CreateToken(context, source, details); 
      if (details.IsSet(ScanFlags.IsNotKeyword))
        return token;
      //check if it is keyword
      string text = token.Text;
      if (!Grammar.CaseSensitive)
        text = text.ToLower();
      if (_keywordHash.ContainsKey(text))
        token.IsKeyword = true;
      return token; 
    }

    protected override Token QuickParse(CompilerContext context, ISourceStream source) {
      if (AllFirstChars.IndexOf(source.CurrentChar) < 0) 
        return null;
      source.Position++;
      while (AllChars.IndexOf(source.CurrentChar) >= 0 && !source.EOF())
        source.Position++;
      //if it is not a terminator then cancel; we need to go through full algorithm
      if (_terminators.IndexOf(source.CurrentChar) < 0) return null; 
      string text = source.GetLexeme();
      return Token.Create(this, context, source.TokenStart, text);
    }

    protected override bool ReadBody(ISourceStream source, ScanDetails details) {
      int start = source.Position;
      bool allowEscapes = !details.IsSet(ScanFlags.DisableEscapes);
      CharList outputChars = new CharList();
      while (!source.EOF()) {
        char current = source.CurrentChar;
        if (_terminators.IndexOf(current) >= 0) break;
        if (allowEscapes && current == this.EscapeChar) {
          current = ReadUnicodeEscape(source, details);
          //We  need to back off the position. ReadUnicodeEscape sets the position to symbol right after escape digits.  
          //This is the char that we should process in next iteration, so we must backup one char, to pretend the escaped
          // char is at position of last digit of escape sequence. 
          source.Position--; 
          if (details.HasError()) 
            return false;
        }
        //Check if current character is OK
        if (!CharOk(current, source.Position == start)) 
          break; 
        //Check if we need to skip this char
        UnicodeCategory currCat = char.GetUnicodeCategory(current); //I know, it suxx, we do it twice, fix it later
        if (!this.CharsToRemoveCategories.Contains(currCat))
          outputChars.Add(current); //add it to output (identifier)
        source.Position++;
      }//while
      if (outputChars.Count == 0)
        return false;
      //Convert collected chars to string
      details.Body =  new string(outputChars.ToArray());
      return !string.IsNullOrEmpty(details.Body); 
    }

    private bool CharOk(char ch, bool first) {
      //first check char lists, then categories
      string all = first? AllFirstChars : AllChars;
      if(all.IndexOf(ch) >= 0) return true; 
      //check categories
      UnicodeCategory chCat = char.GetUnicodeCategory(ch);
      UnicodeCategoryList catList = first ? StartCharCategories : CharCategories;
      if (catList.Contains(chCat)) return true;
      return false; 
    }

    private char ReadUnicodeEscape(ISourceStream source, ScanDetails details) {
      //Position is currently at "\" symbol
      source.Position++; //move to U/u char
      int len;
      switch (source.CurrentChar) {
        case 'u': len = 4; break;
        case 'U': len = 8; break; 
        default:
          details.Error = "Invalid escape symbol, expected 'u' or 'U' only.";
          return '\0'; 
      }
      if (source.Position + len > source.Text.Length) {
        details.Error = "Invalid escape symbol";
        return '\0';
      }
      source.Position++; //move to the first digit
      string digits = source.Text.Substring(source.Position, len);
      char result = (char)Convert.ToUInt32(digits, 16);
      source.Position += len;
      details.Flags |= ScanFlags.HasEscapes;
      return result;
    }

    protected override bool ConvertValue(ScanDetails details) {
      if (details.IsSet(ScanFlags.IncludePrefix))
        details.Value = details.Prefix + details.Body;
      else
        details.Value = details.Body;
      return true; 
    }

    //TODO: put into account non-Ascii aplhabets specified by means of Unicode categories!
    public override IList<string> GetFirsts() {
      KeyList list = new KeyList();
      list.AddRange(Prefixes);
      if (string.IsNullOrEmpty(AllFirstChars)) 
        return list;
      char[] chars = AllFirstChars.ToCharArray();
      foreach (char ch in chars)
        list.Add(ch.ToString());
      if (IsSet(TermOptions.CanStartWithEscape))
        list.Add(this.EscapeChar.ToString());
      return list;
    }
    #endregion 

  }//class


} //namespace

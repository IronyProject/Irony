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

namespace Irony.Compiler {
  //Identifier terminal. Matches alpha-numeric sequences that usually represent identifiers and keywords.
  // Note that we strongly recommend to recognize keywords as identifier tokens in scanner, and let
  // parser decide what is it exactly - unless keywords are reserved by the language, 
  // and grammar becomes ambiguous if we don't distinguish them before parser. 
  // Distinguishing keywords from identifiers is a job for Parser, not Scanner!
  // In any case, don't create separate terminals/symbols for keywords, but 
  // use IdentifierTerminal's ReservedWords property.
  public class IdentifierTerminal : Terminal {
    public IdentifierTerminal(string name, string extraChars, string extraFirstChars)
      : base(name) {
      _extraChars = extraChars;
      _extraFirstChars = extraFirstChars;
      MatchMode = TokenMatchMode.ByValueThenByType;
    }
    public IdentifierTerminal(string name) : this(name, null, null) { }

    #region properties: ExtraChars, ExtraFirstChars
    public string ExtraChars {
      get { return _extraChars; }
      set { _extraChars = value; }
    }  string _extraChars = "_";

    public string ExtraFirstChars {
      get { return _extraFirstChars; }
      set { _extraFirstChars = value; }
    } string _extraFirstChars = "_";

    public readonly KeyList ReservedWords = new KeyList();
    #endregion

    public void AddReservedWords(params string[] words) {
      ReservedWords.AddRange(words);
    }

    private bool CharOk(char ch) {
      bool ok = char.IsLetterOrDigit(ch) ||
        _extraChars != null && _extraChars.IndexOf(ch) >= 0;
      return ok;
    }
    private bool FirstCharOk(char ch) {
      bool ok = char.IsLetter(ch) ||
        _extraFirstChars != null && _extraFirstChars.IndexOf(ch) >= 0;
      return ok;
    }
    public override Token TryMatch(CompilerContext context, ISourceStream source) {
      if (!FirstCharOk(source.CurrentChar))
        return null;
      source.Position++;
      while (CharOk(source.CurrentChar))
        source.Position++;
      string text = source.GetLexeme();
      Terminal term = (ReservedWords.Contains(text) ? Grammar.ReservedWord : this);
      return new Token(term, source.TokenStart, text);
    }//method

    private const string AllLetters = "abcdefghijklmnopqrstuvwxyz";
    public override IList<string> GetPrefixes() {
      string tmp = AllLetters + AllLetters.ToUpper() + ExtraFirstChars;
      char[] chars = tmp.ToCharArray();
      KeyList list = new KeyList();
      foreach (char ch in chars)
        list.Add(ch.ToString());
      return list;
    }
  }//class


} //namespace

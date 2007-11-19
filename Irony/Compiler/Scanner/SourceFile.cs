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
//using System.Text.RegularExpressions;

namespace Irony.Compiler {

  public interface ISourceStream {
    int Position { get;set;}
    char CurrentChar { get;} //char at Position
    char NextChar { get;}    //preview char at Position+1

    string Text { get;} //returns entire text buffer
    //returns substring from TokenStart.Position till (Position - 1)
    string GetLexeme();
    SourceLocation TokenStart { get;}
    bool EOF();
  }


  #region SourceLocation struct
  public struct SourceLocation {
    public int Position;
    public int Line;
    public int Column;
    public SourceLocation(int position, int line, int column) {
      Position = position;
      Line = line;
      Column = column;
    }
    public override string ToString() {
      return "L" + Line + ":" + "C" + Column;
    }
  }//SourceLocation
  #endregion

  #region SourceFile class
  public class SourceFile : ISourceStream {
    public SourceFile(string text, string fileName, int tabWidth): this(text, fileName) {
      _tabWidth = tabWidth;
    }
    public SourceFile(string text, string fileName) {
      _text = text;
      _fileName = fileName;
      Reset();
    }

    public string FileName {
      get { return _fileName; }
    } string _fileName;

    //TabWidth value of 1 means "ignore tabs"; for languages like Python set it to real tab width of the editor (8?).
    public int TabWidth {
      get {return _tabWidth;}
      set {_tabWidth = value;}
    } int  _tabWidth = 1;

    public void Reset() {
      Position = 0;
      _tokenStart = new SourceLocation();
      _nextNewLinePosition = _text.IndexOf('\n');
    }

    #region ISourceFile Members
    public int Position {
      get {return _position; }
      set {
        _position = value;
        try {  _currentChar = _text[_position]; } 
          catch { _currentChar = '\0'; }
      }
    } int _position;

    public bool EOF() {
      return _position >= Text.Length;
    }
    public char CurrentChar {
      get { return _currentChar; }
    } char _currentChar;

    public char NextChar {
      get {
        try {
          return _text[_position + 1];
        } catch { return '\0'; }
      }
    }
    public string Text {
      get { return _text; }
    }  string _text;

    //returns substring from TokenStart.Position till (Position - 1)
    public string GetLexeme() {
      string text = _text.Substring(_tokenStart.Position, _position - _tokenStart.Position);
      return text;
    }
    public SourceLocation TokenStart {
      get {return _tokenStart;}
      internal set { _tokenStart = value; }
    } SourceLocation  _tokenStart;

    #endregion

    public override string ToString() {
      //show just 30 chars from current position
      string result;
      if (Position + 30 < Text.Length)
        result = Text.Substring(Position, 30);
      else
        result = Text.Substring(Position);
      return result;
    }
    
    private int _nextNewLinePosition; //private field to cache position of next \n character
    //This is all about source scanning optimization - this seemingly strange code is aimed at improving perfomance,
    // so keep this in mind when reading it. 
    internal void SetNextTokenStart(string skipWhitespaceChars) {
      while (skipWhitespaceChars.IndexOf(CurrentChar) >= 0)
        Position++;
      int newPosition = this.Position;
      //currently _tokenStart field contains location (pos/line/col) of the last created token. 
      // First, check if new position is in the same line; if so, just adjust column and return
      //  Note that this case is not line start, so we do not need to check tab chars (and adjust column) 
      if (newPosition <= _nextNewLinePosition || _nextNewLinePosition < 0) {
        _tokenStart.Column += newPosition - _tokenStart.Position;
        _tokenStart.Position = newPosition;
        return;
      }
      //So new position is on new line (beyond _nextNewLinePosition)
      //First count \n chars in the string fragment
      int lineStart = _nextNewLinePosition;
      int nlCount = 1; //we start after old _nextNewLinePosition, so we count one NewLine char
      ScanTextForChar(_text, '\n', lineStart + 1, newPosition - 1, ref nlCount, ref lineStart);
      _tokenStart.Line += nlCount;
      //at this moment lineStart is at start of line where newPosition is located 
      //Calc # of tab chars from lineStart to newPosition to adjust column#
      int tabCount = 0;
      int dummy = 0;
      if (_tabWidth > 1)
        ScanTextForChar(_text, '\t', lineStart, newPosition - 1, ref tabCount, ref dummy);

      //adjust TokenStart with calculated information
      _tokenStart.Position = newPosition;
      _tokenStart.Column = newPosition - lineStart - 1;
      if (tabCount > 0)
        _tokenStart.Column += (_tabWidth - 1) * tabCount; // "-1" to count for tab char itself
      
      //finally cache new line
      _nextNewLinePosition = _text.IndexOf('\n', newPosition);
    }

    private static void ScanTextForChar(string text, char ch, int from, int until, ref int count, ref int lastPosition) {
      if (from > until) return;
      while (true) {
        int next = text.IndexOf(ch, from, until - from + 1);
        if (next < 0) return;
        lastPosition = next;
        count++;
        from = next + 1;
      }

    }


  }//class
  #endregion

}//namespace

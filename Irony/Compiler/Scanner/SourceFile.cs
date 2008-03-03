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
    SourceLocation TokenStart { get; set;}
    int TabWidth { get;}
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
    public SourceFile(string text, string fileName, int tabWidth) {
      _text = text;
      _fileName = fileName;
      _tabWidth = tabWidth;
    }
    public SourceFile(string text, string fileName): this(text, fileName, 8) {
    }

    public string FileName {
      get { return _fileName; }
    } string _fileName;

    public int TabWidth {
      get {return _tabWidth;}
      set {_tabWidth = value;}
    } int  _tabWidth; // = 8;

    #region ISourceFile Members
    public int Position {
      get {return _position; }
      set {_position = value;}
    } int _position;

    public bool EOF() {
      return _position >= Text.Length;
    }
    public char CurrentChar {
      get {
        try {
          return _text[_position];
        } catch { return '\0'; }
      }
    }

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
      set { _tokenStart = value; }
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
    

  }//class
  #endregion

}//namespace

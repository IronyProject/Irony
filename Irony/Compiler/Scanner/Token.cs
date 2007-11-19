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

  public class TokenList : List<Token> {}

  //Tokens are produced by scanner and fed to parser, possibly passing through Token filters first. 
  // Token is derived from AstNode because tokens are pushed into Parser stack (like non-terminal nodes),
  // and then they can be included as sub-nodes into AST tree. So Token is a kind of AstNode. 
  public class Token : AstNode  {
    public Token(Terminal terminal, SourceLocation location, string text, object value)
      : this(terminal, location, text){
      _value = value;
    }
    public Token(Terminal terminal, SourceLocation location, string text)   : base (null, terminal, location, null){
      _text = text;
      _value = text;
      if (text != null)
        _length = text.Length;
    }
    
    public Terminal Terminal   {
      get {return base.Element as Terminal;}
    } 

    public int Length  {
      get {return _length;}
    } int  _length;

    public string Text  {
      get {return _text;}
      set {_text = value;}
    } string  _text;

    public object Value {
      get { return _value; }
      set { _value = value; }
    } object _value;

    public override string ToString() {
      if (Value == null)
        return "null";
      string result = Value.ToString();
      if (result != Terminal.ToString())
        result += ", " + Terminal.ToString();
      return result;
    }

  }//class


}//namespace

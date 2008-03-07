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
  //Tokens are produced by scanner and fed to parser, optionally passing through Token filters in between. 
  // Token is derived from AstNode because tokens are pushed into Parser stack (like non-terminal nodes),
  // and they can be included as nodes into AST tree. So Token is a primitive AstNode. 
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
      [System.Diagnostics.DebuggerStepThrough]
      get {return base.Term as Terminal;}
    }
    public SymbolTerminal Symbol {
      [System.Diagnostics.DebuggerStepThrough]
      get { return base.Term as SymbolTerminal; }
    }
    public TokenCategory Category  {
      [System.Diagnostics.DebuggerStepThrough]
      get { return Terminal.Category; }
    }
    [System.Diagnostics.DebuggerStepThrough]
    public bool IsError() {
      return Category == TokenCategory.Error;
    }

    public int Length  {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _length; }
    } int  _length;

    public string Text  {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _text; }
      set {_text = value;}
    } string  _text;

    public object Value {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _value; }
      set { _value = value; }
    } object _value;

    //Details of scanning; optional
    public ScanDetails Details;

    [System.Diagnostics.DebuggerStepThrough]
    public override string ToString() {
      if (Terminal is SymbolTerminal)
        return _text + " [Symbol]";
      else 
        return (_text==null? string.Empty : _text + " ") + Terminal.ToString();
    }

  }//class


}//namespace

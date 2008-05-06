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
    protected Token(AstNodeArgs args)  : base(args){   }
    
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
    public string Text  {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _text; }
      set {_text = value;}
    } string  _text;

    public object Value {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _value; }
      set { 
        _value = value;
        _valueString = (_value == null ? string.Empty : _value.ToString());
      }
    } object _value;

    public string ValueString {
      get { return _valueString;}
    } string _valueString;
 
    //Details of scanning; optional
    public ScanDetails Details;

    [System.Diagnostics.DebuggerStepThrough]
    public bool IsError() {
      return Category == TokenCategory.Error;
    }
    [System.Diagnostics.DebuggerStepThrough]
    public bool IsMultiToken() {
      return ChildNodes.Count > 0;
    }

    public int Length {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _text == null ? 0 : _text.Length; }
    }

    public bool IsKeyword;

    public bool MatchByValue {
      get {
        if (IsKeyword) return true;
        if (Text == null) return false;
        return (Terminal.MatchMode & TokenMatchMode.ByValue) != 0;
      }
    }
    public bool MatchByType {
      get {
        if (IsKeyword) return false;
        return (Terminal.MatchMode & TokenMatchMode.ByType) != 0;
      }
    }

    public override void Evaluate(Irony.Runtime.EvaluationContext context) {
      context.CurrentResult = this.Value;
    }

    [System.Diagnostics.DebuggerStepThrough]
    public override string ToString() {
      if (Terminal is SymbolTerminal)
        return _text + " [Symbol]";
      if (IsKeyword)
        return _text + " " + "[Keyword]";
      return ValueString + " " + Terminal.ToString();
    }

    public static Token Create(Terminal term, CompilerContext context, SourceLocation location, string text) {
      return Create(term, context, location, text, text);
    }
    public static Token Create(Terminal term, CompilerContext context, SourceLocation location, string text, object value) {
      int textLen = text == null ? 0 : text.Length;
      SourceSpan span = new SourceSpan(location, textLen);
      AstNodeArgs args = new AstNodeArgs(term, context, span, null);
      Token token = new Token(args);
      token.Text = text;
      token.Value = value;
      return token;
    }

    public static Token CreateMultiToken(Terminal term, CompilerContext context, TokenList tokens) {
      SourceSpan span = new SourceSpan();
      AstNodeArgs args = new AstNodeArgs(term, context, span, null);
      Token token = new Token(args);
      token.ChildNodes.AddRange(tokens.ToArray());
      return token; 
    }

    public override bool IsEmpty() {
      return false;
    }

  }//class


}//namespace

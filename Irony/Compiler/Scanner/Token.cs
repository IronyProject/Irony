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
using System.Xml;

namespace Irony.Compiler {

  public class TokenList : List<Token> {}
  //Tokens are produced by scanner and fed to parser, optionally passing through Token filters in between. 
  // Token is derived from AstNode because tokens are pushed into Parser stack (like non-terminal nodes),
  // and they can be included as nodes into AST tree. So Token is a primitive AstNode. 
  public class Token : AstNode  {
    public Token(NodeArgs args) : base(args) {
      this.EditorInfo = Terminal.EditorInfo;  //set to term's EditorInfo by default
      if (Terminal.Category == TokenCategory.Literal)
        this.Evaluate = EvaluateAssign;
      else
        this.Evaluate = EvaluateEmpty; 
    
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
    public TokenEditorInfo EditorInfo;

    public Token OtherBrace {  //matching opening/closing brace
      get { return _otherBrace; }
    } Token _otherBrace;

    public static void LinkMatchingBraces(Token openingBrace, Token closingBrace) {
      openingBrace._otherBrace = closingBrace;
      closingBrace._otherBrace = openingBrace;
    }

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
    public short ScannerState; //Scanner state after producing token 

    private void EvaluateAssign(Irony.Runtime.EvaluationContext context) {
      context.CurrentResult = this.Value;
    }
    private void EvaluateEmpty(Irony.Runtime.EvaluationContext context) {
    }

    [System.Diagnostics.DebuggerStepThrough]
    public override string ToString() {
      string result = string.IsNullOrEmpty(Role) ? string.Empty : Role + ":";
      if (Terminal is SymbolTerminal)
        result += _text + " [Symbol]";
      else if (IsKeyword)
        result += _text + " " + "[Keyword]";
      else
        result += ValueString + " " + Terminal.ToString();
      return result; 
    }

    public static Token Create(Terminal term, CompilerContext context, SourceLocation location, string text) {
      return Create(context, term, location, text, text);
    }
    public static Token Create(CompilerContext context, Terminal term, SourceLocation location, string text, object value) {
      int textLen = text == null ? 0 : text.Length;
      SourceSpan span = new SourceSpan(location, textLen);
      NodeArgs args = new NodeArgs(context, term, span, null); 
      Token token = new Token(args);
      token.Text = text;
      token.Value = value;
      return token;
    }

    public static Token CreateMultiToken(CompilerContext context, Terminal term, TokenList tokens) {
      if (tokens.Count == 0)
        throw new ApplicationException("Cannot create MultiToken from empty token list.");
      SourceLocation startLoc = tokens[0].Location;
      int endpos = tokens[tokens.Count - 1].Span.EndPos;
      SourceSpan span = new SourceSpan(startLoc, endpos - startLoc.Position);
      NodeArgs args = new NodeArgs(context, term, span, null);
      Token result = new Token(args);
      foreach (Token child in tokens) 
        result.ChildNodes.Add(child);
      return result; 
    }

    public override bool IsEmpty() {
      return false;
    }

    protected override void XmlSetAttributes(XmlElement thisElement) {
      base.XmlSetAttributes(thisElement);
      thisElement.SetAttribute("Value", this.ValueString); //Adds value string
      if (Value != null)
        thisElement.SetAttribute("ValueType", this.Value.GetType().FullName); //Adds value string

    }


  }//class


}//namespace

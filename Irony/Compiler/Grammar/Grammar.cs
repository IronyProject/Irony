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

  public class Grammar {

    #region properties: CaseSensitive, WhitespaceChars, Delimiters ExtraTerminals, Root, TokenFilters
    public bool CaseSensitive = true;
    public string WhitespaceChars = " \t\r\n\v";
    //List of chars that unambigously identify the start of new token. 
    //used in scanner error recovery, and in quick parse path in Number literals 
    public string Delimiters = ",;[](){}"; 

    //Terminals not present in grammar expressions and not reachable from the Root
    // (Comment terminal is usually one of them)
    public readonly TerminalList ExtraTerminals = new TerminalList();

    //Default node type; if null then GenericNode type is used. 
    public Type DefaultNodeType = typeof(AstNode);

    public NonTerminal Root  {
      [System.Diagnostics.DebuggerStepThrough]
      get { return _root; }
      set { _root = value;  }
    } NonTerminal _root;

    public TokenFilterList TokenFilters = new TokenFilterList();
    #endregion 

    #region Register methods
    public void RegisterPunctuation(params string[] symbols) {
      foreach (string symbol in symbols) {
        SymbolTerminal term = SymbolTerminal.GetSymbol(symbol);
        term.SetOption(TermOptions.IsPunctuation);
      }
    }
    
    public void RegisterPunctuation(params BnfTerm[] elements) {
      foreach (BnfTerm term in elements) 
        term.SetOption(TermOptions.IsPunctuation);
    }

    public void RegisterOperators(int precedence, params string[] opSymbols) {
      RegisterOperators(precedence, Associativity.Left, opSymbols);
    }

    public void RegisterOperators(int precedence, Associativity associativity, params string[] opSymbols) {
      foreach (string op in opSymbols) {
        SymbolTerminal opSymbol = SymbolTerminal.GetSymbol(op);
        opSymbol.SetOption(TermOptions.IsOperator, true);
        opSymbol.Precedence = precedence;
        opSymbol.Associativity = associativity;
      }
    }//method

    public void RegisterBracePair(string openBrace, string closeBrace) {
      SymbolTerminal openS = SymbolTerminal.GetSymbol(openBrace);
      SymbolTerminal closeS = SymbolTerminal.GetSymbol(closeBrace);
      openS.SetOption(TermOptions.IsOpenBrace);
      openS.IsPairFor = closeS;
      closeS.SetOption(TermOptions.IsCloseBrace);
      closeS.IsPairFor = openS;
    }
    #endregion

    #region virtual methods: TryMatch, CreateNode, GetSyntaxErrorMessage
    //This method is called if Scanner failed to produce token
    public virtual Token TryMatch(CompilerContext context, ISourceStream source) {
      return null;
    }
    // Override this method in language grammar if you want a custom node creation mechanism.
    public virtual AstNode CreateNode(CompilerContext context, ActionRecord reduceAction, 
                                      SourceLocation location, AstNodeList childNodes) {
      return null;      
    }
    public virtual string GetSyntaxErrorMessage(CompilerContext context, KeyList expectedList) {
      return null; //Irony then would construct default message
    }
    #endregion

    #region Static utility methods used in custom grammars: Symbol(), ToElement, WithStar, WithPlus, WithQ
    protected static SymbolTerminal Symbol(string symbol) {
      return SymbolTerminal.GetSymbol(symbol);
    }
    protected static SymbolTerminal Symbol(string symbol, string name) {
      return SymbolTerminal.GetSymbol(symbol, name);
    }
    protected static BnfTerm ToElement(BnfExpression expression) {
      string name = expression.ToString();
      return new NonTerminal(name, expression);
    }
    protected static BnfTerm WithStar(BnfExpression expression) {
      return ToElement(expression).Star();
    }
    protected static BnfTerm WithPlus(BnfExpression expression) {
      return ToElement(expression).Plus();
    }
    protected static BnfTerm WithQ(BnfExpression expression) {
      return ToElement(expression).Q();
    }
    public static Token CreateSyntaxErrorToken(SourceLocation location, string message, params object[] args) {
      if (args != null && args.Length > 0)
        message = string.Format(message, args);
      return new Token(Grammar.SyntaxError, location, message);
    }
    #endregion

    #region Standard terminals: EOF, Empty, NewLine, Indent, Dedent
    // Empty object is used to identify optional element: 
    //    term.Rule = term1 | Empty;
    public readonly static Terminal Empty = new Terminal("EMPTY");
    // The following terminals are used in indent-sensitive languages like Python;
    // they are not produced by scanner but are produced by CodeOutlineFilter after scanning
    public readonly static Terminal NewLine = new Terminal("LF", TokenCategory.Outline);
    public readonly static Terminal Indent = new Terminal("INDENT", TokenCategory.Outline);
    public readonly static Terminal Dedent = new Terminal("DEDENT", TokenCategory.Outline);
    // Identifies end of file
    // Note: using Eof in grammar rules is optional. Parser automatically adds this symbol 
    // as a lookahead to Root non-terminal
    public readonly static Terminal Eof = new Terminal("EOF", TokenCategory.Outline);

    //End-of-Statement terminal
    public readonly static Terminal Eos = new Terminal("EOS", TokenCategory.Outline);
    // Special terminal for ReservedWord token produced by IdentifierTerminal when lexeme matches one of reserved words. 
    // It is sometimes(not always) necessary to distinguish reserved words from free identifiers in the input stream.
    // The main distinction from Identifier is that ReservedWord can be matched only by Value (keyword itself), 
    //  not by Terminal type.
    public readonly static Terminal ReservedWord = new Terminal("ReservedWord", TokenMatchMode.ByValue);

    public readonly static Terminal SyntaxError = new Terminal("SYNTAX_ERROR", TokenCategory.Error);
    #endregion

        
  }//class

}//namespace

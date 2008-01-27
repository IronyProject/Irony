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

  #region Grammar class
  public abstract class Grammar {

    #region properties: CaseSensitive, WhitespaceChars, ExtraTerminals, PunctuationSymbols, TokenFilters
    public bool CaseSensitive = true;
    public string WhitespaceChars = " \t\r\n\v";
    public string Delimiters = ""; //list of chars that unambigously identify the start of new token. 
                                   //used in scanner error recovery. 

    //Terminals not present in grammar expressions and not reachable from the Root
    // (Comment terminal is usually one of them)
    public readonly TerminalList ExtraTerminals = new TerminalList();

    //Punctuation symbols are those that are excluded from arguments of node constructors
    public readonly KeyList PunctuationSymbols = new KeyList();

    //Default node type; if null then GenericNode type is used. 
    public Type DefaultNodeType;

    //TODO: give this a second thought
    #region Comments
    //  If the following flag is true, the scanner removes all keyword-like terminals (those that start with a letter)
    //  from the list of terminals found in grammar rules. These symbols are treated 
    //  as something else in the input stream  and the grammar should include a terminal (usuall Identifier)
    //  that would match these keywords. 
    //  For ex., if there is a symbol "begin" used somewhere in grammar rules, it will not be included 
    //  into final list of terminals, and word "begin" in input stream will be represented by the Identifier token.  
    //  The parser will match it to the "begin" symbol specified in the expression 
    //   thus recognizing it as a keyword. 
    //  This is a pure optimization option, to improve scanner performance (terminal lookup by char returns less terminals). 
    //  It is recommended to set it to true for most languages. 
    //  For some languages like PHP that has all variable IDs start with "$" it should be set to false.
    #endregion
    public bool NoKeywordTerminals = true;

    public NonTerminal Root  {
      get {return _root;}
      set { _root = value;  }
    } NonTerminal _root;

    public TokenFilterList TokenFilters = new TokenFilterList();

    #endregion 

    #region Operators handling
    public readonly OperatorInfoTable Operators = new OperatorInfoTable();

    public void RegisterOperators(int precedence, params string[] opSymbols) {
      RegisterOperators(precedence, Associativity.Left, opSymbols);
    }
    public void RegisterOperators(int precedence, Associativity associativity, params string[] opSymbols) {
      foreach (string op in opSymbols) {
        if (Operators.ContainsKey(op))
          throw new ApplicationException("Operator '" + op + "' is registered more than once.");
        Operators[op] = new OperatorInfo(op, precedence, associativity);
      }
    }//method
    #endregion

    #region virtual methods: TryMatch, CreateNode, GetSyntaxErrorMessage
    //This method is called if Scanner failed to produce token
    public virtual Token TryMatch(CompilerContext context, ISourceStream source) {
      return null;
    }
    // Override this method in language grammar if you want a custom node creation mechanism.
    public virtual AstNode CreateNode(CompilerContext context, ActionRecord reduceAction, 
                                      SourceLocation location, AstNodeList childNodes) {
      AstNode node;
      //First check and try custom NodeCreator method attached to non-terminal
      if (reduceAction.NonTerminal.NodeCreator != null) {
        node = reduceAction.NonTerminal.NodeCreator(context, reduceAction, location, childNodes);
        if (node != null) return node;
      }
      //General node-creation path
      Type nodeType = reduceAction.NonTerminal.NodeType ?? this.DefaultNodeType;
      if (childNodes.Count == 0) {
        //Create NULL node
        node = null; // new AstNode(reduceAction.NonTerminal, location, true);
      } else if (nodeType == null || nodeType == typeof(GenericNode)) {
        //GenericNode
        if (childNodes.Count == 1) 
          //Node bubbling. For the default case, if no node type is specified (meaning "use GenericNode"),
          // and new node has just one child node (meaning reduce on identity-like production A->B),
          // then we do not create new node but simply return the child node itself. So nodes "bubble-up" to higher
          // levels. This simplifies the default syntax tree. 
          node = childNodes[0];
        else
          node = new GenericNode(context, reduceAction.NonTerminal, location, childNodes);
      } else {
        //Custom node type
        node = (AstNode)Activator.CreateInstance(nodeType, context, reduceAction.NonTerminal, location, childNodes);
      }
      return node;      
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
    protected static BnfElement ToElement(BnfExpression expression) {
      string name = expression.ToString();
      return new NonTerminal(name, expression);
    }
    protected static BnfElement WithStar(BnfExpression expression) {
      return ToElement(expression).Star();
    }
    protected static BnfElement WithPlus(BnfExpression expression) {
      return ToElement(expression).Plus();
    }
    protected static BnfElement WithQ(BnfExpression expression) {
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
    //    elem.Rule = elem1 | Empty;
    public readonly static BnfElement Empty = new BnfElement("EMPTY");
    // The following terminals are used in indent-sensitive languages like Python 
    public readonly static Terminal NewLine = new Terminal("LF", TokenCategory.Outline);
    public readonly static Terminal Indent = new Terminal("INDENT", TokenCategory.Outline);
    public readonly static Terminal Dedent = new Terminal("UNINDENT", TokenCategory.Outline);
    // Identifies end of file
    // Note: using Eof in grammar rules is optional. Parser automatically adds this symbol 
    // as a lookahead to Root non-terminal
    public readonly static Terminal Eof = new Terminal("EOF", TokenCategory.Outline);

    //End-of-Statement terminal
    public readonly static Terminal Eos = new Terminal("EOS", TokenCategory.Outline);
    // Special terminal for ReservedWord token produced by IdentifierTerminal when lexeme matches one of reserved words. 
    // It is sometimes(not always) necessary to distinguish reserved words from free identifiers in the input stream.
    public readonly static Terminal ReservedWord = new Terminal("ReservedWord", TokenMatchMode.ByValue);

    public readonly static Terminal SyntaxError = new ErrorTerminal("SYNTAX_ERROR");
    #endregion

        
  }//class
  #endregion


}//namespace

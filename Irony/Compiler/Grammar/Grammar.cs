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
    //List of chars that unambigously identify the start of new token. 
    //used in scanner error recovery, and in quick parse path in Number literals 
    public string Delimiters = ",;[](){}";

    public string WhitespaceChars = " \t\r\n\v";
    
    //Used for line counting in source file
    public string LineTerminators = "\n\r\v";

    //Language options
    public LanguageOptions Options = LanguageOptions.Default;
    public bool OptionIsSet(LanguageOptions option) {
      return (Options & option) != 0;
    }

    //Terminals not present in grammar expressions and not reachable from the Root
    // (Comment terminal is usually one of them)
    // Tokens produced by these terminals will be ignored by parser input. 
    public readonly TerminalList NonGrammarTerminals = new TerminalList();

    //Terminals that either don't have explicitly declared Firsts symbols, or can start with chars not covered by these Firsts 
    // For ex., identifier in c# can start with a Unicode char in one of several Unicode classes, not necessarily latin letter.
    //  Whenever terminals with explicit Firsts() cannot produce a token, the Scanner would call terminals from this fallback 
    // collection to see if they can produce it. 
    // Note that IdentifierTerminal automatically add itself to this collection if its StartCharCategories list is not empty, 
    // so programmer does not need to do this explicitly
    public readonly TerminalList FallbackTerminals = new TerminalList();

    //Default node type; if null then GenericNode type is used. 
    public Type DefaultNodeType = typeof(AstNode);


    public BnfTerm Root;
    public readonly TokenFilterList TokenFilters = new TokenFilterList();

    //derived lists
    public readonly BnfTermList AllTerms = new BnfTermList();

    public readonly StringSet Errors = new StringSet();
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

    #region virtual methods: TryMatch, CreateNode, GetSyntaxErrorMessage, CreateRuntime
    //This method is called if Scanner failed to produce token
    public virtual Token TryMatch(CompilerContext context, ISourceStream source) {
      return null;
    }
    // Override this method in language grammar if you want a custom node creation mechanism.
    public virtual AstNode CreateNode(CompilerContext context, object reduceAction, 
                                      SourceSpan sourceSpan, AstNodeList childNodes) {
      return null;      
    }
    public virtual string GetSyntaxErrorMessage(CompilerContext context, StringSet expectedSymbolSet) {
      return null; //Irony then would construct default message
    }
    public virtual void OnActionSelected(IParser parser, Token input, object action) {
    }
    public virtual object OnActionConflict(IParser parser, Token input, object action) {
      return action;
    }
    public virtual Irony.Runtime.LanguageRuntime CreateRuntime() {
      return new Irony.Runtime.LanguageRuntime();
    }

    #endregion

    #region Static utility methods used in custom grammars: Symbol(), CreateSyntaxErrorToken
    protected static SymbolTerminal Symbol(string symbol) {
      return SymbolTerminal.GetSymbol(symbol);
    }
    protected static SymbolTerminal Symbol(string symbol, string name) {
      return SymbolTerminal.GetSymbol(symbol, name);
    }
    #endregion


    #region MakePlusRule, MakeStarRule methods
    public BnfExpression MakePlusRule(NonTerminal listNonTerminal, BnfTerm listMember) {
      return MakePlusRule(listNonTerminal, null, listMember);
    }
    public BnfExpression MakePlusRule(NonTerminal listNonTerminal, BnfTerm delimiter, BnfTerm listMember) {
      listNonTerminal.SetOption(TermOptions.IsList);
      if (delimiter == null)
        listNonTerminal.Rule = listMember | listNonTerminal + listMember;
      else
        listNonTerminal.Rule = listMember | listNonTerminal + delimiter + listMember;
      return listNonTerminal.Rule;
    }
    public BnfExpression MakeStarRule(NonTerminal listNonTerminal, BnfTerm listMember) {
      return MakeStarRule(listNonTerminal, null, listMember);
    }
    public BnfExpression MakeStarRule(NonTerminal listNonTerminal, BnfTerm delimiter, BnfTerm listMember) {
      if (delimiter == null) {
        //it is much simpler case
        listNonTerminal.SetOption(TermOptions.IsList);
        listNonTerminal.Rule = Empty | listNonTerminal + listMember;
        return listNonTerminal.Rule;
      }
      NonTerminal tmp = new NonTerminal(listMember.Name + "+");
      MakePlusRule(tmp, delimiter, listMember);
      listNonTerminal.Rule = Empty | tmp;
      listNonTerminal.SetOption(TermOptions.IsStarList);
      return listNonTerminal.Rule;
    }
    #endregion

    #region Hint utilities
    protected GrammarHint PreferShiftHere() {
      return new GrammarHint(HintType.PreferShift);
    }
    protected GrammarHint ReduceThis() {
      return new GrammarHint(HintType.ReduceThis);
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

    public readonly static Terminal SyntaxError = new Terminal("SYNTAX_ERROR", TokenCategory.Error);


    #endregion


    #region Preparing for processing
    public bool Prepared {
      get { return _prepared; }
    } bool _prepared;

    public void Prepare() {
      CollectAllTerms();
      //Init all terms and token filters 
      foreach (BnfTerm term in this.AllTerms)
        term.Init(this);
      foreach (TokenFilter filter in TokenFilters)
        filter.Init(this);
      _prepared = true; 
    }

    int _unnamedCount; //internal counter for generating names for unnamed non-terminals
    public void CollectAllTerms() {
      _unnamedCount = 0;
      AllTerms.Clear();
      //set IsNonGrammar flag in all NonGrammarTerminals and add them to Terminals collection
      foreach (Terminal t in NonGrammarTerminals) {
        t.SetOption(TermOptions.IsNonGrammar);
        AllTerms.Add(t);
      }
      _unnamedCount = 0;
      CollectAllRecursive(Root);
    }

    private void CollectAllRecursive(BnfTerm element) {
      //Terminal
      Terminal term = element as Terminal;
      // Do not add pseudo terminals defined as static singletons in Grammar class (Empty, Eof, etc)
      //  We will never see these terminals in the input stream.
      //   Filter them by type - their type is exactly "Terminal", not derived class. 
      if (term != null && !AllTerms.Contains(term) && term.GetType() != typeof(Terminal)) {
        AllTerms.Add(term);
        return;
      }
      //NonTerminal
      NonTerminal nt = element as NonTerminal;
      if (nt == null || AllTerms.Contains(nt))
        return;

      if (nt.Name == null) {
        if (nt.Rule != null && !string.IsNullOrEmpty(nt.Rule.Name))
          nt.Name = nt.Rule.Name;
        else
          nt.Name = "NT" + (_unnamedCount++);
      }
      AllTerms.Add(nt);
      if (nt.Rule == null) {
        ThrowError("Non-terminal {0} has uninitialized Rule property.", nt.Name);
        return;
      }
      //check all child elements
      foreach (BnfTermList elemList in nt.Rule.Data)
        for (int i = 0; i < elemList.Count; i++) {
          BnfTerm child = elemList[i];
          if (child == null){ 
            ThrowError("Rule for NonTerminal {0} contains null as an operand in position {1} in one of productions.", nt, i);
            continue; //for i loop 
          }
          //Check for nested expression - convert to non-terminal
          BnfExpression expr = child as BnfExpression;
          if (expr != null) {
            child = new NonTerminal(null, expr);
            elemList[i] = child;
          }
          CollectAllRecursive(child);
        }
    }//method

    private void ThrowError(string message, params object[] values) {
      if (values != null && values.Length > 0)
        message = string.Format(message, values);
      throw new ApplicationException(message);
    }

    #endregion

        
  }//class

}//namespace

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
using System.Globalization;
using System.Text;
using Irony.Ast;
using Irony.Interpreter;

namespace Irony.Parsing { 

  public partial class Grammar {

    #region properties
    /// <summary>
    /// Gets case sensitivity of the grammar. Read-only, true by default. 
    /// Can be set to false only through a parameter to grammar constructor.
    /// </summary>
    public readonly bool CaseSensitive = true;
    public readonly StringComparer LanguageStringComparer; 
    
    public ParseMethod ParseMethod = ParseMethod.Lalr;

    //List of chars that unambigously identify the start of new token. 
    //used in scanner error recovery, and in quick parse path in NumberLiterals, Identifiers 
    public string Delimiters = " \t\r\n\v,;[](){}\0"; //  '\0' is EOF, that's how it is represented in source stream

    public string WhitespaceChars = " \t\r\n\v";
    
    //Used for line counting in source file
    public string LineTerminators = "\n\r\v";

    #region Language Flags
    public LanguageFlags LanguageFlags = LanguageFlags.Default;

    public bool FlagIsSet(LanguageFlags flag) {
      return (LanguageFlags & flag) != 0;
    }
    #endregion

    //Terminals not present in grammar expressions and not reachable from the Root
    // (Comment terminal is usually one of them)
    // Tokens produced by these terminals will be ignored by parser input. 
    public readonly TerminalSet NonGrammarTerminals = new TerminalSet();

    //Terminals that either don't have explicitly declared Firsts symbols, or can start with chars not covered by these Firsts 
    // For ex., identifier in c# can start with a Unicode char in one of several Unicode classes, not necessarily latin letter.
    //  Whenever terminals with explicit Firsts() cannot produce a token, the Scanner would call terminals from this fallback 
    // collection to see if they can produce it. 
    // Note that IdentifierTerminal automatically add itself to this collection if its StartCharCategories list is not empty, 
    // so programmer does not need to do this explicitly
    public readonly TerminalList FallbackTerminals = new TerminalList();

    //Default node type; if null then GenericNode type is used. 
    public Type DefaultNodeType;
    public Type DefaultLiteralNodeType = typeof(LiteralValueNode); //default node type for literals


    public NonTerminal Root;
    
    public string GrammarComments; //shown in Grammar info tab

    public StringSet PrefixUnaryOperators;
    public StringSet PostfixUnaryOperators;

    //Console-related properties, initialized in grammar constructor
    public string ConsoleTitle;
    public string ConsoleGreeting;
    public string ConsolePrompt; //default prompt
    public string ConsolePromptMoreInput; //prompt to show when more input is expected

    #endregion 

    #region constructors
    
    public Grammar() : this(true) { } //case sensitive by default

    public Grammar(bool caseSensitive) {
      _currentGrammar = this;
      this.CaseSensitive = caseSensitive;
      bool ignoreCase =  !this.CaseSensitive;
      LanguageStringComparer = StringComparer.Create(System.Globalization.CultureInfo.InvariantCulture, ignoreCase);
      KeyTerms = new KeyTermTable(LanguageStringComparer);
      //Initialize unary operators sets
      PrefixUnaryOperators = new StringSet(LanguageStringComparer);
      PostfixUnaryOperators = new StringSet(LanguageStringComparer);
      PrefixUnaryOperators.AddRange ("+", "-", "!", "++", "--");
      PostfixUnaryOperators.AddRange("++", "--");
      NewLinePlus = CreateNewLinePlus();
      //Initialize console attributes
      ConsoleTitle = Resources.MsgDefaultConsoleTitle;
      ConsoleGreeting = string.Format(Resources.MsgDefaultConsoleGreeting, this.GetType().Name);
      ConsolePrompt = ">"; 
      ConsolePromptMoreInput = "."; 
    }
    #endregion
    
    #region Reserved words handling
    //Reserved words handling 
    public void MarkReservedWords(params string[] reservedWords) {
      foreach (var word in reservedWords) {
        var wdTerm = ToTerm(word);
        wdTerm.SetOption(TermOptions.IsReservedWord);
        //Reserved words get the highest priority, so they get to be tried before identifiers
        wdTerm.Priority = 1000 + word.Length;
      }
    }
    #endregion 

    #region Register/Mark methods
    public void RegisterPunctuation(params string[] symbols) {
      foreach (string symbol in symbols) {
        KeyTerm term = ToTerm(symbol);
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
        KeyTerm opSymbol = ToTerm(op);
        opSymbol.SetOption(TermOptions.IsOperator);
        opSymbol.Precedence = precedence;
        opSymbol.Associativity = associativity;
      }
    }//method

    public void RegisterOperators(int precedence, params BnfTerm[] opTerms) {
      RegisterOperators(precedence, Associativity.Left, opTerms);
    }
    public void RegisterOperators(int precedence, Associativity associativity, params BnfTerm[] opTerms) {
      foreach (var term in opTerms) {
        term.SetOption(TermOptions.IsOperator);
        term.Precedence = precedence;
        term.Associativity = associativity;
      }
    }

    public void RegisterBracePair(string openBrace, string closeBrace) {
      KeyTerm openS = ToTerm(openBrace);
      KeyTerm closeS = ToTerm(closeBrace);
      openS.SetOption(TermOptions.IsOpenBrace);
      openS.IsPairFor = closeS;
      closeS.SetOption(TermOptions.IsCloseBrace);
      closeS.IsPairFor = openS;
    }
    public void MarkTransient(params NonTerminal[] nonTerminals) {
      foreach (NonTerminal nt in nonTerminals)
        nt.Options |= TermOptions.IsTransient;
    }
    //MemberSelect are symbols invoking member list dropdowns in editor; for ex: . (dot), ::
    public void MarkMemberSelect(params string[] symbols) {
      foreach (var symbol in symbols)
        ToTerm(symbol).SetOption(TermOptions.IsMemberSelect);
    }
    //Sets IsNotReported flag on terminals. As a result the terminal wouldn't appear in expected terminal list
    // in syntax error messages
    public void MarkNotReported(params BnfTerm[] terms) {
      foreach (var term in terms)
        term.SetOption(TermOptions.IsNotReported);
    }
    public void MarkNotReported(params string[] symbols) {
      foreach (var symbol in symbols)
        ToTerm(symbol).SetOption(TermOptions.IsNotReported);
    }

    #endregion

    #region virtual methods: TryMatch, CreateNode, GetSyntaxErrorMessage, CreateRuntime, RunSample
    public virtual void CreateTokenFilters(LanguageData language, TokenFilterList filters) {
    }

    //This method is called if Scanner fails to produce a token; it offers custom method a chance to produce the token    
    public virtual Token TryMatch(ParsingContext context, ISourceStream source) {
      return null;
    }

    public virtual void CreateAstNode(ParsingContext context, ParseTreeNode nodeInfo) {
      var term = nodeInfo.Term;
      if (term.AstNodeCreator != null) {
        term.AstNodeCreator(context, nodeInfo);
        //We assume that Node creator method creates node and initializes it, so parser does not need to call 
        // IAstNodeInit.InitNode() method on node object.
        return;
      }
      Type nodeType = term.AstNodeType ?? this.DefaultNodeType;
      if (nodeType == null) 
        return; //we give a warning on grammar validation about this situation
      nodeInfo.AstNode =  Activator.CreateInstance(nodeType);
      //Initialize node
      var iInit = nodeInfo.AstNode as IAstNodeInit;
      if (iInit != null)
        iInit.Init(context, nodeInfo); 
    }

    /// <summary>
    /// Override this method to help scanner select a terminal to create token when there are more than one candidates
    /// for an input char
    /// </summary>
    public virtual void OnScannerSelectTerminal(SelectTerminalArgs args) {

    }

    /// <summary>
    /// Override this method to provide custom conflict resolution; for example, custom code may decide proper shift or reduce
    /// action based on preview of tokens ahead. 
    /// </summary>
    public virtual void OnResolvingConflict(ConflictResolutionArgs args) {
      //args.Result is Shift by default
    }

    //The method is called after GrammarData is constructed 
    public virtual void OnGrammarDataConstructed(LanguageData language) {
    }

    public virtual void OnLanguageDataConstructed(LanguageData language) {
    }
    //Constructs the error message in situation when parser has no available action for current input.
    // override this method if you want to change this message
    public virtual string ConstructParserErrorMessage(ParsingContext context, ParserState state, BnfTermSet expectedTerms, ParseTreeNode currentInput) {
      return string.Format(Resources.ErrParserUnexpInput, expectedTerms.ToErrorString());
       
    }
    
    public virtual LanguageRuntime CreateRuntime(LanguageData data) {
      return new LanguageRuntime(data);
    }

    //This method allows custom implementation of running a sample in Grammar Explorer
    // By default it evaluates a parse tree using default interpreter 
    public virtual string RunSample(ParseTree parsedSample) {
      var interpreter = new ScriptInterpreter(this);
      interpreter.Evaluate(parsedSample);
      return interpreter.GetOutput(); 
    }


    #endregion

    #region MakePlusRule, MakeStarRule methods
    public static BnfExpression MakePlusRule(NonTerminal listNonTerminal, BnfTerm listMember) {
      return MakePlusRule(listNonTerminal, null, listMember);
    }
    public static BnfExpression MakePlusRule(NonTerminal listNonTerminal, BnfTerm delimiter, BnfTerm listMember) {
      listNonTerminal.SetOption(TermOptions.IsList);
      if (delimiter == null)
        listNonTerminal.Rule = listMember | listNonTerminal + listMember;
      else
        listNonTerminal.Rule = listMember | listNonTerminal + delimiter + listMember;
      return listNonTerminal.Rule;
    }
    public static BnfExpression MakeStarRule(NonTerminal listNonTerminal, BnfTerm listMember) {
      return MakeStarRule(listNonTerminal, null, listMember);
    }
    public static BnfExpression MakeStarRule(NonTerminal listNonTerminal, BnfTerm delimiter, BnfTerm listMember) {
      listNonTerminal.SetOption(TermOptions.IsList);
      if (delimiter == null) {
        //it is much simpler case
        listNonTerminal.Rule = _currentGrammar.Empty | listNonTerminal + listMember;
        return listNonTerminal.Rule;
      }
      //Note that deceptively simple version of the star-rule 
      //       Elem* -> Empty | Elem | Elem* + delim + Elem
      //  does not work when you have delimiters. This simple version allows lists starting with delimiters -
      // which is wrong. The correct formula is to first define "Elem+"-list, and then define "Elem*" list 
      // as "Elem* -> Empty|Elem+" 
      NonTerminal plusList = new NonTerminal(listMember.Name + "+");
      MakePlusRule(plusList, delimiter, listMember);
      plusList.SetOption(TermOptions.IsTransient); //important - mark it as Transient so it will be eliminated from AST tree
      listNonTerminal.Rule = _currentGrammar.Empty | plusList;
      return listNonTerminal.Rule;
    }
    #endregion

    #region Hint utilities
    protected GrammarHint PreferShiftHere() {
      return new GrammarHint(HintType.ResolveToShift, null); 
    }
    protected GrammarHint ReduceHere() {
      return new GrammarHint(HintType.ResolveToReduce, null);
    }
    protected GrammarHint ResolveInCode() {
      return new GrammarHint(HintType.ResolveInCode, null); 
    }
    protected GrammarHint WrapTail() {
      return new GrammarHint(HintType.WrapTail, null);
    }
    protected GrammarHint ImplyPrecedence(int precedence) {
      return new GrammarHint(HintType.Precedence, precedence);
    }

    #endregion

    #region Standard terminals: EOF, Empty, NewLine, Indent, Dedent
    // Empty object is used to identify optional element: 
    //    term.Rule = term1 | Empty;
    public readonly Terminal Empty = new Terminal("EMPTY");
    // The following terminals are used in indent-sensitive languages like Python;
    // they are not produced by scanner but are produced by CodeOutlineFilter after scanning
    public readonly NewLineTerminal NewLine = new NewLineTerminal("LF");
    public readonly Terminal Indent = new Terminal("INDENT", TokenCategory.Outline);
    public readonly Terminal Dedent = new Terminal("DEDENT", TokenCategory.Outline);
    // Identifies end of file
    // Note: using Eof in grammar rules is optional. Parser automatically adds this symbol 
    // as a lookahead to Root non-terminal
    public readonly Terminal Eof = new Terminal("EOF", TokenCategory.Outline);

    //End-of-Statement terminal - used in indentation-sensitive language to signal end-of-statement;
    // it is not always synced with CRLF chars, and CodeOutlineFilter carefully produces Eos tokens
    // (as well as Indent and Dedent) based on line/col information in incoming content tokens.
    public readonly Terminal Eos = new Terminal("EOS", Resources.LabelEosLabel, TokenCategory.Outline);
    
    //Used for error tokens
    public readonly Terminal SyntaxError = new Terminal("SYNTAX_ERROR", TokenCategory.Error);

    public NonTerminal NewLinePlus;

    private NonTerminal CreateNewLinePlus() {
      NewLine.SetOption(TermOptions.IsTransient); 
      var result = new NonTerminal("LF+");
      result.SetOption(TermOptions.IsList);
      result.Rule = NewLine | result + NewLine;
      return result;
    }


    #endregion

    #region KeyTerms (keywords + special symbols)
    public KeyTermTable KeyTerms;

    [Obsolete("Method Symbol(...) is deprecated, use ToTerm(...) instead.")]
    public KeyTerm Symbol(string symbol) {
      return ToTerm(symbol); 
    }

    [Obsolete("Method Symbol(...) is deprecated, use ToTerm(...) instead.")]
    public KeyTerm Symbol(string symbol, string name) {
      return ToTerm(symbol, name); 
    }


    public KeyTerm ToTerm(string text) {
      return ToTerm(text, text);
    }
    public KeyTerm ToTerm(string text, string name) {
      KeyTerm term;
      if (KeyTerms.TryGetValue(text, out term)) {
        //update name if it was specified now and not before
        if (string.IsNullOrEmpty(term.Name) && !string.IsNullOrEmpty(name))
          term.Name = name;
        return term; 
      }
      //create new term
      if (!CaseSensitive)
        text = text.ToLower(CultureInfo.InvariantCulture);
      string.Intern(text); 
      term = new KeyTerm(text, name);
      KeyTerms[text] = term;
      return term; 
    }

    #endregion

    #region CurrentGrammar static field
    //Static per-thread instance; Grammar constructor sets it to self (this). 
    // This field/property is used by operator overloads (which are static) to access Grammar's predefined terminals like Empty,
    //  and SymbolTerms dictionary to convert string literals to symbol terminals and add them to the SymbolTerms dictionary
    [ThreadStatic]
    private static Grammar _currentGrammar;
    public static Grammar CurrentGrammar {
      get { return _currentGrammar; }
    }
    internal static void ClearCurrentGrammar() {
      _currentGrammar = null; 
    }
    #endregion

  }//class

}//namespace

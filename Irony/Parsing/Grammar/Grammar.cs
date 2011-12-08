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
using System.Linq.Expressions;
using System.Text;

namespace Irony.Parsing {

  #region enumerations: LanguageFlags, Associativity, TermListOptions
  [Flags]
  public enum LanguageFlags {
    None = 0,

    //Compilation options
    //Be careful - use this flag ONLY if you use NewLine terminal in grammar explicitly!
    // - it happens only in line-based languages like Basic.
    NewLineBeforeEOF = 0x01,
    //Emit LineStart token
    EmitLineStartToken = 0x02,
    DisableScannerParserLink = 0x04, //in grammars that define TokenFilters (like Python) this flag should be set
    CreateAst = 0x08, //create AST nodes 

    //Runtime
    // CanRunSample = 0x0100, //DEPRECATED, Grammar Explorer uses ICanRunSample interface implementation as indicator/runner of samples
    SupportsCommandLine = 0x0200,
    TailRecursive = 0x0400, //Tail-recursive language - Scheme is one example
    SupportsBigInt = 0x01000,
    SupportsComplex = 0x02000,
    SupportsRational = 0x04000,



    //Default value
    Default = None,
  }

  //Operator associativity types
  public enum Associativity {
    Left,
    Right,
    Neutral  //honestly don't know what that means, but it is mentioned in literature 
  }

  [Flags]
  public enum TermListOptions {
    None = 0,
    AllowEmpty = 0x01,
    AllowTrailingDelimiter = 0x02,

    // In some cases this hint would help to resolve the conflicts that come up when you have two lists separated by a nullable term.
    // This hint would resolve the conflict, telling the parser to include as many as possible elements in the first list, and the rest, if any, would go
    // to the second list. By default, this flag is included in Star and Plus lists. 
    AddPreferShiftHint = 0x04,
    //Combinations - use these 
    PlusList = AddPreferShiftHint, 
    StarList = AllowEmpty | AddPreferShiftHint,
  }
  #endregion

  public partial class Grammar {

    #region properties
    /// <summary>
    /// Gets case sensitivity of the grammar. Read-only, true by default. 
    /// Can be set to false only through a parameter to grammar constructor.
    /// </summary>
    public readonly bool CaseSensitive = true;
    public readonly StringComparer LanguageStringComparer;
    public readonly StringComparison StringComparisonMode; 
    
    //List of chars that unambigously identify the start of new token. 
    //used in scanner error recovery, and in quick parse path in NumberLiterals, Identifiers 
    public string Delimiters = null; 

    public string WhitespaceChars = " \t\r\n\v";
    
    //Used for line counting in source file
    public string LineTerminators = "\n\r\v";

    #region Language Flags
    public LanguageFlags LanguageFlags = LanguageFlags.Default;

    #endregion

    public TermReportGroupList TermReportGroups = new TermReportGroupList();
    
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
    public readonly TerminalSet FallbackTerminals = new TerminalSet();

    public Type DefaultNodeType;
    public Type DefaultLiteralNodeType; //default node type for literals
    public Type DefaultIdentifierNodeType; //default node type for identifiers


    /// <summary>
    /// The main root entry for the grammar. 
    /// </summary>
    public NonTerminal Root;
    
    /// <summary>
    /// Alternative roots for parsing code snippets.
    /// </summary>
    public NonTerminalSet SnippetRoots = new NonTerminalSet();
    
    public string GrammarComments; //shown in Grammar info tab

    public CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

    //Console-related properties, initialized in grammar constructor
    public string ConsoleTitle;
    public string ConsoleGreeting;
    public string ConsolePrompt; //default prompt
    public string ConsolePromptMoreInput; //prompt to show when more input is expected

    public readonly OperatorInfoDictionary OperatorMappings;
    #endregion 

    #region constructors
    
    public Grammar() : this(true) { } //case sensitive by default

    public Grammar(bool caseSensitive) {
      _currentGrammar = this;
      this.CaseSensitive = caseSensitive;
      bool ignoreCase =  !this.CaseSensitive;
      LanguageStringComparer = StringComparer.Create(System.Globalization.CultureInfo.InvariantCulture, ignoreCase);
      StringComparisonMode = CaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
      KeyTerms = new KeyTermTable(LanguageStringComparer);
      //Initialize console attributes
      ConsoleTitle = Resources.MsgDefaultConsoleTitle;
      ConsoleGreeting = string.Format(Resources.MsgDefaultConsoleGreeting, this.GetType().Name);
      ConsolePrompt = ">"; 
      ConsolePromptMoreInput = ".";
      OperatorMappings = OperatorUtility.GetDefaultOperatorMappings(caseSensitive); 
    }
    #endregion
    
    #region Reserved words handling
    //Reserved words handling 
    public void MarkReservedWords(params string[] reservedWords) {
      foreach (var word in reservedWords) {
        var wdTerm = ToTerm(word);
        wdTerm.SetFlag(TermFlags.IsReservedWord);
      }
    }
    #endregion 

    #region Register/Mark methods
    public void RegisterOperators(int precedence, params string[] opSymbols) {
      RegisterOperators(precedence, Associativity.Left, opSymbols);
    }

    public void RegisterOperators(int precedence, Associativity associativity, params string[] opSymbols) {
      foreach (string op in opSymbols) {
        KeyTerm opSymbol = ToTerm(op);
        opSymbol.SetFlag(TermFlags.IsOperator);
        opSymbol.Precedence = precedence;
        opSymbol.Associativity = associativity;
      }
    }//method

    public void RegisterOperators(int precedence, params BnfTerm[] opTerms) {
      RegisterOperators(precedence, Associativity.Left, opTerms);
    }
    public void RegisterOperators(int precedence, Associativity associativity, params BnfTerm[] opTerms) {
      foreach (var term in opTerms) {
        term.SetFlag(TermFlags.IsOperator);
        term.Precedence = precedence;
        term.Associativity = associativity;
      }
    }

    public void RegisterBracePair(string openBrace, string closeBrace) {
      KeyTerm openS = ToTerm(openBrace);
      KeyTerm closeS = ToTerm(closeBrace);
      openS.SetFlag(TermFlags.IsOpenBrace);
      openS.IsPairFor = closeS;
      closeS.SetFlag(TermFlags.IsCloseBrace);
      closeS.IsPairFor = openS;
    }

    public void MarkPunctuation(params string[] symbols) {
      foreach (string symbol in symbols) {
        KeyTerm term = ToTerm(symbol);
        term.SetFlag(TermFlags.IsPunctuation|TermFlags.NoAstNode);
      }
    }
    
    public void MarkPunctuation(params BnfTerm[] terms) {
      foreach (BnfTerm term in terms) 
        term.SetFlag(TermFlags.IsPunctuation|TermFlags.NoAstNode);
    }

    
    public void MarkTransient(params NonTerminal[] nonTerminals) {
      foreach (NonTerminal nt in nonTerminals)
        nt.Flags |= TermFlags.IsTransient | TermFlags.NoAstNode;
    }
    //MemberSelect are symbols invoking member list dropdowns in editor; for ex: . (dot), ::
    public void MarkMemberSelect(params string[] symbols) {
      foreach (var symbol in symbols)
        ToTerm(symbol).SetFlag(TermFlags.IsMemberSelect);
    }
    //Sets IsNotReported flag on terminals. As a result the terminal wouldn't appear in expected terminal list
    // in syntax error messages
    public void MarkNotReported(params BnfTerm[] terms) {
      foreach (var term in terms)
        term.SetFlag(TermFlags.IsNotReported);
    }
    public void MarkNotReported(params string[] symbols) {
      foreach (var symbol in symbols)
        ToTerm(symbol).SetFlag(TermFlags.IsNotReported);
    }

    #endregion

    #region virtual methods: TryMatch, CreateNode, CreateRuntime, RunSample
    public virtual void CreateTokenFilters(LanguageData language, TokenFilterList filters) {
    }

    //This method is called if Scanner fails to produce a token; it offers custom method a chance to produce the token    
    public virtual Token TryMatch(ParsingContext context, ISourceStream source) {
      return null;
    }

    //Gives a way to customize parse tree nodes captions in the tree view. 
    public virtual string GetParseNodeCaption(ParseTreeNode node) {
      if (node.IsError)
        return node.Term.Name + " (Syntax error)";
      if (node.Token != null)
        return node.Token.ToString();
      if(node.Term == null) //special case for initial node pushed into the stack at parser start
        return (node.State != null ? string.Empty : "(State " + node.State.Name + ")"); //  Resources.LabelInitialState;
      var ntTerm = node.Term as NonTerminal;
      if(ntTerm != null && !string.IsNullOrEmpty(ntTerm.NodeCaptionTemplate))
        return ntTerm.GetNodeCaption(node); 
      return node.Term.Name; 
    }

    //Gives a chance of custom AST node creation at Grammar level
    // by default calls Term's method
    public virtual void CreateAstNode(ParsingContext context, ParseTreeNode parseTreeNode) {
      parseTreeNode.Term.CreateAstNode(context, parseTreeNode);
    }

    /// <summary>
    /// Override this method to help scanner select a terminal to create token when there are more than one candidates
    /// for an input char. context.CurrentTerminals contains candidate terminals; leave a single terminal in this list
    /// as the one to use.
    /// </summary>
    public virtual void OnScannerSelectTerminal(ParsingContext context) { }

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
    public virtual string ConstructParserErrorMessage(ParsingContext context, StringSet expectedTerms) {
      return string.Format(Resources.ErrParserUnexpInput, expectedTerms.ToString(", "));
    }

    // Override this method to perform custom error processing
    public virtual void ReportParseError(ParsingContext context) {
        string error = null;
        if (context.CurrentParserInput.Term == this.SyntaxError)
            error = context.CurrentParserInput.Token.Value as string; //scanner error
        else if (context.CurrentParserInput.Term == this.Indent)
            error = Resources.ErrUnexpIndent;
        else if (context.CurrentParserInput.Term == this.Eof && context.OpenBraces.Count > 0) {
            //report unclosed braces/parenthesis
            var openBrace = context.OpenBraces.Peek();
            error = string.Format(Resources.ErrNoClosingBrace, openBrace.Text);
        } else {
            var expectedTerms = context.GetExpectedTermSet(); 
            if (expectedTerms.Count > 0) 
              error = ConstructParserErrorMessage(context, expectedTerms); 
              //error = string.Format(Resources.ErrParserUnexpInput, expectedTerms.ToString(" ")
            else 
              error = Resources.ErrUnexpEof;
        }
        context.AddParserError(error);
    }//method
    #endregion



    #region MakePlusRule, MakeStarRule methods
    public BnfExpression MakePlusRule(NonTerminal listNonTerminal, BnfTerm listMember) {
      return MakeListRule(listNonTerminal, null, listMember);
    }
    public BnfExpression MakePlusRule(NonTerminal listNonTerminal, BnfTerm delimiter, BnfTerm listMember) {
      return MakeListRule(listNonTerminal, delimiter, listMember);
    }
    public BnfExpression MakeStarRule(NonTerminal listNonTerminal, BnfTerm listMember) {
      return MakeListRule(listNonTerminal, null, listMember, TermListOptions.StarList);
    }
    public BnfExpression MakeStarRule(NonTerminal listNonTerminal, BnfTerm delimiter, BnfTerm listMember) {
      return MakeListRule(listNonTerminal, delimiter, listMember, TermListOptions.StarList);
    }

    //Note: Here and in other make-list methods with delimiter. More logical would be the parameters order (list, listMember, delimiter=null).
    // But for historical reasons it's the way it is, and I think it's too late to change and to reverse the order of delimiter and listMember.
    // Too many existing grammars would be broken. The big trouble is that these two parameters are of the same type, so compiler would not 
    // detect that order had changed (if we change it) for existing grammars. The grammar would stop working at runtime, and it would 
    // require some effort to debug and find the cause of the problem. For these reasons, we leave it as is. 
    [Obsolete("Method overload is obsolete - use MakeListRule instead")]
    public BnfExpression MakePlusRule(NonTerminal listNonTerminal, BnfTerm delimiter, BnfTerm listMember, TermListOptions options) {
      return MakeListRule(listNonTerminal, delimiter, listMember, options);
   }

    [Obsolete("Method overload is obsolete - use MakeListRule instead")]
    public BnfExpression MakeStarRule(NonTerminal listNonTerminal, BnfTerm delimiter, BnfTerm listMember, TermListOptions options) {
      return MakeListRule(listNonTerminal, delimiter, listMember, options | TermListOptions.StarList);
    }

    protected BnfExpression MakeListRule(NonTerminal list, BnfTerm delimiter, BnfTerm listMember, TermListOptions options = TermListOptions.PlusList) {
      //If it is a star-list (allows empty), then we first build plus-list
      var isStarList = options.IsSet(TermListOptions.AllowEmpty);
      NonTerminal plusList = isStarList ? new NonTerminal(listMember.Name + "+") : list;
      //"list" is the real list for which we will construct expression - it is either extra plus-list or original listNonTerminal. 
      // In the latter case we will use it later to construct expression for listNonTerminal
      plusList.Rule = plusList;  // rule => list
      if (delimiter != null)
        plusList.Rule += delimiter;  // rule => list + delim
      if (options.IsSet(TermListOptions.AddPreferShiftHint))
        plusList.Rule += PreferShiftHere(); // rule => list + delim + PreferShiftHere()
      plusList.Rule += listMember;          // rule => list + delim + PreferShiftHere() + elem
      plusList.Rule |= listMember;        // rule => list + delim + PreferShiftHere() + elem | elem
      //trailing delimiter
      if (options.IsSet(TermListOptions.AllowTrailingDelimiter) & delimiter != null)
        plusList.Rule |= list + delimiter; // => list + delim + PreferShiftHere() + elem | elem | list + delim
      // set Rule value
      plusList.SetFlag(TermFlags.IsList);
      //If we do not use exra list - we're done, return list.Rule
      if (plusList == list) 
        return list.Rule;
      // Let's setup listNonTerminal.Rule using plus-list we just created
      //If we are here, TermListOptions.AllowEmpty is set, so we have star-list
      list.Rule = Empty | plusList;
      plusList.SetFlag(TermFlags.NoAstNode); 
      list.SetFlag(TermFlags.IsListContainer); //indicates that real list is one level lower
      return list.Rule; 
    }//method
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
    protected TokenPreviewHint ReduceIf(string thisSymbol, params string[] comesBefore) {
      return new TokenPreviewHint(ParserActionType.Reduce, thisSymbol, comesBefore);
    }
    protected TokenPreviewHint ReduceIf(Terminal thisSymbol, params Terminal[] comesBefore) {
      return new TokenPreviewHint(ParserActionType.Reduce, thisSymbol, comesBefore);
    }
    protected TokenPreviewHint ShiftIf(string thisSymbol, params string[] comesBefore) {
      return new TokenPreviewHint(ParserActionType.Shift, thisSymbol, comesBefore);
    }
    protected TokenPreviewHint ShiftIf(Terminal thisSymbol, params Terminal[] comesBefore) {
      return new TokenPreviewHint(ParserActionType.Shift, thisSymbol, comesBefore);
    }
    protected GrammarHint ImplyPrecedenceHere(int precedence) {
      return ImplyPrecedenceHere(precedence, Associativity.Left); 
    }
    protected GrammarHint ImplyPrecedenceHere(int precedence, Associativity associativity) {
      var hint = new GrammarHint(HintType.Precedence, null);
      hint.Precedence = precedence;
      hint.Associativity = associativity;
      return hint; 
    }

    #endregion

    #region Term report group methods
    /// <summary>
    /// Creates a terminal reporting group, so all terminals in the group will be reported as a single "alias" in syntex error messages like
    /// "Syntax error, expected: [list of terms]"
    /// </summary>
    /// <param name="alias">An alias for all terminals in the group.</param>
    /// <param name="symbols">Symbols to be included into the group.</param>
    protected void AddTermsReportGroup(string alias, params string[] symbols) {
      TermReportGroups.Add(new TermReportGroup(alias, TermReportGroupType.Normal, SymbolsToTerms(symbols)));
    }
    /// <summary>
    /// Creates a terminal reporting group, so all terminals in the group will be reported as a single "alias" in syntex error messages like
    /// "Syntax error, expected: [list of terms]"
    /// </summary>
    /// <param name="alias">An alias for all terminals in the group.</param>
    /// <param name="terminals">Terminals to be included into the group.</param>
    protected void AddTermsReportGroup(string alias, params Terminal[] terminals) {
      TermReportGroups.Add(new TermReportGroup(alias, TermReportGroupType.Normal, terminals));
    }
    /// <summary>
    /// Adds symbols to a group with no-report type, so symbols will not be shown in expected lists in syntax error messages. 
    /// </summary>
    /// <param name="symbols">Symbols to exclude.</param>
    protected void AddToNoReportGroup(params string[] symbols) {
      TermReportGroups.Add(new TermReportGroup(string.Empty, TermReportGroupType.DoNotReport, SymbolsToTerms(symbols)));
    }
    /// <summary>
    /// Adds symbols to a group with no-report type, so symbols will not be shown in expected lists in syntax error messages. 
    /// </summary>
    /// <param name="symbols">Symbols to exclude.</param>
    protected void AddToNoReportGroup(params Terminal[] terminals) {
      TermReportGroups.Add(new TermReportGroup(string.Empty, TermReportGroupType.DoNotReport, terminals));
    }
    /// <summary>
    /// Adds a group and an alias for all operator symbols used in the grammar.
    /// </summary>
    /// <param name="alias">An alias for operator symbols.</param>
    protected void AddOperatorReportGroup(string alias) {
      TermReportGroups.Add(new TermReportGroup(alias, TermReportGroupType.Operator, null)); //operators will be filled later
    }

    private IEnumerable<Terminal> SymbolsToTerms(IEnumerable<string> symbols) {
      var termList = new TerminalList(); 
      foreach(var symbol in symbols)
        termList.Add(ToTerm(symbol));
      return termList; 
    }
    #endregion

    #region Standard terminals: EOF, Empty, NewLine, Indent, Dedent
    // Empty object is used to identify optional element: 
    //    term.Rule = term1 | Empty;
    public readonly Terminal Empty = new Terminal("EMPTY");
    // The following terminals are used in indent-sensitive languages like Python;
    // they are not produced by scanner but are produced by CodeOutlineFilter after scanning
    public readonly NewLineTerminal NewLine = new NewLineTerminal("LF");
    public readonly Terminal Indent = new Terminal("INDENT", TokenCategory.Outline, TermFlags.IsNonScanner);
    public readonly Terminal Dedent = new Terminal("DEDENT", TokenCategory.Outline, TermFlags.IsNonScanner);
    //End-of-Statement terminal - used in indentation-sensitive language to signal end-of-statement;
    // it is not always synced with CRLF chars, and CodeOutlineFilter carefully produces Eos tokens
    // (as well as Indent and Dedent) based on line/col information in incoming content tokens.
    public readonly Terminal Eos = new Terminal("EOS", Resources.LabelEosLabel, TokenCategory.Outline, TermFlags.IsNonScanner);
    // Identifies end of file
    // Note: using Eof in grammar rules is optional. Parser automatically adds this symbol 
    // as a lookahead to Root non-terminal
    public readonly Terminal Eof = new Terminal("EOF", TokenCategory.Outline);

    //Used as a "line-start" indicator
    public readonly Terminal LineStartTerminal = new Terminal("LINE_START", TokenCategory.Outline);

    //Used for error tokens
    public readonly Terminal SyntaxError = new Terminal("SYNTAX_ERROR", TokenCategory.Error, TermFlags.IsNonScanner);

    public NonTerminal NewLinePlus {
      get {
        if(_newLinePlus == null) {
          _newLinePlus = new NonTerminal("LF+");
          //We do no use MakePlusRule method; we specify the rule explicitly to add PrefereShiftHere call - this solves some unintended shift-reduce conflicts
          // when using NewLinePlus 
          _newLinePlus.Rule = NewLine | _newLinePlus + PreferShiftHere() + NewLine;
          MarkPunctuation(_newLinePlus);
          _newLinePlus.SetFlag(TermFlags.IsList);
        }
        return _newLinePlus;
      }
    } NonTerminal _newLinePlus;

    public NonTerminal NewLineStar {
      get {
        if(_newLineStar == null) {
          _newLineStar = new NonTerminal("LF*");
          MarkPunctuation(_newLineStar);
          _newLineStar.Rule = MakeStarRule(_newLineStar, NewLine);
        }
        return _newLineStar;
      }
    } NonTerminal _newLineStar;

    #endregion

    #region KeyTerms (keywords + special symbols)
    public KeyTermTable KeyTerms;

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

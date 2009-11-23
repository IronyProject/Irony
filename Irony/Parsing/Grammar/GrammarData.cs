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
using System.Linq;
using System.Text;

namespace Irony.Parsing { 

  //GrammarData is a container for all basic info about the grammar
  // GrammarData is a field in LanguageData object. 
  public class GrammarData {
    public readonly LanguageData Language; 
    public readonly Grammar Grammar;
    public NonTerminal AugmentedRoot;
    public readonly BnfTermSet AllTerms = new BnfTermSet();
    public readonly TerminalList Terminals = new TerminalList();
    public readonly NonTerminalList NonTerminals = new NonTerminalList();
    public string WhitespaceAndDelimiters { get; internal set; }

    public GrammarData(LanguageData language) {
      Language = language;
      Grammar = language.Grammar;
    }

  }//class

  [Flags]
  public enum LanguageFlags {
    None = 0,

    //Compilation options
    //Be careful - use this flag ONLY if you use NewLine terminal in grammar explicitly!
    // - it happens only in line-based languages like Basic.
    NewLineBeforeEOF = 0x01,
    //Automatically detect transient non-terminals - whose rules are just OR of other single terms
    // nodes for these terminals would be eliminated from parse tree. Formerly this stuff was called "node bubbling"
    AutoDetectTransient = 0x02,
    DisableScannerParserLink = 0x04, //in grammars that define TokenFilters (like Python) this flag should be set
    CreateAst = 0x08, //create AST nodes 

    //Runtime
    CanRunSample = 0x0100,
    SupportsCommandLine = 0x0200,
    TailRecursive = 0x0400, //Tail-recursive language - Scheme is one example

    //Default value
    Default = None,
  }

  public enum ParseMethod {
    Lalr,    //canonical LALR
    Nlalr,   //non-canonical LALR
  }

  //Operator associativity types
  public enum Associativity {
    Left,
    Right,
    Neutral  //don't know what that means 
  }

}//namespace

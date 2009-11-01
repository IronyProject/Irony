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

  public class TerminalLookupTable : Dictionary<char, TerminalList> { }

  // ScannerData is a container for all detailed info needed by scanner to read input. 
  public class ScannerData {
    public readonly LanguageData Language;
    public readonly TerminalLookupTable TerminalsLookup = new TerminalLookupTable(); //hash table for fast terminal lookup by input char
    public readonly TerminalList FallbackTerminals = new TerminalList(); //terminals that have no explicit prefixes
    public string ScannerRecoverySymbols; //whitespace plus delimiters - chars to look for when recovering
    public char[] LineTerminators; //used for line counting
    public readonly TerminalList MultilineTerminals = new TerminalList();

    public ScannerData(LanguageData language) {
      Language  = language;
    }
  }//class

  public class SelectTerminalArgs : EventArgs {
    public ParsingContext Context;
    public Scanner Scanner;
    public char Current;
    public TerminalList Terminals;
    public Terminal SelectedTerminal;
    internal void SetData(ParsingContext context, char current, TerminalList terminals) {
      Context = context;
      Scanner = Context.Parser.Scanner; 
      Current = current;
      Terminals = terminals;
      SelectedTerminal = null; 
    }

  }
}//namespace

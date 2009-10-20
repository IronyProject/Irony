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
using System.Runtime.InteropServices;

namespace Irony.Parsing {

  public class TerminalLookupTable : Dictionary<char, TerminalList> { }

  // ScannerData is a container for all info needed by scanner to read input. 
  // ScannerData is a field in LanguageData structure and is used by Scanner. 
  public class ScannerData {
    public readonly LanguageData Language;
    public readonly TerminalLookupTable TerminalsLookup = new TerminalLookupTable(); //hash table for fast terminal lookup by input char
    public readonly TerminalList FallbackTerminals = new TerminalList(); //terminals that have no explicit prefixes
    public string ScannerRecoverySymbols;
    public char[] LineTerminators; //used for line counting
    public readonly TerminalList MultilineTerminals = new TerminalList();
//    public readonly TokenFilterList TokenFilters = new TokenFilterList(); 

    public ScannerData(LanguageData language) {
      Language  = language;
    }
  }//class

  // A struct used for packing/unpacking ScannerState int value; used for VS integration.
  [StructLayout(LayoutKind.Explicit)]
  public struct VsScannerStateMap {
    [FieldOffset(0)]
    public int Value;
    [FieldOffset(0)]
    public byte TerminalIndex;   //1-based index of active multiline term in MultilineTerminals
    [FieldOffset(1)]
    public byte TokenSubType;         //terminal subtype (used in StringLiteral to identify string kind)
    [FieldOffset(2)]
    public short TerminalFlags;  //Terminal flags
  }//struct

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

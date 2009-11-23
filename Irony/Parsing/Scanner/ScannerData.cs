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

  public class TerminalLookupTable : Dictionary<char, TerminalList> { 
    public TerminalList FallbackTerminals = new TerminalList(); //to return when there is no key/value pair

    public TerminalList this[char key] {
      get {
      TerminalList value; 
      if (base.TryGetValue(key, out value))
        return value; 
      //otherwise return FallbackTerminals
      return FallbackTerminals;
      }
      set { base[key] = value; }
    }//this
  }

  // ScannerData is a container for all detailed info needed by scanner to read input. 
  public class ScannerData {
    public readonly LanguageData Language;
    public readonly TerminalLookupTable TerminalsLookup = new TerminalLookupTable(); //hash table for fast terminal lookup by input char
    public readonly TerminalList MultilineTerminals = new TerminalList();
    public char[] LineTerminatorsArray; //used for line counting

    public ScannerData(LanguageData language) {
      Language  = language;
    }
  }//class

}//namespace

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

  public enum TokenCategory {
    Content,
    Outline, //newLine, indent, dedent
    Comment,
    Error,
  }

  public enum TokenMatchMode {
    ByValue = 1,
    ByType = 2,
    ByValueThenByType = ByValue | ByType,
  }

  //Operator associativity types
  public enum Associativity {
    Left,
    Right,
    Neutral  //don't know what that means 
  }

  public enum TermOptions {
    None = 0,
    IsOperator = 0x01,
    IsGrammarSymbol = 0x02,
    IsOpenBrace  = 0x04,
    IsCloseBrace = 0x08,
    IsBrace = IsOpenBrace | IsCloseBrace,
    IsConstant    = 0x10,
    IsPunctuation = 0x20,
    IsDelimiter   = 0x40, 
    IsList        = 0x80,
    IsStarList =   0x100,   //Special case of list with 0 or more elements separated by delimiters. Produced by MakeStarRule method
    IsNonGrammar = 0x0200,  // if set, parser would eliminate the token from the input stream; terms in Grammar.NonGrammarTerminals have this flag set
    
    //Number flags 
    SpecialIgnoreCase = 0x010000,         //Ignore case in suffixes and prefixes
    EnableQuickParse = 0x020000,
    CanStartWithEscape = 0x040000,
    NumberAllowStartEndDot = 0x100000,     //python : http://docs.python.org/ref/floating.html
    NumberIntOnly = 0x200000,


  }

  public enum ScanFlags {
    None = 0,

    //Number flags
		Binary = Bit0, //e.g. GNU GCC C Extension supports binary number literals
    Octal = Bit1,
		//Decimal = Bit2,
    Hex = Bit3,
    NonDecimal = Bit0 | Bit1 | Bit3, 
    HasDot = Bit4,
    HasExp = Bit5,
    HasDotOrExp = HasDot | HasExp,

    //String flags
    IsChar = Bit0,
    AllowDoubledQuote = Bit1, //Convert doubled start/end symbol to a single symbol; for ex. in SQL, '' -> '
    AllowLineBreak = Bit2,
    LineBreakEscaped = Bit3,
    DisableEscapes = Bit4, //also used by IdentifierTerminal
    AllowUEscapes = Bit5, //also used by IdentifierTerminal
    AllowXEscapes = Bit6,
    AllowOctalEscapes = Bit7,
    AllowAllEscapes = AllowUEscapes | AllowXEscapes | AllowOctalEscapes,
    HasEscapes = Bit8, //also used by IdentifierTerminal

    //Identifier
    IncludePrefix = Bit0,
    IsNotKeyword = Bit1,

    Bit0 = 0x01,
    Bit1 = 0x02,
    Bit2 = 0x04,
    Bit3 = 0x08,
    Bit4 = 0x10,
    Bit5 = 0x20,
    Bit6 = 0x40,
    Bit7 = 0x80,

    Bit8 = 0x0100,
    Bit9 = 0x0200,
    Bit10 = 0x0400,
    Bit11 = 0x0800,
    Bit12 = 0x1000,
    Bit13 = 0x2000,
    Bit14 = 0x4000,
    Bit15 = 0x8000,

  }//enum


}

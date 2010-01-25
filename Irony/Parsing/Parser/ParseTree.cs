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

  public enum ParseTreeStatus {
    Parsing,
    Partial,
    Parsed,
    Error,
  }

  public class ParseTree {
    public ParseTreeStatus Status {get; internal set;}
    public readonly string SourceText;
    public readonly string FileName; 
    public readonly TokenList Tokens = new TokenList();
    public readonly TokenList OpenBraces = new TokenList(); 
    public ParseTreeNode Root;
    public readonly ParserMessageList ParserMessages = new ParserMessageList();
    public int ParseTime;

    public ParseTree(string sourceText, string fileName) {
      SourceText = sourceText;
      FileName = fileName;
      Status = ParseTreeStatus.Parsing;
    }

    public bool HasErrors() {
      if (ParserMessages.Count == 0) return false;
      foreach (var err in ParserMessages)
        if (err.Level == ParserErrorLevel.Error) return true;
      return false; 
    }//method

    public void CopyMessages(ParserMessageList others, SourceLocation baseLocation, string messagePrefix) {
      foreach(var other in others) 
        this.ParserMessages.Add(new ParserMessage(other.Level, baseLocation + other.Location, messagePrefix + other.Message, other.ParserState)); 
    }//

  }//class

}

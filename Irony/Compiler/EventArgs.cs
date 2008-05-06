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

  public class TokenEventArgs : EventArgs {
    internal TokenEventArgs(Token token) {
      _token = token;
    }
    public Token Token  {
      get {return _token;}
      set { _token = value; }
    } Token  _token;

  }//class

  public class ParserActionEventArgs : EventArgs {
    public ParserActionEventArgs(ParserState state, Token input, ActionRecord action) {
      State = state;
      Input = input;
      Action = action;
    }

    public readonly ParserState State;
    public readonly Token Input;
    public ActionRecord Action;

    public override string ToString() {
      return State + "/" + Input + ": " + Action;
    }
  }//class

  public class ScannerConvertingValueEventArgs : EventArgs {
    public ScannerConvertingValueEventArgs(ScanDetails details) {
      this.Details = details;
    }
    public readonly ScanDetails Details;
    public object Value;
    public bool Converted;
  }//class

  public class NodeCreatedEventArgs : EventArgs {
    public NodeCreatedEventArgs(AstNode node) {
      Node = node;
    }
    public readonly AstNode Node;
  }

}//namespace

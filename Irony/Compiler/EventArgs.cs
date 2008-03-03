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
      _state = state;
      _input = input;
      _action = action;
    }

    public ParserState State  {
      get {return _state;}
    } ParserState  _state;

    public Token Input  {
      get {return _input;}
    } Token  _input;

    public ActionRecord Action  {
      get {return _action;}
      set {_action = value;}
    } ActionRecord  _action;

    public override string ToString() {
      return _state + "/" + _input + ": " + _action;
    }
  }//class

  public class NumberScanEventArgs : EventArgs {
    public NumberScanEventArgs(NumberTerminal.NumberScanInfo info) {
      this.Info = info;
    }
    public readonly NumberTerminal.NumberScanInfo Info;
  }//class

}//namespace

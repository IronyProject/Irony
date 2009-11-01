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

  public class ValidateTokenEventArgs : EventArgs {
    public ParsingContext Context;
    public ISourceStream Source;
    public TerminalList Terminals; //all terminals that are candidates for producing a token
    public Token Token {
      get { return _token; }
    } Token _token;

    internal void Init(ParsingContext context, ISourceStream source, TerminalList terminals, Token token) {
      Context = context;
      Source = source;
      Terminals = terminals;
      _token = token; 
    }    

    public void SetError(string errorMessage, params object[] args) {
      _token = Source.CreateErrorToken(errorMessage, args);
    }

    public void ReplaceToken(Token token) {
      _token = token;
    }
    public void RejectToken() {
      _token = null; 
    }

  }//EventArgs



}//namespace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {

  public class ValidateTokenEventArgs : EventArgs {
    public CompilerContext Context;
    public ISourceStream Source;
    public TerminalList Terminals; //all terminals that are candidates for producing a token
    public Token Token {
      get { return _token; }
    } Token _token;

    internal void Init(CompilerContext context, ISourceStream source, TerminalList terminals, Token token) {
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

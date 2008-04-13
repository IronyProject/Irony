using System;
using System.Collections.Generic;
using System.Text;

namespace Irony.Compiler {
  //This struct is a container for information used by AstNode constructor (and its descendents). 
  // Using this struct simplifies the signature of custom AST nodes and it allows to easily add parameters in the future
  // if such need arises, without breaking existing code (of custom AST nodes). 
  public struct AstNodeArgs {
    public BnfTerm Term;
    public CompilerContext Context;
    public SourceSpan Span;
    public AstNodeList ChildNodes;
    public AstNodeArgs(BnfTerm term, CompilerContext context, SourceSpan span, AstNodeList childNodes) {
      Context = context;
      Term = term;
      Span = span;
      ChildNodes = childNodes;
    }
    public NonTerminal NonTerminal { 
      get { return Term as NonTerminal; } 
    }
    public Terminal Terminal { 
      get { return Term as Terminal; } 
    }
  }//struct

}//namespace

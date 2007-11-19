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
using Irony.Compiler;

namespace Irony.Samples.Scheme {
  //The following class is an example of custom AST node. Notice how child's tag appears as prefix in node caption
  // in AST view in GrammarExplorer form.
  public class IfNode : GenericNode {
    public IfNode(CompilerContext context, NonTerminal nonTerminal, SourceLocation location, AstNodeList childNodes)
           : base(context, nonTerminal, location, childNodes) {
      _condition = childNodes[2];
      _ifTrue = childNodes[3];
      _ifFalse = childNodes[4];
      _condition.Tag = "Cond";
      _ifTrue.Tag = "IfTrue";
      if (_ifFalse != null) _ifFalse.Tag = "IfFalse";
    }

    public AstNode Condition  {
      get {return _condition;}
    } AstNode  _condition;

    public AstNode IfTrue {
      get {return _ifTrue;}
    } AstNode  _ifTrue;

    public AstNode IfFalse  {
      get {return _ifFalse;}
    } AstNode  _ifFalse;

  }//class

}//namespace

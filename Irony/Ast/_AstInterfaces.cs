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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter;

namespace Irony.Ast { 

  //Basic interface for AST nodes; Init method is the chance for AST node to get references to its child nodes, and all 
  // related information gathered during parsing
  public interface IAstNodeInit {
    void Init(ParsingContext context, ParseTreeNode parseNode);
  }

  public interface IBrowsableAstNode {
    SourceLocation Location {get;} 
    IEnumerable GetChildNodes();
  }

  [Flags]
  public enum AstMode {
    None = 0,
    Read = 0x01,
    Write = 0x02,
    Tail = 0x10,
  }

  public delegate void NodeEvaluate(EvaluationContext context, AstMode mode); 

  public interface IInterpretedAstNode {
    void Evaluate(EvaluationContext context, AstMode mode);
  }

  public interface IAstVisitor {
    void BeginVisit(AstNode node);
    void EndVisit(AstNode node);
  }



}

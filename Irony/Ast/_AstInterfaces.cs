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
using Irony.Parsing;
using Irony.Ast.Interpreter;

namespace Irony.Ast { 

  //Basic interface for AST nodes; Init method is the chance for AST node to get references to its child nodes, and all 
  // related information gathered during parsing
  public interface IAstNodeInit {
    void Init(CompilerContext context, ParseTreeNode parseNode);
  }

  public enum AstUseMode {
    Read = 1,
    Write = 2,
  }

  public interface IInterpretedNode {
    object Evaluate(EvaluationContext context, AstUseMode useMode);
  }

  public interface IAstVisitor {
    void BeginVisit(AstNode node);
    void EndVisit(AstNode node);
  }



}

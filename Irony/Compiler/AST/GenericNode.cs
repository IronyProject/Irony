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
  //Generic node is used as default node for non-terminals. It holds collection of child nodes.
  public class GenericNode : AstNode {
    public GenericNode(CompilerContext context, NonTerminal nonTerminal, SourceLocation location, AstNodeList children)
               :  base(context, nonTerminal, location, children) {
      if (nonTerminal.IsList && children.Count > 0 && children[0] != null) {
        GenericNode gen0 = children[0] as GenericNode;
        if (gen0 != null && gen0.Element == nonTerminal) {
          ChildNodes.AddRange(gen0.ChildNodes);
          ChildNodes.Add(children[children.Count - 1]);
          return;
        }
      }//if 
      //otherwise
      ChildNodes.AddRange(children);
    }
    public readonly AstNodeList ChildNodes = new AstNodeList();

  }//class
}//namespace

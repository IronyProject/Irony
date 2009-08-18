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
using System.Xml;
using Irony.Ast.Interpreter;

namespace Irony.Ast {
  public enum AccessType {
    None,
    Read,
    Write
  }
  public class VarRefNode : AstNode {
    public string Name;

    public VarRefNode(NodeArgs args, AstNode idNode) : base(args) {
      ChildNodes.Clear();
      Name = GetContent(idNode);
    }
    public VarRefNode(NodeArgs args, string name) : base(args) {
      ChildNodes.Clear();
      Name = name;
    }
    public VarRefNode(NodeArgs args) : this(args, args.ChildNodes[0]) {
    }




    public override string ToString() {
      return Name;
    }

  }//class

}

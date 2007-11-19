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
using System.CodeDom;

namespace Irony.Compiler {

  public class AstNodeList : List<AstNode> {}

  //Base AST node class
  public class AstNode {
    public AstNode(CompilerContext context, BnfElement element, SourceLocation location, AstNodeList childNodes) {
      Element = element;
      Location = location;
    }

    #region properties Element, Location, CodeDomObject, Tag
    public readonly BnfElement Element;
    public readonly SourceLocation Location;
    
    public CodeObject CodeDomObject {
      get { return _codeDomObject; }
      set { _codeDomObject = value; }
    }  CodeObject _codeDomObject;

    // Tag is a free-form string used as prefix in ToString() representation of the node. 
    // Node's parent can set it to "property name" or role of the child node in parent's node context. 
    public string Tag  {
      get {return _tag;}
      set {_tag = value;}
    } string  _tag;

    #endregion

    public override string ToString() {
      if (string.IsNullOrEmpty(_tag))
        return Element.Name;
      else
        return Tag + ":" + Element.Name;
    }

  }//class

}//namespace

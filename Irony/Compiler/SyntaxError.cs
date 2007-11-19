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

  //Container for syntax error
  public class SyntaxError {
    public SyntaxError(SourceLocation location, string message, ParserState state) {
      Location = location;
      Message = message;
      State = state;
    }

    public readonly SourceLocation Location;
    public readonly string Message;
    public readonly ParserState State;

    public override string ToString() {
      return Message;
    }
  }//class

  public class SyntaxErrorList : List<SyntaxError> { }

}//namespace

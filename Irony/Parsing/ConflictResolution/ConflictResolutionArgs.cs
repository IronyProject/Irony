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
  /// <summary>
  /// The class provides arguments for custom conflict resolution grammar method.
  /// </summary>
  public class ConflictResolutionArgs : EventArgs {
    public readonly ParsingContext Context;
    public readonly Scanner Scanner;
    //Results 
    public PreferredActionType Result; //shift, reduce or operator
    //constructor
    internal ConflictResolutionArgs(ParsingContext context, ParserAction conflictAction) {
      Context = context;
      Scanner = context.Parser.Scanner;
    }
  }//class

}

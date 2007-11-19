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
  public struct ParserStackElement {
    public readonly AstNode Node;
    public readonly ParserState State;
    public readonly SourceLocation Location;
    public ParserStackElement(AstNode node,  SourceLocation location, ParserState state) {
      Node = node;
      Location = location;
      State = state;
    }
    public override string ToString() {
      return State.Name + " " + Node.ToString();
    }
  }


  public class ParserStack  {
    private ParserStackElement[] _data = new ParserStackElement[100]; 
    
    public int Count  {
      get {return _count;}
    } int  _count; //actual count of elements currently in stack

    public ParserStackElement this[int index] {
      get { return _data[index]; }
    }
    public void Push(AstNode node, SourceLocation location, ParserState state) {
      if (_count == _data.Length) 
        ExtendData();
      _data[_count] = new ParserStackElement(node, location, state);
      _count++;
    }
    public void Pop(int popCount) {
      _count -= popCount;
    }
    public void Reset() {
      _count = 0;
    }
    private void ExtendData() {
      ParserStackElement[] newData = new ParserStackElement[_data.Length + 100];
      Array.Copy(_data, newData, _data.Length);
      _data = newData;
    }
  }//class


}

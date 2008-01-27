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

  public class TerminalList : List<Terminal> { }
  

  public class Terminal : BnfElement {

    public Terminal(string name)  : base(name) {
    }
    public Terminal(string name, TokenCategory category)  : this(name) {
      _category = category;
    }
    public Terminal(string name, TokenMatchMode matchMode) : this(name) {
      this.MatchMode = matchMode;
    }
    public TokenMatchMode MatchMode {
      get { return _matchMode; }
      set { _matchMode = value; }
    } TokenMatchMode _matchMode = TokenMatchMode.ByValueThenByType;

    //Terminals are not nullable
    public override bool Nullable {
      get { return false; }
      set { }
    }
    public TokenCategory Category {
      get { return _category; }
      protected set { _category = value; }
    } TokenCategory _category = TokenCategory.Content;


    //Priority is used when more than one terminal matches the input. 
    // When choosing the token, scanner would check priority, then token length. 
    public int Priority  {
      get {return _priority;}
      set {_priority = value;}
    } int  _priority; //default is 0

    #region virtuals
    public virtual Token TryMatch(CompilerContext context, ISourceStream source) {
      return null;
    }
    //Prefixes are used for quick search for possible matching terminal(s) using current character in the input stream.
    // A terminal might declare no prefixes. In this case, the terminal is tried for match for any current input character. 
    public virtual IList<string> GetPrefixes() {
      return null;
    }
    #endregion

    #region static comparison methods
    public static int ByName(Terminal x, Terminal y) {
      return string.Compare(x.ToString(), y.ToString());
    }
    public static int ByPriorityReverse(Terminal x, Terminal y) {
      if (x.Priority > y.Priority)
        return -1;
      if (x.Priority == y.Priority)
        return 0;
      return 1;
    }


    #endregion
  }//class



}

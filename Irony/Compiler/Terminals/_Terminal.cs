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

    protected Grammar Grammar  {
      get {return _grammar;}
    } Grammar  _grammar;

    #region virtuals
    public virtual void Init(Grammar grammar) {
      _grammar = grammar;
    }
    public virtual Token TryMatch(CompilerContext context, ISourceStream source) {
      return null;
    }
    //Prefixes are used for quick search for possible matching terminal(s) using current character in the input stream.
    // A terminal might declare no prefixes. In this case, the terminal is tried for match for any current input character. 
    public virtual IList<string> GetPrefixes() {
      return null;
    }
    #endregion

  }//class



}

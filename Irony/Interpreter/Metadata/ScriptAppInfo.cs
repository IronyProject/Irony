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
using System.Threading;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace Irony.Interpreter {

  /// <summary>
  /// Represents a structure (full meta-data) of a script app. 
  /// </summary>
  public class ScriptAppInfo  {
    public ModuleInfo[] Modules;
    public ModuleInfo Main; 
    public LanguageRuntime Runtime;
    public readonly bool LanguageCaseSensitive;

    public ScriptAppInfo(LanguageRuntime runtime, ParseTree parseTree) {
      Runtime = runtime;
      LanguageCaseSensitive = Runtime.Language.Grammar.CaseSensitive;
      if (parseTree != null) {
        var root = parseTree.Root.AstNode as AstNode;
        //root.Reset(); 
        if (root != null && root.Module != null)
          this.Main = root.Module;
        else {
          this.Main = new ModuleInfo("main", parseTree.FileName, LanguageCaseSensitive);
          this.Main.Node = root;
          root.Module = Main;
        }
      } else
        Main = new ModuleInfo("main", "main", LanguageCaseSensitive);
      Modules = new ModuleInfo[] { Main };
    }

    public void Execute(ScriptThread thread) {
      try {
        var root = Main.Node;
        if (root == null)
          thread.ThrowScriptError("No root node.");
        var result = root.Evaluate(thread);
        if (result != null)
          thread.App.WriteLine(result.ToString());
      } catch (ScriptException) {
        throw;
      }
    }

  }//class

}

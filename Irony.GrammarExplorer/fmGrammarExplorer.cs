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
//with contributions by Andrew Bradnan
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;
using Irony.Compiler;
using Irony.Compiler.Lalr;
using Irony.Runtime;
using Irony.GrammarExplorer.Properties;
using System.Reflection;


namespace Irony.GrammarExplorer {
  public partial class fmGrammarExplorer : Form {
    public fmGrammarExplorer() {
      InitializeComponent();
    }
    private void fmExploreGrammar_Load(object sender, EventArgs e) {
      try {
        cboLanguage.SelectedIndex = Settings.Default.LanguageIndex;
        txtSource.Text = Settings.Default.SourceSample;
        txtSearch.Text = Settings.Default.SearchPattern; 
      } catch { }
    }
    private void fmExploreGrammar_FormClosing(object sender, FormClosingEventArgs e) {
      Settings.Default.SourceSample = txtSource.Text;
      Settings.Default.LanguageIndex = cboLanguage.SelectedIndex;
      Settings.Default.SearchPattern = txtSearch.Text;
      Settings.Default.Save();
    }//method



    LanguageCompiler Compiler  {
      get {return _compiler;}
    } LanguageCompiler  _compiler;
    AstNode _rootNode;

    private void btnParse_Click(object sender, EventArgs e) {
      ParseSample();
    }

    private void ParseSample() {
      _rootNode = null;
      ResetResultPanels();
      if (chkShowTrace.Checked) {
        Compiler.LalrParser.ActionSelected += Parser_ParserAction;
        Compiler.LalrParser.TokenReceived +=  Parser_TokenReceived; 
        _tokens.Clear();
      }
      try {
        SourceFile source = new SourceFile(txtSource.Text, "source", 8);
        _rootNode = Compiler.Parse(txtSource.Text);
      } catch (Exception ex) {
        lstErrors.Items.Add(ex);
        lstErrors.Items.Add("Parser state: " + Compiler.LalrParser.CurrentState);
        tabResults.SelectedTab = pageParseErrors;
        ShowParseStack();
        throw;
      } finally {
        Compiler.LalrParser.ActionSelected -= Parser_ParserAction;
        Compiler.LalrParser.TokenReceived -= Parser_TokenReceived;
      }
      ShowCompilerErrors();
      if (chkShowTrace.Checked)
        foreach (Token tkn in _tokens)
          lstTokens.Items.Add(tkn);
      ShowStats();
      ShowAstNodes(_rootNode);
    }

    private void ShowCompilerErrors() {
      if (Compiler.Context.Errors.Count > 0) {
        lstErrors.Items.Clear();
        if (tabResults.SelectedTab == pageResult)
          tabResults.SelectedTab = pageParseErrors;
        foreach (SyntaxError err in Compiler.Context.Errors)
          lstErrors.Items.Add(err);
      }
    }

    private void ResetResultPanels() {
      _tokens.Clear();
      lblStatLines.Text = "compiling...";
      lblStatTokens.Text = "";
      lblStatTime.Text = "";
      lstTokens.Items.Clear();
      lstErrors.Items.Clear();
      lstParseTrace.Items.Clear();
      lstParseStack.Items.Clear();
      lstTokens.Items.Clear();
      tvAstNodes.Nodes.Clear();
      lblErrCount.Text = "";
      txtErrDetails.Text = "";
      Application.DoEvents();
    }
    private void ShowStats() {
      lblStatLines.Text = Compiler.LalrParser.LineCount.ToString();
      lblStatTokens.Text = Compiler.LalrParser.TokenCount.ToString();
      lblStatTime.Text = Compiler.CompileTime.ToString();
      lblErrCount.Text = Compiler.Context.Errors.Count.ToString();
      Application.DoEvents();
      //Note: this time is "pure" compile time; actual delay after cliking "Compile" includes time to fill TreeView control 
      //  showing compiled syntax tree. 
    }

    private TokenList _tokens = new TokenList();
    void Parser_TokenReceived(object sender, TokenEventArgs e) {
      _tokens.Add(e.Token);
    }


    void Parser_ParserAction(object sender, Irony.Compiler.Lalr.ParserActionEventArgs e) {
      lstParseTrace.Items.Add(e.ToString());
    }

    private void ShowParseStack() {
      lstParseStack.Items.Clear();
      for (int i = 0; i < Compiler.LalrParser.Stack.Count; i++ ) {
        lstParseStack.Items.Add(Compiler.LalrParser.Stack[i]);
      }
    }

    private void ShowAstNodes(AstNode rootNode) {
      tvAstNodes.Nodes.Clear();
      AddAstNode(null, rootNode);
    }
    private void AddAstNode(TreeNode parent, AstNode node) {
      if (node == null) return;
      if (node is Token) {
        Token token = node as Token;
        if (token.Terminal.Category != TokenCategory.Content) return; 
      }
      string txt = node.ToString();
      TreeNode tn = (parent == null? 
        tvAstNodes.Nodes.Add(txt) : parent.Nodes.Add(txt) );
      tn.Tag = node; 
      foreach(AstNode child in node.ChildNodes)
        AddAstNode(tn, child);
     
    }
    private void RefreshGrammarInfo() {
      lstProds.Items.Clear();
      txtLR0Items.Text = "";
      txtParserStates.Text = "" ;
      txtGrammarErrors.Text = "(no errors found)";
      if (Compiler == null) return;

      //Temporarily - this works only for LALR parser
      Irony.Compiler.Lalr.ParserControlData data =((Irony.Compiler.Lalr.Parser) Compiler.Parser).Data;
      
      txtTerms.Text = TextUtils.TerminalsToText(Compiler.Scanner.Data.Terminals);
      txtNonTerms.Text = TextUtils.NonTerminalsToText(data.NonTerminals);
      //Productions and LR0 items
      foreach (Production pr in data.Productions) {
        lstProds.Items.Add(pr);
      }
      //States
      txtParserStates.Text = TextUtils.StateListToText(data.States);
      //Validation errors
      StringSet errors = Compiler.Grammar.Errors;
      if (errors.Count > 0) {
        txtGrammarErrors.Text = errors.ToString(Environment.NewLine);
        txtGrammarErrors.Text += "\r\n\r\nTotal errors: " + errors.Count;
        tabGrammar.SelectedTab = pageGrErrors;
      }
    }//methold

    private void lstProds_SelectedIndexChanged(object sender, EventArgs e) {
      Production pr = (Production) lstProds.SelectedItem;
      if (pr == null) return;
      string text = string.Empty;
      string nl = Environment.NewLine;
      foreach (LR0Item item in pr.LR0Items) {
        text += item.ToString() + nl + 
          "    Tail-Nullable = " + item.TailIsNullable + nl +
          "    Tail-Firsts   = " + item.TailFirsts.ToString(" ") + nl;
      }
      txtLR0Items.Text = text;
    }
    
    public string LoadFile(string fileName) {
      StreamReader rdr = new StreamReader(fileName);
      string result = rdr.ReadToEnd();
      return result;
    }

    private void lstErrors_SelectedIndexChanged(object sender, EventArgs e) {
      ShowError();
    }

    private void ShowError() {
      txtErrDetails.Text = "";
      if (lstErrors.SelectedItem == null) return;
      SyntaxError se = lstErrors.SelectedItem as SyntaxError;
      if (se == null) return;
      ShowLocation(se.Location, 1);
      txtErrDetails.Text = se.Message + "\r\n (L:C = " + se.Location + ")";
    }

    private void lstTokens_Click(object sender, EventArgs e) {
      ShowSelectedToken();
    }
    private void ShowSelectedToken() {
      if (lstTokens.SelectedIndex < 0)
        return;
      Token token = (Token) lstTokens.SelectedItem;
      ShowLocation(token.Location, token.Length);
    }
    private void ShowLocation(SourceLocation location, int length) {
      if (location.Position < 0) return;
      txtSource.Select(location.Position, length);
      txtSource.ScrollToCaret();
      //lblLoc.Text = location.ToString();
    }

    private void lstTokens_SelectedIndexChanged(object sender, EventArgs e) {
      ShowSelectedToken();
    }

    private void ShowNode(TreeNode node) {
      if (node == null) return;
      AstNode ast = node.Tag as AstNode;
      if (ast == null) return;
      ShowLocation(ast.Location, 1); 
    }

    private void tvAstNodes_AfterSelect(object sender, TreeViewEventArgs e) {
      ShowNode(tvAstNodes.SelectedNode);
    }

    private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e) {
      SymbolTerminal.ClearSymbols();
      Grammar grammar = null;
      btnRun.Enabled = false;
      txtOutput.Text = string.Empty;
      _rootNode = null;
      switch (cboLanguage.SelectedIndex) {
        case 0: //ExpressionGrammar
          grammar = new Irony.Samples.ExpressionGrammar();
          break;
        case 1: //Scheme
          grammar = new Irony.Samples.Scheme.SchemeGrammar();
          btnRun.Enabled = true; 
          break;
        case 2: //Script.NET
          grammar = new  Irony.Samples.ScriptNET.ScriptdotnetGrammar();
          //grammar = new ScriptNET.ScriptdotnetGrammar();
          break;
        case 3: //c#
          grammar = new Irony.Samples.CSharp.CSharpGrammar();
          break;
        case 4: //GwBasic
          grammar = new Irony.Samples.GWBasicGrammar();
          break;
        case 5: //Tutorial
          grammar = new Irony.Tutorial.Part1.CalcGrammar();
          btnRun.Enabled = true;
          break;
      }//switch
      Stopwatch sw = new Stopwatch();
      try {
        sw.Start();
        _compiler = new LanguageCompiler(grammar);
        sw.Stop();
      } finally {
        RefreshGrammarInfo();
        lblInitTime.Text = sw.ElapsedMilliseconds.ToString();
      }//finally
    }

    private void btnFileOpen_Click(object sender, EventArgs e) {
      if (dlgOpenFile.ShowDialog() != DialogResult.OK) return;
      LoadSourceFile(dlgOpenFile.FileName);
      
    }
    private void LoadSourceFile(string path) {
      _rootNode = null;
      StreamReader reader = null;
      try {
        reader = new StreamReader(path);
        txtSource.Text = reader.ReadToEnd();
        txtSource.Select(0, 0);
      } catch (Exception e) {
        MessageBox.Show(e.Message);
      } finally {
        if (reader != null)
          reader.Close();
      }      
    }


    #region Search
    //The following methods are contributed by Andrew Bradnan; pasted here with minor changes
    private void btnSearch_Click(object sender, EventArgs e) {
      DoSearch();
    }//method

    private void txtSearch_KeyPress(object sender, KeyPressEventArgs e) {
      if (e.KeyChar == '\r')  // <Enter> key
        DoSearch();
    }

    private void DoSearch() {
      lblSearchError.Visible = false; 
      TextBox cur = GetSearchContentBox();
      if (cur == null) return;

      int idxStart = cur.SelectionStart + cur.SelectionLength;
      // Compile the regular expression.
      Regex r = new Regex(txtSearch.Text, RegexOptions.IgnoreCase);
      // Match the regular expression pattern against a text string.
      Match m = r.Match(cur.Text.Substring(idxStart));

      if (m.Success) {
        int i = 0;
        Group g = m.Groups[i];
        CaptureCollection cc = g.Captures;
        Capture c = cc[0];

        cur.SelectionStart = c.Index + idxStart + 100;
        cur.ScrollToCaret();
        cur.SelectionStart = c.Index + idxStart;
        cur.SelectionLength = c.Length;
        cur.Focus();
        return;
      } else {
        lblSearchError.Text = "Not found.";
        lblSearchError.Visible = true; 
      }

        
    }

    public TextBox GetSearchContentBox()		{
      switch (tabGrammar.SelectedIndex) {
        case 0:
          return txtTerms;
        case 1:
          return txtNonTerms;
        //case 2:
        //this.pageProds);
        case 3:
          return txtParserStates;
        case 4:
          return txtGrammarErrors;
        case 5:
          return txtSource;
        default:
          return null;
      }//switch
	  }

    #endregion

    StringBuilder _outBuffer;
    private void btnRun_Click(object sender, EventArgs e) {
      Stopwatch sw = new Stopwatch();
      txtOutput.Text = "";
      _outBuffer = new StringBuilder();
      EvaluationContext context = null;
      try {
        if (_rootNode == null)
          ParseSample();
        if (Compiler.Context.Errors.Count > 0) return;
        
        Compiler.AnalyzeCode(_rootNode, Compiler.Context); 
        if (Compiler.Context.Errors.Count > 0) return;

        LanguageRuntime runtime = Compiler.Context.Runtime;
        if (runtime == null)
          throw new RuntimeException("Runtime is not implemented for the language (Grammar.CreateRuntime() returned null). Cannot execute the program.");
        context = new EvaluationContext(runtime, _rootNode);
        context.Runtime.ConsoleWrite += Ops_ConsoleWrite;
        GC.Collect(); //collect now to avoid spontaneous collection on repeated runs 
        sw.Start();
        _rootNode.Evaluate(context);
        sw.Stop();
        lblRunTime.Text = sw.ElapsedMilliseconds.ToString();
      } catch(RuntimeException rex) {
        //catch and add runtime to compiler context, so they will be shown in the form
        Compiler.Context.ReportError(rex.Location, rex.Message);
      } finally {
        sw.Stop();
        if (context != null) {
          context.Runtime.ConsoleWrite -= Ops_ConsoleWrite;
          txtOutput.Text = _outBuffer.ToString();
          if (context.CurrentResult != Unassigned.Value)
            txtOutput.Text += context.CurrentResult;
        }
        if (Compiler.Context.Errors.Count > 0)
          ShowCompilerErrors();

      }//finally
    }//method

    void Ops_ConsoleWrite(object sender, ConsoleWriteEventArgs e) {
      _outBuffer.Append(e.Text);
    }

    private void txtSource_TextChanged(object sender, EventArgs e) {
      _rootNode = null; //force it to recompile on run
    }




  }//class
}
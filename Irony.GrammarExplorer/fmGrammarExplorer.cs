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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using Irony.Compiler;
using Irony.GrammarExplorer.Properties;

namespace Irony.GrammarExplorer {
  public partial class fmGrammarExplorer : Form {
    public fmGrammarExplorer() {
      InitializeComponent();
    }
    private void fmExploreGrammar_Load(object sender, EventArgs e) {
      try {
        cboLanguage.SelectedIndex = Settings.Default.LanguageIndex;
        txtSource.Text = Settings.Default.SourceSample;
      } catch { }
    }
    private void fmExploreGrammar_FormClosing(object sender, FormClosingEventArgs e) {
      Settings.Default.SourceSample = txtSource.Text;
      Settings.Default.LanguageIndex = cboLanguage.SelectedIndex;
      Settings.Default.Save();
    }//method



    LanguageCompiler Compiler  {
      get {return _compiler;}
    } LanguageCompiler  _compiler;

    private void btnParse_Click(object sender, EventArgs e) {
      AstNode rootNode = null;
      ResetResultPanels();
      if (chkShowTrace.Checked) {
        Compiler.Parser.ActionSelected += Parser_ParserAction;
        Compiler.Parser.TokenReceived +=  Parser_TokenReceived; 
        _tokens.Clear();
      }
      try {
        SourceFile source = new SourceFile(txtSource.Text, "source", 8);
        rootNode = Compiler.Parse(txtSource.Text);
      } catch (Exception ex) {
        lstErrors.Items.Add(ex);
        lstErrors.Items.Add("Parser state: " + Compiler.Parser.CurrentState);
        tabResults.SelectedTab = pageParseErrors;
        ShowParseStack();
        throw;
      } finally {
        Compiler.Parser.ActionSelected -= Parser_ParserAction;
        Compiler.Parser.TokenReceived -= Parser_TokenReceived;
      }
      if (Compiler.Context.Errors.Count > 0) {
        if (tabResults.SelectedTab == pageResult)
          tabResults.SelectedTab = pageParseErrors;
        foreach (SyntaxError err in Compiler.Context.Errors) 
          lstErrors.Items.Add(err);
      }
      if (chkShowTrace.Checked)
        foreach (Token tkn in _tokens)
          lstTokens.Items.Add(tkn);
      ShowStats();
      ShowAstNodes(rootNode);
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
      lblStatLines.Text = Compiler.Parser.LineCount.ToString();
      lblStatTokens.Text = Compiler.Parser.TokenCount.ToString();
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


    void Parser_ParserAction(object sender, ParserActionEventArgs e) {
      lstParseTrace.Items.Add(e.ToString());
    }

    private void ShowParseStack() {
      lstParseStack.Items.Clear();
      for (int i = 0; i < Compiler.Parser.Stack.Count; i++ ) {
        lstParseStack.Items.Add(Compiler.Parser.Stack[i]);
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
      GenericNode genNode = node as GenericNode;
      if (genNode == null) return;
      foreach(AstNode child in genNode.ChildNodes)
        AddAstNode(tn, child);
     
    }
    private void RefreshGrammarInfo() {
      lstProds.Items.Clear();
      txtLR0Items.Text = "";
      txtParserStates.Text = "" ;
      txtGrammarErrors.Text = "(no errors found)";
      if (Compiler == null) return;
      GrammarData data = Compiler.Parser.Data;
      txtTerms.Text = data.GetTerminalsAsText();
      txtNonTerms.Text = data.GetNonTerminalsAsText();
      //Productions and LR0 items
      foreach (Production pr in data.Productions) {
        lstProds.Items.Add(pr);
      }
      //States
      txtParserStates.Text = data.GetStatesAsText();
      //Validation errors
      if (data.Errors.Count > 0)
        txtGrammarErrors.Text = data.Errors.ToString(Environment.NewLine);
      lblInitTime.Text = Compiler.InitTime.ToString();
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
      txtErrDetails.Text = se.Message + "\r\n (L:C = " + se.Location + ", parser state: " + se.State + ")";
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
      Grammar grammar = null; 
      switch (cboLanguage.SelectedIndex) {
        case 0: //ExpressionGrammar
          grammar = new Irony.Samples.ExpressionGrammar();
          break;
        case 1: //Scheme
          grammar = new Irony.Samples.Scheme.SchemeGrammar();
          break;
        case 2: //Python
          grammar = new Irony.Samples.Python.PythonGrammar();
          break;
        case 3: //Ruby
          grammar = new Irony.Samples.Ruby.RubyGrammar();
          break;
      }//switch
      try {
        _compiler = new LanguageCompiler(grammar);
      } finally {
      RefreshGrammarInfo();
      }//finally
    }

    private void btnFileOpen_Click(object sender, EventArgs e) {
      if (dlgOpenFile.ShowDialog() != DialogResult.OK) return;
      LoadSourceFile(dlgOpenFile.FileName);
      
    }
    private void LoadSourceFile(string path) {
      try {
        StreamReader rdr = new StreamReader(path);
        string src = rdr.ReadToEnd();
        string[] lines = src.Split(new string[] { "\r\n" }, StringSplitOptions.None);
        txtSource.Lines = lines;
        txtSource.Select(0, 0);
      } catch (Exception e) {
        MessageBox.Show(e.Message);
      }
    }



  }//class
}
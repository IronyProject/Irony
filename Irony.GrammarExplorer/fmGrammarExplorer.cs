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
using System.Reflection;
using Irony.Compiler;
using Irony.Runtime;
using Irony.EditorServices;
using Irony.GrammarExplorer.Properties;


namespace Irony.GrammarExplorer {
  public partial class fmGrammarExplorer : Form {
    public fmGrammarExplorer() {
      InitializeComponent();
    }
    LanguageCompiler _compiler;
    CompilerContext _compilerContext;
    AstNode _rootNode;
    RichTextBoxHighligter _highlighter;

    private void fmExploreGrammar_Load(object sender, EventArgs e) {
      try {
        txtSource.Text = Settings.Default.SourceSample;
        txtSearch.Text = Settings.Default.SearchPattern;
        GrammarItemList grammars = GrammarItemList.FromXml(Settings.Default.Grammars);
        grammars.ShowIn(cboGrammars);
        cboGrammars.SelectedIndex = Settings.Default.LanguageIndex; //this will start colorizer
      } catch { }
    }
    private void fmExploreGrammar_FormClosing(object sender, FormClosingEventArgs e) {
      Settings.Default.SourceSample = txtSource.Text;
      Settings.Default.LanguageIndex = cboGrammars.SelectedIndex;
      Settings.Default.SearchPattern = txtSearch.Text;
      var grammars = GrammarItemList.FromCombo(cboGrammars);
      Settings.Default.Grammars = grammars.ToXml(); 
      Settings.Default.Save();
    }//method

    private void StartHighligter() {
      if (_highlighter != null) 
        StopHighlighter(); 
      var adapter = new EditorAdapter(_compiler);
      var viewAdapter = new EditorViewAdapter(adapter, txtSource);
      _highlighter = new RichTextBoxHighligter(txtSource, viewAdapter);
      adapter.Activate();
    }
    private void StopHighlighter() {
      _highlighter.Dispose();
      _highlighter = null;
      txtSource.Text = txtSource.Text; //remove all old highlighting
    }


    private void btnParse_Click(object sender, EventArgs e) {
      ParseSample();
    }

    private void ParseSample() {
      _rootNode = null;
      _compilerContext = new CompilerContext(_compiler);
      ResetResultPanels();
      if (chkShowTrace.Checked) {
        _compiler.LalrParser.ActionSelected += Parser_ParserAction;
        _compiler.LalrParser.TokenReceived +=  Parser_TokenReceived; 
        _tokens.Clear();
      }
      SourceFile source = new SourceFile(txtSource.Text, "source", 8);
      try {
        _rootNode = _compiler.Parse(_compilerContext, source);
      } catch (Exception ex) {
        lstErrors.Items.Add(ex);
        lstErrors.Items.Add("Parser state: " + _compiler.LalrParser.CurrentState);
        tabResults.SelectedTab = pageParseErrors;
        ShowParseStack();
        throw;
      } finally {
        _compiler.LalrParser.ActionSelected -= Parser_ParserAction;
        _compiler.LalrParser.TokenReceived -= Parser_TokenReceived;
      }
      ShowCompilerErrors();
      if (chkShowTrace.Checked)
        foreach (Token tkn in _tokens)
          lstTokens.Items.Add(tkn);
      ShowStats();
      ShowAstNodes(_rootNode);
    }

    private void ShowCompilerErrors() {
      if (_compilerContext.Errors.Count > 0) {
        lstErrors.Items.Clear();
        if (tabResults.SelectedTab == pageResult)
          tabResults.SelectedTab = pageParseErrors;
        foreach (SyntaxError err in _compilerContext.Errors)
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
      lblStatLines.Text = _compiler.LalrParser.LineCount.ToString();
      lblStatTokens.Text = _compiler.LalrParser.TokenCount.ToString();
      lblStatTime.Text = _compiler.CompileTime.ToString();
      lblErrCount.Text = _compilerContext.Errors.Count.ToString();
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
      for (int i = 0; i < _compiler.LalrParser.Stack.Count; i++ ) {
        lstParseStack.Items.Add(_compiler.LalrParser.Stack[i]);
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
      txtParserStates.Text = "" ;
      txtGrammarErrors.Text = "(no errors found)";
      if (_compiler == null) return;

      txtTerms.Text = TextUtils.TerminalsToText(_compiler.Grammar.Terminals);
      txtNonTerms.Text = TextUtils.NonTerminalsToText(_compiler.Grammar.NonTerminals);
      //States
      txtParserStates.Text = _compiler.Parser.GetStateList();
      //Validation errors
      StringSet errors = _compiler.Grammar.Errors;
      if (errors.Count > 0) {
        txtGrammarErrors.Text = errors.ToString(Environment.NewLine);
        txtGrammarErrors.Text += "\r\n\r\nTotal errors: " + errors.Count;
        tabGrammar.SelectedTab = pageGrErrors;
      }
    }//methold
    
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
      txtSource.SelectionStart = location.Position;
      txtSource.SelectionLength = length;
      //txtSource.Select(location.Position, length);
      txtSource.ScrollToCaret();
      txtSource.Focus(); 
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

      GrammarItem selItem = cboGrammars.SelectedItem as GrammarItem;
      grammar = selItem.CreateGrammar();
      btnRun.Enabled = grammar.FlagIsSet(LanguageFlags.SupportsInterpreter);
      Stopwatch sw = new Stopwatch();
      try {
        sw.Start();
        _compiler = new LanguageCompiler(grammar);
        _compilerContext = new CompilerContext(_compiler); 
        sw.Stop();
        StartHighligter();
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
        txtSource.Text = null;  //to clear any old formatting
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
      TextBoxBase textBox = GetSearchContentBox();
      if (textBox == null) return;

      int idxStart = textBox.SelectionStart + textBox.SelectionLength;
      // Compile the regular expression.
      Regex r = new Regex(txtSearch.Text, RegexOptions.IgnoreCase);
      // Match the regular expression pattern against a text string.
      Match m = r.Match(textBox.Text.Substring(idxStart));
      if (m.Success) {
        int i = 0;
        Group g = m.Groups[i];
        CaptureCollection cc = g.Captures;
        Capture c = cc[0];

        textBox.SelectionStart = c.Index + idxStart + 100;
        textBox.ScrollToCaret();
        textBox.SelectionStart = c.Index + idxStart;
        textBox.SelectionLength = c.Length;
        textBox.Focus();
        return;
      } else {
        lblSearchError.Text = "Not found.";
        lblSearchError.Visible = true; 
      }
    }//method

    public TextBoxBase GetSearchContentBox()		{
      switch (tabGrammar.SelectedIndex) {
        case 0:
          return txtTerms;
        case 1:
          return txtNonTerms;
        case 2:
          return txtParserStates;
        case 3:
          return txtGrammarErrors;
        case 4:
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
        if (_compilerContext.Errors.Count > 0) return;
        
        _compiler.AnalyzeCode(_rootNode, _compilerContext); 
        if (_compilerContext.Errors.Count > 0) return;

        LanguageRuntime runtime = _compilerContext.Runtime;
        if (runtime == null)
          throw new RuntimeException("Runtime is not implemented for the language (Grammar.CreateRuntime() returned null). Cannot execute the program.");
        context = new EvaluationContext(runtime, _rootNode);
        context.Runtime.ConsoleWrite += Ops_ConsoleWrite;
        sw.Start();
        _rootNode.Evaluate(context);
        sw.Stop();
        lblRunTime.Text = sw.ElapsedMilliseconds.ToString();
      } catch(RuntimeException rex) {
        //catch and add runtime to compiler context, so they will be shown in the form
        _compilerContext.ReportError(rex.Location, rex.Message);
      } finally {
        sw.Stop();
        if (context != null) {
          context.Runtime.ConsoleWrite -= Ops_ConsoleWrite;
          txtOutput.Text = _outBuffer.ToString();
          if (context.CurrentResult != Unassigned.Value)
            txtOutput.Text += context.CurrentResult;
        }
        if (_compilerContext.Errors.Count > 0)
          ShowCompilerErrors();

      }//finally
    }//method

    void Ops_ConsoleWrite(object sender, ConsoleWriteEventArgs e) {
      _outBuffer.Append(e.Text);
    }

    private void txtSource_TextChanged(object sender, EventArgs e) {
      _rootNode = null; //force it to recompile on run
    }

    private void btnManageGrammars_Click(object sender, EventArgs e) {
      menuGrammars.Show(btnManageGrammars, 0, btnManageGrammars.Height); 
    }

    private void menuGrammars_Opening(object sender, CancelEventArgs e) {
      miRemove.Enabled = cboGrammars.Items.Count > 0; 
    }

    private void miRemove_Click(object sender, EventArgs e) {
      if (MessageBox.Show("Are you sure you want to remove grammmar " + cboGrammars.SelectedItem + "?",
        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
        cboGrammars.Items.RemoveAt(cboGrammars.SelectedIndex);
        _compiler = null;
        if (cboGrammars.Items.Count > 0)
          cboGrammars.SelectedIndex = 0; 
      }
    }

    private void miAdd_Click(object sender, EventArgs e) {
       if (dlgSelectAssembly.ShowDialog() != DialogResult.OK) return;
       string location = dlgSelectAssembly.FileName;
      Assembly asm = Assembly.LoadFrom(location);
      var types = asm.GetTypes();
      GrammarItemList grammars = new GrammarItemList(); 
      foreach(Type t in types) {
        if (!t.IsSubclassOf(typeof(Grammar))) continue;
        grammars.Add(new GrammarItem(t, location));
      }
      if (grammars.Count == 0) {
        MessageBox.Show("No classes derived from Irony.Grammar were found in the assembly.");
        return; 
      }
      grammars = fmSelectGrammars.SelectGrammars(grammars);
      if (grammars == null) return;
      foreach (GrammarItem item in grammars)
        cboGrammars.Items.Add(item); 
    }//method



  }//class
}
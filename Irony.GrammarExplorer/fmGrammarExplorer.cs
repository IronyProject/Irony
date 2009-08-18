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
using System.Xml;
using Irony.Parsing;
using Irony.Diagnostics;
using Irony.EditorServices;
using Irony.Ast.Interpreter;
using Irony.GrammarExplorer.Properties;

using Irony.Samples.Apress.Examples;

namespace Irony.GrammarExplorer {
  public partial class fmGrammarExplorer : Form {
    public fmGrammarExplorer() {
      InitializeComponent();
    }

    //fields
    Grammar _grammar; 
    Compiler _compiler;
    CompilerContext _compilerContext;
    ParseTree _parseTree;

    #region Form load/unload events
    private void fmExploreGrammar_Load(object sender, EventArgs e) {
      ClearLanguageInfo();
      try {
        txtSource.Text = Settings.Default.SourceSample;
        txtSearch.Text = Settings.Default.SearchPattern;
        var grammars = GrammarItemList.FromXml(Settings.Default.Grammars);
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
    #endregion 

    #region Show... methods
    //Show... methods ######################################################################################################################
    private void ClearLanguageInfo() {
      lblLanguage.Text = string.Empty;
      lblLanguageVersion.Text = string.Empty;
      lblLanguageDescr.Text = string.Empty;
      txtGrammarComments.Text = string.Empty;
    }
    private void ClearCompileResults() {
      lblSrcLineCount.Text = string.Empty;
      lblSrcTokenCount.Text = "";
      lblCompileTime.Text = "";
      lblCompileErrorCount.Text = "";

      lstTokens.Items.Clear();
      gridCompileErrors.Rows.Clear();
      gridParserTrace.Rows.Clear();
      lstTokens.Items.Clear();
      tvParseTree.Nodes.Clear();
      Application.DoEvents();
    }

    private void ShowLanguageInfo() {
      if (_grammar == null) return;
      var langAttr = LanguageAttribute.GetValue(_grammar.GetType());
      if (langAttr == null) return;
      lblLanguage.Text = langAttr.LanguageName;
      lblLanguageVersion.Text = langAttr.Version;
      lblLanguageDescr.Text = langAttr.Description;
      txtGrammarComments.Text = _grammar.GrammarComments;
    }

    private void ShowCompilerErrors() {
      gridCompileErrors.Rows.Clear();
      if (_parseTree == null || _parseTree.Errors.Count == 0) return; 
      foreach (var err in _parseTree.Errors) 
        gridCompileErrors.Rows.Add(err.Location, err, err.ParserState);
      if (tabBottom.SelectedTab != pageParserOutput)
        tabBottom.SelectedTab = pageParserOutput;
    }

    private void ShowParseTrace() {
      gridParserTrace.Rows.Clear();
      foreach (var entry in _compilerContext.ParserTrace) {
        int index = gridParserTrace.Rows.Add(entry.State, entry.StackTop, entry.Input,
            entry.Message, entry.NewState); 
        if (entry.IsError)
          gridParserTrace.Rows[gridParserTrace.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Red;
      }
      //Show tokens
      foreach (Token tkn in _parseTree.Tokens) {
        if (chkExcludeComments.Checked && tkn.Category == TokenCategory.Comment) continue; 
        lstTokens.Items.Add(tkn);
      }
    }//method

    private void ShowCompileStats() {
      if (_parseTree == null) return;
      lblSrcLineCount.Text = string.Empty;
      if (_parseTree.Tokens.Count > 0)
        lblSrcLineCount.Text = (_parseTree.Tokens[_parseTree.Tokens.Count - 1].Location.Line + 1).ToString();
      lblSrcTokenCount.Text = _parseTree.Tokens.Count.ToString();
      lblCompileTime.Text = _parseTree.ParseTime.ToString();
      lblCompileErrorCount.Text = _parseTree.Errors.Count.ToString();
      Application.DoEvents();
      //Note: this time is "pure" compile time; actual delay after cliking "Compile" includes time to fill TreeView control 
      //  showing compiled syntax tree. 
    }

    private void ShowParseTree(ParseTree parseTree) {
      tvParseTree.Nodes.Clear();
      if (parseTree == null) return; 
      AddParseNodeRec(null, parseTree.Root);
    }
    private void AddParseNodeRec(TreeNode parent, ParseTreeNode nodeInfo) {
      if (nodeInfo == null) return;
      Token token = nodeInfo.AstNode as Token;
      if (token != null) {
        if (token.Terminal.Category != TokenCategory.Content && token.Terminal.Category != TokenCategory.Literal) return; 
      }
      string txt = nodeInfo.ToString();
      TreeNode newNode = (parent == null? 
        tvParseTree.Nodes.Add(txt) : parent.Nodes.Add(txt) );
      newNode.Tag = nodeInfo; 
      foreach(var child in nodeInfo.ChildNodes)
        AddParseNodeRec(newNode, child);
    }

    private void ShowParserConstructionResults() {
      txtParserStates.Text = string.Empty;
      gridGrammarErrors.Rows.Clear();
      txtTerms.Text = string.Empty;
      txtNonTerms.Text = string.Empty;
      txtParserStates.Text = string.Empty;
      tabBottom.SelectedTab = pageLanguage;
      if (_compiler == null) return;
      txtTerms.Text = DiagnosticUtils.PrintTerminals(_compiler.Language);
      txtNonTerms.Text = DiagnosticUtils.PrintNonTerminals(_compiler.Language);
      txtParserStates.Text = DiagnosticUtils.PrintStateList(_compiler.Language);
      ShowGrammarErrors();
    }//method

    private void ShowGrammarErrors() {
      gridGrammarErrors.Rows.Clear();
      var errors = _compiler.Language.Errors;
      if (errors.Count == 0) return;
      foreach (var err in errors)
        gridGrammarErrors.Rows.Add(err.Level.ToString(), err.Message, err.State);
      if (tabBottom.SelectedTab != pageGrammarErrors)
        tabBottom.SelectedTab = pageGrammarErrors;
    }

    private void ShowSourceLocation(SourceLocation location, int length) {
      if (location.Position < 0) return;
      txtSource.SelectionStart = location.Position;
      txtSource.SelectionLength = length;
      //txtSource.Select(location.Position, length);
      txtSource.ScrollToCaret();
      if (tabGrammar.SelectedTab != pageTest)
        tabGrammar.SelectedTab = pageTest;
      txtSource.Focus();
      //lblLoc.Text = location.ToString();
    }
    private void ShowSourceLocationAndTraceToken(SourceLocation location, int length) {
      ShowSourceLocation(location, length);
      //find token in trace
      for (int i = 0; i < lstTokens.Items.Count; i++) {
        var tkn = lstTokens.Items[i] as Token;
        if (tkn.Location.Position == location.Position) {
          lstTokens.SelectedIndex = i;
          return;
        }//if
      }//for i
    }
    private void LocateParserState(ParserState state) {
      if (state == null) return;
      if (tabGrammar.SelectedTab != pageParserStates)
        tabGrammar.SelectedTab = pageParserStates;
      //first scroll to the bottom, so that scrolling to needed position brings it to top
      txtParserStates.SelectionStart = txtParserStates.Text.Length - 1;
      txtParserStates.ScrollToCaret();
      DoSearch(txtParserStates, "State " + state.Name, 0);
    }

    #endregion 

    #region Grammar combo menu commands
    private void menuGrammars_Opening(object sender, CancelEventArgs e) {
      miRemove.Enabled = cboGrammars.Items.Count > 0;
    }

    private void miAdd_Click(object sender, EventArgs e) {
      if (dlgSelectAssembly.ShowDialog() != DialogResult.OK) return;
      string location = dlgSelectAssembly.FileName;
      if (string.IsNullOrEmpty(location)) return; 
      var oldGrammars = new GrammarItemList(); 
      foreach(var item in cboGrammars.Items)
        oldGrammars.Add((GrammarItem) item);
      var grammars = fmSelectGrammars.SelectGrammars(location, oldGrammars);
      if (grammars == null) return;
      foreach (GrammarItem item in grammars)
        cboGrammars.Items.Add(item);
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

    private void miRemoveAll_Click(object sender, EventArgs e) {
      if (MessageBox.Show("Are you sure you want to remove all grammmars in the list?",
        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
        cboGrammars.Items.Clear();
        _compiler = null;
      }
    }
    #endregion

    #region Parsing and running
    private void CreateGrammar() {
      GrammarItem selItem = cboGrammars.SelectedItem as GrammarItem;
      _grammar = selItem.CreateGrammar();
      cboParseMethod.SelectedIndex = (int) _grammar.ParseMethod;
    }
    private void ConstructParser() {
      StopHighlighter();
      btnRun.Enabled = false;
      txtOutput.Text = string.Empty;
      _parseTree = null;

      btnRun.Enabled = false;// grammar.FlagIsSet(LanguageFlags.SupportsInterpreter); //temp, interpreter does not work
      _grammar.ParseMethod = (ParseMethod)cboParseMethod.SelectedIndex;
      Stopwatch sw = new Stopwatch();
      try {
        sw.Start();
        _compiler = new Compiler(_grammar);
        _compilerContext = new CompilerContext(_compiler);
        sw.Stop();
        StartHighligter();
      } finally {
        ShowParserConstructionResults();
        lblParserConstrTime.Text = sw.ElapsedMilliseconds.ToString();
      }//finally
    }

    private void ParseSample() {
      ClearCompileResults();
      if (_compiler == null || !_compiler.Language.CanParse()) return; 
      _parseTree = null;
      _compilerContext = new CompilerContext(_compiler);
      _compilerContext.SetOption(CompilerOptions.TraceParser, chkParserTrace.Checked);
      try {
        _compiler.Parse(_compilerContext, txtSource.Text, "<source>");
      } catch (Exception ex) {
        gridCompileErrors.Rows.Add(null, ex.Message, null);
        tabBottom.SelectedTab = pageParserOutput;
        throw;
      } finally {
        _parseTree = _compilerContext.CurrentParseTree;
        ShowCompilerErrors();
        if (chkParserTrace.Checked) {
          ShowParseTrace();
        }
        ShowCompileStats();
        ShowParseTree(_parseTree);
      }
    }


    StringBuilder _outBuffer;
    private void RunSample() {
      Stopwatch sw = new Stopwatch();
      txtOutput.Text = "";
      _outBuffer = new StringBuilder();
      EvaluationContext evalContext = null;
      try {
        if (_parseTree == null)
          ParseSample();
        if (_parseTree.Errors.Count > 0) return;
        
        //_compiler.AnalyzeCode(_rootNode, _compilerContext); -- code should be analyzed already
        if (_parseTree.Errors.Count > 0) return;
/*
        AstNode rootNode = _parseTree.Root.AstNode as AstNode;
        if (rootNode == null)
          throw new Exception("Root node info has no custom root node - cannot evaluate."); 
        evalContext = new EvaluationContext(_compilerContext.Runtime, rootNode);
        evalContext.Runtime.ConsoleWrite += Ops_ConsoleWrite;
        sw.Start();
        rootNode.Evaluate(evalContext);
        sw.Stop();
        lblRunTime.Text = sw.ElapsedMilliseconds.ToString();
 */ 
      } catch(RuntimeException rex) {
        //catch and add runtime to compiler context, so they will be shown in the form
        _compilerContext.AddError(rex.Location, rex.Message);
      } finally {
        sw.Stop();
        if (evalContext != null) {
          evalContext.Runtime.ConsoleWrite -= Ops_ConsoleWrite;
          txtOutput.Text = _outBuffer.ToString();
          if (evalContext.Result != Unassigned.Value)
            txtOutput.Text += evalContext.Result;
        }
        if (_parseTree.Errors.Count > 0)
          ShowCompilerErrors();

      }//finally
    }//method

    void Ops_ConsoleWrite(object sender, ConsoleWriteEventArgs e) {
      _outBuffer.Append(e.Text);
    }
    #endregion

    #region miscellaneous: LoadSourceFile, Search, Source highlighting
    private void LoadSourceFile(string path) {
      _parseTree = null;
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

    //Source highlighting 
    RichTextBoxHighligter _highlighter;
    private void StartHighligter() {
      if (_highlighter != null)
        StopHighlighter();
      if (!_compiler.Language.CanParse()) return; 
      _highlighter = new RichTextBoxHighligter(txtSource, _compiler);
      _highlighter.Adapter.Activate();
    }
    private void StopHighlighter() {
      if (_highlighter == null) return;
      _highlighter.Dispose();
      _highlighter = null;
      txtSource.Text = txtSource.Text; //remove all old highlighting
    }

    //The following methods are contributed by Andrew Bradnan; pasted here with minor changes
    private void DoSearch() {
      lblSearchError.Visible = false;
      TextBoxBase textBox = GetSearchContentBox();
      if (textBox == null) return;
      int idxStart = textBox.SelectionStart + textBox.SelectionLength;
      if (!DoSearch(textBox, txtSearch.Text, idxStart)) {
        lblSearchError.Text = "Not found.";
        lblSearchError.Visible = true;
      }
    }//method

    private bool DoSearch(TextBoxBase textBox, string fragment, int start) {
      textBox.SelectionLength = 0;
      // Compile the regular expression.
      Regex r = new Regex(fragment, RegexOptions.IgnoreCase);
      // Match the regular expression pattern against a text string.
      Match m = r.Match(textBox.Text.Substring(start));
      if (m.Success) {
        int i = 0;
        Group g = m.Groups[i];
        CaptureCollection cc = g.Captures;
        Capture c = cc[0];
        textBox.SelectionStart = c.Index + start;
        textBox.SelectionLength = c.Length;
        textBox.Focus();
        textBox.ScrollToCaret();
        return true;
      }
      return false;
    }//method

    public TextBoxBase GetSearchContentBox() {
      switch (tabGrammar.SelectedIndex) {
        case 0:
          return txtTerms;
        case 1:
          return txtNonTerms;
        case 2:
          return txtParserStates;
        case 4:
          return txtSource;
        default:
          return null;
      }//switch
    }

    #endregion

    #region Controls event handlers
    //Controls event handlers ###################################################################################################
    private void btnParse_Click(object sender, EventArgs e) {
      ParseSample();
    }
    private void btnRun_Click(object sender, EventArgs e) {
      RunSample();
    }

    private void tvAstNodes_AfterSelect(object sender, TreeViewEventArgs e) {
      var vtreeNode = tvParseTree.SelectedNode;
      if (vtreeNode == null) return;
      var parseNode = vtreeNode.Tag as ParseTreeNode;
      if (parseNode == null) return;
      ShowSourceLocation(parseNode.Span.Location, 1);
    }

    bool _changingGrammar;
    private void cboGrammars_SelectedIndexChanged(object sender, EventArgs e) {
      try {
        ClearLanguageInfo();
        ClearCompileResults();
        _changingGrammar = true;
        CreateGrammar();
        ShowLanguageInfo();
        ConstructParser();
      } finally {
        _changingGrammar = false; //in case of exception
      }
    }
    private void btnFileOpen_Click(object sender, EventArgs e) {
      if (dlgOpenFile.ShowDialog() != DialogResult.OK) return;
      LoadSourceFile(dlgOpenFile.FileName);

    }
    private void txtSource_TextChanged(object sender, EventArgs e) {
      _parseTree = null; //force it to recompile on run
    }

    private void btnManageGrammars_Click(object sender, EventArgs e) {
      menuGrammars.Show(btnManageGrammars, 0, btnManageGrammars.Height);
    }
    private void btnToXml_Click(object sender, EventArgs e) {
      txtOutput.Text = string.Empty;
      if (_parseTree == null)
        ParseSample(); 
      if (_parseTree == null)  return;
      txtOutput.Text += _parseTree.ToXml();
      txtOutput.Select(0, 0);
      tabBottom.SelectedTab = pageOutput;
    }

    private void cboParseMethod_SelectedIndexChanged(object sender, EventArgs e) {
      //changing grammar causes setting of parse method combo, so to prevent double-call to ConstructParser
      // we don't do it here if _changingGrammar is set
      if (!_changingGrammar) 
        ConstructParser();
    }

    private void gridParserTrace_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
      if (_compilerContext == null || e.RowIndex < 0 || e.RowIndex >= _compilerContext.ParserTrace.Count) return;
      var entry = _compilerContext.ParserTrace[e.RowIndex];
      switch (e.ColumnIndex) {
        case 0: //state
          LocateParserState(entry.State);
          break;
        case 1: //stack top
          if (entry.StackTop != null)
            ShowSourceLocationAndTraceToken(entry.StackTop.Span.Location, entry.StackTop.Span.Length);
          break;
        case 2: //input
          if (entry.Input != null)
            ShowSourceLocationAndTraceToken(entry.Input.Span.Location, entry.Input.Span.Length);
          break;
        case 3: //action
          break;
        case 4: //state
          LocateParserState(entry.NewState);
          break;
      }//switch
    }

    private void lstTokens_DoubleClick(object sender, EventArgs e) {
      if (lstTokens.SelectedIndex < 0)
        return;
      Token token = (Token)lstTokens.SelectedItem;
      ShowSourceLocation(token.Location, token.Length);
    }
    private void gridCompileErrors_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
      if (e.RowIndex < 0 || e.RowIndex >= gridCompileErrors.Rows.Count) return;
      var err = gridCompileErrors.Rows[e.RowIndex].Cells[1].Value as SyntaxError;
      switch (e.ColumnIndex) {
        case 0: //state
        case 1: //stack top
          ShowSourceLocation(err.Location, 1);
          break;
        case 2: //input
          if (err.ParserState != null)
            LocateParserState(err.ParserState);
          break;
      }//switch
    }

    private void gridGrammarErrors_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
      if (e.RowIndex < 0 || e.RowIndex >= gridGrammarErrors.Rows.Count) return;
      switch (e.ColumnIndex) {
        case 2: //input
          var state = gridGrammarErrors.Rows[e.RowIndex].Cells[2].Value as ParserState;
          if (state != null)
            LocateParserState(state);
          break;
      }//switch

    }


    private void btnSearch_Click(object sender, EventArgs e) {
      DoSearch();
    }//method

    private void txtSearch_KeyPress(object sender, KeyPressEventArgs e) {
      if (e.KeyChar == '\r')  // <Enter> key
        DoSearch();
    }

    #endregion

  }//class
}
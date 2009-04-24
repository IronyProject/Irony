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
using System.Xml;
using Irony.CompilerServices;
using Irony.Diagnostics;
using Irony.Scripting.Runtime;
using Irony.EditorServices;
using Irony.GrammarExplorer.Properties;


namespace Irony.GrammarExplorer {
  public partial class fmGrammarExplorer : Form {
    public fmGrammarExplorer() {
      InitializeComponent();
    }

    //fields
    Compiler _compiler;
    CompilerContext _compilerContext;
    ParseTree _parseTree;

    #region Form load/unload events
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
    #endregion 

    #region Show... methods
    //Show... methods ######################################################################################################################
    private void ResetResultPanels() {
      lblStatLines.Text = "compiling...";
      lblStatTokens.Text = "";
      lblStatTime.Text = "";
      lstTokens.Items.Clear();
      gridCompileErrors.Rows.Clear();
      gridParserTrace.Rows.Clear();
      lstTokens.Items.Clear();
      tvParseTree.Nodes.Clear();
      lblErrCount.Text = "";
      Application.DoEvents();
    }

    private void ShowCompilerErrors() {
      gridCompileErrors.Rows.Clear();
      if (_parseTree == null || _parseTree.Errors.Count == 0) return; 
      foreach (var err in _parseTree.Errors) 
        gridCompileErrors.Rows.Add(err.Location, err, err.ParserState);
      if (tabBottom.SelectedTab != pageErrors)
        tabBottom.SelectedTab = pageErrors;
    }

    private void ShowParseTrace() {
      gridParserTrace.Rows.Clear();
      foreach (var entry in _compilerContext.ParserTrace) {
        int index = gridParserTrace.Rows.Add(entry.State, entry.StackTop.ToString(), entry.Input.ToString(),
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

    private void ShowStats() {
      if (_parseTree == null) return;
      lblStatLines.Text = string.Empty;
      if (_parseTree.Tokens.Count > 0)
        lblStatLines.Text = (_parseTree.Tokens[_parseTree.Tokens.Count - 1].Location.Line + 1).ToString();
      lblStatTokens.Text = _parseTree.Tokens.Count.ToString();
      lblStatTime.Text = _parseTree.ParseTime.ToString();
      lblErrCount.Text = _parseTree.Errors.Count.ToString();
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

    private void ShowGrammarInfo() {
      txtParserStates.Text = "" ;
      txtGrammarErrors.Text = "(no errors found)";
      if (_compiler == null) return;

      txtTerms.Text = DiagnosticUtils.PrintTerminals(_compiler.Language);
      txtNonTerms.Text = DiagnosticUtils.PrintNonTerminals(_compiler.Language);
      //States
      txtParserStates.Text = DiagnosticUtils.PrintStateList(_compiler.Language);
      //Validation errors
      StringSet errors = _compiler.Language.Errors;
      if (errors.Count > 0) {
        txtGrammarErrors.Text = errors.ToString(Environment.NewLine);
        txtGrammarErrors.Text += "\r\n\r\nTotal errors: " + errors.Count;
        tabGrammar.SelectedTab = pageGrErrors;
      }
      if (!string.IsNullOrEmpty(_compiler.Language.Grammar.GrammarComments)) {
        txtGrammarErrors.Text += "\r\n\r\nComments:\r\n" + _compiler.Language.Grammar.GrammarComments;
      }
    }//method

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
      Assembly asm = Assembly.LoadFrom(location);
      var types = asm.GetTypes();
      GrammarItemList grammars = new GrammarItemList();
      foreach (Type t in types) {
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
    private void ConstructParser() {
      StopHighlighter();
      btnRun.Enabled = false;
      txtOutput.Text = string.Empty;
      _parseTree = null;

      GrammarItem selItem = cboGrammars.SelectedItem as GrammarItem;
      Grammar grammar = selItem.CreateGrammar();
      btnRun.Enabled = false;// grammar.FlagIsSet(LanguageFlags.SupportsInterpreter); //temp, interpreter does not work
      ParseMethod method;
      if (_changingGrammar) {
        method = grammar.ParseMethod;
        cboParseMethod.SelectedIndex = (int)method;
      } else {
        method = (ParseMethod)cboParseMethod.SelectedIndex;
      }
      Stopwatch sw = new Stopwatch();
      try {
        sw.Start();
        _compiler = new Compiler(grammar, method);
        _compilerContext = new CompilerContext(_compiler);
        sw.Stop();
        StartHighligter();
      } finally {
        ShowGrammarInfo();
        lblInitTime.Text = sw.ElapsedMilliseconds.ToString();
      }//finally
    }

    private void ParseSample() {
      _parseTree = null;
      _compilerContext = new CompilerContext(_compiler);
      _compilerContext.SetOption(CompilerOptions.TraceParser, chkParserTrace.Checked);
      ResetResultPanels();
      try {
        _compiler.Parse(_compilerContext, txtSource.Text, "<source>");
      } catch (Exception ex) {
        gridCompileErrors.Rows.Add(null, ex.Message, null);
        tabBottom.SelectedTab = pageErrors;
        throw;
      } finally {
        _parseTree = _compilerContext.CurrentParseTree;
        ShowCompilerErrors();
        if (chkParserTrace.Checked) {
          ShowParseTrace();
        }
        ShowStats();
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
        _compilerContext.ReportError(rex.Location, rex.Message);
      } finally {
        sw.Stop();
        if (evalContext != null) {
          evalContext.Runtime.ConsoleWrite -= Ops_ConsoleWrite;
          txtOutput.Text = _outBuffer.ToString();
          if (evalContext.CurrentResult != Unassigned.Value)
            txtOutput.Text += evalContext.CurrentResult;
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
      var adapter = new EditorAdapter(_compiler);
      var viewAdapter = new EditorViewAdapter(adapter, txtSource);
      _highlighter = new RichTextBoxHighligter(txtSource, viewAdapter);
      adapter.Activate();
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
        case 3:
          return txtGrammarErrors;
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
      ShowSourceLocation(parseNode.Span.Start, 1);
    }

    bool _changingGrammar;
    private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e) {
      try {
        _changingGrammar = true;
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
      /*      if (_parseTree == null) ParseSample();
            if (_parseTree == null) return;
            var rootNode = _parseTree.Root.AstNode as AstNode;
            if (rootNode == null) return; 
            txtOutput.Text = rootNode.XmlGetXmlString();
      */
    }

    private void cboParseMethod_SelectedIndexChanged(object sender, EventArgs e) {
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
            ShowSourceLocationAndTraceToken(entry.StackTop.Span.Start, entry.StackTop.Span.Length);
          break;
        case 2: //input
          if (entry.Input != null)
            ShowSourceLocationAndTraceToken(entry.Input.Span.Start, entry.Input.Span.Length);
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
      if (_parseTree == null || e.RowIndex < 0 || e.RowIndex >= _parseTree.Errors.Count) return;
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
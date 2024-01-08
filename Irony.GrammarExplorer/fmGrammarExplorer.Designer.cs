namespace Irony.GrammarExplorer {
  partial class fmGrammarExplorer {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fmGrammarExplorer));
      Properties.Settings settings1 = new Properties.Settings();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
      tabGrammar = new System.Windows.Forms.TabControl();
      pageTerminals = new System.Windows.Forms.TabPage();
      txtTerms = new System.Windows.Forms.TextBox();
      pageNonTerms = new System.Windows.Forms.TabPage();
      txtNonTerms = new System.Windows.Forms.TextBox();
      pageParserStates = new System.Windows.Forms.TabPage();
      txtParserStates = new System.Windows.Forms.TextBox();
      pageTest = new System.Windows.Forms.TabPage();
      txtSource = new System.Windows.Forms.TextBox();
      panel1 = new System.Windows.Forms.Panel();
      btnLocate = new System.Windows.Forms.Button();
      btnRun = new System.Windows.Forms.Button();
      btnFileOpen = new System.Windows.Forms.Button();
      btnParse = new System.Windows.Forms.Button();
      splitRight = new System.Windows.Forms.Splitter();
      tabOutput = new System.Windows.Forms.TabControl();
      pageSyntaxTree = new System.Windows.Forms.TabPage();
      tvParseTree = new System.Windows.Forms.TreeView();
      pageAst = new System.Windows.Forms.TabPage();
      tvAst = new System.Windows.Forms.TreeView();
      chkParserTrace = new System.Windows.Forms.CheckBox();
      pnlLang = new System.Windows.Forms.Panel();
      chkAutoRefresh = new System.Windows.Forms.CheckBox();
      btnManageGrammars = new System.Windows.Forms.Button();
      lblSearchError = new System.Windows.Forms.Label();
      btnSearch = new System.Windows.Forms.Button();
      txtSearch = new System.Windows.Forms.TextBox();
      label2 = new System.Windows.Forms.Label();
      cboGrammars = new System.Windows.Forms.ComboBox();
      menuGrammars = new System.Windows.Forms.ContextMenuStrip(components);
      miAdd = new System.Windows.Forms.ToolStripMenuItem();
      miRefresh = new System.Windows.Forms.ToolStripMenuItem();
      miSeparator = new System.Windows.Forms.ToolStripSeparator();
      miRemove = new System.Windows.Forms.ToolStripMenuItem();
      miRemoveAll = new System.Windows.Forms.ToolStripMenuItem();
      dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
      dlgSelectAssembly = new System.Windows.Forms.OpenFileDialog();
      splitBottom = new System.Windows.Forms.Splitter();
      tabBottom = new System.Windows.Forms.TabControl();
      pageLanguage = new System.Windows.Forms.TabPage();
      grpLanguageInfo = new System.Windows.Forms.GroupBox();
      label8 = new System.Windows.Forms.Label();
      lblParserStateCount = new System.Windows.Forms.Label();
      lblLanguageDescr = new System.Windows.Forms.Label();
      txtGrammarComments = new System.Windows.Forms.TextBox();
      label11 = new System.Windows.Forms.Label();
      label9 = new System.Windows.Forms.Label();
      lblLanguageVersion = new System.Windows.Forms.Label();
      label10 = new System.Windows.Forms.Label();
      lblLanguage = new System.Windows.Forms.Label();
      label4 = new System.Windows.Forms.Label();
      label6 = new System.Windows.Forms.Label();
      lblParserConstrTime = new System.Windows.Forms.Label();
      pageGrammarErrors = new System.Windows.Forms.TabPage();
      gridGrammarErrors = new System.Windows.Forms.DataGridView();
      dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      pageParserOutput = new System.Windows.Forms.TabPage();
      groupBox1 = new System.Windows.Forms.GroupBox();
      gridCompileErrors = new System.Windows.Forms.DataGridView();
      dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      grpCompileInfo = new System.Windows.Forms.GroupBox();
      label12 = new System.Windows.Forms.Label();
      lblParseErrorCount = new System.Windows.Forms.Label();
      label1 = new System.Windows.Forms.Label();
      lblParseTime = new System.Windows.Forms.Label();
      label7 = new System.Windows.Forms.Label();
      lblSrcLineCount = new System.Windows.Forms.Label();
      label3 = new System.Windows.Forms.Label();
      lblSrcTokenCount = new System.Windows.Forms.Label();
      pageParserTrace = new System.Windows.Forms.TabPage();
      grpParserActions = new System.Windows.Forms.GroupBox();
      gridParserTrace = new System.Windows.Forms.DataGridView();
      State = new System.Windows.Forms.DataGridViewTextBoxColumn();
      Stack = new System.Windows.Forms.DataGridViewTextBoxColumn();
      Input = new System.Windows.Forms.DataGridViewTextBoxColumn();
      Action = new System.Windows.Forms.DataGridViewTextBoxColumn();
      splitter1 = new System.Windows.Forms.Splitter();
      grpTokens = new System.Windows.Forms.GroupBox();
      lstTokens = new System.Windows.Forms.ListBox();
      pnlParserTraceTop = new System.Windows.Forms.Panel();
      chkExcludeComments = new System.Windows.Forms.CheckBox();
      lblTraceComment = new System.Windows.Forms.Label();
      pageOutput = new System.Windows.Forms.TabPage();
      txtOutput = new ConsoleTextBox();
      pnlRuntimeInfo = new System.Windows.Forms.Panel();
      label14 = new System.Windows.Forms.Label();
      lblGCCount = new System.Windows.Forms.Label();
      label13 = new System.Windows.Forms.Label();
      lnkShowErrStack = new System.Windows.Forms.LinkLabel();
      lnkShowErrLocation = new System.Windows.Forms.LinkLabel();
      label5 = new System.Windows.Forms.Label();
      lblRunTime = new System.Windows.Forms.Label();
      toolTip = new System.Windows.Forms.ToolTip(components);
      tabGrammar.SuspendLayout();
      pageTerminals.SuspendLayout();
      pageNonTerms.SuspendLayout();
      pageParserStates.SuspendLayout();
      pageTest.SuspendLayout();
      panel1.SuspendLayout();
      tabOutput.SuspendLayout();
      pageSyntaxTree.SuspendLayout();
      pageAst.SuspendLayout();
      pnlLang.SuspendLayout();
      menuGrammars.SuspendLayout();
      tabBottom.SuspendLayout();
      pageLanguage.SuspendLayout();
      grpLanguageInfo.SuspendLayout();
      pageGrammarErrors.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)gridGrammarErrors).BeginInit();
      pageParserOutput.SuspendLayout();
      groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)gridCompileErrors).BeginInit();
      grpCompileInfo.SuspendLayout();
      pageParserTrace.SuspendLayout();
      grpParserActions.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)gridParserTrace).BeginInit();
      grpTokens.SuspendLayout();
      pnlParserTraceTop.SuspendLayout();
      pageOutput.SuspendLayout();
      pnlRuntimeInfo.SuspendLayout();
      SuspendLayout();
      // 
      // tabGrammar
      // 
      tabGrammar.Controls.Add(pageTerminals);
      tabGrammar.Controls.Add(pageNonTerms);
      tabGrammar.Controls.Add(pageParserStates);
      tabGrammar.Controls.Add(pageTest);
      tabGrammar.Dock = System.Windows.Forms.DockStyle.Fill;
      tabGrammar.Location = new System.Drawing.Point(0, 67);
      tabGrammar.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      tabGrammar.Name = "tabGrammar";
      tabGrammar.SelectedIndex = 0;
      tabGrammar.Size = new System.Drawing.Size(2044, 879);
      tabGrammar.TabIndex = 0;
      // 
      // pageTerminals
      // 
      pageTerminals.Controls.Add(txtTerms);
      pageTerminals.Location = new System.Drawing.Point(4, 39);
      pageTerminals.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageTerminals.Name = "pageTerminals";
      pageTerminals.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageTerminals.Size = new System.Drawing.Size(2036, 836);
      pageTerminals.TabIndex = 5;
      pageTerminals.Text = "Terminals";
      pageTerminals.UseVisualStyleBackColor = true;
      // 
      // txtTerms
      // 
      txtTerms.AcceptsTab = true;
      txtTerms.Dock = System.Windows.Forms.DockStyle.Fill;
      txtTerms.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
      txtTerms.HideSelection = false;
      txtTerms.Location = new System.Drawing.Point(6, 7);
      txtTerms.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      txtTerms.Multiline = true;
      txtTerms.Name = "txtTerms";
      txtTerms.ReadOnly = true;
      txtTerms.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      txtTerms.Size = new System.Drawing.Size(2024, 822);
      txtTerms.TabIndex = 2;
      // 
      // pageNonTerms
      // 
      pageNonTerms.Controls.Add(txtNonTerms);
      pageNonTerms.Location = new System.Drawing.Point(4, 39);
      pageNonTerms.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageNonTerms.Name = "pageNonTerms";
      pageNonTerms.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageNonTerms.Size = new System.Drawing.Size(2036, 836);
      pageNonTerms.TabIndex = 0;
      pageNonTerms.Text = "Non-Terminals";
      pageNonTerms.UseVisualStyleBackColor = true;
      // 
      // txtNonTerms
      // 
      txtNonTerms.Dock = System.Windows.Forms.DockStyle.Fill;
      txtNonTerms.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
      txtNonTerms.HideSelection = false;
      txtNonTerms.Location = new System.Drawing.Point(6, 7);
      txtNonTerms.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      txtNonTerms.Multiline = true;
      txtNonTerms.Name = "txtNonTerms";
      txtNonTerms.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      txtNonTerms.Size = new System.Drawing.Size(2024, 822);
      txtNonTerms.TabIndex = 1;
      txtNonTerms.WordWrap = false;
      // 
      // pageParserStates
      // 
      pageParserStates.Controls.Add(txtParserStates);
      pageParserStates.Location = new System.Drawing.Point(4, 39);
      pageParserStates.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageParserStates.Name = "pageParserStates";
      pageParserStates.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageParserStates.Size = new System.Drawing.Size(2036, 836);
      pageParserStates.TabIndex = 1;
      pageParserStates.Text = "Parser States";
      pageParserStates.UseVisualStyleBackColor = true;
      // 
      // txtParserStates
      // 
      txtParserStates.Dock = System.Windows.Forms.DockStyle.Fill;
      txtParserStates.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
      txtParserStates.HideSelection = false;
      txtParserStates.Location = new System.Drawing.Point(6, 7);
      txtParserStates.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      txtParserStates.Multiline = true;
      txtParserStates.Name = "txtParserStates";
      txtParserStates.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      txtParserStates.Size = new System.Drawing.Size(2024, 822);
      txtParserStates.TabIndex = 2;
      txtParserStates.WordWrap = false;
      // 
      // pageTest
      // 
      pageTest.Controls.Add(txtSource);
      pageTest.Controls.Add(panel1);
      pageTest.Controls.Add(splitRight);
      pageTest.Controls.Add(tabOutput);
      pageTest.Location = new System.Drawing.Point(4, 39);
      pageTest.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageTest.Name = "pageTest";
      pageTest.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageTest.Size = new System.Drawing.Size(2036, 836);
      pageTest.TabIndex = 4;
      pageTest.Text = "Test";
      pageTest.UseVisualStyleBackColor = true;
      // 
      // txtSource
      // 
      txtSource.Dock = System.Windows.Forms.DockStyle.Fill;
      txtSource.Location = new System.Drawing.Point(6, 76);
      txtSource.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      txtSource.Multiline = true;
      txtSource.Name = "txtSource";
      txtSource.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      txtSource.Size = new System.Drawing.Size(1312, 753);
      txtSource.TabIndex = 23;
      txtSource.TextChanged += txtSource_TextChanged;
      // 
      // panel1
      // 
      panel1.Controls.Add(btnLocate);
      panel1.Controls.Add(btnRun);
      panel1.Controls.Add(btnFileOpen);
      panel1.Controls.Add(btnParse);
      panel1.Dock = System.Windows.Forms.DockStyle.Top;
      panel1.Location = new System.Drawing.Point(6, 7);
      panel1.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      panel1.Name = "panel1";
      panel1.Size = new System.Drawing.Size(1312, 69);
      panel1.TabIndex = 2;
      // 
      // btnLocate
      // 
      btnLocate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      btnLocate.Location = new System.Drawing.Point(1164, 7);
      btnLocate.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      btnLocate.Name = "btnLocate";
      btnLocate.Size = new System.Drawing.Size(130, 53);
      btnLocate.TabIndex = 10;
      btnLocate.Text = "Locate >>";
      toolTip.SetToolTip(btnLocate, "Locate the source position in parse/Ast tree. ");
      btnLocate.UseVisualStyleBackColor = true;
      btnLocate.Click += btnLocate_Click;
      // 
      // btnRun
      // 
      btnRun.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      btnRun.Location = new System.Drawing.Point(1020, 7);
      btnRun.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      btnRun.Name = "btnRun";
      btnRun.Size = new System.Drawing.Size(130, 53);
      btnRun.TabIndex = 7;
      btnRun.Text = "Run";
      toolTip.SetToolTip(btnRun, "Run the source sample");
      btnRun.UseVisualStyleBackColor = true;
      btnRun.Click += btnRun_Click;
      // 
      // btnFileOpen
      // 
      btnFileOpen.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      btnFileOpen.Location = new System.Drawing.Point(732, 7);
      btnFileOpen.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      btnFileOpen.Name = "btnFileOpen";
      btnFileOpen.Size = new System.Drawing.Size(130, 53);
      btnFileOpen.TabIndex = 6;
      btnFileOpen.Text = "Load ...";
      toolTip.SetToolTip(btnFileOpen, "Load a source sample...");
      btnFileOpen.UseVisualStyleBackColor = true;
      btnFileOpen.Click += btnFileOpen_Click;
      // 
      // btnParse
      // 
      btnParse.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      btnParse.Location = new System.Drawing.Point(874, 7);
      btnParse.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      btnParse.Name = "btnParse";
      btnParse.Size = new System.Drawing.Size(134, 53);
      btnParse.TabIndex = 1;
      btnParse.Text = "Parse";
      toolTip.SetToolTip(btnParse, "Parse source sample");
      btnParse.UseVisualStyleBackColor = true;
      btnParse.Click += btnParse_Click;
      // 
      // splitRight
      // 
      splitRight.Dock = System.Windows.Forms.DockStyle.Right;
      splitRight.Location = new System.Drawing.Point(1318, 7);
      splitRight.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      splitRight.Name = "splitRight";
      splitRight.Size = new System.Drawing.Size(12, 822);
      splitRight.TabIndex = 14;
      splitRight.TabStop = false;
      // 
      // tabOutput
      // 
      tabOutput.Controls.Add(pageSyntaxTree);
      tabOutput.Controls.Add(pageAst);
      tabOutput.Dock = System.Windows.Forms.DockStyle.Right;
      tabOutput.Location = new System.Drawing.Point(1330, 7);
      tabOutput.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      tabOutput.Name = "tabOutput";
      tabOutput.SelectedIndex = 0;
      tabOutput.Size = new System.Drawing.Size(700, 822);
      tabOutput.TabIndex = 13;
      // 
      // pageSyntaxTree
      // 
      pageSyntaxTree.Controls.Add(tvParseTree);
      pageSyntaxTree.ForeColor = System.Drawing.SystemColors.ControlText;
      pageSyntaxTree.Location = new System.Drawing.Point(4, 39);
      pageSyntaxTree.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageSyntaxTree.Name = "pageSyntaxTree";
      pageSyntaxTree.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageSyntaxTree.Size = new System.Drawing.Size(692, 779);
      pageSyntaxTree.TabIndex = 1;
      pageSyntaxTree.Text = "Parse Tree";
      pageSyntaxTree.UseVisualStyleBackColor = true;
      // 
      // tvParseTree
      // 
      tvParseTree.Dock = System.Windows.Forms.DockStyle.Fill;
      tvParseTree.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
      tvParseTree.HideSelection = false;
      tvParseTree.Indent = 16;
      tvParseTree.Location = new System.Drawing.Point(6, 7);
      tvParseTree.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      tvParseTree.Name = "tvParseTree";
      tvParseTree.Size = new System.Drawing.Size(680, 765);
      tvParseTree.TabIndex = 0;
      tvParseTree.AfterSelect += tvParseTree_AfterSelect;
      // 
      // pageAst
      // 
      pageAst.Controls.Add(tvAst);
      pageAst.Location = new System.Drawing.Point(4, 39);
      pageAst.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageAst.Name = "pageAst";
      pageAst.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageAst.Size = new System.Drawing.Size(692, 779);
      pageAst.TabIndex = 0;
      pageAst.Text = "AST";
      pageAst.UseVisualStyleBackColor = true;
      // 
      // tvAst
      // 
      tvAst.Dock = System.Windows.Forms.DockStyle.Fill;
      tvAst.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
      tvAst.HideSelection = false;
      tvAst.Indent = 16;
      tvAst.Location = new System.Drawing.Point(6, 7);
      tvAst.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      tvAst.Name = "tvAst";
      tvAst.Size = new System.Drawing.Size(680, 765);
      tvAst.TabIndex = 1;
      tvAst.AfterSelect += tvAst_AfterSelect;
      // 
      // chkParserTrace
      // 
      chkParserTrace.AutoSize = true;
      chkParserTrace.Location = new System.Drawing.Point(6, 7);
      chkParserTrace.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      chkParserTrace.Name = "chkParserTrace";
      chkParserTrace.Size = new System.Drawing.Size(155, 34);
      chkParserTrace.TabIndex = 0;
      chkParserTrace.Text = "Enable Trace";
      chkParserTrace.UseVisualStyleBackColor = true;
      // 
      // pnlLang
      // 
      pnlLang.Controls.Add(chkAutoRefresh);
      pnlLang.Controls.Add(btnManageGrammars);
      pnlLang.Controls.Add(lblSearchError);
      pnlLang.Controls.Add(btnSearch);
      pnlLang.Controls.Add(txtSearch);
      pnlLang.Controls.Add(label2);
      pnlLang.Controls.Add(cboGrammars);
      pnlLang.Dock = System.Windows.Forms.DockStyle.Top;
      pnlLang.Location = new System.Drawing.Point(0, 0);
      pnlLang.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pnlLang.Name = "pnlLang";
      pnlLang.Size = new System.Drawing.Size(2044, 67);
      pnlLang.TabIndex = 13;
      // 
      // chkAutoRefresh
      // 
      chkAutoRefresh.AutoSize = true;
      chkAutoRefresh.Checked = true;
      chkAutoRefresh.CheckState = System.Windows.Forms.CheckState.Checked;
      chkAutoRefresh.Location = new System.Drawing.Point(678, 16);
      chkAutoRefresh.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      chkAutoRefresh.Name = "chkAutoRefresh";
      chkAutoRefresh.Size = new System.Drawing.Size(156, 34);
      chkAutoRefresh.TabIndex = 13;
      chkAutoRefresh.Text = "Auto-refresh";
      toolTip.SetToolTip(chkAutoRefresh, resources.GetString("chkAutoRefresh.ToolTip"));
      chkAutoRefresh.UseVisualStyleBackColor = true;
      // 
      // btnManageGrammars
      // 
      btnManageGrammars.Location = new System.Drawing.Point(612, 5);
      btnManageGrammars.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      btnManageGrammars.Name = "btnManageGrammars";
      btnManageGrammars.Size = new System.Drawing.Size(56, 55);
      btnManageGrammars.TabIndex = 12;
      btnManageGrammars.Text = "...";
      btnManageGrammars.UseVisualStyleBackColor = true;
      btnManageGrammars.Click += btnManageGrammars_Click;
      // 
      // lblSearchError
      // 
      lblSearchError.AutoSize = true;
      lblSearchError.ForeColor = System.Drawing.Color.Red;
      lblSearchError.Location = new System.Drawing.Point(1462, 21);
      lblSearchError.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      lblSearchError.Name = "lblSearchError";
      lblSearchError.Size = new System.Drawing.Size(109, 30);
      lblSearchError.TabIndex = 11;
      lblSearchError.Text = "Not found";
      lblSearchError.Visible = false;
      // 
      // btnSearch
      // 
      btnSearch.Location = new System.Drawing.Point(1344, 5);
      btnSearch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      btnSearch.Name = "btnSearch";
      btnSearch.Size = new System.Drawing.Size(110, 55);
      btnSearch.TabIndex = 10;
      btnSearch.Text = "Find";
      btnSearch.UseVisualStyleBackColor = true;
      btnSearch.Click += btnSearch_Click;
      // 
      // txtSearch
      // 
      txtSearch.AcceptsReturn = true;
      settings1.AutoRefresh = true;
      settings1.DisableHili = false;
      settings1.EnableTrace = false;
      settings1.Grammars = "";
      settings1.LanguageIndex = 0;
      settings1.SearchPattern = "";
      settings1.SettingsKey = "";
      settings1.SourceSample = "";
      txtSearch.DataBindings.Add(new System.Windows.Forms.Binding("Text", settings1, "SearchPattern", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      txtSearch.Location = new System.Drawing.Point(1090, 9);
      txtSearch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      txtSearch.Name = "txtSearch";
      txtSearch.Size = new System.Drawing.Size(242, 35);
      txtSearch.TabIndex = 8;
      txtSearch.KeyPress += txtSearch_KeyPress;
      // 
      // label2
      // 
      label2.AutoSize = true;
      label2.Location = new System.Drawing.Point(20, 14);
      label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label2.Name = "label2";
      label2.Size = new System.Drawing.Size(104, 30);
      label2.TabIndex = 4;
      label2.Text = "Grammar:";
      // 
      // cboGrammars
      // 
      cboGrammars.ContextMenuStrip = menuGrammars;
      cboGrammars.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      cboGrammars.FormattingEnabled = true;
      cboGrammars.Location = new System.Drawing.Point(134, 7);
      cboGrammars.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      cboGrammars.Name = "cboGrammars";
      cboGrammars.Size = new System.Drawing.Size(464, 38);
      cboGrammars.TabIndex = 3;
      cboGrammars.SelectedIndexChanged += cboGrammars_SelectedIndexChanged;
      // 
      // menuGrammars
      // 
      menuGrammars.ImageScalingSize = new System.Drawing.Size(28, 28);
      menuGrammars.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { miAdd, miRefresh, miSeparator, miRemove, miRemoveAll });
      menuGrammars.Name = "menuGrammars";
      menuGrammars.Size = new System.Drawing.Size(243, 154);
      menuGrammars.Opening += menuGrammars_Opening;
      // 
      // miAdd
      // 
      miAdd.Name = "miAdd";
      miAdd.Size = new System.Drawing.Size(242, 36);
      miAdd.Text = "Add grammar...";
      miAdd.Click += miAdd_Click;
      // 
      // miRefresh
      // 
      miRefresh.Name = "miRefresh";
      miRefresh.Size = new System.Drawing.Size(242, 36);
      miRefresh.Text = "Refresh selected";
      miRefresh.ToolTipText = "Reload grammar assembly and refresh the current grammar.\r\nUse Auto-refresh checkbox to do this automatically\r\nevery time the target assembly file is updated (recompiled).";
      miRefresh.Click += miRefresh_Click;
      // 
      // miSeparator
      // 
      miSeparator.Name = "miSeparator";
      miSeparator.Size = new System.Drawing.Size(239, 6);
      // 
      // miRemove
      // 
      miRemove.Name = "miRemove";
      miRemove.Size = new System.Drawing.Size(242, 36);
      miRemove.Text = "Remove selected";
      miRemove.Click += miRemove_Click;
      // 
      // miRemoveAll
      // 
      miRemoveAll.Name = "miRemoveAll";
      miRemoveAll.Size = new System.Drawing.Size(242, 36);
      miRemoveAll.Text = "Remove all";
      miRemoveAll.Click += miRemoveAll_Click;
      // 
      // dlgSelectAssembly
      // 
      dlgSelectAssembly.DefaultExt = "dll";
      dlgSelectAssembly.Filter = "DLL files|*.dll";
      dlgSelectAssembly.Title = "Select Grammar Assembly ";
      // 
      // splitBottom
      // 
      splitBottom.BackColor = System.Drawing.SystemColors.Control;
      splitBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
      splitBottom.Location = new System.Drawing.Point(0, 946);
      splitBottom.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      splitBottom.Name = "splitBottom";
      splitBottom.Size = new System.Drawing.Size(2044, 14);
      splitBottom.TabIndex = 22;
      splitBottom.TabStop = false;
      // 
      // tabBottom
      // 
      tabBottom.Controls.Add(pageLanguage);
      tabBottom.Controls.Add(pageGrammarErrors);
      tabBottom.Controls.Add(pageParserOutput);
      tabBottom.Controls.Add(pageParserTrace);
      tabBottom.Controls.Add(pageOutput);
      tabBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
      tabBottom.Location = new System.Drawing.Point(0, 960);
      tabBottom.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      tabBottom.Name = "tabBottom";
      tabBottom.SelectedIndex = 0;
      tabBottom.Size = new System.Drawing.Size(2044, 432);
      tabBottom.TabIndex = 0;
      // 
      // pageLanguage
      // 
      pageLanguage.Controls.Add(grpLanguageInfo);
      pageLanguage.Location = new System.Drawing.Point(4, 39);
      pageLanguage.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageLanguage.Name = "pageLanguage";
      pageLanguage.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageLanguage.Size = new System.Drawing.Size(2036, 389);
      pageLanguage.TabIndex = 1;
      pageLanguage.Text = "Grammar Info";
      pageLanguage.UseVisualStyleBackColor = true;
      // 
      // grpLanguageInfo
      // 
      grpLanguageInfo.Controls.Add(label8);
      grpLanguageInfo.Controls.Add(lblParserStateCount);
      grpLanguageInfo.Controls.Add(lblLanguageDescr);
      grpLanguageInfo.Controls.Add(txtGrammarComments);
      grpLanguageInfo.Controls.Add(label11);
      grpLanguageInfo.Controls.Add(label9);
      grpLanguageInfo.Controls.Add(lblLanguageVersion);
      grpLanguageInfo.Controls.Add(label10);
      grpLanguageInfo.Controls.Add(lblLanguage);
      grpLanguageInfo.Controls.Add(label4);
      grpLanguageInfo.Controls.Add(label6);
      grpLanguageInfo.Controls.Add(lblParserConstrTime);
      grpLanguageInfo.Dock = System.Windows.Forms.DockStyle.Fill;
      grpLanguageInfo.Location = new System.Drawing.Point(6, 7);
      grpLanguageInfo.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      grpLanguageInfo.Name = "grpLanguageInfo";
      grpLanguageInfo.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      grpLanguageInfo.Size = new System.Drawing.Size(2024, 375);
      grpLanguageInfo.TabIndex = 3;
      grpLanguageInfo.TabStop = false;
      // 
      // label8
      // 
      label8.AutoSize = true;
      label8.Location = new System.Drawing.Point(12, 261);
      label8.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label8.Name = "label8";
      label8.Size = new System.Drawing.Size(184, 30);
      label8.TabIndex = 26;
      label8.Text = "Parser state count:";
      // 
      // lblParserStateCount
      // 
      lblParserStateCount.AutoSize = true;
      lblParserStateCount.Location = new System.Drawing.Point(334, 261);
      lblParserStateCount.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      lblParserStateCount.Name = "lblParserStateCount";
      lblParserStateCount.Size = new System.Drawing.Size(24, 30);
      lblParserStateCount.TabIndex = 25;
      lblParserStateCount.Text = "0";
      // 
      // lblLanguageDescr
      // 
      lblLanguageDescr.Location = new System.Drawing.Point(214, 88);
      lblLanguageDescr.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      lblLanguageDescr.Name = "lblLanguageDescr";
      lblLanguageDescr.Size = new System.Drawing.Size(1226, 51);
      lblLanguageDescr.TabIndex = 24;
      lblLanguageDescr.Text = "(description)";
      // 
      // txtGrammarComments
      // 
      txtGrammarComments.BackColor = System.Drawing.SystemColors.Window;
      txtGrammarComments.BorderStyle = System.Windows.Forms.BorderStyle.None;
      txtGrammarComments.Location = new System.Drawing.Point(222, 145);
      txtGrammarComments.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      txtGrammarComments.Multiline = true;
      txtGrammarComments.Name = "txtGrammarComments";
      txtGrammarComments.ReadOnly = true;
      txtGrammarComments.Size = new System.Drawing.Size(1218, 108);
      txtGrammarComments.TabIndex = 23;
      // 
      // label11
      // 
      label11.AutoSize = true;
      label11.Location = new System.Drawing.Point(12, 141);
      label11.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label11.Name = "label11";
      label11.Size = new System.Drawing.Size(201, 30);
      label11.TabIndex = 22;
      label11.Text = "Grammar Comment:";
      // 
      // label9
      // 
      label9.AutoSize = true;
      label9.Location = new System.Drawing.Point(12, 88);
      label9.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label9.Name = "label9";
      label9.Size = new System.Drawing.Size(123, 30);
      label9.TabIndex = 20;
      label9.Text = "Description:";
      // 
      // lblLanguageVersion
      // 
      lblLanguageVersion.Location = new System.Drawing.Point(556, 37);
      lblLanguageVersion.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      lblLanguageVersion.Name = "lblLanguageVersion";
      lblLanguageVersion.Size = new System.Drawing.Size(160, 39);
      lblLanguageVersion.TabIndex = 19;
      lblLanguageVersion.Text = "(Version)";
      // 
      // label10
      // 
      label10.AutoSize = true;
      label10.Location = new System.Drawing.Point(454, 37);
      label10.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label10.Name = "label10";
      label10.Size = new System.Drawing.Size(86, 30);
      label10.TabIndex = 18;
      label10.Text = "Version:";
      // 
      // lblLanguage
      // 
      lblLanguage.Location = new System.Drawing.Point(214, 37);
      lblLanguage.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      lblLanguage.Name = "lblLanguage";
      lblLanguage.Size = new System.Drawing.Size(460, 39);
      lblLanguage.TabIndex = 17;
      lblLanguage.Text = "(Language name)";
      // 
      // label4
      // 
      label4.AutoSize = true;
      label4.Location = new System.Drawing.Point(12, 37);
      label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label4.Name = "label4";
      label4.Size = new System.Drawing.Size(109, 30);
      label4.TabIndex = 16;
      label4.Text = "Language:";
      // 
      // label6
      // 
      label6.AutoSize = true;
      label6.Location = new System.Drawing.Point(12, 305);
      label6.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label6.Name = "label6";
      label6.Size = new System.Drawing.Size(280, 30);
      label6.TabIndex = 15;
      label6.Text = "Parser construction time, ms:";
      // 
      // lblParserConstrTime
      // 
      lblParserConstrTime.AutoSize = true;
      lblParserConstrTime.Location = new System.Drawing.Point(334, 305);
      lblParserConstrTime.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      lblParserConstrTime.Name = "lblParserConstrTime";
      lblParserConstrTime.Size = new System.Drawing.Size(24, 30);
      lblParserConstrTime.TabIndex = 14;
      lblParserConstrTime.Text = "0";
      // 
      // pageGrammarErrors
      // 
      pageGrammarErrors.Controls.Add(gridGrammarErrors);
      pageGrammarErrors.Location = new System.Drawing.Point(4, 39);
      pageGrammarErrors.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageGrammarErrors.Name = "pageGrammarErrors";
      pageGrammarErrors.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageGrammarErrors.Size = new System.Drawing.Size(2036, 389);
      pageGrammarErrors.TabIndex = 4;
      pageGrammarErrors.Text = "Grammar Errors";
      pageGrammarErrors.UseVisualStyleBackColor = true;
      // 
      // gridGrammarErrors
      // 
      gridGrammarErrors.AllowUserToAddRows = false;
      gridGrammarErrors.AllowUserToDeleteRows = false;
      gridGrammarErrors.ColumnHeadersHeight = 24;
      gridGrammarErrors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      gridGrammarErrors.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn5, dataGridViewTextBoxColumn6 });
      gridGrammarErrors.Dock = System.Windows.Forms.DockStyle.Fill;
      gridGrammarErrors.Location = new System.Drawing.Point(6, 7);
      gridGrammarErrors.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      gridGrammarErrors.MultiSelect = false;
      gridGrammarErrors.Name = "gridGrammarErrors";
      gridGrammarErrors.ReadOnly = true;
      gridGrammarErrors.RowHeadersVisible = false;
      gridGrammarErrors.RowHeadersWidth = 72;
      gridGrammarErrors.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
      gridGrammarErrors.Size = new System.Drawing.Size(2024, 375);
      gridGrammarErrors.TabIndex = 3;
      gridGrammarErrors.CellDoubleClick += gridGrammarErrors_CellDoubleClick;
      // 
      // dataGridViewTextBoxColumn2
      // 
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewTextBoxColumn2.DefaultCellStyle = dataGridViewCellStyle1;
      dataGridViewTextBoxColumn2.HeaderText = "Error Level";
      dataGridViewTextBoxColumn2.MinimumWidth = 9;
      dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
      dataGridViewTextBoxColumn2.ReadOnly = true;
      dataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      dataGridViewTextBoxColumn2.ToolTipText = "Double-click grid cell to locate in source code";
      dataGridViewTextBoxColumn2.Width = 175;
      // 
      // dataGridViewTextBoxColumn5
      // 
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      dataGridViewTextBoxColumn5.DefaultCellStyle = dataGridViewCellStyle2;
      dataGridViewTextBoxColumn5.HeaderText = "Description";
      dataGridViewTextBoxColumn5.MinimumWidth = 9;
      dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
      dataGridViewTextBoxColumn5.ReadOnly = true;
      dataGridViewTextBoxColumn5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      dataGridViewTextBoxColumn5.Width = 800;
      // 
      // dataGridViewTextBoxColumn6
      // 
      dataGridViewTextBoxColumn6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewTextBoxColumn6.DataPropertyName = "State";
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewTextBoxColumn6.DefaultCellStyle = dataGridViewCellStyle3;
      dataGridViewTextBoxColumn6.HeaderText = "Parser State";
      dataGridViewTextBoxColumn6.MinimumWidth = 9;
      dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
      dataGridViewTextBoxColumn6.ReadOnly = true;
      dataGridViewTextBoxColumn6.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      dataGridViewTextBoxColumn6.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      dataGridViewTextBoxColumn6.ToolTipText = "Double-click grid cell to navigate to state details";
      dataGridViewTextBoxColumn6.Width = 127;
      // 
      // pageParserOutput
      // 
      pageParserOutput.Controls.Add(groupBox1);
      pageParserOutput.Controls.Add(grpCompileInfo);
      pageParserOutput.Location = new System.Drawing.Point(4, 39);
      pageParserOutput.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageParserOutput.Name = "pageParserOutput";
      pageParserOutput.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageParserOutput.Size = new System.Drawing.Size(2036, 389);
      pageParserOutput.TabIndex = 2;
      pageParserOutput.Text = "Parser Output";
      pageParserOutput.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      groupBox1.Controls.Add(gridCompileErrors);
      groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      groupBox1.Location = new System.Drawing.Point(316, 7);
      groupBox1.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      groupBox1.Name = "groupBox1";
      groupBox1.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      groupBox1.Size = new System.Drawing.Size(1714, 375);
      groupBox1.TabIndex = 3;
      groupBox1.TabStop = false;
      groupBox1.Text = "Compile Errors";
      // 
      // gridCompileErrors
      // 
      gridCompileErrors.AllowUserToAddRows = false;
      gridCompileErrors.AllowUserToDeleteRows = false;
      gridCompileErrors.ColumnHeadersHeight = 24;
      gridCompileErrors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      gridCompileErrors.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4, dataGridViewTextBoxColumn1 });
      gridCompileErrors.Dock = System.Windows.Forms.DockStyle.Fill;
      gridCompileErrors.Location = new System.Drawing.Point(6, 35);
      gridCompileErrors.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      gridCompileErrors.MultiSelect = false;
      gridCompileErrors.Name = "gridCompileErrors";
      gridCompileErrors.ReadOnly = true;
      gridCompileErrors.RowHeadersVisible = false;
      gridCompileErrors.RowHeadersWidth = 72;
      gridCompileErrors.RowTemplate.Height = 24;
      gridCompileErrors.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
      gridCompileErrors.Size = new System.Drawing.Size(1702, 333);
      gridCompileErrors.TabIndex = 2;
      gridCompileErrors.CellDoubleClick += gridCompileErrors_CellDoubleClick;
      // 
      // dataGridViewTextBoxColumn3
      // 
      dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewTextBoxColumn3.DefaultCellStyle = dataGridViewCellStyle4;
      dataGridViewTextBoxColumn3.HeaderText = "L, C";
      dataGridViewTextBoxColumn3.MinimumWidth = 9;
      dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
      dataGridViewTextBoxColumn3.ReadOnly = true;
      dataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      dataGridViewTextBoxColumn3.ToolTipText = "Double-click grid cell to locate in source code";
      dataGridViewTextBoxColumn3.Width = 50;
      // 
      // dataGridViewTextBoxColumn4
      // 
      dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      dataGridViewTextBoxColumn4.DefaultCellStyle = dataGridViewCellStyle5;
      dataGridViewTextBoxColumn4.HeaderText = "Error Message";
      dataGridViewTextBoxColumn4.MinimumWidth = 9;
      dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
      dataGridViewTextBoxColumn4.ReadOnly = true;
      dataGridViewTextBoxColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      dataGridViewTextBoxColumn4.Width = 1000;
      // 
      // dataGridViewTextBoxColumn1
      // 
      dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewTextBoxColumn1.DataPropertyName = "State";
      dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewTextBoxColumn1.DefaultCellStyle = dataGridViewCellStyle6;
      dataGridViewTextBoxColumn1.HeaderText = "Parser State";
      dataGridViewTextBoxColumn1.MinimumWidth = 9;
      dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
      dataGridViewTextBoxColumn1.ReadOnly = true;
      dataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      dataGridViewTextBoxColumn1.ToolTipText = "Double-click grid cell to navigate to state details";
      dataGridViewTextBoxColumn1.Width = 127;
      // 
      // grpCompileInfo
      // 
      grpCompileInfo.Controls.Add(label12);
      grpCompileInfo.Controls.Add(lblParseErrorCount);
      grpCompileInfo.Controls.Add(label1);
      grpCompileInfo.Controls.Add(lblParseTime);
      grpCompileInfo.Controls.Add(label7);
      grpCompileInfo.Controls.Add(lblSrcLineCount);
      grpCompileInfo.Controls.Add(label3);
      grpCompileInfo.Controls.Add(lblSrcTokenCount);
      grpCompileInfo.Dock = System.Windows.Forms.DockStyle.Left;
      grpCompileInfo.Location = new System.Drawing.Point(6, 7);
      grpCompileInfo.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      grpCompileInfo.Name = "grpCompileInfo";
      grpCompileInfo.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      grpCompileInfo.Size = new System.Drawing.Size(310, 375);
      grpCompileInfo.TabIndex = 5;
      grpCompileInfo.TabStop = false;
      grpCompileInfo.Text = "Statistics";
      // 
      // label12
      // 
      label12.AutoSize = true;
      label12.Location = new System.Drawing.Point(24, 187);
      label12.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label12.Name = "label12";
      label12.Size = new System.Drawing.Size(71, 30);
      label12.TabIndex = 19;
      label12.Text = "Errors:";
      // 
      // lblParseErrorCount
      // 
      lblParseErrorCount.AutoSize = true;
      lblParseErrorCount.Location = new System.Drawing.Point(216, 187);
      lblParseErrorCount.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      lblParseErrorCount.Name = "lblParseErrorCount";
      lblParseErrorCount.Size = new System.Drawing.Size(24, 30);
      lblParseErrorCount.TabIndex = 18;
      lblParseErrorCount.Text = "0";
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Location = new System.Drawing.Point(24, 136);
      label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(156, 30);
      label1.TabIndex = 17;
      label1.Text = "Parse Time, ms:";
      // 
      // lblParseTime
      // 
      lblParseTime.AutoSize = true;
      lblParseTime.Location = new System.Drawing.Point(216, 136);
      lblParseTime.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      lblParseTime.Name = "lblParseTime";
      lblParseTime.Size = new System.Drawing.Size(24, 30);
      lblParseTime.TabIndex = 16;
      lblParseTime.Text = "0";
      // 
      // label7
      // 
      label7.AutoSize = true;
      label7.Location = new System.Drawing.Point(24, 37);
      label7.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label7.Name = "label7";
      label7.Size = new System.Drawing.Size(65, 30);
      label7.TabIndex = 15;
      label7.Text = "Lines:";
      // 
      // lblSrcLineCount
      // 
      lblSrcLineCount.AutoSize = true;
      lblSrcLineCount.Location = new System.Drawing.Point(216, 37);
      lblSrcLineCount.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      lblSrcLineCount.Name = "lblSrcLineCount";
      lblSrcLineCount.Size = new System.Drawing.Size(24, 30);
      lblSrcLineCount.TabIndex = 14;
      lblSrcLineCount.Text = "0";
      // 
      // label3
      // 
      label3.AutoSize = true;
      label3.Location = new System.Drawing.Point(24, 85);
      label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label3.Name = "label3";
      label3.Size = new System.Drawing.Size(81, 30);
      label3.TabIndex = 13;
      label3.Text = "Tokens:";
      // 
      // lblSrcTokenCount
      // 
      lblSrcTokenCount.AutoSize = true;
      lblSrcTokenCount.Location = new System.Drawing.Point(216, 85);
      lblSrcTokenCount.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      lblSrcTokenCount.Name = "lblSrcTokenCount";
      lblSrcTokenCount.Size = new System.Drawing.Size(24, 30);
      lblSrcTokenCount.TabIndex = 12;
      lblSrcTokenCount.Text = "0";
      // 
      // pageParserTrace
      // 
      pageParserTrace.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      pageParserTrace.Controls.Add(grpParserActions);
      pageParserTrace.Controls.Add(splitter1);
      pageParserTrace.Controls.Add(grpTokens);
      pageParserTrace.Controls.Add(pnlParserTraceTop);
      pageParserTrace.Location = new System.Drawing.Point(4, 39);
      pageParserTrace.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageParserTrace.Name = "pageParserTrace";
      pageParserTrace.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageParserTrace.Size = new System.Drawing.Size(2036, 389);
      pageParserTrace.TabIndex = 3;
      pageParserTrace.Text = "Parser Trace";
      pageParserTrace.UseVisualStyleBackColor = true;
      // 
      // grpParserActions
      // 
      grpParserActions.Controls.Add(gridParserTrace);
      grpParserActions.Dock = System.Windows.Forms.DockStyle.Fill;
      grpParserActions.Location = new System.Drawing.Point(6, 62);
      grpParserActions.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      grpParserActions.Name = "grpParserActions";
      grpParserActions.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      grpParserActions.Size = new System.Drawing.Size(1492, 318);
      grpParserActions.TabIndex = 4;
      grpParserActions.TabStop = false;
      // 
      // gridParserTrace
      // 
      gridParserTrace.AllowUserToAddRows = false;
      gridParserTrace.AllowUserToDeleteRows = false;
      gridParserTrace.AllowUserToResizeRows = false;
      gridParserTrace.ColumnHeadersHeight = 24;
      gridParserTrace.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      gridParserTrace.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { State, Stack, Input, Action });
      gridParserTrace.Dock = System.Windows.Forms.DockStyle.Fill;
      gridParserTrace.Location = new System.Drawing.Point(6, 35);
      gridParserTrace.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      gridParserTrace.MultiSelect = false;
      gridParserTrace.Name = "gridParserTrace";
      gridParserTrace.ReadOnly = true;
      gridParserTrace.RowHeadersVisible = false;
      gridParserTrace.RowHeadersWidth = 72;
      gridParserTrace.RowTemplate.Height = 24;
      gridParserTrace.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
      gridParserTrace.Size = new System.Drawing.Size(1480, 276);
      gridParserTrace.TabIndex = 0;
      gridParserTrace.CellDoubleClick += gridParserTrace_CellDoubleClick;
      // 
      // State
      // 
      State.DataPropertyName = "State";
      dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      State.DefaultCellStyle = dataGridViewCellStyle7;
      State.HeaderText = "State";
      State.MinimumWidth = 9;
      State.Name = "State";
      State.ReadOnly = true;
      State.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      State.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      State.ToolTipText = "Double-click grid cell to navigate to state details";
      State.Width = 60;
      // 
      // Stack
      // 
      Stack.DataPropertyName = "StackTop";
      dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      Stack.DefaultCellStyle = dataGridViewCellStyle8;
      Stack.HeaderText = "Stack Top";
      Stack.MinimumWidth = 9;
      Stack.Name = "Stack";
      Stack.ReadOnly = true;
      Stack.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      Stack.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      Stack.ToolTipText = "Double-click grid cell to locate node in source code";
      Stack.Width = 200;
      // 
      // Input
      // 
      Input.DataPropertyName = "Input";
      Input.HeaderText = "Input";
      Input.MinimumWidth = 9;
      Input.Name = "Input";
      Input.ReadOnly = true;
      Input.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      Input.ToolTipText = "Double-click grid cell to locate in source code";
      Input.Width = 200;
      // 
      // Action
      // 
      Action.DataPropertyName = "Action";
      Action.HeaderText = "Action";
      Action.MinimumWidth = 9;
      Action.Name = "Action";
      Action.ReadOnly = true;
      Action.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      Action.Width = 600;
      // 
      // splitter1
      // 
      splitter1.BackColor = System.Drawing.SystemColors.Control;
      splitter1.Dock = System.Windows.Forms.DockStyle.Right;
      splitter1.Location = new System.Drawing.Point(1498, 62);
      splitter1.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      splitter1.Name = "splitter1";
      splitter1.Size = new System.Drawing.Size(12, 318);
      splitter1.TabIndex = 15;
      splitter1.TabStop = false;
      // 
      // grpTokens
      // 
      grpTokens.Controls.Add(lstTokens);
      grpTokens.Dock = System.Windows.Forms.DockStyle.Right;
      grpTokens.Location = new System.Drawing.Point(1510, 62);
      grpTokens.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      grpTokens.Name = "grpTokens";
      grpTokens.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      grpTokens.Size = new System.Drawing.Size(518, 318);
      grpTokens.TabIndex = 3;
      grpTokens.TabStop = false;
      grpTokens.Text = "Tokens";
      // 
      // lstTokens
      // 
      lstTokens.Dock = System.Windows.Forms.DockStyle.Fill;
      lstTokens.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
      lstTokens.FormattingEnabled = true;
      lstTokens.ItemHeight = 21;
      lstTokens.Location = new System.Drawing.Point(6, 35);
      lstTokens.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      lstTokens.Name = "lstTokens";
      lstTokens.Size = new System.Drawing.Size(506, 276);
      lstTokens.TabIndex = 2;
      lstTokens.Click += lstTokens_Click;
      // 
      // pnlParserTraceTop
      // 
      pnlParserTraceTop.BackColor = System.Drawing.SystemColors.Control;
      pnlParserTraceTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      pnlParserTraceTop.Controls.Add(chkExcludeComments);
      pnlParserTraceTop.Controls.Add(lblTraceComment);
      pnlParserTraceTop.Controls.Add(chkParserTrace);
      pnlParserTraceTop.Dock = System.Windows.Forms.DockStyle.Top;
      pnlParserTraceTop.Location = new System.Drawing.Point(6, 7);
      pnlParserTraceTop.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pnlParserTraceTop.Name = "pnlParserTraceTop";
      pnlParserTraceTop.Size = new System.Drawing.Size(2022, 55);
      pnlParserTraceTop.TabIndex = 1;
      // 
      // chkExcludeComments
      // 
      chkExcludeComments.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      chkExcludeComments.AutoSize = true;
      chkExcludeComments.Checked = true;
      chkExcludeComments.CheckState = System.Windows.Forms.CheckState.Checked;
      chkExcludeComments.Location = new System.Drawing.Point(1725, 7);
      chkExcludeComments.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      chkExcludeComments.Name = "chkExcludeComments";
      chkExcludeComments.Size = new System.Drawing.Size(271, 34);
      chkExcludeComments.TabIndex = 2;
      chkExcludeComments.Text = "Exclude comment tokens";
      chkExcludeComments.UseVisualStyleBackColor = true;
      // 
      // lblTraceComment
      // 
      lblTraceComment.AutoSize = true;
      lblTraceComment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
      lblTraceComment.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      lblTraceComment.Location = new System.Drawing.Point(256, 7);
      lblTraceComment.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      lblTraceComment.Name = "lblTraceComment";
      lblTraceComment.Size = new System.Drawing.Size(638, 25);
      lblTraceComment.TabIndex = 1;
      lblTraceComment.Text = "(Double-click grid cell to navigate to parser state or source code position)";
      // 
      // pageOutput
      // 
      pageOutput.Controls.Add(txtOutput);
      pageOutput.Controls.Add(pnlRuntimeInfo);
      pageOutput.Location = new System.Drawing.Point(4, 39);
      pageOutput.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageOutput.Name = "pageOutput";
      pageOutput.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pageOutput.Size = new System.Drawing.Size(2036, 389);
      pageOutput.TabIndex = 0;
      pageOutput.Text = "Runtime Output";
      pageOutput.UseVisualStyleBackColor = true;
      // 
      // txtOutput
      // 
      txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
      txtOutput.Location = new System.Drawing.Point(6, 7);
      txtOutput.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      txtOutput.Multiline = true;
      txtOutput.Name = "txtOutput";
      txtOutput.Size = new System.Drawing.Size(1722, 375);
      txtOutput.TabIndex = 1;
      txtOutput.Text = "(runtime output is broken)";
      // 
      // pnlRuntimeInfo
      // 
      pnlRuntimeInfo.Controls.Add(label14);
      pnlRuntimeInfo.Controls.Add(lblGCCount);
      pnlRuntimeInfo.Controls.Add(label13);
      pnlRuntimeInfo.Controls.Add(lnkShowErrStack);
      pnlRuntimeInfo.Controls.Add(lnkShowErrLocation);
      pnlRuntimeInfo.Controls.Add(label5);
      pnlRuntimeInfo.Controls.Add(lblRunTime);
      pnlRuntimeInfo.Dock = System.Windows.Forms.DockStyle.Right;
      pnlRuntimeInfo.Location = new System.Drawing.Point(1728, 7);
      pnlRuntimeInfo.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      pnlRuntimeInfo.Name = "pnlRuntimeInfo";
      pnlRuntimeInfo.Size = new System.Drawing.Size(302, 375);
      pnlRuntimeInfo.TabIndex = 2;
      // 
      // label14
      // 
      label14.AutoSize = true;
      label14.Location = new System.Drawing.Point(12, 53);
      label14.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label14.Name = "label14";
      label14.Size = new System.Drawing.Size(205, 30);
      label14.TabIndex = 24;
      label14.Text = "GC Collection Count:";
      // 
      // lblGCCount
      // 
      lblGCCount.AutoSize = true;
      lblGCCount.Location = new System.Drawing.Point(248, 53);
      lblGCCount.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      lblGCCount.Name = "lblGCCount";
      lblGCCount.Size = new System.Drawing.Size(24, 30);
      lblGCCount.TabIndex = 23;
      lblGCCount.Text = "0";
      // 
      // label13
      // 
      label13.AutoSize = true;
      label13.Location = new System.Drawing.Point(10, 95);
      label13.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label13.Name = "label13";
      label13.Size = new System.Drawing.Size(146, 30);
      label13.TabIndex = 22;
      label13.Text = "Runtime error:";
      // 
      // lnkShowErrStack
      // 
      lnkShowErrStack.AutoSize = true;
      lnkShowErrStack.Enabled = false;
      lnkShowErrStack.Location = new System.Drawing.Point(46, 198);
      lnkShowErrStack.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      lnkShowErrStack.Name = "lnkShowErrStack";
      lnkShowErrStack.Size = new System.Drawing.Size(151, 30);
      lnkShowErrStack.TabIndex = 21;
      lnkShowErrStack.TabStop = true;
      lnkShowErrStack.Text = "Show full stack";
      lnkShowErrStack.LinkClicked += lnkShowErrStack_LinkClicked;
      // 
      // lnkShowErrLocation
      // 
      lnkShowErrLocation.AutoSize = true;
      lnkShowErrLocation.Enabled = false;
      lnkShowErrLocation.Location = new System.Drawing.Point(46, 143);
      lnkShowErrLocation.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      lnkShowErrLocation.Name = "lnkShowErrLocation";
      lnkShowErrLocation.Size = new System.Drawing.Size(193, 30);
      lnkShowErrLocation.TabIndex = 20;
      lnkShowErrLocation.TabStop = true;
      lnkShowErrLocation.Text = "Show error location";
      lnkShowErrLocation.LinkClicked += lnkShowErrLocation_LinkClicked;
      // 
      // label5
      // 
      label5.AutoSize = true;
      label5.Location = new System.Drawing.Point(10, 7);
      label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label5.Name = "label5";
      label5.Size = new System.Drawing.Size(193, 30);
      label5.TabIndex = 19;
      label5.Text = "Execution time, ms:";
      // 
      // lblRunTime
      // 
      lblRunTime.AutoSize = true;
      lblRunTime.Location = new System.Drawing.Point(246, 7);
      lblRunTime.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      lblRunTime.Name = "lblRunTime";
      lblRunTime.Size = new System.Drawing.Size(24, 30);
      lblRunTime.TabIndex = 18;
      lblRunTime.Text = "0";
      // 
      // fmGrammarExplorer
      // 
      AutoScaleDimensions = new System.Drawing.SizeF(12F, 30F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      ClientSize = new System.Drawing.Size(2044, 1392);
      Controls.Add(tabGrammar);
      Controls.Add(splitBottom);
      Controls.Add(pnlLang);
      Controls.Add(tabBottom);
      Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
      Name = "fmGrammarExplorer";
      StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      Text = "Irony Grammar Explorer";
      FormClosing += fmExploreGrammar_FormClosing;
      Load += fmExploreGrammar_Load;
      tabGrammar.ResumeLayout(false);
      pageTerminals.ResumeLayout(false);
      pageTerminals.PerformLayout();
      pageNonTerms.ResumeLayout(false);
      pageNonTerms.PerformLayout();
      pageParserStates.ResumeLayout(false);
      pageParserStates.PerformLayout();
      pageTest.ResumeLayout(false);
      pageTest.PerformLayout();
      panel1.ResumeLayout(false);
      tabOutput.ResumeLayout(false);
      pageSyntaxTree.ResumeLayout(false);
      pageAst.ResumeLayout(false);
      pnlLang.ResumeLayout(false);
      pnlLang.PerformLayout();
      menuGrammars.ResumeLayout(false);
      tabBottom.ResumeLayout(false);
      pageLanguage.ResumeLayout(false);
      grpLanguageInfo.ResumeLayout(false);
      grpLanguageInfo.PerformLayout();
      pageGrammarErrors.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)gridGrammarErrors).EndInit();
      pageParserOutput.ResumeLayout(false);
      groupBox1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)gridCompileErrors).EndInit();
      grpCompileInfo.ResumeLayout(false);
      grpCompileInfo.PerformLayout();
      pageParserTrace.ResumeLayout(false);
      grpParserActions.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)gridParserTrace).EndInit();
      grpTokens.ResumeLayout(false);
      pnlParserTraceTop.ResumeLayout(false);
      pnlParserTraceTop.PerformLayout();
      pageOutput.ResumeLayout(false);
      pageOutput.PerformLayout();
      pnlRuntimeInfo.ResumeLayout(false);
      pnlRuntimeInfo.PerformLayout();
      ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TabControl tabGrammar;
    private System.Windows.Forms.TabPage pageNonTerms;
    private System.Windows.Forms.TabPage pageParserStates;
    private System.Windows.Forms.TextBox txtNonTerms;
    private System.Windows.Forms.TextBox txtParserStates;
    private System.Windows.Forms.Panel pnlLang;
    private System.Windows.Forms.ComboBox cboGrammars;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TabPage pageTest;
    private System.Windows.Forms.Splitter splitRight;
    private System.Windows.Forms.TabControl tabOutput;
    private System.Windows.Forms.TabPage pageAst;
    private System.Windows.Forms.TabPage pageSyntaxTree;
    private System.Windows.Forms.TreeView tvParseTree;
    private System.Windows.Forms.OpenFileDialog dlgOpenFile;
    private System.Windows.Forms.TabPage pageTerminals;
    private System.Windows.Forms.TextBox txtTerms;
    private System.Windows.Forms.Button btnSearch;
    private System.Windows.Forms.TextBox txtSearch;
    private System.Windows.Forms.Label lblSearchError;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnRun;
    private System.Windows.Forms.CheckBox chkParserTrace;
    private System.Windows.Forms.Button btnFileOpen;
	private System.Windows.Forms.Button btnParse;
    private System.Windows.Forms.Button btnManageGrammars;
    private System.Windows.Forms.ContextMenuStrip menuGrammars;
    private System.Windows.Forms.ToolStripMenuItem miAdd;
    private System.Windows.Forms.ToolStripMenuItem miRemove;
    private System.Windows.Forms.OpenFileDialog dlgSelectAssembly;
    private System.Windows.Forms.ToolStripMenuItem miRemoveAll;
    private System.Windows.Forms.TabControl tabBottom;
    private System.Windows.Forms.TabPage pageOutput;
    private ConsoleTextBox txtOutput;
    private System.Windows.Forms.TabPage pageLanguage;
    private System.Windows.Forms.Splitter splitBottom;
    private System.Windows.Forms.GroupBox grpLanguageInfo;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label lblParserConstrTime;
    private System.Windows.Forms.TabPage pageParserOutput;
    private System.Windows.Forms.TabPage pageParserTrace;
    private System.Windows.Forms.TreeView tvAst;
    private System.Windows.Forms.DataGridView gridParserTrace;
    private System.Windows.Forms.GroupBox grpTokens;
    private System.Windows.Forms.Panel pnlParserTraceTop;
    private System.Windows.Forms.GroupBox grpParserActions;
    private System.Windows.Forms.Splitter splitter1;
    private System.Windows.Forms.ListBox lstTokens;
    private System.Windows.Forms.Label lblTraceComment;
    private System.Windows.Forms.DataGridView gridCompileErrors;
    private System.Windows.Forms.CheckBox chkExcludeComments;
    private System.Windows.Forms.TabPage pageGrammarErrors;
    private System.Windows.Forms.DataGridView gridGrammarErrors;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label lblParseTime;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label lblSrcLineCount;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label lblSrcTokenCount;
    private System.Windows.Forms.GroupBox grpCompileInfo;
    private System.Windows.Forms.Label lblLanguage;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Panel pnlRuntimeInfo;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label lblRunTime;
    private System.Windows.Forms.TextBox txtGrammarComments;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Label lblLanguageVersion;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Label lblParseErrorCount;
    private System.Windows.Forms.Label lblLanguageDescr;
    private System.Windows.Forms.LinkLabel lnkShowErrLocation;
    private System.Windows.Forms.LinkLabel lnkShowErrStack;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label lblParserStateCount;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
    private System.Windows.Forms.CheckBox chkAutoRefresh;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.Label lblGCCount;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private System.Windows.Forms.DataGridViewTextBoxColumn State;
    private System.Windows.Forms.DataGridViewTextBoxColumn Stack;
    private System.Windows.Forms.DataGridViewTextBoxColumn Input;
    private System.Windows.Forms.DataGridViewTextBoxColumn Action;
    private System.Windows.Forms.Button btnLocate;
    private System.Windows.Forms.TextBox txtSource;
    private System.Windows.Forms.ToolStripMenuItem miRefresh;
    private System.Windows.Forms.ToolStripSeparator miSeparator;

  }
}


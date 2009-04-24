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
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
      this.tabGrammar = new System.Windows.Forms.TabControl();
      this.pageTerminals = new System.Windows.Forms.TabPage();
      this.txtTerms = new System.Windows.Forms.TextBox();
      this.pageNonTerms = new System.Windows.Forms.TabPage();
      this.txtNonTerms = new System.Windows.Forms.TextBox();
      this.pageParserStates = new System.Windows.Forms.TabPage();
      this.txtParserStates = new System.Windows.Forms.TextBox();
      this.pageGrErrors = new System.Windows.Forms.TabPage();
      this.txtGrammarErrors = new System.Windows.Forms.TextBox();
      this.pageTest = new System.Windows.Forms.TabPage();
      this.txtSource = new System.Windows.Forms.RichTextBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnToXml = new System.Windows.Forms.Button();
      this.btnRun = new System.Windows.Forms.Button();
      this.btnFileOpen = new System.Windows.Forms.Button();
      this.btnParse = new System.Windows.Forms.Button();
      this.splitter3 = new System.Windows.Forms.Splitter();
      this.tabOutput = new System.Windows.Forms.TabControl();
      this.pageSyntaxTree = new System.Windows.Forms.TabPage();
      this.tvParseTree = new System.Windows.Forms.TreeView();
      this.pageAst = new System.Windows.Forms.TabPage();
      this.tvAst = new System.Windows.Forms.TreeView();
      this.chkParserTrace = new System.Windows.Forms.CheckBox();
      this.pnlLang = new System.Windows.Forms.Panel();
      this.cboParseMethod = new System.Windows.Forms.ComboBox();
      this.label8 = new System.Windows.Forms.Label();
      this.btnManageGrammars = new System.Windows.Forms.Button();
      this.lblSearchError = new System.Windows.Forms.Label();
      this.btnSearch = new System.Windows.Forms.Button();
      this.txtSearch = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cboGrammars = new System.Windows.Forms.ComboBox();
      this.menuGrammars = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.miAdd = new System.Windows.Forms.ToolStripMenuItem();
      this.miRemove = new System.Windows.Forms.ToolStripMenuItem();
      this.miRemoveAll = new System.Windows.Forms.ToolStripMenuItem();
      this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
      this.dlgSelectAssembly = new System.Windows.Forms.OpenFileDialog();
      this.splitBottom = new System.Windows.Forms.Splitter();
      this.tabBottom = new System.Windows.Forms.TabControl();
      this.pageStats = new System.Windows.Forms.TabPage();
      this.groupBox4 = new System.Windows.Forms.GroupBox();
      this.label5 = new System.Windows.Forms.Label();
      this.lblRunTime = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.lblInitTime = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.lblErrCount = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.lblStatLines = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.lblStatTokens = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.lblStatTime = new System.Windows.Forms.Label();
      this.pageOutput = new System.Windows.Forms.TabPage();
      this.txtOutput = new System.Windows.Forms.TextBox();
      this.pageErrors = new System.Windows.Forms.TabPage();
      this.gridCompileErrors = new System.Windows.Forms.DataGridView();
      this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.pageParserTrace = new System.Windows.Forms.TabPage();
      this.grpParserActions = new System.Windows.Forms.GroupBox();
      this.gridParserTrace = new System.Windows.Forms.DataGridView();
      this.State = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Stack = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Input = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Action = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.NewState = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.splitter1 = new System.Windows.Forms.Splitter();
      this.grpTokens = new System.Windows.Forms.GroupBox();
      this.lstTokens = new System.Windows.Forms.ListBox();
      this.pnlParserTraceTop = new System.Windows.Forms.Panel();
      this.chkExcludeComments = new System.Windows.Forms.CheckBox();
      this.lblTraceComment = new System.Windows.Forms.Label();
      this.bindParserTrace = new System.Windows.Forms.BindingSource(this.components);
      this.tabGrammar.SuspendLayout();
      this.pageTerminals.SuspendLayout();
      this.pageNonTerms.SuspendLayout();
      this.pageParserStates.SuspendLayout();
      this.pageGrErrors.SuspendLayout();
      this.pageTest.SuspendLayout();
      this.panel1.SuspendLayout();
      this.tabOutput.SuspendLayout();
      this.pageSyntaxTree.SuspendLayout();
      this.pageAst.SuspendLayout();
      this.pnlLang.SuspendLayout();
      this.menuGrammars.SuspendLayout();
      this.tabBottom.SuspendLayout();
      this.pageStats.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.pageOutput.SuspendLayout();
      this.pageErrors.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.gridCompileErrors)).BeginInit();
      this.pageParserTrace.SuspendLayout();
      this.grpParserActions.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.gridParserTrace)).BeginInit();
      this.grpTokens.SuspendLayout();
      this.pnlParserTraceTop.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.bindParserTrace)).BeginInit();
      this.SuspendLayout();
      // 
      // tabGrammar
      // 
      this.tabGrammar.Controls.Add(this.pageTerminals);
      this.tabGrammar.Controls.Add(this.pageNonTerms);
      this.tabGrammar.Controls.Add(this.pageParserStates);
      this.tabGrammar.Controls.Add(this.pageGrErrors);
      this.tabGrammar.Controls.Add(this.pageTest);
      this.tabGrammar.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabGrammar.Location = new System.Drawing.Point(0, 29);
      this.tabGrammar.Name = "tabGrammar";
      this.tabGrammar.SelectedIndex = 0;
      this.tabGrammar.Size = new System.Drawing.Size(1104, 464);
      this.tabGrammar.TabIndex = 0;
      // 
      // pageTerminals
      // 
      this.pageTerminals.Controls.Add(this.txtTerms);
      this.pageTerminals.Location = new System.Drawing.Point(4, 22);
      this.pageTerminals.Name = "pageTerminals";
      this.pageTerminals.Padding = new System.Windows.Forms.Padding(3);
      this.pageTerminals.Size = new System.Drawing.Size(1096, 438);
      this.pageTerminals.TabIndex = 5;
      this.pageTerminals.Text = "Terminals";
      this.pageTerminals.UseVisualStyleBackColor = true;
      // 
      // txtTerms
      // 
      this.txtTerms.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtTerms.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtTerms.HideSelection = false;
      this.txtTerms.Location = new System.Drawing.Point(3, 3);
      this.txtTerms.Multiline = true;
      this.txtTerms.Name = "txtTerms";
      this.txtTerms.ReadOnly = true;
      this.txtTerms.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtTerms.Size = new System.Drawing.Size(1090, 432);
      this.txtTerms.TabIndex = 2;
      // 
      // pageNonTerms
      // 
      this.pageNonTerms.Controls.Add(this.txtNonTerms);
      this.pageNonTerms.Location = new System.Drawing.Point(4, 22);
      this.pageNonTerms.Name = "pageNonTerms";
      this.pageNonTerms.Padding = new System.Windows.Forms.Padding(3);
      this.pageNonTerms.Size = new System.Drawing.Size(1096, 438);
      this.pageNonTerms.TabIndex = 0;
      this.pageNonTerms.Text = "Non-Terminals";
      this.pageNonTerms.UseVisualStyleBackColor = true;
      // 
      // txtNonTerms
      // 
      this.txtNonTerms.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtNonTerms.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtNonTerms.HideSelection = false;
      this.txtNonTerms.Location = new System.Drawing.Point(3, 3);
      this.txtNonTerms.Multiline = true;
      this.txtNonTerms.Name = "txtNonTerms";
      this.txtNonTerms.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtNonTerms.Size = new System.Drawing.Size(1090, 432);
      this.txtNonTerms.TabIndex = 1;
      this.txtNonTerms.WordWrap = false;
      // 
      // pageParserStates
      // 
      this.pageParserStates.Controls.Add(this.txtParserStates);
      this.pageParserStates.Location = new System.Drawing.Point(4, 22);
      this.pageParserStates.Name = "pageParserStates";
      this.pageParserStates.Padding = new System.Windows.Forms.Padding(3);
      this.pageParserStates.Size = new System.Drawing.Size(1096, 438);
      this.pageParserStates.TabIndex = 1;
      this.pageParserStates.Text = "Parser States";
      this.pageParserStates.UseVisualStyleBackColor = true;
      // 
      // txtParserStates
      // 
      this.txtParserStates.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtParserStates.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtParserStates.HideSelection = false;
      this.txtParserStates.Location = new System.Drawing.Point(3, 3);
      this.txtParserStates.Multiline = true;
      this.txtParserStates.Name = "txtParserStates";
      this.txtParserStates.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtParserStates.Size = new System.Drawing.Size(1090, 432);
      this.txtParserStates.TabIndex = 2;
      this.txtParserStates.WordWrap = false;
      // 
      // pageGrErrors
      // 
      this.pageGrErrors.Controls.Add(this.txtGrammarErrors);
      this.pageGrErrors.Location = new System.Drawing.Point(4, 22);
      this.pageGrErrors.Name = "pageGrErrors";
      this.pageGrErrors.Padding = new System.Windows.Forms.Padding(3);
      this.pageGrErrors.Size = new System.Drawing.Size(1096, 438);
      this.pageGrErrors.TabIndex = 2;
      this.pageGrErrors.Text = "Grammar Errors";
      this.pageGrErrors.UseVisualStyleBackColor = true;
      // 
      // txtGrammarErrors
      // 
      this.txtGrammarErrors.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtGrammarErrors.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtGrammarErrors.HideSelection = false;
      this.txtGrammarErrors.Location = new System.Drawing.Point(3, 3);
      this.txtGrammarErrors.Multiline = true;
      this.txtGrammarErrors.Name = "txtGrammarErrors";
      this.txtGrammarErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtGrammarErrors.Size = new System.Drawing.Size(1090, 432);
      this.txtGrammarErrors.TabIndex = 2;
      // 
      // pageTest
      // 
      this.pageTest.Controls.Add(this.txtSource);
      this.pageTest.Controls.Add(this.panel1);
      this.pageTest.Controls.Add(this.splitter3);
      this.pageTest.Controls.Add(this.tabOutput);
      this.pageTest.Location = new System.Drawing.Point(4, 22);
      this.pageTest.Name = "pageTest";
      this.pageTest.Padding = new System.Windows.Forms.Padding(3);
      this.pageTest.Size = new System.Drawing.Size(1096, 438);
      this.pageTest.TabIndex = 4;
      this.pageTest.Text = "Test";
      this.pageTest.UseVisualStyleBackColor = true;
      // 
      // txtSource
      // 
      this.txtSource.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtSource.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtSource.HideSelection = false;
      this.txtSource.Location = new System.Drawing.Point(3, 33);
      this.txtSource.Name = "txtSource";
      this.txtSource.Size = new System.Drawing.Size(734, 402);
      this.txtSource.TabIndex = 22;
      this.txtSource.Text = "";
      this.txtSource.TextChanged += new System.EventHandler(this.txtSource_TextChanged);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnToXml);
      this.panel1.Controls.Add(this.btnRun);
      this.panel1.Controls.Add(this.btnFileOpen);
      this.panel1.Controls.Add(this.btnParse);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(3, 3);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(734, 30);
      this.panel1.TabIndex = 2;
      // 
      // btnToXml
      // 
      this.btnToXml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnToXml.Location = new System.Drawing.Point(655, 3);
      this.btnToXml.Name = "btnToXml";
      this.btnToXml.Size = new System.Drawing.Size(65, 23);
      this.btnToXml.TabIndex = 8;
      this.btnToXml.Text = "->XML";
      this.btnToXml.UseVisualStyleBackColor = true;
      this.btnToXml.Click += new System.EventHandler(this.btnToXml_Click);
      // 
      // btnRun
      // 
      this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRun.Location = new System.Drawing.Point(584, 3);
      this.btnRun.Name = "btnRun";
      this.btnRun.Size = new System.Drawing.Size(65, 23);
      this.btnRun.TabIndex = 7;
      this.btnRun.Text = "Run";
      this.btnRun.UseVisualStyleBackColor = true;
      this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
      // 
      // btnFileOpen
      // 
      this.btnFileOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnFileOpen.Location = new System.Drawing.Point(440, 3);
      this.btnFileOpen.Name = "btnFileOpen";
      this.btnFileOpen.Size = new System.Drawing.Size(65, 23);
      this.btnFileOpen.TabIndex = 6;
      this.btnFileOpen.Text = "Load ...";
      this.btnFileOpen.UseVisualStyleBackColor = true;
      this.btnFileOpen.Click += new System.EventHandler(this.btnFileOpen_Click);
      // 
      // btnParse
      // 
      this.btnParse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnParse.Location = new System.Drawing.Point(511, 3);
      this.btnParse.Name = "btnParse";
      this.btnParse.Size = new System.Drawing.Size(67, 23);
      this.btnParse.TabIndex = 1;
      this.btnParse.Text = "Parse";
      this.btnParse.UseVisualStyleBackColor = true;
      this.btnParse.Click += new System.EventHandler(this.btnParse_Click);
      // 
      // splitter3
      // 
      this.splitter3.Dock = System.Windows.Forms.DockStyle.Right;
      this.splitter3.Location = new System.Drawing.Point(737, 3);
      this.splitter3.Name = "splitter3";
      this.splitter3.Size = new System.Drawing.Size(6, 432);
      this.splitter3.TabIndex = 14;
      this.splitter3.TabStop = false;
      // 
      // tabOutput
      // 
      this.tabOutput.Controls.Add(this.pageSyntaxTree);
      this.tabOutput.Controls.Add(this.pageAst);
      this.tabOutput.Dock = System.Windows.Forms.DockStyle.Right;
      this.tabOutput.Location = new System.Drawing.Point(743, 3);
      this.tabOutput.Name = "tabOutput";
      this.tabOutput.SelectedIndex = 0;
      this.tabOutput.Size = new System.Drawing.Size(350, 432);
      this.tabOutput.TabIndex = 13;
      // 
      // pageSyntaxTree
      // 
      this.pageSyntaxTree.Controls.Add(this.tvParseTree);
      this.pageSyntaxTree.ForeColor = System.Drawing.SystemColors.ControlText;
      this.pageSyntaxTree.Location = new System.Drawing.Point(4, 22);
      this.pageSyntaxTree.Name = "pageSyntaxTree";
      this.pageSyntaxTree.Padding = new System.Windows.Forms.Padding(3);
      this.pageSyntaxTree.Size = new System.Drawing.Size(342, 406);
      this.pageSyntaxTree.TabIndex = 1;
      this.pageSyntaxTree.Text = "Parse Tree";
      this.pageSyntaxTree.UseVisualStyleBackColor = true;
      // 
      // tvParseTree
      // 
      this.tvParseTree.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tvParseTree.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tvParseTree.Indent = 16;
      this.tvParseTree.Location = new System.Drawing.Point(3, 3);
      this.tvParseTree.Name = "tvParseTree";
      this.tvParseTree.Size = new System.Drawing.Size(336, 400);
      this.tvParseTree.TabIndex = 0;
      this.tvParseTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvAstNodes_AfterSelect);
      // 
      // pageAst
      // 
      this.pageAst.Controls.Add(this.tvAst);
      this.pageAst.Location = new System.Drawing.Point(4, 22);
      this.pageAst.Name = "pageAst";
      this.pageAst.Padding = new System.Windows.Forms.Padding(3);
      this.pageAst.Size = new System.Drawing.Size(342, 406);
      this.pageAst.TabIndex = 0;
      this.pageAst.Text = "AST";
      this.pageAst.UseVisualStyleBackColor = true;
      // 
      // tvAst
      // 
      this.tvAst.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tvAst.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tvAst.Indent = 16;
      this.tvAst.Location = new System.Drawing.Point(3, 3);
      this.tvAst.Name = "tvAst";
      this.tvAst.Size = new System.Drawing.Size(336, 400);
      this.tvAst.TabIndex = 1;
      // 
      // chkParserTrace
      // 
      this.chkParserTrace.AutoSize = true;
      this.chkParserTrace.Location = new System.Drawing.Point(3, 3);
      this.chkParserTrace.Name = "chkParserTrace";
      this.chkParserTrace.Size = new System.Drawing.Size(90, 17);
      this.chkParserTrace.TabIndex = 0;
      this.chkParserTrace.Text = "Enable Trace";
      this.chkParserTrace.UseVisualStyleBackColor = true;
      // 
      // pnlLang
      // 
      this.pnlLang.Controls.Add(this.cboParseMethod);
      this.pnlLang.Controls.Add(this.label8);
      this.pnlLang.Controls.Add(this.btnManageGrammars);
      this.pnlLang.Controls.Add(this.lblSearchError);
      this.pnlLang.Controls.Add(this.btnSearch);
      this.pnlLang.Controls.Add(this.txtSearch);
      this.pnlLang.Controls.Add(this.label2);
      this.pnlLang.Controls.Add(this.cboGrammars);
      this.pnlLang.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlLang.Location = new System.Drawing.Point(0, 0);
      this.pnlLang.Name = "pnlLang";
      this.pnlLang.Size = new System.Drawing.Size(1104, 29);
      this.pnlLang.TabIndex = 13;
      // 
      // cboParseMethod
      // 
      this.cboParseMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboParseMethod.FormattingEnabled = true;
      this.cboParseMethod.Items.AddRange(new object[] {
            "LALR",
            "NLALR"});
      this.cboParseMethod.Location = new System.Drawing.Point(406, 5);
      this.cboParseMethod.Name = "cboParseMethod";
      this.cboParseMethod.Size = new System.Drawing.Size(102, 21);
      this.cboParseMethod.TabIndex = 14;
      this.cboParseMethod.SelectedIndexChanged += new System.EventHandler(this.cboParseMethod_SelectedIndexChanged);
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(325, 9);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(75, 13);
      this.label8.TabIndex = 13;
      this.label8.Text = "Parse method:";
      // 
      // btnManageGrammars
      // 
      this.btnManageGrammars.Location = new System.Drawing.Point(281, 2);
      this.btnManageGrammars.Margin = new System.Windows.Forms.Padding(2);
      this.btnManageGrammars.Name = "btnManageGrammars";
      this.btnManageGrammars.Size = new System.Drawing.Size(28, 24);
      this.btnManageGrammars.TabIndex = 12;
      this.btnManageGrammars.Text = "...";
      this.btnManageGrammars.UseVisualStyleBackColor = true;
      this.btnManageGrammars.Click += new System.EventHandler(this.btnManageGrammars_Click);
      // 
      // lblSearchError
      // 
      this.lblSearchError.AutoSize = true;
      this.lblSearchError.ForeColor = System.Drawing.Color.Red;
      this.lblSearchError.Location = new System.Drawing.Point(731, 9);
      this.lblSearchError.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lblSearchError.Name = "lblSearchError";
      this.lblSearchError.Size = new System.Drawing.Size(54, 13);
      this.lblSearchError.TabIndex = 11;
      this.lblSearchError.Text = "Not found";
      this.lblSearchError.Visible = false;
      // 
      // btnSearch
      // 
      this.btnSearch.Location = new System.Drawing.Point(672, 4);
      this.btnSearch.Margin = new System.Windows.Forms.Padding(2);
      this.btnSearch.Name = "btnSearch";
      this.btnSearch.Size = new System.Drawing.Size(55, 23);
      this.btnSearch.TabIndex = 10;
      this.btnSearch.Text = "Find";
      this.btnSearch.UseVisualStyleBackColor = true;
      this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
      // 
      // txtSearch
      // 
      this.txtSearch.AcceptsReturn = true;
      this.txtSearch.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Irony.GrammarExplorer.Properties.Settings.Default, "SearchPattern", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.txtSearch.Location = new System.Drawing.Point(545, 4);
      this.txtSearch.Margin = new System.Windows.Forms.Padding(2);
      this.txtSearch.Name = "txtSearch";
      this.txtSearch.Size = new System.Drawing.Size(123, 20);
      this.txtSearch.TabIndex = 8;
      this.txtSearch.Text = global::Irony.GrammarExplorer.Properties.Settings.Default.SearchPattern;
      this.txtSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSearch_KeyPress);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(24, 6);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(52, 13);
      this.label2.TabIndex = 4;
      this.label2.Text = "Grammar:";
      // 
      // cboGrammars
      // 
      this.cboGrammars.ContextMenuStrip = this.menuGrammars;
      this.cboGrammars.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboGrammars.FormattingEnabled = true;
      this.cboGrammars.Items.AddRange(new object[] {
            "ExpressionGrammar",
            "Scheme",
            "Script.NET",
            "c# 3.0",
            "GW Basic",
            "Tutorial - CalcGrammar Part 1",
            "Tutorial - CalcGrammar Part 2"});
      this.cboGrammars.Location = new System.Drawing.Point(90, 3);
      this.cboGrammars.Name = "cboGrammars";
      this.cboGrammars.Size = new System.Drawing.Size(189, 21);
      this.cboGrammars.TabIndex = 3;
      this.cboGrammars.SelectedIndexChanged += new System.EventHandler(this.cboLanguage_SelectedIndexChanged);
      // 
      // menuGrammars
      // 
      this.menuGrammars.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miAdd,
            this.miRemove,
            this.miRemoveAll});
      this.menuGrammars.Name = "menuGrammars";
      this.menuGrammars.Size = new System.Drawing.Size(164, 70);
      this.menuGrammars.Opening += new System.ComponentModel.CancelEventHandler(this.menuGrammars_Opening);
      // 
      // miAdd
      // 
      this.miAdd.Name = "miAdd";
      this.miAdd.Size = new System.Drawing.Size(163, 22);
      this.miAdd.Text = "Add grammar...";
      this.miAdd.Click += new System.EventHandler(this.miAdd_Click);
      // 
      // miRemove
      // 
      this.miRemove.Name = "miRemove";
      this.miRemove.Size = new System.Drawing.Size(163, 22);
      this.miRemove.Text = "Remove selected";
      this.miRemove.Click += new System.EventHandler(this.miRemove_Click);
      // 
      // miRemoveAll
      // 
      this.miRemoveAll.Name = "miRemoveAll";
      this.miRemoveAll.Size = new System.Drawing.Size(163, 22);
      this.miRemoveAll.Text = "Remove all";
      this.miRemoveAll.Click += new System.EventHandler(this.miRemoveAll_Click);
      // 
      // dlgSelectAssembly
      // 
      this.dlgSelectAssembly.DefaultExt = "dll";
      this.dlgSelectAssembly.Filter = "DLL files|*.dll";
      this.dlgSelectAssembly.Title = "Select Grammar Assembly ";
      // 
      // splitBottom
      // 
      this.splitBottom.BackColor = System.Drawing.SystemColors.Control;
      this.splitBottom.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.splitBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.splitBottom.Location = new System.Drawing.Point(0, 493);
      this.splitBottom.Name = "splitBottom";
      this.splitBottom.Size = new System.Drawing.Size(1104, 6);
      this.splitBottom.TabIndex = 22;
      this.splitBottom.TabStop = false;
      // 
      // tabBottom
      // 
      this.tabBottom.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
      this.tabBottom.Controls.Add(this.pageStats);
      this.tabBottom.Controls.Add(this.pageOutput);
      this.tabBottom.Controls.Add(this.pageErrors);
      this.tabBottom.Controls.Add(this.pageParserTrace);
      this.tabBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.tabBottom.Location = new System.Drawing.Point(0, 499);
      this.tabBottom.Name = "tabBottom";
      this.tabBottom.SelectedIndex = 0;
      this.tabBottom.Size = new System.Drawing.Size(1104, 187);
      this.tabBottom.TabIndex = 0;
      // 
      // pageStats
      // 
      this.pageStats.Controls.Add(this.groupBox4);
      this.pageStats.Location = new System.Drawing.Point(4, 25);
      this.pageStats.Name = "pageStats";
      this.pageStats.Padding = new System.Windows.Forms.Padding(3);
      this.pageStats.Size = new System.Drawing.Size(1096, 158);
      this.pageStats.TabIndex = 1;
      this.pageStats.Text = "Statistics";
      this.pageStats.UseVisualStyleBackColor = true;
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.label5);
      this.groupBox4.Controls.Add(this.lblRunTime);
      this.groupBox4.Controls.Add(this.label6);
      this.groupBox4.Controls.Add(this.lblInitTime);
      this.groupBox4.Controls.Add(this.label4);
      this.groupBox4.Controls.Add(this.lblErrCount);
      this.groupBox4.Controls.Add(this.label7);
      this.groupBox4.Controls.Add(this.lblStatLines);
      this.groupBox4.Controls.Add(this.label3);
      this.groupBox4.Controls.Add(this.lblStatTokens);
      this.groupBox4.Controls.Add(this.label1);
      this.groupBox4.Controls.Add(this.lblStatTime);
      this.groupBox4.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox4.Location = new System.Drawing.Point(3, 3);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(1090, 156);
      this.groupBox4.TabIndex = 3;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Statistics";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(17, 62);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(75, 13);
      this.label5.TabIndex = 17;
      this.label5.Text = "Run Time, ms:";
      // 
      // lblRunTime
      // 
      this.lblRunTime.AutoSize = true;
      this.lblRunTime.Location = new System.Drawing.Point(135, 62);
      this.lblRunTime.Name = "lblRunTime";
      this.lblRunTime.Size = new System.Drawing.Size(13, 13);
      this.lblRunTime.TabIndex = 16;
      this.lblRunTime.Text = "0";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(17, 16);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(65, 13);
      this.label6.TabIndex = 15;
      this.label6.Text = "Init time, ms:";
      // 
      // lblInitTime
      // 
      this.lblInitTime.AutoSize = true;
      this.lblInitTime.Location = new System.Drawing.Point(135, 16);
      this.lblInitTime.Name = "lblInitTime";
      this.lblInitTime.Size = new System.Drawing.Size(13, 13);
      this.lblInitTime.TabIndex = 14;
      this.lblInitTime.Text = "0";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(203, 62);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(37, 13);
      this.label4.TabIndex = 13;
      this.label4.Text = "Errors:";
      // 
      // lblErrCount
      // 
      this.lblErrCount.AutoSize = true;
      this.lblErrCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblErrCount.ForeColor = System.Drawing.SystemColors.ControlText;
      this.lblErrCount.Location = new System.Drawing.Point(271, 62);
      this.lblErrCount.Name = "lblErrCount";
      this.lblErrCount.Size = new System.Drawing.Size(13, 13);
      this.lblErrCount.TabIndex = 12;
      this.lblErrCount.Text = "0";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(203, 16);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(35, 13);
      this.label7.TabIndex = 11;
      this.label7.Text = "Lines:";
      // 
      // lblStatLines
      // 
      this.lblStatLines.AutoSize = true;
      this.lblStatLines.Location = new System.Drawing.Point(271, 16);
      this.lblStatLines.Name = "lblStatLines";
      this.lblStatLines.Size = new System.Drawing.Size(13, 13);
      this.lblStatLines.TabIndex = 10;
      this.lblStatLines.Text = "0";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(203, 39);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(46, 13);
      this.label3.TabIndex = 9;
      this.label3.Text = "Tokens:";
      // 
      // lblStatTokens
      // 
      this.lblStatTokens.AutoSize = true;
      this.lblStatTokens.Location = new System.Drawing.Point(271, 39);
      this.lblStatTokens.Name = "lblStatTokens";
      this.lblStatTokens.Size = new System.Drawing.Size(13, 13);
      this.lblStatTokens.TabIndex = 8;
      this.lblStatTokens.Text = "0";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(17, 39);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(82, 13);
      this.label1.TabIndex = 7;
      this.label1.Text = "Parse Time, ms:";
      // 
      // lblStatTime
      // 
      this.lblStatTime.AutoSize = true;
      this.lblStatTime.Location = new System.Drawing.Point(135, 39);
      this.lblStatTime.Name = "lblStatTime";
      this.lblStatTime.Size = new System.Drawing.Size(13, 13);
      this.lblStatTime.TabIndex = 6;
      this.lblStatTime.Text = "0";
      // 
      // pageOutput
      // 
      this.pageOutput.Controls.Add(this.txtOutput);
      this.pageOutput.Location = new System.Drawing.Point(4, 25);
      this.pageOutput.Name = "pageOutput";
      this.pageOutput.Padding = new System.Windows.Forms.Padding(3);
      this.pageOutput.Size = new System.Drawing.Size(1096, 158);
      this.pageOutput.TabIndex = 0;
      this.pageOutput.Text = "Output";
      this.pageOutput.UseVisualStyleBackColor = true;
      // 
      // txtOutput
      // 
      this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtOutput.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtOutput.Location = new System.Drawing.Point(3, 3);
      this.txtOutput.Multiline = true;
      this.txtOutput.Name = "txtOutput";
      this.txtOutput.ReadOnly = true;
      this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtOutput.Size = new System.Drawing.Size(1090, 152);
      this.txtOutput.TabIndex = 1;
      // 
      // pageErrors
      // 
      this.pageErrors.Controls.Add(this.gridCompileErrors);
      this.pageErrors.Location = new System.Drawing.Point(4, 25);
      this.pageErrors.Name = "pageErrors";
      this.pageErrors.Padding = new System.Windows.Forms.Padding(3);
      this.pageErrors.Size = new System.Drawing.Size(1096, 158);
      this.pageErrors.TabIndex = 2;
      this.pageErrors.Text = "Compile Errors";
      this.pageErrors.UseVisualStyleBackColor = true;
      // 
      // gridCompileErrors
      // 
      this.gridCompileErrors.AllowUserToAddRows = false;
      this.gridCompileErrors.AllowUserToDeleteRows = false;
      this.gridCompileErrors.AllowUserToResizeRows = false;
      this.gridCompileErrors.ColumnHeadersHeight = 24;
      this.gridCompileErrors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.gridCompileErrors.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn1});
      this.gridCompileErrors.Dock = System.Windows.Forms.DockStyle.Fill;
      this.gridCompileErrors.Location = new System.Drawing.Point(3, 3);
      this.gridCompileErrors.MultiSelect = false;
      this.gridCompileErrors.Name = "gridCompileErrors";
      this.gridCompileErrors.ReadOnly = true;
      this.gridCompileErrors.RowHeadersVisible = false;
      this.gridCompileErrors.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
      this.gridCompileErrors.Size = new System.Drawing.Size(1090, 152);
      this.gridCompileErrors.TabIndex = 2;
      this.gridCompileErrors.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridCompileErrors_CellDoubleClick);
      // 
      // dataGridViewTextBoxColumn3
      // 
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      this.dataGridViewTextBoxColumn3.DefaultCellStyle = dataGridViewCellStyle1;
      this.dataGridViewTextBoxColumn3.HeaderText = "L, C";
      this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
      this.dataGridViewTextBoxColumn3.ReadOnly = true;
      this.dataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.dataGridViewTextBoxColumn3.ToolTipText = "Double-click grid cell to locate in source code";
      this.dataGridViewTextBoxColumn3.Width = 50;
      // 
      // dataGridViewTextBoxColumn4
      // 
      this.dataGridViewTextBoxColumn4.HeaderText = "Error Message";
      this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
      this.dataGridViewTextBoxColumn4.ReadOnly = true;
      this.dataGridViewTextBoxColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.dataGridViewTextBoxColumn4.Width = 600;
      // 
      // dataGridViewTextBoxColumn1
      // 
      this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.dataGridViewTextBoxColumn1.DataPropertyName = "State";
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      this.dataGridViewTextBoxColumn1.DefaultCellStyle = dataGridViewCellStyle2;
      this.dataGridViewTextBoxColumn1.HeaderText = "Parser State";
      this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
      this.dataGridViewTextBoxColumn1.ReadOnly = true;
      this.dataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.dataGridViewTextBoxColumn1.ToolTipText = "Double-click grid cell to navigate to state details";
      this.dataGridViewTextBoxColumn1.Width = 71;
      // 
      // pageParserTrace
      // 
      this.pageParserTrace.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pageParserTrace.Controls.Add(this.grpParserActions);
      this.pageParserTrace.Controls.Add(this.splitter1);
      this.pageParserTrace.Controls.Add(this.grpTokens);
      this.pageParserTrace.Controls.Add(this.pnlParserTraceTop);
      this.pageParserTrace.Location = new System.Drawing.Point(4, 25);
      this.pageParserTrace.Name = "pageParserTrace";
      this.pageParserTrace.Padding = new System.Windows.Forms.Padding(3);
      this.pageParserTrace.Size = new System.Drawing.Size(1096, 158);
      this.pageParserTrace.TabIndex = 3;
      this.pageParserTrace.Text = "Parser Trace";
      this.pageParserTrace.UseVisualStyleBackColor = true;
      // 
      // grpParserActions
      // 
      this.grpParserActions.Controls.Add(this.gridParserTrace);
      this.grpParserActions.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpParserActions.Location = new System.Drawing.Point(3, 28);
      this.grpParserActions.Name = "grpParserActions";
      this.grpParserActions.Size = new System.Drawing.Size(804, 125);
      this.grpParserActions.TabIndex = 4;
      this.grpParserActions.TabStop = false;
      // 
      // gridParserTrace
      // 
      this.gridParserTrace.AllowUserToAddRows = false;
      this.gridParserTrace.AllowUserToDeleteRows = false;
      this.gridParserTrace.AllowUserToResizeRows = false;
      this.gridParserTrace.ColumnHeadersHeight = 24;
      this.gridParserTrace.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.gridParserTrace.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.State,
            this.Stack,
            this.Input,
            this.Action,
            this.NewState});
      this.gridParserTrace.Dock = System.Windows.Forms.DockStyle.Fill;
      this.gridParserTrace.Location = new System.Drawing.Point(3, 16);
      this.gridParserTrace.MultiSelect = false;
      this.gridParserTrace.Name = "gridParserTrace";
      this.gridParserTrace.ReadOnly = true;
      this.gridParserTrace.RowHeadersVisible = false;
      this.gridParserTrace.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
      this.gridParserTrace.Size = new System.Drawing.Size(798, 106);
      this.gridParserTrace.TabIndex = 0;
      this.gridParserTrace.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridParserTrace_CellDoubleClick);
      // 
      // State
      // 
      this.State.DataPropertyName = "State";
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      this.State.DefaultCellStyle = dataGridViewCellStyle3;
      this.State.HeaderText = "State";
      this.State.Name = "State";
      this.State.ReadOnly = true;
      this.State.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.State.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.State.ToolTipText = "Double-click grid cell to navigate to state details";
      this.State.Width = 60;
      // 
      // Stack
      // 
      this.Stack.DataPropertyName = "StackTop";
      dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      this.Stack.DefaultCellStyle = dataGridViewCellStyle4;
      this.Stack.HeaderText = "Stack Top";
      this.Stack.Name = "Stack";
      this.Stack.ReadOnly = true;
      this.Stack.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.Stack.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.Stack.ToolTipText = "Double-click grid cell to locate node in source code";
      this.Stack.Width = 220;
      // 
      // Input
      // 
      this.Input.DataPropertyName = "Input";
      this.Input.HeaderText = "Input";
      this.Input.Name = "Input";
      this.Input.ReadOnly = true;
      this.Input.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.Input.ToolTipText = "Double-click grid cell to locate in source code";
      this.Input.Width = 150;
      // 
      // Action
      // 
      this.Action.DataPropertyName = "Action";
      this.Action.HeaderText = "Action";
      this.Action.Name = "Action";
      this.Action.ReadOnly = true;
      this.Action.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.Action.Width = 220;
      // 
      // NewState
      // 
      this.NewState.HeaderText = "New State";
      this.NewState.Name = "NewState";
      this.NewState.ReadOnly = true;
      this.NewState.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.NewState.ToolTipText = "Double-click grid cell to navigate to state details";
      this.NewState.Width = 90;
      // 
      // splitter1
      // 
      this.splitter1.BackColor = System.Drawing.SystemColors.Control;
      this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
      this.splitter1.Location = new System.Drawing.Point(807, 28);
      this.splitter1.Name = "splitter1";
      this.splitter1.Size = new System.Drawing.Size(6, 125);
      this.splitter1.TabIndex = 15;
      this.splitter1.TabStop = false;
      // 
      // grpTokens
      // 
      this.grpTokens.Controls.Add(this.lstTokens);
      this.grpTokens.Dock = System.Windows.Forms.DockStyle.Right;
      this.grpTokens.Location = new System.Drawing.Point(813, 28);
      this.grpTokens.Name = "grpTokens";
      this.grpTokens.Size = new System.Drawing.Size(278, 125);
      this.grpTokens.TabIndex = 3;
      this.grpTokens.TabStop = false;
      this.grpTokens.Text = "Tokens";
      // 
      // lstTokens
      // 
      this.lstTokens.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lstTokens.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lstTokens.FormattingEnabled = true;
      this.lstTokens.ItemHeight = 14;
      this.lstTokens.Location = new System.Drawing.Point(3, 16);
      this.lstTokens.Name = "lstTokens";
      this.lstTokens.Size = new System.Drawing.Size(272, 102);
      this.lstTokens.TabIndex = 2;
      this.lstTokens.DoubleClick += new System.EventHandler(this.lstTokens_DoubleClick);
      // 
      // pnlParserTraceTop
      // 
      this.pnlParserTraceTop.BackColor = System.Drawing.SystemColors.Control;
      this.pnlParserTraceTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pnlParserTraceTop.Controls.Add(this.chkExcludeComments);
      this.pnlParserTraceTop.Controls.Add(this.lblTraceComment);
      this.pnlParserTraceTop.Controls.Add(this.chkParserTrace);
      this.pnlParserTraceTop.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlParserTraceTop.Location = new System.Drawing.Point(3, 3);
      this.pnlParserTraceTop.Name = "pnlParserTraceTop";
      this.pnlParserTraceTop.Size = new System.Drawing.Size(1088, 25);
      this.pnlParserTraceTop.TabIndex = 1;
      // 
      // chkExcludeComments
      // 
      this.chkExcludeComments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chkExcludeComments.AutoSize = true;
      this.chkExcludeComments.Checked = true;
      this.chkExcludeComments.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkExcludeComments.Location = new System.Drawing.Point(929, 3);
      this.chkExcludeComments.Name = "chkExcludeComments";
      this.chkExcludeComments.Size = new System.Drawing.Size(145, 17);
      this.chkExcludeComments.TabIndex = 2;
      this.chkExcludeComments.Text = "Exclude comment tokens";
      this.chkExcludeComments.UseVisualStyleBackColor = true;
      // 
      // lblTraceComment
      // 
      this.lblTraceComment.AutoSize = true;
      this.lblTraceComment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblTraceComment.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      this.lblTraceComment.Location = new System.Drawing.Point(128, 3);
      this.lblTraceComment.Name = "lblTraceComment";
      this.lblTraceComment.Size = new System.Drawing.Size(350, 13);
      this.lblTraceComment.TabIndex = 1;
      this.lblTraceComment.Text = "(Double-click grid cell to navigate to parser state or source code position)";
      // 
      // bindParserTrace
      // 
      this.bindParserTrace.DataSource = typeof(Irony.Diagnostics.ParserTrace);
      // 
      // fmGrammarExplorer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1104, 686);
      this.Controls.Add(this.tabGrammar);
      this.Controls.Add(this.splitBottom);
      this.Controls.Add(this.pnlLang);
      this.Controls.Add(this.tabBottom);
      this.Name = "fmGrammarExplorer";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Irony Grammar Explorer";
      this.Load += new System.EventHandler(this.fmExploreGrammar_Load);
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.fmExploreGrammar_FormClosing);
      this.tabGrammar.ResumeLayout(false);
      this.pageTerminals.ResumeLayout(false);
      this.pageTerminals.PerformLayout();
      this.pageNonTerms.ResumeLayout(false);
      this.pageNonTerms.PerformLayout();
      this.pageParserStates.ResumeLayout(false);
      this.pageParserStates.PerformLayout();
      this.pageGrErrors.ResumeLayout(false);
      this.pageGrErrors.PerformLayout();
      this.pageTest.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.tabOutput.ResumeLayout(false);
      this.pageSyntaxTree.ResumeLayout(false);
      this.pageAst.ResumeLayout(false);
      this.pnlLang.ResumeLayout(false);
      this.pnlLang.PerformLayout();
      this.menuGrammars.ResumeLayout(false);
      this.tabBottom.ResumeLayout(false);
      this.pageStats.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.pageOutput.ResumeLayout(false);
      this.pageOutput.PerformLayout();
      this.pageErrors.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.gridCompileErrors)).EndInit();
      this.pageParserTrace.ResumeLayout(false);
      this.grpParserActions.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.gridParserTrace)).EndInit();
      this.grpTokens.ResumeLayout(false);
      this.pnlParserTraceTop.ResumeLayout(false);
      this.pnlParserTraceTop.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.bindParserTrace)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabGrammar;
    private System.Windows.Forms.TabPage pageNonTerms;
    private System.Windows.Forms.TabPage pageParserStates;
    private System.Windows.Forms.TabPage pageGrErrors;
    private System.Windows.Forms.TextBox txtNonTerms;
    private System.Windows.Forms.TextBox txtParserStates;
    private System.Windows.Forms.TextBox txtGrammarErrors;
    private System.Windows.Forms.Panel pnlLang;
    private System.Windows.Forms.ComboBox cboGrammars;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TabPage pageTest;
    private System.Windows.Forms.Splitter splitter3;
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
    private System.Windows.Forms.RichTextBox txtSource;
    private System.Windows.Forms.Button btnManageGrammars;
    private System.Windows.Forms.ContextMenuStrip menuGrammars;
    private System.Windows.Forms.ToolStripMenuItem miAdd;
    private System.Windows.Forms.ToolStripMenuItem miRemove;
    private System.Windows.Forms.OpenFileDialog dlgSelectAssembly;
    private System.Windows.Forms.ToolStripMenuItem miRemoveAll;
    private System.Windows.Forms.Button btnToXml;
    private System.Windows.Forms.ComboBox cboParseMethod;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.TabControl tabBottom;
    private System.Windows.Forms.TabPage pageOutput;
    private System.Windows.Forms.TextBox txtOutput;
    private System.Windows.Forms.TabPage pageStats;
    private System.Windows.Forms.Splitter splitBottom;
    private System.Windows.Forms.GroupBox groupBox4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label lblRunTime;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label lblInitTime;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label lblErrCount;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label lblStatLines;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label lblStatTokens;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label lblStatTime;
    private System.Windows.Forms.TabPage pageErrors;
    private System.Windows.Forms.TabPage pageParserTrace;
    private System.Windows.Forms.TreeView tvAst;
    private System.Windows.Forms.DataGridView gridParserTrace;
    private System.Windows.Forms.BindingSource bindParserTrace;
    private System.Windows.Forms.DataGridViewTextBoxColumn State;
    private System.Windows.Forms.DataGridViewTextBoxColumn Stack;
    private System.Windows.Forms.DataGridViewTextBoxColumn Input;
    private System.Windows.Forms.DataGridViewTextBoxColumn Action;
    private System.Windows.Forms.DataGridViewTextBoxColumn NewState;
    private System.Windows.Forms.GroupBox grpTokens;
    private System.Windows.Forms.Panel pnlParserTraceTop;
    private System.Windows.Forms.GroupBox grpParserActions;
    private System.Windows.Forms.Splitter splitter1;
    private System.Windows.Forms.ListBox lstTokens;
    private System.Windows.Forms.Label lblTraceComment;
    private System.Windows.Forms.DataGridView gridCompileErrors;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private System.Windows.Forms.CheckBox chkExcludeComments;

  }
}


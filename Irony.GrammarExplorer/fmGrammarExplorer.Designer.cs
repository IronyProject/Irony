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
      this.lblStatTime = new System.Windows.Forms.Label();
      this.lstParseTrace = new System.Windows.Forms.ListBox();
      this.lstErrors = new System.Windows.Forms.ListBox();
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
      this.splitter4 = new System.Windows.Forms.Splitter();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnRun = new System.Windows.Forms.Button();
      this.chkShowTrace = new System.Windows.Forms.CheckBox();
      this.btnFileOpen = new System.Windows.Forms.Button();
      this.btnParse = new System.Windows.Forms.Button();
      this.grpOutput = new System.Windows.Forms.GroupBox();
      this.txtOutput = new System.Windows.Forms.TextBox();
      this.splitter3 = new System.Windows.Forms.Splitter();
      this.tabResults = new System.Windows.Forms.TabControl();
      this.pageResult = new System.Windows.Forms.TabPage();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.tvAstNodes = new System.Windows.Forms.TreeView();
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
      this.pageParseErrors = new System.Windows.Forms.TabPage();
      this.txtErrDetails = new System.Windows.Forms.TextBox();
      this.pageParseTrace = new System.Windows.Forms.TabPage();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.splitter2 = new System.Windows.Forms.Splitter();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.lstParseStack = new System.Windows.Forms.ListBox();
      this.pageTokens = new System.Windows.Forms.TabPage();
      this.lstTokens = new System.Windows.Forms.ListBox();
      this.pnlLang = new System.Windows.Forms.Panel();
      this.btnManageGrammars = new System.Windows.Forms.Button();
      this.lblSearchError = new System.Windows.Forms.Label();
      this.SearchLabel = new System.Windows.Forms.Label();
      this.btnSearch = new System.Windows.Forms.Button();
      this.txtSearch = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cboGrammars = new System.Windows.Forms.ComboBox();
      this.menuGrammars = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.miAdd = new System.Windows.Forms.ToolStripMenuItem();
      this.miRemove = new System.Windows.Forms.ToolStripMenuItem();
      this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
      this.dlgSelectAssembly = new System.Windows.Forms.OpenFileDialog();
      this.tabGrammar.SuspendLayout();
      this.pageTerminals.SuspendLayout();
      this.pageNonTerms.SuspendLayout();
      this.pageParserStates.SuspendLayout();
      this.pageGrErrors.SuspendLayout();
      this.pageTest.SuspendLayout();
      this.panel1.SuspendLayout();
      this.grpOutput.SuspendLayout();
      this.tabResults.SuspendLayout();
      this.pageResult.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.pageParseErrors.SuspendLayout();
      this.pageParseTrace.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.pageTokens.SuspendLayout();
      this.pnlLang.SuspendLayout();
      this.menuGrammars.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblStatTime
      // 
      this.lblStatTime.AutoSize = true;
      this.lblStatTime.Location = new System.Drawing.Point(129, 89);
      this.lblStatTime.Name = "lblStatTime";
      this.lblStatTime.Size = new System.Drawing.Size(13, 13);
      this.lblStatTime.TabIndex = 6;
      this.lblStatTime.Text = "0";
      // 
      // lstParseTrace
      // 
      this.lstParseTrace.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lstParseTrace.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lstParseTrace.FormattingEnabled = true;
      this.lstParseTrace.ItemHeight = 14;
      this.lstParseTrace.Location = new System.Drawing.Point(3, 16);
      this.lstParseTrace.Name = "lstParseTrace";
      this.lstParseTrace.Size = new System.Drawing.Size(330, 312);
      this.lstParseTrace.TabIndex = 2;
      // 
      // lstErrors
      // 
      this.lstErrors.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lstErrors.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lstErrors.FormattingEnabled = true;
      this.lstErrors.ItemHeight = 14;
      this.lstErrors.Location = new System.Drawing.Point(3, 3);
      this.lstErrors.Name = "lstErrors";
      this.lstErrors.Size = new System.Drawing.Size(336, 480);
      this.lstErrors.TabIndex = 0;
      this.lstErrors.SelectedIndexChanged += new System.EventHandler(this.lstErrors_SelectedIndexChanged);
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
      this.tabGrammar.Size = new System.Drawing.Size(1103, 647);
      this.tabGrammar.TabIndex = 0;
      // 
      // pageTerminals
      // 
      this.pageTerminals.Controls.Add(this.txtTerms);
      this.pageTerminals.Location = new System.Drawing.Point(4, 22);
      this.pageTerminals.Name = "pageTerminals";
      this.pageTerminals.Padding = new System.Windows.Forms.Padding(3);
      this.pageTerminals.Size = new System.Drawing.Size(1095, 621);
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
      this.txtTerms.Size = new System.Drawing.Size(1089, 615);
      this.txtTerms.TabIndex = 2;
      // 
      // pageNonTerms
      // 
      this.pageNonTerms.Controls.Add(this.txtNonTerms);
      this.pageNonTerms.Location = new System.Drawing.Point(4, 22);
      this.pageNonTerms.Name = "pageNonTerms";
      this.pageNonTerms.Padding = new System.Windows.Forms.Padding(3);
      this.pageNonTerms.Size = new System.Drawing.Size(1095, 621);
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
      this.txtNonTerms.Size = new System.Drawing.Size(1089, 615);
      this.txtNonTerms.TabIndex = 1;
      this.txtNonTerms.WordWrap = false;
      // 
      // pageParserStates
      // 
      this.pageParserStates.Controls.Add(this.txtParserStates);
      this.pageParserStates.Location = new System.Drawing.Point(4, 22);
      this.pageParserStates.Name = "pageParserStates";
      this.pageParserStates.Padding = new System.Windows.Forms.Padding(3);
      this.pageParserStates.Size = new System.Drawing.Size(1095, 621);
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
      this.txtParserStates.Size = new System.Drawing.Size(1089, 615);
      this.txtParserStates.TabIndex = 2;
      this.txtParserStates.WordWrap = false;
      // 
      // pageGrErrors
      // 
      this.pageGrErrors.Controls.Add(this.txtGrammarErrors);
      this.pageGrErrors.Location = new System.Drawing.Point(4, 22);
      this.pageGrErrors.Name = "pageGrErrors";
      this.pageGrErrors.Padding = new System.Windows.Forms.Padding(3);
      this.pageGrErrors.Size = new System.Drawing.Size(1095, 621);
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
      this.txtGrammarErrors.Size = new System.Drawing.Size(1089, 615);
      this.txtGrammarErrors.TabIndex = 2;
      // 
      // pageTest
      // 
      this.pageTest.Controls.Add(this.txtSource);
      this.pageTest.Controls.Add(this.splitter4);
      this.pageTest.Controls.Add(this.panel1);
      this.pageTest.Controls.Add(this.grpOutput);
      this.pageTest.Controls.Add(this.splitter3);
      this.pageTest.Controls.Add(this.tabResults);
      this.pageTest.Location = new System.Drawing.Point(4, 22);
      this.pageTest.Name = "pageTest";
      this.pageTest.Padding = new System.Windows.Forms.Padding(3);
      this.pageTest.Size = new System.Drawing.Size(1095, 621);
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
      this.txtSource.Size = new System.Drawing.Size(729, 466);
      this.txtSource.TabIndex = 22;
      this.txtSource.Text = "";
      this.txtSource.TextChanged += new System.EventHandler(this.txtSource_TextChanged);
      // 
      // splitter4
      // 
      this.splitter4.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.splitter4.Location = new System.Drawing.Point(3, 499);
      this.splitter4.Name = "splitter4";
      this.splitter4.Size = new System.Drawing.Size(729, 10);
      this.splitter4.TabIndex = 21;
      this.splitter4.TabStop = false;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnRun);
      this.panel1.Controls.Add(this.chkShowTrace);
      this.panel1.Controls.Add(this.btnFileOpen);
      this.panel1.Controls.Add(this.btnParse);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(3, 3);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(729, 30);
      this.panel1.TabIndex = 2;
      // 
      // btnRun
      // 
      this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRun.Location = new System.Drawing.Point(595, 3);
      this.btnRun.Name = "btnRun";
      this.btnRun.Size = new System.Drawing.Size(65, 23);
      this.btnRun.TabIndex = 7;
      this.btnRun.Text = "Run";
      this.btnRun.UseVisualStyleBackColor = true;
      this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
      // 
      // chkShowTrace
      // 
      this.chkShowTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chkShowTrace.AutoSize = true;
      this.chkShowTrace.Location = new System.Drawing.Point(669, 7);
      this.chkShowTrace.Name = "chkShowTrace";
      this.chkShowTrace.Size = new System.Drawing.Size(54, 17);
      this.chkShowTrace.TabIndex = 0;
      this.chkShowTrace.Text = "Trace";
      this.chkShowTrace.UseVisualStyleBackColor = true;
      // 
      // btnFileOpen
      // 
      this.btnFileOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnFileOpen.Location = new System.Drawing.Point(451, 3);
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
      this.btnParse.Location = new System.Drawing.Point(522, 3);
      this.btnParse.Name = "btnParse";
      this.btnParse.Size = new System.Drawing.Size(67, 23);
      this.btnParse.TabIndex = 1;
      this.btnParse.Text = "Parse";
      this.btnParse.UseVisualStyleBackColor = true;
      this.btnParse.Click += new System.EventHandler(this.btnParse_Click);
      // 
      // grpOutput
      // 
      this.grpOutput.Controls.Add(this.txtOutput);
      this.grpOutput.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.grpOutput.Location = new System.Drawing.Point(3, 509);
      this.grpOutput.Name = "grpOutput";
      this.grpOutput.Size = new System.Drawing.Size(729, 109);
      this.grpOutput.TabIndex = 16;
      this.grpOutput.TabStop = false;
      this.grpOutput.Text = "Output";
      // 
      // txtOutput
      // 
      this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtOutput.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtOutput.Location = new System.Drawing.Point(3, 16);
      this.txtOutput.Multiline = true;
      this.txtOutput.Name = "txtOutput";
      this.txtOutput.ReadOnly = true;
      this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtOutput.Size = new System.Drawing.Size(723, 90);
      this.txtOutput.TabIndex = 0;
      // 
      // splitter3
      // 
      this.splitter3.Dock = System.Windows.Forms.DockStyle.Right;
      this.splitter3.Location = new System.Drawing.Point(732, 3);
      this.splitter3.Name = "splitter3";
      this.splitter3.Size = new System.Drawing.Size(10, 615);
      this.splitter3.TabIndex = 14;
      this.splitter3.TabStop = false;
      // 
      // tabResults
      // 
      this.tabResults.Controls.Add(this.pageResult);
      this.tabResults.Controls.Add(this.pageParseErrors);
      this.tabResults.Controls.Add(this.pageParseTrace);
      this.tabResults.Controls.Add(this.pageTokens);
      this.tabResults.Dock = System.Windows.Forms.DockStyle.Right;
      this.tabResults.Location = new System.Drawing.Point(742, 3);
      this.tabResults.Name = "tabResults";
      this.tabResults.SelectedIndex = 0;
      this.tabResults.Size = new System.Drawing.Size(350, 615);
      this.tabResults.TabIndex = 13;
      // 
      // pageResult
      // 
      this.pageResult.Controls.Add(this.groupBox3);
      this.pageResult.Controls.Add(this.groupBox4);
      this.pageResult.ForeColor = System.Drawing.SystemColors.ControlText;
      this.pageResult.Location = new System.Drawing.Point(4, 22);
      this.pageResult.Name = "pageResult";
      this.pageResult.Padding = new System.Windows.Forms.Padding(3);
      this.pageResult.Size = new System.Drawing.Size(342, 589);
      this.pageResult.TabIndex = 1;
      this.pageResult.Text = "Results";
      this.pageResult.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.tvAstNodes);
      this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox3.Location = new System.Drawing.Point(3, 159);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(336, 427);
      this.groupBox3.TabIndex = 1;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Output Syntax Tree";
      // 
      // tvAstNodes
      // 
      this.tvAstNodes.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tvAstNodes.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tvAstNodes.Indent = 16;
      this.tvAstNodes.Location = new System.Drawing.Point(3, 16);
      this.tvAstNodes.Name = "tvAstNodes";
      this.tvAstNodes.Size = new System.Drawing.Size(330, 408);
      this.tvAstNodes.TabIndex = 0;
      this.tvAstNodes.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvAstNodes_AfterSelect);
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
      this.groupBox4.Size = new System.Drawing.Size(336, 156);
      this.groupBox4.TabIndex = 2;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Statistics";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(17, 132);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(75, 13);
      this.label5.TabIndex = 17;
      this.label5.Text = "Run Time, ms:";
      // 
      // lblRunTime
      // 
      this.lblRunTime.AutoSize = true;
      this.lblRunTime.Location = new System.Drawing.Point(130, 132);
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
      this.lblInitTime.Location = new System.Drawing.Point(129, 16);
      this.lblInitTime.Name = "lblInitTime";
      this.lblInitTime.Size = new System.Drawing.Size(13, 13);
      this.lblInitTime.TabIndex = 14;
      this.lblInitTime.Text = "0";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(17, 112);
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
      this.lblErrCount.Location = new System.Drawing.Point(129, 112);
      this.lblErrCount.Name = "lblErrCount";
      this.lblErrCount.Size = new System.Drawing.Size(13, 13);
      this.lblErrCount.TabIndex = 12;
      this.lblErrCount.Text = "0";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(17, 42);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(35, 13);
      this.label7.TabIndex = 11;
      this.label7.Text = "Lines:";
      // 
      // lblStatLines
      // 
      this.lblStatLines.AutoSize = true;
      this.lblStatLines.Location = new System.Drawing.Point(129, 44);
      this.lblStatLines.Name = "lblStatLines";
      this.lblStatLines.Size = new System.Drawing.Size(13, 13);
      this.lblStatLines.TabIndex = 10;
      this.lblStatLines.Text = "0";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(15, 66);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(46, 13);
      this.label3.TabIndex = 9;
      this.label3.Text = "Tokens:";
      // 
      // lblStatTokens
      // 
      this.lblStatTokens.AutoSize = true;
      this.lblStatTokens.Location = new System.Drawing.Point(129, 66);
      this.lblStatTokens.Name = "lblStatTokens";
      this.lblStatTokens.Size = new System.Drawing.Size(13, 13);
      this.lblStatTokens.TabIndex = 8;
      this.lblStatTokens.Text = "0";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(15, 89);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(82, 13);
      this.label1.TabIndex = 7;
      this.label1.Text = "Parse Time, ms:";
      // 
      // pageParseErrors
      // 
      this.pageParseErrors.Controls.Add(this.lstErrors);
      this.pageParseErrors.Controls.Add(this.txtErrDetails);
      this.pageParseErrors.Location = new System.Drawing.Point(4, 22);
      this.pageParseErrors.Name = "pageParseErrors";
      this.pageParseErrors.Padding = new System.Windows.Forms.Padding(3);
      this.pageParseErrors.Size = new System.Drawing.Size(342, 589);
      this.pageParseErrors.TabIndex = 3;
      this.pageParseErrors.Text = "Errors";
      this.pageParseErrors.UseVisualStyleBackColor = true;
      // 
      // txtErrDetails
      // 
      this.txtErrDetails.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.txtErrDetails.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtErrDetails.HideSelection = false;
      this.txtErrDetails.Location = new System.Drawing.Point(3, 483);
      this.txtErrDetails.Multiline = true;
      this.txtErrDetails.Name = "txtErrDetails";
      this.txtErrDetails.ReadOnly = true;
      this.txtErrDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtErrDetails.Size = new System.Drawing.Size(336, 103);
      this.txtErrDetails.TabIndex = 3;
      // 
      // pageParseTrace
      // 
      this.pageParseTrace.Controls.Add(this.groupBox1);
      this.pageParseTrace.Controls.Add(this.splitter2);
      this.pageParseTrace.Controls.Add(this.groupBox2);
      this.pageParseTrace.Location = new System.Drawing.Point(4, 22);
      this.pageParseTrace.Name = "pageParseTrace";
      this.pageParseTrace.Padding = new System.Windows.Forms.Padding(3);
      this.pageParseTrace.Size = new System.Drawing.Size(342, 589);
      this.pageParseTrace.TabIndex = 2;
      this.pageParseTrace.Text = "Parser Trace";
      this.pageParseTrace.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.lstParseTrace);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(3, 3);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(336, 331);
      this.groupBox1.TabIndex = 5;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Parser Trace";
      // 
      // splitter2
      // 
      this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.splitter2.Location = new System.Drawing.Point(3, 334);
      this.splitter2.Name = "splitter2";
      this.splitter2.Size = new System.Drawing.Size(336, 10);
      this.splitter2.TabIndex = 3;
      this.splitter2.TabStop = false;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.lstParseStack);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.groupBox2.Location = new System.Drawing.Point(3, 344);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(336, 242);
      this.groupBox2.TabIndex = 6;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Remaining Parser Stack (for error only)";
      // 
      // lstParseStack
      // 
      this.lstParseStack.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lstParseStack.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lstParseStack.FormattingEnabled = true;
      this.lstParseStack.ItemHeight = 14;
      this.lstParseStack.Location = new System.Drawing.Point(3, 16);
      this.lstParseStack.Name = "lstParseStack";
      this.lstParseStack.Size = new System.Drawing.Size(330, 214);
      this.lstParseStack.TabIndex = 4;
      // 
      // pageTokens
      // 
      this.pageTokens.Controls.Add(this.lstTokens);
      this.pageTokens.Location = new System.Drawing.Point(4, 22);
      this.pageTokens.Name = "pageTokens";
      this.pageTokens.Padding = new System.Windows.Forms.Padding(3);
      this.pageTokens.Size = new System.Drawing.Size(342, 589);
      this.pageTokens.TabIndex = 0;
      this.pageTokens.Text = "Tokens";
      this.pageTokens.UseVisualStyleBackColor = true;
      // 
      // lstTokens
      // 
      this.lstTokens.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lstTokens.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lstTokens.FormattingEnabled = true;
      this.lstTokens.ItemHeight = 14;
      this.lstTokens.Location = new System.Drawing.Point(3, 3);
      this.lstTokens.Name = "lstTokens";
      this.lstTokens.Size = new System.Drawing.Size(336, 578);
      this.lstTokens.TabIndex = 1;
      this.lstTokens.SelectedIndexChanged += new System.EventHandler(this.lstTokens_SelectedIndexChanged);
      // 
      // pnlLang
      // 
      this.pnlLang.Controls.Add(this.btnManageGrammars);
      this.pnlLang.Controls.Add(this.lblSearchError);
      this.pnlLang.Controls.Add(this.SearchLabel);
      this.pnlLang.Controls.Add(this.btnSearch);
      this.pnlLang.Controls.Add(this.txtSearch);
      this.pnlLang.Controls.Add(this.label2);
      this.pnlLang.Controls.Add(this.cboGrammars);
      this.pnlLang.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlLang.Location = new System.Drawing.Point(0, 0);
      this.pnlLang.Name = "pnlLang";
      this.pnlLang.Size = new System.Drawing.Size(1103, 29);
      this.pnlLang.TabIndex = 13;
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
      this.lblSearchError.Location = new System.Drawing.Point(809, 6);
      this.lblSearchError.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lblSearchError.Name = "lblSearchError";
      this.lblSearchError.Size = new System.Drawing.Size(54, 13);
      this.lblSearchError.TabIndex = 11;
      this.lblSearchError.Text = "Not found";
      this.lblSearchError.Visible = false;
      // 
      // SearchLabel
      // 
      this.SearchLabel.AutoSize = true;
      this.SearchLabel.Location = new System.Drawing.Point(429, 6);
      this.SearchLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.SearchLabel.Name = "SearchLabel";
      this.SearchLabel.Size = new System.Drawing.Size(79, 13);
      this.SearchLabel.TabIndex = 9;
      this.SearchLabel.Text = "Search (regex):";
      // 
      // btnSearch
      // 
      this.btnSearch.Location = new System.Drawing.Point(734, 2);
      this.btnSearch.Margin = new System.Windows.Forms.Padding(2);
      this.btnSearch.Name = "btnSearch";
      this.btnSearch.Size = new System.Drawing.Size(71, 24);
      this.btnSearch.TabIndex = 10;
      this.btnSearch.Text = "&Search";
      this.btnSearch.UseVisualStyleBackColor = true;
      this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
      // 
      // txtSearch
      // 
      this.txtSearch.AcceptsReturn = true;
      this.txtSearch.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Irony.GrammarExplorer.Properties.Settings.Default, "SearchPattern", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.txtSearch.Location = new System.Drawing.Point(512, 3);
      this.txtSearch.Margin = new System.Windows.Forms.Padding(2);
      this.txtSearch.Name = "txtSearch";
      this.txtSearch.Size = new System.Drawing.Size(218, 20);
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
            this.miRemove});
      this.menuGrammars.Name = "menuGrammars";
      this.menuGrammars.Size = new System.Drawing.Size(159, 48);
      this.menuGrammars.Opening += new System.ComponentModel.CancelEventHandler(this.menuGrammars_Opening);
      // 
      // miAdd
      // 
      this.miAdd.Name = "miAdd";
      this.miAdd.Size = new System.Drawing.Size(158, 22);
      this.miAdd.Text = "Add Grammar...";
      this.miAdd.Click += new System.EventHandler(this.miAdd_Click);
      // 
      // miRemove
      // 
      this.miRemove.Name = "miRemove";
      this.miRemove.Size = new System.Drawing.Size(158, 22);
      this.miRemove.Text = "Remove";
      this.miRemove.Click += new System.EventHandler(this.miRemove_Click);
      // 
      // dlgSelectAssembly
      // 
      this.dlgSelectAssembly.DefaultExt = "dll";
      this.dlgSelectAssembly.Filter = "DLL files|*.dll";
      this.dlgSelectAssembly.Title = "Select Grammar Assembly ";
      // 
      // fmGrammarExplorer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1103, 676);
      this.Controls.Add(this.tabGrammar);
      this.Controls.Add(this.pnlLang);
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
      this.panel1.PerformLayout();
      this.grpOutput.ResumeLayout(false);
      this.grpOutput.PerformLayout();
      this.tabResults.ResumeLayout(false);
      this.pageResult.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.pageParseErrors.ResumeLayout(false);
      this.pageParseErrors.PerformLayout();
      this.pageParseTrace.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.pageTokens.ResumeLayout(false);
      this.pnlLang.ResumeLayout(false);
      this.pnlLang.PerformLayout();
      this.menuGrammars.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label lblStatTime;
    private System.Windows.Forms.ListBox lstErrors;
    private System.Windows.Forms.TabControl tabGrammar;
    private System.Windows.Forms.TabPage pageNonTerms;
    private System.Windows.Forms.TabPage pageParserStates;
    private System.Windows.Forms.TabPage pageGrErrors;
    private System.Windows.Forms.TextBox txtNonTerms;
    private System.Windows.Forms.TextBox txtParserStates;
    private System.Windows.Forms.TextBox txtGrammarErrors;
    private System.Windows.Forms.Panel pnlLang;
    private System.Windows.Forms.ListBox lstParseTrace;
    private System.Windows.Forms.ComboBox cboGrammars;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TabPage pageTest;
    private System.Windows.Forms.Splitter splitter3;
    private System.Windows.Forms.TabControl tabResults;
    private System.Windows.Forms.TabPage pageTokens;
    private System.Windows.Forms.ListBox lstTokens;
    private System.Windows.Forms.TabPage pageResult;
    private System.Windows.Forms.TreeView tvAstNodes;
    private System.Windows.Forms.TabPage pageParseTrace;
    private System.Windows.Forms.TabPage pageParseErrors;
    private System.Windows.Forms.Splitter splitter2;
    private System.Windows.Forms.ListBox lstParseStack;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.OpenFileDialog dlgOpenFile;
    private System.Windows.Forms.TabPage pageTerminals;
    private System.Windows.Forms.TextBox txtTerms;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.GroupBox groupBox4;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label lblStatLines;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label lblStatTokens;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label lblErrCount;
    private System.Windows.Forms.TextBox txtErrDetails;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label lblInitTime;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label SearchLabel;
    private System.Windows.Forms.Button btnSearch;
    private System.Windows.Forms.TextBox txtSearch;
    private System.Windows.Forms.Label lblSearchError;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label lblRunTime;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnRun;
    private System.Windows.Forms.CheckBox chkShowTrace;
    private System.Windows.Forms.Button btnFileOpen;
    private System.Windows.Forms.Button btnParse;
    private System.Windows.Forms.GroupBox grpOutput;
    private System.Windows.Forms.TextBox txtOutput;
    private System.Windows.Forms.Splitter splitter4;
    private System.Windows.Forms.RichTextBox txtSource;
    private System.Windows.Forms.Button btnManageGrammars;
    private System.Windows.Forms.ContextMenuStrip menuGrammars;
    private System.Windows.Forms.ToolStripMenuItem miAdd;
    private System.Windows.Forms.ToolStripMenuItem miRemove;
    private System.Windows.Forms.OpenFileDialog dlgSelectAssembly;

  }
}


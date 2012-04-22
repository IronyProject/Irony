//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  License: GNU Lesser General Public License (LGPLv3)
//
//  Email: pavel_torgashov@mail.ru.
//
//  Copyright (C) Pavel Torgashov, 2011-2012.

//#define debug

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Timer = System.Windows.Forms.Timer;

namespace FastColoredTextBoxNS
{
    /// <summary>
    /// Fast colored textbox
    /// </summary>
    public class FastColoredTextBox : UserControl
    {
        private const int minLeftIndent = 8;
        private const int maxBracketSearchIterations = 1000;
        private const int minLinesForAccuracy = 100000;
        private const int WM_IME_SETCONTEXT = 0x0281;
        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;
        private const int SB_ENDSCROLL = 0x8;

        internal readonly List<LineInfo> lineInfos = new List<LineInfo>();
        private readonly Range selection;
        private readonly Timer timer = new Timer();
        private readonly Timer timer2 = new Timer();
        private readonly List<VisualMarker> visibleMarkers = new List<VisualMarker>();
        private Color changedLineColor;
        private int charHeight;
        private Color currentLineColor;
        private Range delayedTextChangedRange;
        private string descriptionFile;
        private int endFoldingLine = -1;
        private FindForm findForm;
        private Color foldingIndicatorColor;
        private bool handledChar;
        private bool highlightFoldingIndicator;
        private Color indentBackColor;
        private bool isChanged;
        private Language language;
        private Keys lastModifiers;
        private DateTime lastNavigatedDateTime;
        private Range leftBracketPosition;
        private Range leftBracketPosition2;
        private int leftPadding;
        private int lineInterval;
        private Color lineNumberColor;
        private uint lineNumberStartValue;
        private TextSource lines;
        private IntPtr m_hImc;
        private bool mouseIsDrag;
        private bool multiline;
        private bool needRecalc;
        private bool needRiseSelectionChangedDelayed;
        private bool needRiseTextChangedDelayed;
        private bool needRiseVisibleRangeChangedDelayed;
        private int preferredLineWidth;
        private ReplaceForm replaceForm;
        private Range rightBracketPosition;
        private Range rightBracketPosition2;
        private bool scrollBars;
        private Color serviceLinesColor;
        private bool showLineNumbers;
        private FastColoredTextBox sourceTextBox;
        private int startFoldingLine = -1;
        private int updating;
        private Range updatingRange;
        private bool wordWrap;
        private int wordWrapLinesCount;
        private WordWrapMode wordWrapMode = WordWrapMode.WordWrapControlWidth;

        static FastColoredTextBox()
        {
            TypeDescriptor.AddProvider(new FCTBDescriptionProvider(), typeof (FastColoredTextBox));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FastColoredTextBox()
        {
            try
            {
                //drawing optimization
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                SetStyle(ControlStyles.ResizeRedraw, true);
                //append monospace font
                Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
                //create one line
                InitTextSource(CreateTextSource());
                if (lines.Count == 0)
                    lines.InsertLine(0, lines.CreateLine());
                selection = new Range(this) {Start = new Place(0, 0)};
                //default settings
                Cursor = Cursors.IBeam;
                BackColor = Color.White;
                LineNumberColor = Color.Teal;
                IndentBackColor = Color.White;
                ServiceLinesColor = Color.Silver;
                FoldingIndicatorColor = Color.Green;
                CurrentLineColor = Color.Transparent;
                ChangedLineColor = Color.Transparent;
                HighlightFoldingIndicator = true;
                ShowLineNumbers = true;
                TabLength = 4;
                FoldedBlockStyle = new FoldedBlockStyle(Brushes.Gray, null, FontStyle.Regular);
                SelectionStyle = new SelectionStyle(new SolidBrush(Color.FromArgb(50, Color.Blue)));
                BracketsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(80, Color.Lime)));
                BracketsStyle2 = new MarkerStyle(new SolidBrush(Color.FromArgb(60, Color.Red)));
                DelayedEventsInterval = 100;
                DelayedTextChangedInterval = 100;
                AllowSeveralTextStyleDrawing = false;
                LeftBracket = '\x0';
                RightBracket = '\x0';
                LeftBracket2 = '\x0';
                RightBracket2 = '\x0';
                SyntaxHighlighter = new SyntaxHighlighter();
                language = Language.Custom;
                PreferredLineWidth = 0;
                needRecalc = true;
                lastNavigatedDateTime = DateTime.Now;
                AutoIndent = true;
                CommentPrefix = "//";
                lineNumberStartValue = 1;
                multiline = true;
                scrollBars = true;
                AcceptsTab = true;
                AcceptsReturn = true;
                //
                base.AutoScroll = true;
                timer.Tick += timer_Tick;
                timer2.Tick += timer2_Tick;
                //
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private int TopIndent { get; set; }

        /// <summary>
        /// Indicates if tab characters are accepted as input
        /// </summary>
        [DefaultValue(true)]
        [Description("Indicates if tab characters are accepted as input.")]
        public bool AcceptsTab { get; set; }

        /// <summary>
        /// Indicates if return characters are accepted as input
        /// </summary>
        [DefaultValue(true)]
        [Description("Indicates if return characters are accepted as input.")]
        public bool AcceptsReturn { get; set; }

        /// <summary>
        /// Background color for current line
        /// </summary>
        [DefaultValue(typeof (Color), "Transparent")]
        [Description("Background color for current line. Set to Color.Transparent to hide current line highlighting")]
        public Color CurrentLineColor
        {
            get { return currentLineColor; }
            set
            {
                currentLineColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Background color for highlighting of changed lines
        /// </summary>
        [DefaultValue(typeof (Color), "Transparent")]
        [Description(
            "Background color for highlighting of changed lines. Set to Color.Transparent to hide changed line highlighting"
            )]
        public Color ChangedLineColor
        {
            get { return changedLineColor; }
            set
            {
                changedLineColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Fore color (default style color)
        /// </summary>
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set
            {
                base.ForeColor = value;
                lines.InitDefaultStyle();
                Invalidate();
            }
        }

        /// <summary>
        /// Height of char in pixels
        /// </summary>
        [Description("Height of char in pixels")]
        public int CharHeight
        {
            get { return charHeight; }
            private set
            {
                charHeight = value;
                OnCharSizeChanged();
            }
        }

        /// <summary>
        /// Interval between lines (in pixels)
        /// </summary>
        [Description("Interval between lines in pixels")]
        [DefaultValue(0)]
        public int LineInterval
        {
            get { return lineInterval; }
            set
            {
                lineInterval = value;
                Font = Font;
                Invalidate();
            }
        }

        /// <summary>
        /// Width of char in pixels
        /// </summary>
        [Description("Width of char in pixels")]
        public int CharWidth { get; private set; }

        /// <summary>
        /// Spaces count for tab
        /// </summary>
        [DefaultValue(4)]
        [Description("Spaces count for tab")]
        public int TabLength { get; set; }

        /// <summary>
        /// Text was changed
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsChanged
        {
            get { return isChanged; }
            set
            {
                if (!value)
                    //clear line's IsChanged property
                    lines.ClearIsChanged();

                isChanged = value;
            }
        }

        /// <summary>
        /// Text version
        /// </summary>
        /// <remarks>This counter is incremented each time changes the text</remarks>
        [Browsable(false)]
        public int TextVersion { get; private set; }

        /// <summary>
        /// Read only
        /// </summary>
        [DefaultValue(false)]
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Shows line numbers.
        /// </summary>
        [DefaultValue(true)]
        [Description("Shows line numbers.")]
        public bool ShowLineNumbers
        {
            get { return showLineNumbers; }
            set
            {
                showLineNumbers = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Color of line numbers.
        /// </summary>
        [DefaultValue(typeof (Color), "Teal")]
        [Description("Color of line numbers.")]
        public Color LineNumberColor
        {
            get { return lineNumberColor; }
            set
            {
                lineNumberColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Start value of first line number.
        /// </summary>
        [DefaultValue(typeof (uint), "1")]
        [Description("Start value of first line number.")]
        public uint LineNumberStartValue
        {
            get { return lineNumberStartValue; }
            set
            {
                lineNumberStartValue = value;
                needRecalc = true;
                Invalidate();
            }
        }

        /// <summary>
        /// Background color of indent area
        /// </summary>
        [DefaultValue(typeof (Color), "White")]
        [Description("Background color of indent area")]
        public Color IndentBackColor
        {
            get { return indentBackColor; }
            set
            {
                indentBackColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Color of service lines (folding lines, borders of blocks etc.)
        /// </summary>
        [DefaultValue(typeof (Color), "Silver")]
        [Description("Color of service lines (folding lines, borders of blocks etc.)")]
        public Color ServiceLinesColor
        {
            get { return serviceLinesColor; }
            set
            {
                serviceLinesColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Color of folding area indicator
        /// </summary>
        [DefaultValue(typeof (Color), "Green")]
        [Description("Color of folding area indicator")]
        public Color FoldingIndicatorColor
        {
            get { return foldingIndicatorColor; }
            set
            {
                foldingIndicatorColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Enables folding indicator (left vertical line between folding bounds)
        /// </summary>
        [DefaultValue(true)]
        [Description("Enables folding indicator (left vertical line between folding bounds)")]
        public bool HighlightFoldingIndicator
        {
            get { return highlightFoldingIndicator; }
            set
            {
                highlightFoldingIndicator = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Left indent in pixels
        /// </summary>
        [Browsable(false)]
        [Description("Left indent in pixels")]
        public int LeftIndent { get; private set; }

        /// <summary>
        /// Left padding in pixels
        /// </summary>
        [DefaultValue(0)]
        [Description("Left padding in pixels")]
        public int LeftPadding
        {
            get { return leftPadding; }
            set
            {
                leftPadding = value;
                Invalidate();
            }
        }

        /// <summary>
        /// This property draws vertical line after defined char position.
        /// Set to 0 for disable drawing of vertical line.
        /// </summary>
        [DefaultValue(0)]
        [Description(
            "This property draws vertical line after defined char position. Set to 0 for disable drawing of vertical line."
            )]
        public int PreferredLineWidth
        {
            get { return preferredLineWidth; }
            set
            {
                preferredLineWidth = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Styles
        /// Maximum style count is 16
        /// </summary>
        [Browsable(false)]
        public Style[] Styles
        {
            get { return lines.Styles; }
        }

        /// <summary>
        /// Default text style
        /// This style is using when no one other TextStyle is not defined in Char.style
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TextStyle DefaultStyle
        {
            get { return lines.DefaultStyle; }
            set { lines.DefaultStyle = value; }
        }

        /// <summary>
        /// Style for rendering Selection area
        /// </summary>
        [Browsable(false)]
        public SelectionStyle SelectionStyle { get; set; }

        /// <summary>
        /// Style for folded block rendering
        /// </summary>
        [Browsable(false)]
        public TextStyle FoldedBlockStyle { get; set; }

        /// <summary>
        /// Style for brackets highlighting
        /// </summary>
        [Browsable(false)]
        public MarkerStyle BracketsStyle { get; set; }

        /// <summary>
        /// Style for alternative brackets highlighting
        /// </summary>
        [Browsable(false)]
        public MarkerStyle BracketsStyle2 { get; set; }

        /// <summary>
        /// Opening bracket for brackets highlighting.
        /// Set to '\x0' for disable brackets highlighting.
        /// </summary>
        [DefaultValue('\x0')]
        [Description("Opening bracket for brackets highlighting. Set to '\\x0' for disable brackets highlighting.")]
        public char LeftBracket { get; set; }

        /// <summary>
        /// Closing bracket for brackets highlighting.
        /// Set to '\x0' for disable brackets highlighting.
        /// </summary>
        [DefaultValue('\x0')]
        [Description("Closing bracket for brackets highlighting. Set to '\\x0' for disable brackets highlighting.")]
        public char RightBracket { get; set; }

        /// <summary>
        /// Alternative opening bracket for brackets highlighting.
        /// Set to '\x0' for disable brackets highlighting.
        /// </summary>
        [DefaultValue('\x0')]
        [Description(
            "Alternative opening bracket for brackets highlighting. Set to '\\x0' for disable brackets highlighting.")]
        public char LeftBracket2 { get; set; }

        /// <summary>
        /// Alternative closing bracket for brackets highlighting.
        /// Set to '\x0' for disable brackets highlighting.
        /// </summary>
        [DefaultValue('\x0')]
        [Description(
            "Alternative closing bracket for brackets highlighting. Set to '\\x0' for disable brackets highlighting.")]
        public char RightBracket2 { get; set; }

        /// <summary>
        /// Comment line prefix.
        /// </summary>
        [DefaultValue("//")]
        [Description("Comment line prefix.")]
        public string CommentPrefix { get; set; }

        /// <summary>
        /// This property specifies which part of the text will be highlighted as you type (by built-in highlighter).
        /// </summary>
        /// <remarks>When a user enters text, a component of rebuilding the highlight (because the text is changed).
        /// This property specifies exactly which section of the text will be re-highlighted.
        /// This can be useful to highlight multi-line comments, for example.</remarks>
        [DefaultValue(typeof (HighlightingRangeType), "ChangedRange")]
        [Description("This property specifies which part of the text will be highlighted as you type.")]
        public HighlightingRangeType HighlightingRangeType { get; set; }

        /// <summary>
        /// Is keyboard in replace mode (wide caret) ?
        /// </summary>
        [Browsable(false)]
        public bool IsReplaceMode
        {
            get
            {
                return IsKeyLocked(Keys.Insert) && Selection.Start == Selection.End &&
                       Selection.Start.iChar < lines[Selection.Start.iLine].Count;
            }
        }

        /// <summary>
        /// Allows text rendering several styles same time.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Description("Allows text rendering several styles same time.")]
        public bool AllowSeveralTextStyleDrawing { get; set; }

        /// <summary>
        /// Allows AutoIndent. Inserts spaces before new line.
        /// </summary>
        [DefaultValue(true)]
        [Description("Allows auto indent. Inserts spaces before line chars.")]
        public bool AutoIndent { get; set; }

        /// <summary>
        /// Minimal delay(ms) for delayed events (except TextChangedDelayed).
        /// </summary>
        [Browsable(true)]
        [DefaultValue(100)]
        [Description("Minimal delay(ms) for delayed events (except TextChangedDelayed).")]
        public int DelayedEventsInterval
        {
            get { return timer.Interval; }
            set { timer.Interval = value; }
        }

        /// <summary>
        /// Minimal delay(ms) for TextChangedDelayed event.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(100)]
        [Description("Minimal delay(ms) for TextChangedDelayed event.")]
        public int DelayedTextChangedInterval
        {
            get { return timer2.Interval; }
            set { timer2.Interval = value; }
        }

        /// <summary>
        /// Language for highlighting by built-in highlighter.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(typeof (Language), "Custom")]
        [Description("Language for highlighting by built-in highlighter.")]
        public Language Language
        {
            get { return language; }
            set
            {
                language = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Syntax Highlighter
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SyntaxHighlighter SyntaxHighlighter { get; set; }

        /// <summary>
        /// XML file with description of syntax highlighting.
        /// This property works only with Language == Language.Custom.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(null)]
        [Editor("System.Windows.Forms.Design.FileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [Description("XML file with description of syntax highlighting. This property works only with Language == Language.Custom.")]
        public string DescriptionFile
        {
            get { return descriptionFile; }
            set
            {
                descriptionFile = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Position of left highlighted bracket.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Range LeftBracketPosition
        {
            get { return leftBracketPosition; }
        }

        /// <summary>
        /// Position of right highlighted bracket.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Range RightBracketPosition
        {
            get { return rightBracketPosition; }
        }

        /// <summary>
        /// Position of left highlighted alternative bracket.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Range LeftBracketPosition2
        {
            get { return leftBracketPosition2; }
        }

        /// <summary>
        /// Position of right highlighted alternative bracket.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Range RightBracketPosition2
        {
            get { return rightBracketPosition2; }
        }

        /// <summary>
        /// Start line index of current highlighted folding area. Return -1 if start of area is not found.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int StartFoldingLine
        {
            get { return startFoldingLine; }
        }

        /// <summary>
        /// End line index of current highlighted folding area. Return -1 if end of area is not found.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int EndFoldingLine
        {
            get { return endFoldingLine; }
        }

        /// <summary>
        /// TextSource
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TextSource TextSource
        {
            get { return lines; }
            set { InitTextSource(value); }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasSourceTextBox
        {
            get { return SourceTextBox != null; }
        }

        /// <summary>
        /// The source of the text.
        /// Allows to get text from other FastColoredTextBox.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(null)]
        [Description("Allows to get text from other FastColoredTextBox.")]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FastColoredTextBox SourceTextBox
        {
            get { return sourceTextBox; }
            set
            {
                if (value == sourceTextBox)
                    return;

                sourceTextBox = value;

                if (sourceTextBox == null)
                {
                    InitTextSource(CreateTextSource());
                    lines.InsertLine(0, TextSource.CreateLine());
                    IsChanged = false;
                }
                else
                {
                    InitTextSource(SourceTextBox.TextSource);
                    isChanged = false;
                }
                Invalidate();
            }
        }

        /// <summary>
        /// Returns current visible range of text
        /// </summary>
        [Browsable(false)]
        public Range VisibleRange
        {
            get
            {
                return GetRange(
                    PointToPlace(new Point(LeftIndent, 0)),
                    PointToPlace(new Point(ClientSize.Width, ClientSize.Height))
                    );
            }
        }

        /// <summary>
        /// Current selection range
        /// </summary>
        [Browsable(false)]
        public Range Selection
        {
            get { return selection; }
            set
            {
                selection.BeginUpdate();
                selection.Start = value.Start;
                selection.End = value.End;
                selection.EndUpdate();
                Invalidate();
            }
        }

        /// <summary>
        /// Background color.
        /// </summary>
        [DefaultValue(typeof (Color), "White")]
        [Description("Background color.")]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        [Browsable(true)]
        [DefaultValue(true)]
        [Description("Scollbars visibility.")]
        public bool ShowScrollBars
        {
            get { return scrollBars; }
            set
            {
                if (value == scrollBars) return;
                scrollBars = value;
                needRecalc = true;
                Invalidate();
            }
        }

        /// <summary>
        /// Multiline
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Description("Multiline mode.")]
        public bool Multiline
        {
            get { return multiline; }
            set
            {
                if (multiline == value) return;
                multiline = value;
                needRecalc = true;
                if (multiline)
                {
                    base.AutoScroll = true;
                    ShowScrollBars = true;
                }
                else
                {
                    base.AutoScroll = false;
                    ShowScrollBars = false;
                    if (lines.Count > 1)
                        lines.RemoveLine(1, lines.Count - 1);
                    lines.Manager.ClearHistory();
                }
                Invalidate();
            }
        }

        /// <summary>
        /// WordWrap.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Description("WordWrap.")]
        public bool WordWrap
        {
            get { return wordWrap; }
            set
            {
                if (wordWrap == value) return;
                wordWrap = value;
                RecalcWordWrap(0, LinesCount - 1);
                Invalidate();
            }
        }

        /// <summary>
        /// WordWrap mode.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(typeof (WordWrapMode), "WordWrapControlWidth")]
        [Description("WordWrap mode.")]
        public WordWrapMode WordWrapMode
        {
            get { return wordWrapMode; }
            set
            {
                if (wordWrapMode == value) return;
                wordWrapMode = value;
                RecalcWordWrap(0, LinesCount - 1);
                Invalidate();
            }
        }


        /// <summary>
        /// Count of lines with wordwrap effect
        /// </summary>
        [Browsable(false)]
        public int WordWrapLinesCount
        {
            get
            {
                if (needRecalc)
                    Recalc();
                return wordWrapLinesCount;
            }
        }

        /// <summary>
        /// Do not change this property
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool AutoScroll
        {
            get { return base.AutoScroll; }
            set { ; }
        }

        /// <summary>
        /// Count of lines
        /// </summary>
        [Browsable(false)]
        public int LinesCount
        {
            get { return lines.Count; }
        }

        /// <summary>
        /// Gets or sets char and styleId for given place
        /// This property does not fire OnTextChanged event
        /// </summary>
        public Char this[Place place]
        {
            get { return lines[place.iLine][place.iChar]; }
            set { lines[place.iLine][place.iChar] = value; }
        }

        /// <summary>
        /// Gets Line
        /// </summary>
        public Line this[int iLine]
        {
            get { return lines[iLine]; }
        }

        /// <summary>
        /// Text of control
        /// </summary>
        [Browsable(true)]
        [Localizable(true)]
        [Editor(
            "System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
            , typeof (UITypeEditor))]
        [SettingsBindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Text of the control.")]
        [Bindable(true)]
        public override string Text
        {
            get
            {
                var sel = new Range(this);
                sel.SelectAll();
                return sel.Text;
            }

            set
            {
                SetAsCurrentTB();

                Selection.BeginUpdate();
                try
                {
                    Selection.SelectAll();
                    InsertText(value);
                    GoHome();
                }
                finally
                {
                    Selection.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Text lines
        /// </summary>
        [Browsable(false)]
        public IList<string> Lines
        {
            get { return lines.Lines; }
        }

        [Browsable(false)]
        public new Padding Padding
        {
            get { return new Padding(0, 0, 0, 0); }
            set { ; }
        }

        /// <summary>
        /// Gets colored text as HTML
        /// </summary>
        /// <remarks>For more flexibility you can use ExportToHTML class also</remarks>
        [Browsable(false)]
        public string Html
        {
            get
            {
                var exporter = new ExportToHTML();
                exporter.UseNbsp = false;
                exporter.UseStyleTag = false;
                exporter.UseBr = false;
                return "<pre>" + exporter.GetHtml(this) + "</pre>";
            }
        }

        /// <summary>
        /// Text of current selection
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get { return Selection.Text; }
            set { InsertText(value); }
        }

        /// <summary>
        /// Start position of selection
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart
        {
            get { return Math.Min(PlaceToPosition(Selection.Start), PlaceToPosition(Selection.End)); }
            set { Selection.Start = PositionToPlace(value); }
        }

        /// <summary>
        /// Length of selected text
        /// </summary>
        [Browsable(false)]
        [DefaultValue(0)]
        public int SelectionLength
        {
            get { return Math.Abs(PlaceToPosition(Selection.Start) - PlaceToPosition(Selection.End)); }
            set
            {
                if (value > 0)
                    Selection.End = PositionToPlace(SelectionStart + value);
            }
        }

        /// <summary>
        /// Font
        /// </summary>
        /// <remarks>Use only monospaced font</remarks>
        [DefaultValue(typeof (Font), "Consolas, 9.75")]
        public override Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                //check monospace font
                SizeF sizeM = GetCharSize(base.Font, 'M');
                SizeF sizeDot = GetCharSize(base.Font, '.');
                if (sizeM != sizeDot)
                    base.Font = new Font("Courier New", base.Font.SizeInPoints, FontStyle.Regular, GraphicsUnit.Point);
                //clac size
                SizeF size = GetCharSize(base.Font, 'M');
                CharWidth = (int) Math.Round(size.Width*1f /*0.85*/) - 1 /*0*/;
                CharHeight = lineInterval + (int) Math.Round(size.Height*1f /*0.9*/) - 1 /*0*/;
                //
                NeedRecalc();
                Invalidate();
            }
        }

        private new Size AutoScrollMinSize
        {
            set
            {
                if (scrollBars)
                {
                    base.AutoScroll = true;
                    Size newSize = value;
                    if (WordWrap)
                    {
                        int maxWidth = GetMaxLineWordWrapedWidth();
                        newSize = new Size(Math.Min(newSize.Width, maxWidth), newSize.Height);
                    }
                    base.AutoScrollMinSize = newSize;
                }
                else
                {
                    base.AutoScroll = false;
                    base.AutoScrollMinSize = new Size(0, 0);
                    VerticalScroll.Visible = false;
                    HorizontalScroll.Visible = false;
                    HorizontalScroll.Maximum = value.Width;
                    VerticalScroll.Maximum = value.Height;
                }
            }

            get
            {
                if (scrollBars)
                    return base.AutoScrollMinSize;
                else
                    return new Size(HorizontalScroll.Maximum, VerticalScroll.Maximum);
            }
        }

        /// <summary>
        /// Indicates that IME is allowed (for CJK language entering)
        /// </summary>
        [Browsable(false)]
        public bool ImeAllowed
        {
            get
            {
                return ImeMode != ImeMode.Disable &&
                       ImeMode != ImeMode.Off &&
                       ImeMode != ImeMode.NoControl;
            }
        }

        /// <summary>
        /// Is undo enabled?
        /// </summary>
        [Browsable(false)]
        public bool UndoEnabled
        {
            get { return lines.Manager.UndoEnabled; }
        }

        /// <summary>
        /// Is redo enabled?
        /// </summary>
        [Browsable(false)]
        public bool RedoEnabled
        {
            get { return lines.Manager.RedoEnabled; }
        }

        private int LeftIndentLine
        {
            get { return LeftIndent - minLeftIndent/2 - 3; }
        }

        /// <summary>
        /// Range of all text
        /// </summary>
        [Browsable(false)]
        public Range Range
        {
            get { return new Range(this, new Place(0, 0), new Place(lines[lines.Count - 1].Count, lines.Count - 1)); }
        }

        /// <summary>
        /// Occurs when VisibleRange is changed
        /// </summary>
        public virtual void OnVisibleRangeChanged()
        {
            needRiseVisibleRangeChangedDelayed = true;
            ResetTimer(timer);
            if (VisibleRangeChanged != null)
                VisibleRangeChanged(this, new EventArgs());
        }

        /// <summary>
        /// Invalidates the entire surface of the control and causes the control to be redrawn.
        /// This method is thread safe and does not require Invoke.
        /// </summary>
        public new void Invalidate()
        {
            if (InvokeRequired)
                BeginInvoke(new MethodInvoker(Invalidate));
            else
                base.Invalidate();
        }

        protected virtual void OnCharSizeChanged()
        {
            VerticalScroll.SmallChange = charHeight;
            VerticalScroll.LargeChange = 10*charHeight;
            HorizontalScroll.SmallChange = CharWidth;
        }

        /// <summary>
        /// TextChanged event.
        /// It occurs after insert, delete, clear, undo and redo operations.
        /// </summary>
        [Browsable(true)]
        [Description("It occurs after insert, delete, clear, undo and redo operations.")]
        public new event EventHandler<TextChangedEventArgs> TextChanged;

        /// <summary>
        /// Fake event for correct data binding
        /// </summary>
        [Browsable(false)]
        internal new event EventHandler BindingTextChanged;

        /// <summary>
        /// TextChanging event.
        /// It occurs before insert, delete, clear, undo and redo operations.
        /// </summary>
        [Browsable(true)]
        [Description("It occurs before insert, delete, clear, undo and redo operations.")]
        public event EventHandler<TextChangingEventArgs> TextChanging;

        /// <summary>
        /// SelectionChanged event.
        /// It occurs after changing of selection.
        /// </summary>
        [Browsable(true)]
        [Description("It occurs after changing of selection.")]
        public event EventHandler SelectionChanged;

        /// <summary>
        /// VisibleRangeChanged event.
        /// It occurs after changing of visible range.
        /// </summary>
        [Browsable(true)]
        [Description("It occurs after changing of visible range.")]
        public event EventHandler VisibleRangeChanged;

        /// <summary>
        /// TextChangedDelayed event.
        /// It occurs after insert, delete, clear, undo and redo operations.
        /// This event occurs with a delay relative to TextChanged, and fires only once.
        /// </summary>
        [Browsable(true)]
        [Description(
            "It occurs after insert, delete, clear, undo and redo operations. This event occurs with a delay relative to TextChanged, and fires only once."
            )]
        public event EventHandler<TextChangedEventArgs> TextChangedDelayed;

        /// <summary>
        /// SelectionChangedDelayed event.
        /// It occurs after changing of selection.
        /// This event occurs with a delay relative to SelectionChanged, and fires only once.
        /// </summary>
        [Browsable(true)]
        [Description(
            "It occurs after changing of selection. This event occurs with a delay relative to SelectionChanged, and fires only once."
            )]
        public event EventHandler SelectionChangedDelayed;

        /// <summary>
        /// VisibleRangeChangedDelayed event.
        /// It occurs after changing of visible range.
        /// This event occurs with a delay relative to VisibleRangeChanged, and fires only once.
        /// </summary>
        [Browsable(true)]
        [Description(
            "It occurs after changing of visible range. This event occurs with a delay relative to VisibleRangeChanged, and fires only once."
            )]
        public event EventHandler VisibleRangeChangedDelayed;

        /// <summary>
        /// It occurs when user click on VisualMarker.
        /// </summary>
        [Browsable(true)]
        [Description("It occurs when user click on VisualMarker.")]
        public event EventHandler<VisualMarkerEventArgs> VisualMarkerClick;

        /// <summary>
        /// It occurs when visible char is enetering (alphabetic, digit, punctuation, DEL, BACKSPACE)
        /// </summary>
        /// <remarks>Set Handle to True for cancel key</remarks>
        [Browsable(true)]
        [Description("It occurs when visible char is enetering (alphabetic, digit, punctuation, DEL, BACKSPACE).")]
        public event KeyPressEventHandler KeyPressing;

        /// <summary>
        /// It occurs when visible char is enetered (alphabetic, digit, punctuation, DEL, BACKSPACE)
        /// </summary>
        [Browsable(true)]
        [Description("It occurs when visible char is enetered (alphabetic, digit, punctuation, DEL, BACKSPACE).")]
        public event KeyPressEventHandler KeyPressed;

        /// <summary>
        /// It occurs when calculates AutoIndent for new line
        /// </summary>
        [Browsable(true)]
        [Description("It occurs when calculates AutoIndent for new line.")]
        public event EventHandler<AutoIndentEventArgs> AutoIndentNeeded;

        /// <summary>
        /// It occurs when line background is painting
        /// </summary>
        [Browsable(true)]
        [Description("It occurs when line background is painting.")]
        public event EventHandler<PaintLineEventArgs> PaintLine;

        /// <summary>
        /// Occurs when line was inserted/added
        /// </summary>
        [Browsable(true)]
        [Description("Occurs when line was inserted/added.")]
        public event EventHandler<LineInsertedEventArgs> LineInserted;

        /// <summary>
        /// Occurs when line was removed
        /// </summary>
        [Browsable(true)]
        [Description("Occurs when line was removed.")]
        public event EventHandler<LineRemovedEventArgs> LineRemoved;

        /// <summary>
        /// Occurs when current highlighted folding area is changed.
        /// Current folding area see in StartFoldingLine and EndFoldingLine.
        /// </summary>
        /// <remarks></remarks>
        [Browsable(true)]
        [Description("Occurs when current highlighted folding area is changed.")]
        public event EventHandler<EventArgs> FoldingHighlightChanged;

        /// <summary>
        /// Occurs when undo/redo stack is changed
        /// </summary>
        /// <remarks></remarks>
        [Browsable(true)]
        [Description("Occurs when undo/redo stack is changed.")]
        public event EventHandler<EventArgs> UndoRedoStateChanged;

        private TextSource CreateTextSource()
        {
            return new TextSource(this);
        }

        private void SetAsCurrentTB()
        {
            TextSource.CurrentTB = this;
        }

        private void InitTextSource(TextSource ts)
        {
            if (lines != null)
            {
                ts.LineInserted -= ts_LineInserted;
                ts.LineRemoved -= ts_LineRemoved;
                ts.TextChanged -= ts_TextChanged;
                ts.RecalcNeeded -= ts_RecalcNeeded;
                ts.TextChanging -= ts_TextChanging;

                lines.Dispose();
            }

            lineInfos.Clear();

            lines = ts;

            if (ts != null)
            {
                ts.LineInserted += ts_LineInserted;
                ts.LineRemoved += ts_LineRemoved;
                ts.TextChanged += ts_TextChanged;
                ts.RecalcNeeded += ts_RecalcNeeded;
                ts.TextChanging += ts_TextChanging;
                while (lineInfos.Count < ts.Count)
                    lineInfos.Add(new LineInfo(-1));
            }

            isChanged = false;
            needRecalc = true;
        }

        private void ts_TextChanging(object sender, TextChangingEventArgs e)
        {
            if (TextSource.CurrentTB == this)
            {
                string text = e.InsertingText;
                OnTextChanging(ref text);
                e.InsertingText = text;
            }
        }

        private void ts_RecalcNeeded(object sender, TextSource.TextChangedEventArgs e)
        {
            if (e.iFromLine == e.iToLine && !WordWrap && lines.Count > minLinesForAccuracy)
                RecalcScrollByOneLine(e.iFromLine);
            else
                needRecalc = true;
        }

        /// <summary>
        /// Call this method if the recalc of the position of lines is needed.
        /// </summary>
        public void NeedRecalc()
        {
            needRecalc = true;
        }

        private void ts_TextChanged(object sender, TextSource.TextChangedEventArgs e)
        {
            if (e.iFromLine == e.iToLine && !WordWrap)
                RecalcScrollByOneLine(e.iFromLine);
            else
                needRecalc = true;

            Invalidate();
            if (TextSource.CurrentTB == this)
                OnTextChanged(e.iFromLine, e.iToLine);
        }

        private void ts_LineRemoved(object sender, LineRemovedEventArgs e)
        {
            lineInfos.RemoveRange(e.Index, e.Count);
            OnLineRemoved(e.Index, e.Count, e.RemovedLineUniqueIds);
        }

        private void ts_LineInserted(object sender, LineInsertedEventArgs e)
        {
            var temp = new List<LineInfo>(e.Count);
            for (int i = 0; i < e.Count; i++)
                temp.Add(new LineInfo(-1));
            lineInfos.InsertRange(e.Index, temp);
            OnLineInserted(e.Index, e.Count);
        }

        /// <summary>
        /// Navigates forward (by Line.LastVisit property)
        /// </summary>
        public bool NavigateForward()
        {
            DateTime min = DateTime.Now;
            int iLine = -1;
            for (int i = 0; i < LinesCount; i++)
                if (lines.IsLineLoaded(i))
                    if (lines[i].LastVisit > lastNavigatedDateTime && lines[i].LastVisit < min)
                    {
                        min = lines[i].LastVisit;
                        iLine = i;
                    }
            if (iLine >= 0)
            {
                Navigate(iLine);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Navigates backward (by Line.LastVisit property)
        /// </summary>
        public bool NavigateBackward()
        {
            var max = new DateTime();
            int iLine = -1;
            for (int i = 0; i < LinesCount; i++)
                if (lines.IsLineLoaded(i))
                    if (lines[i].LastVisit < lastNavigatedDateTime && lines[i].LastVisit > max)
                    {
                        max = lines[i].LastVisit;
                        iLine = i;
                    }
            if (iLine >= 0)
            {
                Navigate(iLine);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Navigates to defined line, without Line.LastVisit reseting
        /// </summary>
        public void Navigate(int iLine)
        {
            if (iLine >= LinesCount) return;
            lastNavigatedDateTime = lines[iLine].LastVisit;
            Selection.Start = new Place(0, iLine);
            DoSelectionVisible();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            m_hImc = ImmGetContext(Handle);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Enabled = false;
            if (needRiseTextChangedDelayed)
            {
                needRiseTextChangedDelayed = false;
                if (delayedTextChangedRange == null)
                    return;
                delayedTextChangedRange = Range.GetIntersectionWith(delayedTextChangedRange);
                delayedTextChangedRange.Expand();
                OnTextChangedDelayed(delayedTextChangedRange);
                delayedTextChangedRange = null;
            }
        }

        internal void AddVisualMarker(VisualMarker marker)
        {
            visibleMarkers.Add(marker);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Enabled = false;
            if (needRiseSelectionChangedDelayed)
            {
                needRiseSelectionChangedDelayed = false;
                OnSelectionChangedDelayed();
            }
            if (needRiseVisibleRangeChangedDelayed)
            {
                needRiseVisibleRangeChangedDelayed = false;
                OnVisibleRangeChangedDelayed();
            }
        }

        public virtual void OnTextChangedDelayed(Range changedRange)
        {
            if (TextChangedDelayed != null)
                TextChangedDelayed(this, new TextChangedEventArgs(changedRange));
        }

        public virtual void OnSelectionChangedDelayed()
        {
            RecalcScrollByOneLine(Selection.Start.iLine);
            //highlight brackets
            ClearBracketsPositions();
            if (LeftBracket != '\x0' && RightBracket != '\x0')
                HighlightBrackets(LeftBracket, RightBracket, ref leftBracketPosition, ref rightBracketPosition);
            if (LeftBracket2 != '\x0' && RightBracket2 != '\x0')
                HighlightBrackets(LeftBracket2, RightBracket2, ref leftBracketPosition2, ref rightBracketPosition2);
            //remember last visit time
            if (Selection.Start == Selection.End && Selection.Start.iLine < LinesCount)
            {
                if (lastNavigatedDateTime != lines[Selection.Start.iLine].LastVisit)
                {
                    lines[Selection.Start.iLine].LastVisit = DateTime.Now;
                    lastNavigatedDateTime = lines[Selection.Start.iLine].LastVisit;
                }
            }

            if (SelectionChangedDelayed != null)
                SelectionChangedDelayed(this, new EventArgs());
        }

        public virtual void OnVisibleRangeChangedDelayed()
        {
            if (VisibleRangeChangedDelayed != null)
                VisibleRangeChangedDelayed(this, new EventArgs());
        }

        private void ResetTimer(Timer timer)
        {
            timer.Stop();
            if (IsHandleCreated)
                timer.Start();
        }

        /// <summary>
        /// Adds new style
        /// </summary>
        /// <returns>Layer index of this style</returns>
        public int AddStyle(Style style)
        {
            if (style == null) return -1;

            int i = GetStyleIndex(style);
            if (i >= 0)
                return i;

            for (i = Styles.Length - 1; i >= 0; i--)
                if (Styles[i] != null)
                    break;

            i++;
            if (i >= Styles.Length)
                throw new Exception("Maximum count of Styles is exceeded");

            Styles[i] = style;
            return i;
        }

        /// <summary>
        /// Shows find dialog
        /// </summary>
        public void ShowFindDialog()
        {
            ShowFindDialog(null);
        }

        /// <summary>
        /// Shows find dialog
        /// </summary>
        public void ShowFindDialog(string findText)
        {
            if (findForm == null)
                findForm = new FindForm(this);

            if (findText != null)
                findForm.tbFind.Text = findText;
            else if (Selection.Start != Selection.End && Selection.Start.iLine == Selection.End.iLine)
                findForm.tbFind.Text = Selection.Text;

            findForm.tbFind.SelectAll();
            findForm.Show();
        }

        /// <summary>
        /// Shows replace dialog
        /// </summary>
        public void ShowReplaceDialog()
        {
            ShowReplaceDialog(null);
        }

        /// <summary>
        /// Shows replace dialog
        /// </summary>
        public void ShowReplaceDialog(string findText)
        {
            if (ReadOnly)
                return;
            if (replaceForm == null)
                replaceForm = new ReplaceForm(this);

            if (findText != null)
                replaceForm.tbFind.Text = findText;
            else if (Selection.Start != Selection.End && Selection.Start.iLine == Selection.End.iLine)
                replaceForm.tbFind.Text = Selection.Text;

            replaceForm.tbFind.SelectAll();
            replaceForm.Show();
        }

        /// <summary>
        /// Gets length of given line
        /// </summary>
        /// <param name="iLine">Line index</param>
        /// <returns>Length of line</returns>
        public int GetLineLength(int iLine)
        {
            if (iLine < 0 || iLine >= lines.Count)
                throw new ArgumentOutOfRangeException("Line index out of range");

            return lines[iLine].Count;
        }

        /// <summary>
        /// Get range of line
        /// </summary>
        /// <param name="iLine">Line index</param>
        public Range GetLine(int iLine)
        {
            if (iLine < 0 || iLine >= lines.Count)
                throw new ArgumentOutOfRangeException("Line index out of range");

            var sel = new Range(this);
            sel.Start = new Place(0, iLine);
            sel.End = new Place(lines[iLine].Count, iLine);
            return sel;
        }

        /// <summary>
        /// Copy selected text into Clipboard
        /// </summary>
        public void Copy()
        {
            if (Selection.End != Selection.Start)
            {
                var exp = new ExportToHTML();
                exp.UseBr = false;
                exp.UseNbsp = false;
                exp.UseStyleTag = true;
                string html = "<pre>" + exp.GetHtml(Selection) + "</pre>";
                var data = new DataObject();
                data.SetData(DataFormats.UnicodeText, true, Selection.Text);
                data.SetData(DataFormats.Html, PrepareHtmlForClipboard(html));
                //
                var thread = new Thread(() => Clipboard.SetDataObject(data, true));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
        }

        public static MemoryStream PrepareHtmlForClipboard(string html)
        {
            Encoding enc = Encoding.UTF8;

            string begin = "Version:0.9\r\nStartHTML:{0:000000}\r\nEndHTML:{1:000000}"
                           + "\r\nStartFragment:{2:000000}\r\nEndFragment:{3:000000}\r\n";

            string html_begin = "<html>\r\n<head>\r\n"
                                + "<meta http-equiv=\"Content-Type\""
                                + " content=\"text/html; charset=" + enc.WebName + "\">\r\n"
                                + "<title>HTML clipboard</title>\r\n</head>\r\n<body>\r\n"
                                + "<!--StartFragment-->";

            string html_end = "<!--EndFragment-->\r\n</body>\r\n</html>\r\n";

            string begin_sample = String.Format(begin, 0, 0, 0, 0);

            int count_begin = enc.GetByteCount(begin_sample);
            int count_html_begin = enc.GetByteCount(html_begin);
            int count_html = enc.GetByteCount(html);
            int count_html_end = enc.GetByteCount(html_end);

            string html_total = String.Format(
                begin
                , count_begin
                , count_begin + count_html_begin + count_html + count_html_end
                , count_begin + count_html_begin
                , count_begin + count_html_begin + count_html
                                    ) + html_begin + html + html_end;

            return new MemoryStream(enc.GetBytes(html_total));
        }


        /// <summary>
        /// Cut selected text into Clipboard
        /// </summary>
        public void Cut()
        {
            if (Selection.End != Selection.Start)
            {
                Copy();
                ClearSelected();
            }
        }

        /// <summary>
        /// Paste text from clipboard into selection position
        /// </summary>
        public void Paste()
        {
            string text = null;
            var thread = new Thread(() =>
                                        {
                                            if (Clipboard.ContainsText())
                                                text = Clipboard.GetText();
                                        }
                );
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (text != null)
                InsertText(text);
        }

        /// <summary>
        /// Select all chars of text
        /// </summary>
        public void SelectAll()
        {
            Selection.SelectAll();
        }

        /// <summary>
        /// Move caret to end of text
        /// </summary>
        public void GoEnd()
        {
            if (lines.Count > 0)
                Selection.Start = new Place(lines[lines.Count - 1].Count, lines.Count - 1);
            else
                Selection.Start = new Place(0, 0);

            DoCaretVisible();
        }

        /// <summary>
        /// Move caret to first position
        /// </summary>
        public void GoHome()
        {
            Selection.Start = new Place(0, 0);
            VerticalScroll.Value = 0;
            HorizontalScroll.Value = 0;
        }

        /// <summary>
        /// Clear text, styles, history, caches
        /// </summary>
        public void Clear()
        {
            Selection.BeginUpdate();
            try
            {
                Selection.SelectAll();
                ClearSelected();
                lines.Manager.ClearHistory();
                Invalidate();
            }
            finally
            {
                Selection.EndUpdate();
            }
        }

        /// <summary>
        /// Clear buffer of styles
        /// </summary>
        public void ClearStylesBuffer()
        {
            for (int i = 0; i < Styles.Length; i++)
                Styles[i] = null;
        }

        /// <summary>
        /// Clear style of all text
        /// </summary>
        public void ClearStyle(StyleIndex styleIndex)
        {
            foreach (Line line in lines)
                line.ClearStyle(styleIndex);

            for (int i = 0; i < lineInfos.Count; i++)
                SetVisibleState(i, VisibleState.Visible);

            Invalidate();
        }


        /// <summary>
        /// Clears undo and redo stacks
        /// </summary>
        public void ClearUndo()
        {
            lines.Manager.ClearHistory();
        }

        /// <summary>
        /// Insert text into current selection position
        /// </summary>
        /// <param name="text"></param>
        public void InsertText(string text)
        {
            if (text == null)
                return;

            lines.Manager.BeginAutoUndoCommands();
            try
            {
                if (Selection.Start != Selection.End)
                    lines.Manager.ExecuteCommand(new ClearSelectedCommand(TextSource));

                lines.Manager.ExecuteCommand(new InsertTextCommand(TextSource, text));
                if (updating <= 0)
                    DoCaretVisible();
            }
            finally
            {
                lines.Manager.EndAutoUndoCommands();
            }
            //
            Invalidate();
        }

        /// <summary>
        /// Insert text into current selection position (with predefined style)
        /// </summary>
        /// <param name="text"></param>
        public void InsertText(string text, Style style)
        {
            if (text == null)
                return;

            //remember last caret position
            Place last = Selection.Start;
            //insert text
            InsertText(text);
            //get range
            var range = new Range(this, last, Selection.Start);
            //set style for range
            range.SetStyle(style);
        }

        /// <summary>
        /// Append string to end of the Text
        /// </summary>
        /// <param name="text"></param>
        public void AppendText(string text)
        {
            if (text == null)
                return;

            Place oldStart = Selection.Start;
            Place oldEnd = Selection.End;

            Selection.BeginUpdate();
            lines.Manager.BeginAutoUndoCommands();
            try
            {
                if (lines.Count > 0)
                    Selection.Start = new Place(lines[lines.Count - 1].Count, lines.Count - 1);
                else
                    Selection.Start = new Place(0, 0);

                lines.Manager.ExecuteCommand(new InsertTextCommand(TextSource, text));
            }
            finally
            {
                lines.Manager.EndAutoUndoCommands();
                Selection.Start = oldStart;
                Selection.End = oldEnd;
                Selection.EndUpdate();
            }
            //
            Invalidate();
        }

        /// <summary>
        /// Returns index of the style in Styles
        /// -1 otherwise
        /// </summary>
        /// <param name="style"></param>
        /// <returns>Index of the style in Styles</returns>
        public int GetStyleIndex(Style style)
        {
            return Array.IndexOf(Styles, style);
        }

        /// <summary>
        /// Returns StyleIndex mask of given styles
        /// </summary>
        /// <param name="styles"></param>
        /// <returns>StyleIndex mask of given styles</returns>
        public StyleIndex GetStyleIndexMask(Style[] styles)
        {
            StyleIndex mask = StyleIndex.None;
            foreach (Style style in styles)
            {
                int i = GetStyleIndex(style);
                if (i >= 0)
                    mask |= Range.ToStyleIndex(i);
            }

            return mask;
        }

        internal int GetOrSetStyleLayerIndex(Style style)
        {
            int i = GetStyleIndex(style);
            if (i < 0)
                i = AddStyle(style);
            return i;
        }

        public static SizeF GetCharSize(Font font, char c)
        {
            Size sz2 = TextRenderer.MeasureText("<" + c.ToString() + ">", font);
            Size sz3 = TextRenderer.MeasureText("<>", font);

            return new SizeF(sz2.Width - sz3.Width + 1, /*sz2.Height*/font.Height);
        }

        [DllImport("Imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("Imm32.dll")]
        public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HSCROLL || m.Msg == WM_VSCROLL)
                if (m.WParam.ToInt32() != SB_ENDSCROLL)
                    Invalidate();

            base.WndProc(ref m);

            if (ImeAllowed)
                if (m.Msg == WM_IME_SETCONTEXT && m.WParam.ToInt32() == 1)
                {
                    ImmAssociateContext(Handle, m_hImc);
                }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            OnVisibleRangeChanged();
            Invalidate();
        }

        private void InsertChar(char c)
        {
            lines.Manager.BeginAutoUndoCommands();
            try
            {
                if (Selection.Start != Selection.End)
                    lines.Manager.ExecuteCommand(new ClearSelectedCommand(TextSource));

                lines.Manager.ExecuteCommand(new InsertCharCommand(TextSource, c));
            }
            finally
            {
                lines.Manager.EndAutoUndoCommands();
            }

            Invalidate();
        }

        /// <summary>
        /// Deletes selected chars
        /// </summary>
        public void ClearSelected()
        {
            if (Selection.Start != Selection.End)
            {
                lines.Manager.ExecuteCommand(new ClearSelectedCommand(TextSource));
                Invalidate();
            }
        }

        /// <summary>
        /// Deletes current line(s)
        /// </summary>
        public void ClearCurrentLine()
        {
            Selection.Expand();
            lines.Manager.ExecuteCommand(new ClearSelectedCommand(TextSource));
            if (Selection.Start.iLine == 0)
                if (!Selection.GoRightThroughFolded()) return;
            if (Selection.Start.iLine > 0)
                lines.Manager.ExecuteCommand(new InsertCharCommand(TextSource, '\b')); //backspace
            Invalidate();
        }

        private void Recalc()
        {
            if (!needRecalc)
                return;

#if debug
            var sw = Stopwatch.StartNew();
            #endif

            needRecalc = false;
            //calc min left indent
            LeftIndent = LeftPadding;
            long maxLineNumber = LinesCount + lineNumberStartValue - 1;
            int charsForLineNumber = 2 + (maxLineNumber > 0 ? (int) Math.Log10(maxLineNumber) : 0);
            if (Created)
            {
                if (ShowLineNumbers)
                    LeftIndent += charsForLineNumber*CharWidth + minLeftIndent + 1;
            }
            else
                needRecalc = true;
            //calc max line length and count of wordWrapLines
            int maxLineLength = 0;
            wordWrapLinesCount = 0;

            maxLineLength = RecalcMaxLineLength();

            //adjust AutoScrollMinSize
            int minWidth = LeftIndent + (maxLineLength)*CharWidth + 2;
            if (wordWrap)
                switch (WordWrapMode)
                {
                    case WordWrapMode.WordWrapControlWidth:
                    case WordWrapMode.CharWrapControlWidth:
                        minWidth = 0;
                        break;
                    case WordWrapMode.WordWrapPreferredWidth:
                    case WordWrapMode.CharWrapPreferredWidth:
                        minWidth = LeftIndent + PreferredLineWidth*CharWidth + 2;
                        break;
                }
            AutoScrollMinSize = new Size(minWidth, wordWrapLinesCount*CharHeight + TopIndent);

#if debug
            sw.Stop();
            Console.WriteLine("Recalc: " + sw.ElapsedMilliseconds);
            #endif
        }

        private void RecalcScrollByOneLine(int iLine)
        {
            if (iLine >= lines.Count)
                return;

            int maxLineLength = lines[iLine].Count;
            int minWidth = LeftIndent + (maxLineLength)*CharWidth + 2;
            if (AutoScrollMinSize.Width < minWidth)
                AutoScrollMinSize = new Size(minWidth, AutoScrollMinSize.Height);
        }

        private int RecalcMaxLineLength()
        {
            int maxLineLength = 0;
            TextSource lines = this.lines;
            int count = lines.Count;
            int charHeight = CharHeight;
            int topIndent = TopIndent;

            for (int i = 0; i < count; i++)
            {
                int lineLength = lines.GetLineLength(i);
                LineInfo lineInfo = lineInfos[i];
                if (lineLength > maxLineLength && lineInfo.VisibleState == VisibleState.Visible)
                    maxLineLength = lineLength;
                lineInfo.startY = wordWrapLinesCount*charHeight + topIndent;
                wordWrapLinesCount += lineInfo.WordWrapStringsCount;
                lineInfos[i] = lineInfo;
            }

            return maxLineLength;
        }

        private int GetMaxLineWordWrapedWidth()
        {
            if (wordWrap)
                switch (wordWrapMode)
                {
                    case WordWrapMode.WordWrapControlWidth:
                    case WordWrapMode.CharWrapControlWidth:
                        return ClientSize.Width;
                    case WordWrapMode.WordWrapPreferredWidth:
                    case WordWrapMode.CharWrapPreferredWidth:
                        return LeftIndent + PreferredLineWidth*CharWidth + 2;
                }

            return int.MaxValue;
        }

        private void RecalcWordWrap(int fromLine, int toLine)
        {
            int maxCharsPerLine = 0;
            bool charWrap = false;

            switch (WordWrapMode)
            {
                case WordWrapMode.WordWrapControlWidth:
                    maxCharsPerLine = (ClientSize.Width - LeftIndent)/CharWidth;
                    break;
                case WordWrapMode.CharWrapControlWidth:
                    maxCharsPerLine = (ClientSize.Width - LeftIndent)/CharWidth;
                    charWrap = true;
                    break;
                case WordWrapMode.WordWrapPreferredWidth:
                    maxCharsPerLine = PreferredLineWidth;
                    break;
                case WordWrapMode.CharWrapPreferredWidth:
                    maxCharsPerLine = PreferredLineWidth;
                    charWrap = true;
                    break;
            }

            for (int iLine = fromLine; iLine <= toLine; iLine++)
                if (lines.IsLineLoaded(iLine))
                {
                    if (!wordWrap)
                        lineInfos[iLine].CutOffPositions.Clear();
                    else
                    {
                        LineInfo li = lineInfos[iLine];
                        li.CalcCutOffs(maxCharsPerLine, ImeAllowed, charWrap, lines[iLine]);
                        lineInfos[iLine] = li;
                    }
                }
            needRecalc = true;
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            if (WordWrap)
            {
                RecalcWordWrap(0, lines.Count - 1);
                Invalidate();
            }
            OnVisibleRangeChanged();
        }

        /// <summary>
        /// Scroll control for display defined rectangle
        /// </summary>
        /// <param name="rect"></param>
        private void DoVisibleRectangle(Rectangle rect)
        {
            int oldV = VerticalScroll.Value;
            int v = VerticalScroll.Value;
            int h = HorizontalScroll.Value;

            if (rect.Bottom > ClientRectangle.Height)
                v += rect.Bottom - ClientRectangle.Height;
            else if (rect.Top < 0)
                v += rect.Top;

            if (rect.Right > ClientRectangle.Width)
                h += rect.Right - ClientRectangle.Width;
            else if (rect.Left < LeftIndent)
                h += rect.Left - LeftIndent;
            //
            if (!Multiline)
                v = 0;
            //
            try
            {
                if (VerticalScroll.Visible)
                    VerticalScroll.Value = Math.Max(0, v);
                if (HorizontalScroll.Visible)
                    HorizontalScroll.Value = Math.Max(0, h);
            }
            catch (ArgumentOutOfRangeException)
            {
                ;
            }

            if (ShowScrollBars)
            {
                //some magic for update scrolls
                base.AutoScrollMinSize -= new Size(1, 0);
                base.AutoScrollMinSize += new Size(1, 0);
            }
            //
            if (oldV != VerticalScroll.Value)
                OnVisibleRangeChanged();
        }

        /// <summary>
        /// Scroll control for display caret
        /// </summary>
        public void DoCaretVisible()
        {
            Invalidate();
            Recalc();
            Point car = PlaceToPoint(Selection.Start);
            car.Offset(-CharWidth, 0);
            DoVisibleRectangle(new Rectangle(car, new Size(2*CharWidth, 2*CharHeight)));
        }

        /// <summary>
        /// Scroll control left
        /// </summary>
        public void ScrollLeft()
        {
            Invalidate();
            HorizontalScroll.Value = 0;
            AutoScrollMinSize -= new Size(1, 0);
            AutoScrollMinSize += new Size(1, 0);
        }

        /// <summary>
        /// Scroll control for display selection area
        /// </summary>
        public void DoSelectionVisible()
        {
            if (lineInfos[Selection.End.iLine].VisibleState != VisibleState.Visible)
                ExpandBlock(Selection.End.iLine);

            if (lineInfos[Selection.Start.iLine].VisibleState != VisibleState.Visible)
                ExpandBlock(Selection.Start.iLine);

            Recalc();
            DoVisibleRectangle(new Rectangle(PlaceToPoint(new Place(0, Selection.End.iLine)),
                                             new Size(2*CharWidth, 2*CharHeight)));
            Point car = PlaceToPoint(Selection.Start);
            Point car2 = PlaceToPoint(Selection.End);
            car.Offset(-CharWidth, -ClientSize.Height/2);
            DoVisibleRectangle(new Rectangle(car,
                                             new Size(Math.Abs(car2.X - car.X),
                                                      /*Math.Abs(car2.Y-car.Y) + 2 * CharHeight*/ClientSize.Height)));

            Invalidate();
        }


        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyCode == Keys.ShiftKey)
                lastModifiers &= ~Keys.Shift;
            if (e.KeyCode == Keys.Alt)
                lastModifiers &= ~Keys.Alt;
            if (e.KeyCode == Keys.ControlKey)
                lastModifiers &= ~Keys.Control;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            lastModifiers = e.Modifiers;

            handledChar = false;

            if (e.Handled)
            {
                handledChar = true;
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.G:
                    if (e.Modifiers == Keys.Control)
                        ShowGoToDialog();
                    break;
                case Keys.F:
                    if (e.Modifiers == Keys.Control)
                        ShowFindDialog();
                    break;
                case Keys.H:
                    if (e.Modifiers == Keys.Control)
                        ShowReplaceDialog();
                    break;
                case Keys.C:
                    if (e.Modifiers == Keys.Control)
                        Copy();
                    if (e.Modifiers == (Keys.Control | Keys.Shift))
                        CommentSelected();
                    break;
                case Keys.X:
                    if (e.Modifiers == Keys.Control && !ReadOnly)
                        Cut();
                    break;
                case Keys.V:
                    if (e.Modifiers == Keys.Control && !ReadOnly)
                        Paste();
                    break;
                case Keys.A:
                    if (e.Modifiers == Keys.Control)
                        Selection.SelectAll();
                    break;
                case Keys.Z:
                    if (e.Modifiers == Keys.Control && !ReadOnly)
                        Undo();
                    break;
                case Keys.R:
                    if (e.Modifiers == Keys.Control && !ReadOnly)
                        Redo();
                    break;
                case Keys.U:
                    if (e.Modifiers == (Keys.Control | Keys.Shift))
                        LowerCase();
                    if (e.Modifiers == Keys.Control)
                        UpperCase();
                    break;
                case Keys.Tab:
                    if (e.Modifiers == Keys.Shift && !ReadOnly)
                        DecreaseIndent();
                    break;
                case Keys.OemMinus:
                    if (e.Modifiers == Keys.Control)
                        NavigateBackward();
                    if (e.Modifiers == (Keys.Control | Keys.Shift))
                        NavigateForward();
                    break;

                case Keys.Back:
                    if (ReadOnly) break;
                    if (e.Modifiers == Keys.Alt)
                        Undo();
                    else
                    if (e.Modifiers == Keys.None)
                    {
                        if (OnKeyPressing('\b')) //KeyPress event processed key
                            break;
                        if (Selection.End != Selection.Start)
                            ClearSelected();
                        else
                            InsertChar('\b');
                        OnKeyPressed('\b');
                    }else
                    if (e.Modifiers == Keys.Control)
                    {
                        if (OnKeyPressing('\b')) //KeyPress event processed key
                            break;
                        if (Selection.End != Selection.Start)
                            ClearSelected();
                        Selection.GoWordLeft(true);
                        ClearSelected();
                        OnKeyPressed('\b');
                    }
                    break;

                case Keys.Delete:
                    if (ReadOnly) break;
                    if (e.Modifiers == Keys.None)
                    {
                        if (OnKeyPressing((char) 0xff)) //KeyPress event processed key
                            break;
                        if (Selection.End != Selection.Start)
                            ClearSelected();
                        else
                        {
                            if (Selection.GoRightThroughFolded())
                            {
                                int iLine = Selection.Start.iLine;
                                InsertChar('\b');
                                //if removed \n then trim spaces
                                if (iLine != Selection.Start.iLine && AutoIndent)
                                    RemoveSpacesAfterCaret();
                            }
                        }
                        OnKeyPressed((char) 0xff);
                    }else
                    if (e.Modifiers == Keys.Control)
                    {
                        if (OnKeyPressing((char)0xff)) //KeyPress event processed key
                            break;
                        if (Selection.End != Selection.Start)
                            ClearSelected();
                        else
                        {
                            Selection.GoWordRight(true);
                            ClearSelected();
                        }
                        OnKeyPressed((char)0xff);
                    }
                    break;
                case Keys.Space:
                    if (ReadOnly) break;
                    if (e.Modifiers == Keys.None || e.Modifiers == Keys.Shift)
                    {
                        if (OnKeyPressing(' ')) //KeyPress event processed key
                            break;
                        if (Selection.End != Selection.Start)
                            ClearSelected();
                        else
                        {
                            //replace mode? select forward char
                            if (IsReplaceMode)
                            {
                                Selection.GoRight(true);
                                Selection.Inverse();
                            }

                            InsertChar(' ');
                        }
                        OnKeyPressed(' ');
                    }
                    break;

                case Keys.Left:
                    if (e.Modifiers == Keys.Control || e.Modifiers == (Keys.Control | Keys.Shift))
                        Selection.GoWordLeft(e.Shift);
                    if (e.Modifiers == Keys.None || e.Modifiers == Keys.Shift)
                        Selection.GoLeft(e.Shift);
                    break;
                case Keys.Right:
                    if (e.Modifiers == Keys.Control || e.Modifiers == (Keys.Control | Keys.Shift))
                        Selection.GoWordRight(e.Shift);
                    if (e.Modifiers == Keys.None || e.Modifiers == Keys.Shift)
                        Selection.GoRight(e.Shift);
                    break;
                case Keys.Up:
                    if (e.Modifiers == Keys.None || e.Modifiers == Keys.Shift)
                    {
                        Selection.GoUp(e.Shift);
                        ScrollLeft();
                    }
                    break;
                case Keys.Down:
                    if (e.Modifiers == Keys.None || e.Modifiers == Keys.Shift)
                    {
                        Selection.GoDown(e.Shift);
                        ScrollLeft();
                    }
                    break;
                case Keys.PageUp:
                    if (e.Modifiers == Keys.None || e.Modifiers == Keys.Shift)
                    {
                        Selection.GoPageUp(e.Shift);
                        ScrollLeft();
                    }
                    break;
                case Keys.PageDown:
                    if (e.Modifiers == Keys.None || e.Modifiers == Keys.Shift)
                    {
                        Selection.GoPageDown(e.Shift);
                        ScrollLeft();
                    }
                    break;
                case Keys.Home:
                    if (e.Modifiers == Keys.Control || e.Modifiers == (Keys.Control | Keys.Shift))
                        Selection.GoFirst(e.Shift);
                    if (e.Modifiers == Keys.None || e.Modifiers == Keys.Shift)
                    {
                        GoHome(e.Shift);
                        ScrollLeft();
                    }
                    break;
                case Keys.End:
                    if (e.Modifiers == Keys.Control || e.Modifiers == (Keys.Control | Keys.Shift))
                        Selection.GoLast(e.Shift);
                    if (e.Modifiers == Keys.None || e.Modifiers == Keys.Shift)
                        Selection.GoEnd(e.Shift);
                    break;
                default:
                    if ((e.Modifiers & Keys.Control) != 0)
                        return;
                    if ((e.Modifiers & Keys.Alt) != 0)
                        return;
                    if (e.KeyCode == Keys.ShiftKey)
                        return;
                    break;
            }

            e.Handled = true;

            DoCaretVisible();
            Invalidate();
        }

        private void GoHome(bool shift)
        {
            Selection.BeginUpdate();
            try
            {
                int iLine = Selection.Start.iLine;
                int spaces = this[iLine].StartSpacesCount;
                if (Selection.Start.iChar <= spaces)
                    Selection.GoHome(shift);
                else
                {
                    Selection.GoHome(shift);
                    for (int i = 0; i < spaces; i++)
                        Selection.GoRight(shift);
                }
            }
            finally
            {
                Selection.EndUpdate();
            }
        }

        /// <summary>
        /// Convert selected text to upper case
        /// </summary>
        public void UpperCase()
        {
            Range old = Selection.Clone();
            SelectedText = SelectedText.ToUpper();
            Selection.Start = old.Start;
            Selection.End = old.End;
        }

        /// <summary>
        /// Convert selected text to lower case
        /// </summary>
        public void LowerCase()
        {
            Range old = Selection.Clone();
            SelectedText = SelectedText.ToLower();
            Selection.Start = old.Start;
            Selection.End = old.End;
        }

        /// <summary>
        /// Insert/remove comment prefix into selected lines
        /// </summary>
        public void CommentSelected()
        {
            CommentSelected(CommentPrefix);
        }

        /// <summary>
        /// Insert/remove comment prefix into selected lines
        /// </summary>
        public void CommentSelected(string commentPrefix)
        {
            if (string.IsNullOrEmpty(commentPrefix))
                return;
            Selection.Normalize();
            bool isCommented = lines[Selection.Start.iLine].Text.TrimStart().StartsWith(commentPrefix);
            if (isCommented)
                RemoveLinePrefix(commentPrefix);
            else
                InsertLinePrefix(commentPrefix);
        }

        /*
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                bool proc = ProcessKeyPress('\r');
                if (proc)
                {
                    base.OnKeyDown(new KeyEventArgs(Keys.Enter));
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }*/

        public void OnKeyPressing(KeyPressEventArgs args)
        {
            if (KeyPressing != null)
                KeyPressing(this, args);
        }

        private bool OnKeyPressing(char c)
        {
            var args = new KeyPressEventArgs(c);
            OnKeyPressing(args);
            return args.Handled;
        }

        public void OnKeyPressed(char c)
        {
            var args = new KeyPressEventArgs(c);
            if (KeyPressed != null)
                KeyPressed(this, args);
        }

        protected override bool ProcessMnemonic(char charCode)
        {
            if (Focused)
                return ProcessKeyPress(charCode) || base.ProcessMnemonic(charCode);
            else
                return false;
        }

        private bool ProcessKeyPress(char c)
        {
            if (handledChar)
                return true;

            if (c == ' ')
                return true;

            if (c == '\b' && (lastModifiers & Keys.Alt) != 0)
                return true;

            if (char.IsControl(c) && c != '\r' && c != '\t')
                return false;

            if (ReadOnly || !Enabled)
                return false;


            if (lastModifiers != Keys.None &&
                lastModifiers != Keys.Shift &&
                lastModifiers != (Keys.Control | Keys.Alt) && //ALT+CTRL is special chars (AltGr)
                lastModifiers != (Keys.Shift | Keys.Control | Keys.Alt) && //SHIFT + ALT + CTRL is special chars (AltGr)
                (lastModifiers != (Keys.Alt) || char.IsLetterOrDigit(c)) //may be ALT+LetterOrDigit is mnemonic code
                )
                return false; //do not process Ctrl+? and Alt+? keys

            char sourceC = c;
            if (OnKeyPressing(sourceC)) //KeyPress event processed key
                return true;

            if (c == '\r' && !AcceptsReturn)
                return false;

            //tab?
            if (c == '\t')
            {
                if (!AcceptsTab)
                    return false;

                if (Selection.Start == Selection.End)
                {
                    //insert tab as spaces
                    int spaces = TabLength - (Selection.Start.iChar%TabLength);
                    //replace mode? select forward chars
                    if (IsReplaceMode)
                    {
                        for (int i = 0; i < spaces; i++)
                            Selection.GoRight(true);
                        Selection.Inverse();
                    }

                    InsertText(new String(' ', spaces));
                }
                else if ((lastModifiers & Keys.Shift) == 0)
                    IncreaseIndent();
            }
            else
            {
                //replace \r on \n
                if (c == '\r')
                    c = '\n';
                //replace mode? select forward char
                if (IsReplaceMode)
                {
                    Selection.GoRight(true);
                    Selection.Inverse();
                }
                //insert char
                InsertChar(c);
                //do autoindent
                if (AutoIndent)
                {
                    DoCaretVisible();
                    int needSpaces = CalcAutoIndent(Selection.Start.iLine);
                    if (this[Selection.Start.iLine].AutoIndentSpacesNeededCount != needSpaces)
                    {
                        DoAutoIndent(Selection.Start.iLine);
                        this[Selection.Start.iLine].AutoIndentSpacesNeededCount = needSpaces;
                    }
                }
            }

            DoCaretVisible();
            Invalidate();

            OnKeyPressed(sourceC);

            return true;
        }

        private void RemoveSpacesAfterCaret()
        {
            if (Selection.Start != Selection.End)
                return;
            Place end = Selection.Start;
            while (Selection.CharAfterStart == ' ')
                Selection.GoRight(true);
            ClearSelected();
        }

        /// <summary>
        /// Inserts autoindent's spaces in the line
        /// </summary>
        public virtual void DoAutoIndent(int iLine)
        {
            Place oldStart = Selection.Start;
            //
            int needSpaces = CalcAutoIndent(iLine);
            //
            int spaces = lines[iLine].StartSpacesCount;
            int needToInsert = needSpaces - spaces;
            if (needToInsert < 0)
                needToInsert = -Math.Min(-needToInsert, spaces);
            //insert start spaces
            if (needToInsert == 0)
                return;
            Selection.Start = new Place(0, iLine);
            if (needToInsert > 0)
                InsertText(new String(' ', needToInsert));
            else
            {
                Selection.Start = new Place(0, iLine);
                Selection.End = new Place(-needToInsert, iLine);
                ClearSelected();
            }

            Selection.Start = new Place(Math.Min(lines[iLine].Count, Math.Max(0, oldStart.iChar + needToInsert)), iLine);
        }

        /// <summary>
        /// Returns needed start space count for the line
        /// </summary>
        public virtual int CalcAutoIndent(int iLine)
        {
            if (iLine < 0 || iLine >= LinesCount) return 0;


            EventHandler<AutoIndentEventArgs> calculator = AutoIndentNeeded;
            if (calculator == null)
                if (Language != Language.Custom && SyntaxHighlighter != null)
                    calculator = SyntaxHighlighter.AutoIndentNeeded;
                else
                    calculator = CalcAutoIndentShiftByCodeFolding;

            int needSpaces = 0;

            var stack = new Stack<AutoIndentEventArgs>();
            //calc indent for previous lines, find stable line
            int i;
            for (i = iLine - 1; i >= 0; i--)
            {
                var args = new AutoIndentEventArgs(i, lines[i].Text, i > 0 ? lines[i - 1].Text : "", TabLength);
                calculator(this, args);
                stack.Push(args);
                if (args.Shift == 0 && args.LineText.Trim() != "")
                    break;
            }
            int indent = lines[i >= 0 ? i : 0].StartSpacesCount;
            while (stack.Count != 0)
                indent += stack.Pop().ShiftNextLines;
            //clalc shift for current line
            var a = new AutoIndentEventArgs(iLine, lines[iLine].Text, iLine > 0 ? lines[iLine - 1].Text : "", TabLength);
            calculator(this, a);
            needSpaces = indent + a.Shift;

            return needSpaces;
        }

        internal virtual void CalcAutoIndentShiftByCodeFolding(object sender, AutoIndentEventArgs args)
        {
            //inset TAB after start folding marker
            if (string.IsNullOrEmpty(lines[args.iLine].FoldingEndMarker) &&
                !string.IsNullOrEmpty(lines[args.iLine].FoldingStartMarker))
            {
                args.ShiftNextLines = TabLength;
                return;
            }
            //remove TAB before end folding marker
            if (!string.IsNullOrEmpty(lines[args.iLine].FoldingEndMarker) &&
                string.IsNullOrEmpty(lines[args.iLine].FoldingStartMarker))
            {
                args.Shift = -TabLength;
                args.ShiftNextLines = -TabLength;
                return;
            }
        }


        private int GetMinStartSpacesCount(int fromLine, int toLine)
        {
            if (fromLine > toLine)
                return 0;

            int result = int.MaxValue;
            for (int i = fromLine; i <= toLine; i++)
            {
                int count = lines[i].StartSpacesCount;
                if (count < result)
                    result = count;
            }

            return result;
        }

        /// <summary>
        /// Undo last operation
        /// </summary>
        public void Undo()
        {
            lines.Manager.Undo();
            Invalidate();
        }

        /// <summary>
        /// Redo
        /// </summary>
        public void Redo()
        {
            lines.Manager.Redo();
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Tab && !AcceptsTab)
                return false;
            if (keyData == Keys.Enter && !AcceptsReturn)
                return false;

            if ((keyData & Keys.Alt) == Keys.None)
            {
                Keys keys = keyData & Keys.KeyCode;
                if (keys == Keys.Return)
                    return true;
            }

            if ((keyData & Keys.Alt) != Keys.Alt)
            {
                switch ((keyData & Keys.KeyCode))
                {
                    case Keys.Prior:
                    case Keys.Next:
                    case Keys.End:
                    case Keys.Home:
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Up:
                    case Keys.Down:
                        return true;

                    case Keys.Escape:
                        return false;

                    case Keys.Tab:
                        return (keyData & Keys.Control) == Keys.None;
                }
            }

            return base.IsInputKey(keyData);
        }

        [DllImport("User32.dll")]
        private static extern bool CreateCaret(IntPtr hWnd, int hBitmap, int nWidth, int nHeight);

        [DllImport("User32.dll")]
        private static extern bool SetCaretPos(int x, int y);

        [DllImport("User32.dll")]
        private static extern bool DestroyCaret();

        [DllImport("User32.dll")]
        private static extern bool ShowCaret(IntPtr hWnd);

        [DllImport("User32.dll")]
        private static extern bool HideCaret(IntPtr hWnd);

        /// <summary>
        /// Draw control
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (needRecalc)
                Recalc();
#if debug
            var sw = Stopwatch.StartNew();
#endif
            visibleMarkers.Clear();
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            //
            Brush lineNumberBrush = new SolidBrush(LineNumberColor);
            var servicePen = new Pen(ServiceLinesColor);
            Brush changedLineBrush = new SolidBrush(ChangedLineColor);
            Brush indentBrush = new SolidBrush(IndentBackColor);
            var currentLinePen = new Pen(CurrentLineColor);
            Brush currentLineBrush = new SolidBrush(Color.FromArgb(50, CurrentLineColor));
            //draw indent area
            e.Graphics.FillRectangle(indentBrush, 0, 0, LeftIndentLine, ClientSize.Height);
            if (LeftIndent > minLeftIndent)
                e.Graphics.DrawLine(servicePen, LeftIndentLine, 0, LeftIndentLine, ClientSize.Height);
            //draw preferred line width
            if (PreferredLineWidth > 0)
                e.Graphics.DrawLine(servicePen,
                                    new Point(LeftIndent + PreferredLineWidth*CharWidth - HorizontalScroll.Value, 0),
                                    new Point(LeftIndent + PreferredLineWidth*CharWidth - HorizontalScroll.Value, Height));
            //
            int firstChar = HorizontalScroll.Value/CharWidth;
            int lastChar = (HorizontalScroll.Value + ClientSize.Width)/CharWidth;
            //draw chars
            for (int iLine = YtoLineIndex(VerticalScroll.Value); iLine < lines.Count; iLine++)
            {
                Line line = lines[iLine];
                LineInfo lineInfo = lineInfos[iLine];
                //
                if (lineInfo.startY > VerticalScroll.Value + ClientSize.Height)
                    break;
                if (lineInfo.startY + lineInfo.WordWrapStringsCount*CharHeight < VerticalScroll.Value)
                    continue;
                if (lineInfo.VisibleState == VisibleState.Hidden)
                    continue;

                int y = lineInfo.startY - VerticalScroll.Value;
                //
                e.Graphics.SmoothingMode = SmoothingMode.None;
                //draw line background
                if (lineInfo.VisibleState == VisibleState.Visible)
                    if (line.BackgroundBrush != null)
                        e.Graphics.FillRectangle(line.BackgroundBrush,
                                                 new Rectangle(LeftIndent, y, Width,
                                                               CharHeight*lineInfo.WordWrapStringsCount));
                //draw current line background
                if (CurrentLineColor != Color.Transparent && iLine == Selection.Start.iLine)
                    if (Selection.Start == Selection.End)
                        e.Graphics.FillRectangle(currentLineBrush, new Rectangle(LeftIndent, y, Width, CharHeight));
                    else
                        e.Graphics.DrawLine(currentLinePen, LeftIndent, y + CharHeight, Width, y + CharHeight);
                //draw changed line marker
                if (ChangedLineColor != Color.Transparent && line.IsChanged)
                    e.Graphics.FillRectangle(changedLineBrush,
                                             new RectangleF(-10, y, LeftIndent - minLeftIndent - 2 + 10, CharHeight + 1));
                //
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                //OnPaint event
                if (lineInfo.VisibleState == VisibleState.Visible)
                    OnPaintLine(new PaintLineEventArgs(iLine,
                                                       new Rectangle(LeftIndent, y, Width,
                                                                     CharHeight*lineInfo.WordWrapStringsCount),
                                                       e.Graphics, e.ClipRectangle));
                //draw line number
                if (ShowLineNumbers)
                    e.Graphics.DrawString((iLine + lineNumberStartValue).ToString(), Font, lineNumberBrush,
                                          new RectangleF(-10, y, LeftIndent - minLeftIndent - 2 + 10, CharHeight),
                                          new StringFormat(StringFormatFlags.DirectionRightToLeft));
                //create markers
                if (lineInfo.VisibleState == VisibleState.StartOfHiddenBlock)
                    visibleMarkers.Add(new ExpandFoldingMarker(iLine,
                                                               new Rectangle(LeftIndentLine - 4, y + CharHeight/2 - 3, 8,
                                                                             8)));
                if (!string.IsNullOrEmpty(line.FoldingStartMarker) && lineInfo.VisibleState == VisibleState.Visible &&
                    string.IsNullOrEmpty(line.FoldingEndMarker))
                    visibleMarkers.Add(new CollapseFoldingMarker(iLine,
                                                                 new Rectangle(LeftIndentLine - 4, y + CharHeight/2 - 3,
                                                                               8, 8)));
                if (lineInfo.VisibleState == VisibleState.Visible && !string.IsNullOrEmpty(line.FoldingEndMarker) &&
                    string.IsNullOrEmpty(line.FoldingStartMarker))
                    e.Graphics.DrawLine(servicePen, LeftIndentLine, y + CharHeight*lineInfo.WordWrapStringsCount - 1,
                                        LeftIndentLine + 4, y + CharHeight*lineInfo.WordWrapStringsCount - 1);
                //draw wordwrap strings of line
                for (int iWordWrapLine = 0; iWordWrapLine < lineInfo.WordWrapStringsCount; iWordWrapLine++)
                {
                    y = lineInfo.startY + iWordWrapLine*CharHeight - VerticalScroll.Value;
                    //draw chars
                    DrawLineChars(e, firstChar, lastChar, iLine, iWordWrapLine, y);
                }
            }
            //draw brackets highlighting
            if (BracketsStyle != null && leftBracketPosition != null && rightBracketPosition != null)
            {
                BracketsStyle.Draw(e.Graphics, PlaceToPoint(leftBracketPosition.Start), leftBracketPosition);
                BracketsStyle.Draw(e.Graphics, PlaceToPoint(rightBracketPosition.Start), rightBracketPosition);
            }
            if (BracketsStyle2 != null && leftBracketPosition2 != null && rightBracketPosition2 != null)
            {
                BracketsStyle2.Draw(e.Graphics, PlaceToPoint(leftBracketPosition2.Start), leftBracketPosition2);
                BracketsStyle2.Draw(e.Graphics, PlaceToPoint(rightBracketPosition2.Start), rightBracketPosition2);
            }
            //
            e.Graphics.SmoothingMode = SmoothingMode.None;
            //draw folding indicator
            if ((startFoldingLine >= 0 || endFoldingLine >= 0) && Selection.Start == Selection.End)
                if (endFoldingLine < lineInfos.Count)
                {
                    //folding indicator
                    int startFoldingY = (startFoldingLine >= 0 ? lineInfos[startFoldingLine].startY : 0) -
                                        VerticalScroll.Value + CharHeight/2;
                    int endFoldingY = (endFoldingLine >= 0
                                           ? lineInfos[endFoldingLine].startY +
                                             (lineInfos[endFoldingLine].WordWrapStringsCount - 1)*CharHeight
                                           : (WordWrapLinesCount + 1)*CharHeight) - VerticalScroll.Value + CharHeight;

                    using (var indicatorPen = new Pen(Color.FromArgb(100, FoldingIndicatorColor), 4))
                        e.Graphics.DrawLine(indicatorPen, LeftIndent - 5, startFoldingY, LeftIndent - 5, endFoldingY);
                }
            //draw markers
            foreach (VisualMarker m in visibleMarkers)
                m.Draw(e.Graphics, servicePen);
            //draw caret
            Point car = PlaceToPoint(Selection.Start);

            if (Focused && car.X >= LeftIndent)
            {
                int carWidth = IsReplaceMode ? CharWidth : 1;
                CreateCaret(Handle, 0, carWidth, CharHeight + 1);
                SetCaretPos(car.X, car.Y);
                ShowCaret(Handle);
                e.Graphics.DrawLine(Pens.Black, car.X, car.Y, car.X, car.Y + CharHeight);
            }
            else
                HideCaret(Handle);

            //dispose resources
            lineNumberBrush.Dispose();
            servicePen.Dispose();
            changedLineBrush.Dispose();
            indentBrush.Dispose();
            currentLinePen.Dispose();
            currentLineBrush.Dispose();
            //
#if debug
            Console.WriteLine("OnPaint: "+ sw.ElapsedMilliseconds);
#endif
            //
            base.OnPaint(e);
        }

        private void DrawLineChars(PaintEventArgs e, int firstChar, int lastChar, int iLine, int iWordWrapLine, int y)
        {
            Line line = lines[iLine];
            LineInfo lineInfo = lineInfos[iLine];
            int from = lineInfo.GetWordWrapStringStartPosition(iWordWrapLine);
            int to = lineInfo.GetWordWrapStringFinishPosition(iWordWrapLine, line);

            int startX = LeftIndent - HorizontalScroll.Value;
            if (startX < LeftIndent)
                firstChar++;

            lastChar = Math.Min(to - from, lastChar);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            //folded block ?
            if (lineInfo.VisibleState == VisibleState.StartOfHiddenBlock)
            {
                //rendering by FoldedBlockStyle
                FoldedBlockStyle.Draw(e.Graphics, new Point(startX + firstChar*CharWidth, y),
                                      new Range(this, from + firstChar, iLine, from + lastChar + 1, iLine));
            }
            else
            {
                //render by custom styles
                StyleIndex currentStyleIndex = StyleIndex.None;
                int iLastFlushedChar = firstChar - 1;

                for (int iChar = firstChar; iChar <= lastChar; iChar++)
                {
                    StyleIndex style = line[from + iChar].style;
                    if (currentStyleIndex != style)
                    {
                        FlushRendering(e.Graphics, currentStyleIndex,
                                       new Point(startX + (iLastFlushedChar + 1)*CharWidth, y),
                                       new Range(this, from + iLastFlushedChar + 1, iLine, from + iChar, iLine));
                        iLastFlushedChar = iChar - 1;
                        currentStyleIndex = style;
                    }
                }
                FlushRendering(e.Graphics, currentStyleIndex, new Point(startX + (iLastFlushedChar + 1)*CharWidth, y),
                               new Range(this, from + iLastFlushedChar + 1, iLine, from + lastChar + 1, iLine));
            }

            //draw selection
            if (Selection.End != Selection.Start && lastChar >= firstChar)
            {
                e.Graphics.SmoothingMode = SmoothingMode.None;
                var textRange = new Range(this, from + firstChar, iLine, from + lastChar + 1, iLine);
                textRange = Selection.GetIntersectionWith(textRange);
                if (textRange != null && SelectionStyle != null)
                    SelectionStyle.Draw(e.Graphics, new Point(startX + (textRange.Start.iChar - from)*CharWidth, y),
                                        textRange);
            }
        }

        private void FlushRendering(Graphics gr, StyleIndex styleIndex, Point pos, Range range)
        {
            if (range.End > range.Start)
            {
                int mask = 1;
                bool hasTextStyle = false;
                for (int i = 0; i < Styles.Length; i++)
                {
                    if (Styles[i] != null && ((int) styleIndex & mask) != 0)
                    {
                        Style style = Styles[i];
                        bool isTextStyle = style is TextStyle;
                        if (!hasTextStyle || !isTextStyle || AllowSeveralTextStyleDrawing)
                            //cancelling secondary rendering by TextStyle
                            style.Draw(gr, pos, range); //rendering
                        hasTextStyle |= isTextStyle;
                    }
                    mask = mask << 1;
                }
                //draw by default renderer
                if (!hasTextStyle)
                    DefaultStyle.Draw(gr, pos, range);
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            mouseIsDrag = false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                VisualMarker marker = FindVisualMarkerForPoint(e.Location);
                //click on marker
                if (marker != null)
                {
                    mouseIsDrag = false;
                    OnMarkerClick(e, marker);
                    return;
                }
                mouseIsDrag = true;
                //click on text
                Place oldEnd = Selection.End;
                Selection.BeginUpdate();
                Selection.Start = PointToPlace(e.Location);
                if ((lastModifiers & Keys.Shift) != 0)
                    Selection.End = oldEnd;
                Selection.EndUpdate();
                Invalidate();
                return;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Invalidate();
            base.OnMouseWheel(e);
            OnVisibleRangeChanged();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left && mouseIsDrag)
            {
                Place oldEnd = Selection.End;
                Selection.BeginUpdate();
                Selection.Start = PointToPlace(e.Location);
                Selection.End = oldEnd;
                Selection.EndUpdate();
                DoCaretVisible();
                Invalidate();
                return;
            }

            VisualMarker marker = FindVisualMarkerForPoint(e.Location);
            if (marker != null)
                Cursor = marker.Cursor;
            else
                Cursor = Cursors.IBeam;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            VisualMarker m = FindVisualMarkerForPoint(e.Location);
            if (m != null)
            {
                OnMarkerDoubleClick(m);
                return;
            }

            Place p = PointToPlace(e.Location);
            int fromX = p.iChar;
            int toX = p.iChar;

            for (int i = p.iChar; i < lines[p.iLine].Count; i++)
            {
                char c = lines[p.iLine][i].c;
                if (char.IsLetterOrDigit(c) || c == '_')
                    toX = i + 1;
                else
                    break;
            }

            for (int i = p.iChar - 1; i >= 0; i--)
            {
                char c = lines[p.iLine][i].c;
                if (char.IsLetterOrDigit(c) || c == '_')
                    fromX = i;
                else
                    break;
            }

            Selection.Start = new Place(toX, p.iLine);
            Selection.End = new Place(fromX, p.iLine);

            Invalidate();
        }

        private int YtoLineIndex(int y)
        {
            int i = lineInfos.BinarySearch(new LineInfo(-10), new LineYComparer(y));
            i = i < 0 ? -i - 2 : i;
            if (i < 0) return 0;
            if (i > lines.Count - 1) return lines.Count - 1;
            return i;
        }

        /// <summary>
        /// Gets nearest line and char position from coordinates
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Line and char position</returns>
        public Place PointToPlace(Point point)
        {
#if debug
            var sw = Stopwatch.StartNew();
            #endif
            point.Offset(HorizontalScroll.Value, VerticalScroll.Value);
            point.Offset(-LeftIndent, 0);
            int iLine = YtoLineIndex(point.Y);
            int y = 0;

            for (; iLine < lines.Count; iLine++)
            {
                y = lineInfos[iLine].startY + lineInfos[iLine].WordWrapStringsCount*CharHeight;
                if (y > point.Y && lineInfos[iLine].VisibleState == VisibleState.Visible)
                    break;
            }
            if (iLine >= lines.Count)
                iLine = lines.Count - 1;
            if (lineInfos[iLine].VisibleState != VisibleState.Visible)
                iLine = FindPrevVisibleLine(iLine);
            //
            int iWordWrapLine = lineInfos[iLine].WordWrapStringsCount;
            do
            {
                iWordWrapLine--;
                y -= CharHeight;
            } while (y > point.Y);
            if (iWordWrapLine < 0) iWordWrapLine = 0;
            //
            int start = lineInfos[iLine].GetWordWrapStringStartPosition(iWordWrapLine);
            int finish = lineInfos[iLine].GetWordWrapStringFinishPosition(iWordWrapLine, lines[iLine]);
            var x = (int) Math.Round((float) point.X/CharWidth);
            x = x < 0 ? start : start + x;
            if (x > finish)
                x = finish + 1;
            if (x > lines[iLine].Count)
                x = lines[iLine].Count;

#if debug
            Console.WriteLine("PointToPlace: " + sw.ElapsedMilliseconds);
            #endif

            return new Place(x, iLine);
        }

        /// <summary>
        /// Gets nearest absolute text position for given point
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Position</returns>
        public int PointToPosition(Point point)
        {
            return PlaceToPosition(PointToPlace(point));
        }

        /// <summary>
        /// Fires TextChanging event
        /// </summary>
        public virtual void OnTextChanging(ref string text)
        {
            ClearBracketsPositions();

            if (TextChanging != null)
            {
                var args = new TextChangingEventArgs {InsertingText = text};
                TextChanging(this, args);
                text = args.InsertingText;
            }
            ;
        }

        public virtual void OnTextChanging()
        {
            string temp = null;
            OnTextChanging(ref temp);
        }

        /// <summary>
        /// Fires TextChanged event
        /// </summary>
        public virtual void OnTextChanged()
        {
            var r = new Range(this);
            r.SelectAll();
            OnTextChanged(new TextChangedEventArgs(r));
        }

        /// <summary>
        /// Fires TextChanged event
        /// </summary>
        public virtual void OnTextChanged(int fromLine, int toLine)
        {
            var r = new Range(this);
            r.Start = new Place(0, Math.Min(fromLine, toLine));
            r.End = new Place(lines[Math.Max(fromLine, toLine)].Count, Math.Max(fromLine, toLine));
            OnTextChanged(new TextChangedEventArgs(r));
        }

        /// <summary>
        /// Fires TextChanged event
        /// </summary>
        public virtual void OnTextChanged(Range r)
        {
            OnTextChanged(new TextChangedEventArgs(r));
        }

        public void BeginUpdate()
        {
            if (updating == 0)
                updatingRange = null;
            updating++;
        }

        public void EndUpdate()
        {
            updating--;

            if (updating == 0 && updatingRange != null)
            {
                updatingRange.Expand();
                OnTextChanged(updatingRange);
            }
        }


        /// <summary>
        /// Fires TextChanged event
        /// </summary>
        protected virtual void OnTextChanged(TextChangedEventArgs args)
        {
            //
            args.ChangedRange.Normalize();
            //
            if (updating > 0)
            {
                if (updatingRange == null)
                    updatingRange = args.ChangedRange.Clone();
                else
                {
                    if (updatingRange.Start.iLine > args.ChangedRange.Start.iLine)
                        updatingRange.Start = new Place(0, args.ChangedRange.Start.iLine);
                    if (updatingRange.End.iLine < args.ChangedRange.End.iLine)
                        updatingRange.End = new Place(lines[args.ChangedRange.End.iLine].Count,
                                                      args.ChangedRange.End.iLine);
                    updatingRange = updatingRange.GetIntersectionWith(Range);
                }
                return;
            }
            //
#if debug
            var sw = Stopwatch.StartNew();
            #endif
            IsChanged = true;
            TextVersion++;
            MarkLinesAsChanged(args.ChangedRange);
            //
            if (wordWrap)
                RecalcWordWrap(args.ChangedRange.Start.iLine, args.ChangedRange.End.iLine);
            //
            base.OnTextChanged(args);

            //dalayed event stuffs
            if (delayedTextChangedRange == null)
                delayedTextChangedRange = args.ChangedRange.Clone();
            else
                delayedTextChangedRange = delayedTextChangedRange.GetUnionWith(args.ChangedRange);

            needRiseTextChangedDelayed = true;
            ResetTimer(timer2);
            //
            OnSyntaxHighlight(args);
            //
            if (TextChanged != null)
                TextChanged(this, args);
            //
            if (BindingTextChanged != null)
                BindingTextChanged(this, EventArgs.Empty);
            //
            base.OnTextChanged(EventArgs.Empty);
            //
#if debug
            Console.WriteLine("OnTextChanged: " + sw.ElapsedMilliseconds);
            #endif

            OnVisibleRangeChanged();
        }

        private void MarkLinesAsChanged(Range range)
        {
            for (int iLine = range.Start.iLine; iLine <= range.End.iLine; iLine++)
                if (iLine >= 0 && iLine < lines.Count)
                    lines[iLine].IsChanged = true;
        }

        /// <summary>
        /// Fires SelectionCnaged event
        /// </summary>
        public virtual void OnSelectionChanged()
        {
#if debug
            var sw = Stopwatch.StartNew();
            #endif
            //find folding markers for highlighting
            if (HighlightFoldingIndicator)
                HighlightFoldings();
            //
            needRiseSelectionChangedDelayed = true;
            ResetTimer(timer);

            if (SelectionChanged != null)
                SelectionChanged(this, new EventArgs());

#if debug
            Console.WriteLine("OnSelectionChanged: "+ sw.ElapsedMilliseconds);
            #endif
        }

        //find folding markers for highlighting
        private void HighlightFoldings()
        {
            int prevStartFoldingLine = startFoldingLine;
            int prevEndFoldingLine = endFoldingLine;
            //
            startFoldingLine = -1;
            endFoldingLine = -1;
            const int maxLines = 2000;
            //
            string marker = null;
            int counter = 0;
            for (int i = Selection.Start.iLine; i >= Math.Max(Selection.Start.iLine - maxLines, 0); i--)
            {
                bool hasStartMarker = lines.LineHasFoldingStartMarker(i);
                bool hasEndMarker = lines.LineHasFoldingEndMarker(i);

                if (hasEndMarker && hasStartMarker)
                    continue;

                if (hasStartMarker)
                {
                    counter--;
                    if (counter == -1) //found start folding
                    {
                        startFoldingLine = i;
                        marker = lines[i].FoldingStartMarker;
                        break;
                    }
                }
                if (hasEndMarker && i != Selection.Start.iLine)
                    counter++;
            }
            if (startFoldingLine >= 0)
            {
                //find end of block
                endFoldingLine = FindEndOfFoldingBlock(startFoldingLine);
                if (endFoldingLine == startFoldingLine)
                    endFoldingLine = -1;
            }

            if (startFoldingLine != prevStartFoldingLine || endFoldingLine != prevEndFoldingLine)
                OnFoldingHighlightChanged();
        }

        protected virtual void OnFoldingHighlightChanged()
        {
            if (FoldingHighlightChanged != null)
                FoldingHighlightChanged(this, EventArgs.Empty);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            SetAsCurrentTB();
            base.OnGotFocus(e);
            //Invalidate(new Rectangle(PlaceToPoint(Selection.Start), new Size(2, CharHeight+1)));
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            //Invalidate(new Rectangle(PlaceToPoint(Selection.Start), new Size(2, CharHeight+1)));
            Invalidate();
        }

        /// <summary>
        /// Gets absolute text position from line and char position
        /// </summary>
        /// <param name="point">Line and char position</param>
        /// <returns>Index of text char</returns>
        public int PlaceToPosition(Place point)
        {
            if (point.iLine < 0 || point.iLine >= lines.Count ||
                point.iChar >= lines[point.iLine].Count + Environment.NewLine.Length)
                return -1;

            int result = 0;
            for (int i = 0; i < point.iLine; i++)
                result += lines[i].Count + Environment.NewLine.Length;
            result += point.iChar;

            return result;
        }

        /// <summary>
        /// Gets line and char position from absolute text position
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Place PositionToPlace(int pos)
        {
            if (pos < 0)
                return new Place(0, 0);

            for (int i = 0; i < lines.Count; i++)
            {
                int lineLength = lines[i].Count + Environment.NewLine.Length;
                if (pos < lines[i].Count)
                    return new Place(pos, i);
                if (pos < lineLength)
                    return new Place(lines[i].Count, i);

                pos -= lineLength;
            }

            if (lines.Count > 0)
                return new Place(lines[lines.Count - 1].Count, lines.Count - 1);
            else
                return new Place(0, 0);
            //throw new ArgumentOutOfRangeException("Position out of range");
        }

        /// <summary>
        /// Gets point for given line and char position
        /// </summary>
        /// <param name="palce">Line and char position</param>
        /// <returns>Coordiantes</returns>
        public Point PlaceToPoint(Place place)
        {
            if (place.iLine >= lineInfos.Count)
                return new Point();
            int y = lineInfos[place.iLine].startY;
            //
            int iWordWrapIndex = lineInfos[place.iLine].GetWordWrapStringIndex(place.iChar);
            y += iWordWrapIndex*CharHeight;
            int x = (place.iChar - lineInfos[place.iLine].GetWordWrapStringStartPosition(iWordWrapIndex))*CharWidth;
            //
            y = y - VerticalScroll.Value;
            x = LeftIndent + x - HorizontalScroll.Value;

            return new Point(x, y);
        }

        /// <summary>
        /// Get range of text
        /// </summary>
        /// <param name="fromPos">Absolute start position</param>
        /// <param name="toPos">Absolute finish position</param>
        /// <returns>Range</returns>
        public Range GetRange(int fromPos, int toPos)
        {
            var sel = new Range(this);
            sel.Start = PositionToPlace(fromPos);
            sel.End = PositionToPlace(toPos);
            return sel;
        }

        /// <summary>
        /// Get range of text
        /// </summary>
        /// <param name="fromPlace">Line and char position</param>
        /// <param name="toPlace">Line and char position</param>
        /// <returns>Range</returns>
        public Range GetRange(Place fromPlace, Place toPlace)
        {
            return new Range(this, fromPlace, toPlace);
        }

        /// <summary>
        /// Finds ranges for given regex pattern
        /// </summary>
        /// <param name="regexPattern">Regex pattern</param>
        /// <returns>Enumeration of ranges</returns>
        public IEnumerable<Range> GetRanges(string regexPattern)
        {
            var range = new Range(this);
            range.SelectAll();
            //
            foreach (Range r in range.GetRanges(regexPattern, RegexOptions.None))
                yield return r;
        }

        /// <summary>
        /// Finds ranges for given regex pattern
        /// </summary>
        /// <param name="regexPattern">Regex pattern</param>
        /// <returns>Enumeration of ranges</returns>
        public IEnumerable<Range> GetRanges(string regexPattern, RegexOptions options)
        {
            var range = new Range(this);
            range.SelectAll();
            //
            foreach (Range r in range.GetRanges(regexPattern, options))
                yield return r;
        }

        /// <summary>
        /// Get text of given line
        /// </summary>
        /// <param name="iLine">Line index</param>
        /// <returns>Text</returns>
        public string GetLineText(int iLine)
        {
            if (iLine < 0 || iLine >= lines.Count)
                throw new ArgumentOutOfRangeException("Line index out of range");
            var sb = new StringBuilder(lines[iLine].Count);
            foreach (Char c in lines[iLine])
                sb.Append(c.c);
            return sb.ToString();
        }

        /// <summary>
        /// Exapnds folded block
        /// </summary>
        /// <param name="iLine"></param>
        public void ExpandFoldedBlock(int iLine)
        {
            if (iLine < 0 || iLine >= lines.Count)
                throw new ArgumentOutOfRangeException("Line index out of range");
            //find all hidden lines afetr iLine
            int end = iLine;
            for (; end < LinesCount - 1; end++)
            {
                if (lineInfos[end + 1].VisibleState != VisibleState.Hidden)
                    break;
            }
            ;

            ExpandBlock(iLine, end);
        }

        /// <summary>
        /// Expand collapsed block
        /// </summary>
        public void ExpandBlock(int fromLine, int toLine)
        {
            int from = Math.Min(fromLine, toLine);
            int to = Math.Max(fromLine, toLine);
            for (int i = from; i <= to; i++)
                SetVisibleState(i, VisibleState.Visible);
            needRecalc = true;
            Invalidate();
        }

        /// <summary>
        /// Expand collapsed block
        /// </summary>
        /// <param name="iLine">Any line inside collapsed block</param>
        public void ExpandBlock(int iLine)
        {
            if (lineInfos[iLine].VisibleState == VisibleState.Visible)
                return;

            for (int i = iLine; i < LinesCount; i++)
                if (lineInfos[i].VisibleState == VisibleState.Visible)
                    break;
                else
                {
                    SetVisibleState(i, VisibleState.Visible);
                    needRecalc = true;
                }

            for (int i = iLine - 1; i >= 0; i--)
                if (lineInfos[i].VisibleState == VisibleState.Visible)
                    break;
                else
                {
                    SetVisibleState(i, VisibleState.Visible);
                    needRecalc = true;
                }

            Invalidate();
        }

        /// <summary>
        /// Collapses all folding blocks
        /// </summary>
        public void CollapseAllFoldingBlocks()
        {
            for (int i = 0; i < LinesCount; i++)
                if (lines.LineHasFoldingStartMarker(i))
                {
                    int iFinish = FindEndOfFoldingBlock(i);
                    if (iFinish >= 0)
                    {
                        CollapseBlock(i, iFinish);
                        i = iFinish;
                    }
                }

            OnVisibleRangeChanged();
        }

        /// <summary>
        /// Exapnds all folded blocks
        /// </summary>
        /// <param name="iLine"></param>
        public void ExpandAllFoldingBlocks()
        {
            for (int i = 0; i < LinesCount; i++)
                SetVisibleState(i, VisibleState.Visible);

            OnVisibleRangeChanged();
            Invalidate();
        }

        /// <summary>
        /// Collapses folding block
        /// </summary>
        /// <param name="iLine">Start folding line</param>
        public void CollapseFoldingBlock(int iLine)
        {
            if (iLine < 0 || iLine >= lines.Count)
                throw new ArgumentOutOfRangeException("Line index out of range");
            if (string.IsNullOrEmpty(lines[iLine].FoldingStartMarker))
                throw new ArgumentOutOfRangeException("This line is not folding start line");
            //find end of block
            int i = FindEndOfFoldingBlock(iLine);
            //collapse
            if (i >= 0)
                CollapseBlock(iLine, i);
        }

        private int FindEndOfFoldingBlock(int iStartLine)
        {
            int margin = 2000;
            //find end of block
            int counter = 0;
            int i;
            string marker = lines[iStartLine].FoldingStartMarker;

            for (i = iStartLine /*+1*/; i < LinesCount; i++)
            {
                if (lines.LineHasFoldingStartMarker(i) &&
                    lines[i].FoldingStartMarker == marker)
                    counter++;
                if (lines.LineHasFoldingEndMarker(i) &&
                    lines[i].FoldingEndMarker == marker)
                {
                    counter--;
                    if (counter <= 0)
                        return i;
                }
                margin--;
                if (margin < 0)
                    break;
            }

            return -1;
        }

        /// <summary>
        /// Collapse text block
        /// </summary>
        public void CollapseBlock(int fromLine, int toLine)
        {
            int from = Math.Min(fromLine, toLine);
            int to = Math.Max(fromLine, toLine);
            if (from == to)
                return;

            //find first non empty line
            for (; from <= to; from++)
            {
                if (GetLineText(from).Trim().Length > 0)
                {
                    //hide lines
                    for (int i = from + 1; i <= to; i++)
                        SetVisibleState(i, VisibleState.Hidden);
                    SetVisibleState(from, VisibleState.StartOfHiddenBlock);
                    Invalidate();
                    break;
                }
            }
            //Move caret outside
            from = Math.Min(fromLine, toLine);
            to = Math.Max(fromLine, toLine);
            int newLine = FindNextVisibleLine(to);
            if (newLine == to)
                newLine = FindPrevVisibleLine(from);
            Selection.Start = new Place(0, newLine);
            //
            needRecalc = true;
            Invalidate();
        }


        internal int FindNextVisibleLine(int iLine)
        {
            if (iLine >= lines.Count - 1) return iLine;
            int old = iLine;
            do
                iLine++; while (iLine < lines.Count - 1 && lineInfos[iLine].VisibleState != VisibleState.Visible);

            if (lineInfos[iLine].VisibleState != VisibleState.Visible)
                return old;
            else
                return iLine;
        }


        internal int FindPrevVisibleLine(int iLine)
        {
            if (iLine <= 0) return iLine;
            int old = iLine;
            do
                iLine--; while (iLine > 0 && lineInfos[iLine].VisibleState != VisibleState.Visible);

            if (lineInfos[iLine].VisibleState != VisibleState.Visible)
                return old;
            else
                return iLine;
        }

        private VisualMarker FindVisualMarkerForPoint(Point p)
        {
            foreach (VisualMarker m in visibleMarkers)
                if (m.rectangle.Contains(p))
                    return m;
            return null;
        }

        /// <summary>
        /// Insert TAB into front of seletcted lines
        /// </summary>
        public void IncreaseIndent()
        {
            if (Selection.Start == Selection.End)
                return;
            Range old = Selection.Clone();
            int from = Math.Min(Selection.Start.iLine, Selection.End.iLine);
            int to = Math.Max(Selection.Start.iLine, Selection.End.iLine);
            BeginUpdate();
            Selection.BeginUpdate();
            lines.Manager.BeginAutoUndoCommands();
            for (int i = from; i <= to; i++)
            {
                if (lines[i].Count == 0) continue;
                Selection.Start = new Place(0, i);
                lines.Manager.ExecuteCommand(new InsertTextCommand(TextSource, new String(' ', TabLength)));
            }
            lines.Manager.EndAutoUndoCommands();
            Selection.Start = new Place(0, from);
            Selection.End = new Place(lines[to].Count, to);
            needRecalc = true;
            Selection.EndUpdate();
            EndUpdate();
            Invalidate();
        }

        /// <summary>
        /// Remove TAB from front of seletcted lines
        /// </summary>
        public void DecreaseIndent()
        {
            if (Selection.Start == Selection.End)
                return;
            Range old = Selection.Clone();
            int from = Math.Min(Selection.Start.iLine, Selection.End.iLine);
            int to = Math.Max(Selection.Start.iLine, Selection.End.iLine);
            BeginUpdate();
            Selection.BeginUpdate();
            lines.Manager.BeginAutoUndoCommands();
            for (int i = from; i <= to; i++)
            {
                Selection.Start = new Place(0, i);
                Selection.End = new Place(Math.Min(lines[i].Count, TabLength), i);
                if (Selection.Text.Trim() == "")
                    ClearSelected();
            }
            lines.Manager.EndAutoUndoCommands();
            Selection.Start = new Place(0, from);
            Selection.End = new Place(lines[to].Count, to);
            needRecalc = true;
            EndUpdate();
            Selection.EndUpdate();
        }

        /// <summary>
        /// Insert autoindents into selected lines
        /// </summary>
        public void DoAutoIndent()
        {
            Range r = Selection.Clone();
            r.Normalize();
            //
            BeginUpdate();
            Selection.BeginUpdate();
            lines.Manager.BeginAutoUndoCommands();
            //
            for (int i = r.Start.iLine; i <= r.End.iLine; i++)
                DoAutoIndent(i);
            //
            lines.Manager.EndAutoUndoCommands();
            Selection.Start = r.Start;
            Selection.End = r.End;
            Selection.Expand();
            //
            Selection.EndUpdate();
            EndUpdate();
        }

        /// <summary>
        /// Insert prefix into front of seletcted lines
        /// </summary>
        public void InsertLinePrefix(string prefix)
        {
            Range old = Selection.Clone();
            int from = Math.Min(Selection.Start.iLine, Selection.End.iLine);
            int to = Math.Max(Selection.Start.iLine, Selection.End.iLine);
            BeginUpdate();
            Selection.BeginUpdate();
            lines.Manager.BeginAutoUndoCommands();
            int spaces = GetMinStartSpacesCount(from, to);
            for (int i = from; i <= to; i++)
            {
                Selection.Start = new Place(spaces, i);
                lines.Manager.ExecuteCommand(new InsertTextCommand(TextSource, prefix));
            }
            Selection.Start = new Place(0, from);
            Selection.End = new Place(lines[to].Count, to);
            needRecalc = true;
            lines.Manager.EndAutoUndoCommands();
            Selection.EndUpdate();
            EndUpdate();
            Invalidate();
        }

        /// <summary>
        /// Remove prefix from front of seletcted lines
        /// </summary>
        public void RemoveLinePrefix(string prefix)
        {
            Range old = Selection.Clone();
            int from = Math.Min(Selection.Start.iLine, Selection.End.iLine);
            int to = Math.Max(Selection.Start.iLine, Selection.End.iLine);
            BeginUpdate();
            Selection.BeginUpdate();
            lines.Manager.BeginAutoUndoCommands();
            for (int i = from; i <= to; i++)
            {
                string text = lines[i].Text;
                string trimmedText = text.TrimStart();
                if (trimmedText.StartsWith(prefix))
                {
                    int spaces = text.Length - trimmedText.Length;
                    Selection.Start = new Place(spaces, i);
                    Selection.End = new Place(spaces + prefix.Length, i);
                    ClearSelected();
                }
            }
            Selection.Start = new Place(0, from);
            Selection.End = new Place(lines[to].Count, to);
            needRecalc = true;
            lines.Manager.EndAutoUndoCommands();
            Selection.EndUpdate();
            EndUpdate();
        }

        /// <summary>
        /// Begins AutoUndo block.
        /// All changes of text between BeginAutoUndo() and EndAutoUndo() will be canceled in one operation Undo.
        /// </summary>
        public void BeginAutoUndo()
        {
            lines.Manager.BeginAutoUndoCommands();
        }

        /// <summary>
        /// Ends AutoUndo block.
        /// All changes of text between BeginAutoUndo() and EndAutoUndo() will be canceled in one operation Undo.
        /// </summary>
        public void EndAutoUndo()
        {
            lines.Manager.EndAutoUndoCommands();
        }

        public virtual void OnVisualMarkerClick(MouseEventArgs args, StyleVisualMarker marker)
        {
            if (VisualMarkerClick != null)
                VisualMarkerClick(this, new VisualMarkerEventArgs(marker.Style, marker, args));
        }

        protected virtual void OnMarkerClick(MouseEventArgs args, VisualMarker marker)
        {
            if (marker is StyleVisualMarker)
            {
                OnVisualMarkerClick(args, marker as StyleVisualMarker);
                return;
            }
            if (marker is CollapseFoldingMarker)
            {
                CollapseFoldingBlock((marker as CollapseFoldingMarker).iLine);
                OnVisibleRangeChanged();
                Invalidate();
                return;
            }

            if (marker is ExpandFoldingMarker)
            {
                ExpandFoldedBlock((marker as ExpandFoldingMarker).iLine);
                OnVisibleRangeChanged();
                Invalidate();
                return;
            }

            if (marker is FoldedAreaMarker)
            {
                //select folded block
                int iStart = (marker as FoldedAreaMarker).iLine;
                int iEnd = FindEndOfFoldingBlock(iStart);
                if (iEnd < 0)
                    return;
                Selection.BeginUpdate();
                Selection.Start = new Place(0, iStart);
                Selection.End = new Place(lines[iEnd].Count, iEnd);
                Selection.EndUpdate();
                Invalidate();
                return;
            }
        }

        protected virtual void OnMarkerDoubleClick(VisualMarker marker)
        {
            if (marker is FoldedAreaMarker)
            {
                ExpandFoldedBlock((marker as FoldedAreaMarker).iLine);
                Invalidate();
                return;
            }
        }

        private void ClearBracketsPositions()
        {
            leftBracketPosition = null;
            rightBracketPosition = null;
            leftBracketPosition2 = null;
            rightBracketPosition2 = null;
        }

        private void HighlightBrackets(char LeftBracket, char RightBracket, ref Range leftBracketPosition,
                                       ref Range rightBracketPosition)
        {
            if (Selection.Start != Selection.End)
                return;
            //
            Range oldLeftBracketPosition = leftBracketPosition;
            Range oldRightBracketPosition = rightBracketPosition;
            Range range = Selection.Clone(); //need clone because we will move caret
            int counter = 0;
            int maxIterations = maxBracketSearchIterations;
            while (range.GoLeftThroughFolded()) //move caret left
            {
                if (range.CharAfterStart == LeftBracket) counter++;
                if (range.CharAfterStart == RightBracket) counter--;
                if (counter == 1)
                {
                    //highlighting
                    range.End = new Place(range.Start.iChar + 1, range.Start.iLine);
                    leftBracketPosition = range;
                    break;
                }
                //
                maxIterations--;
                if (maxIterations <= 0) break;
            }
            //
            range = Selection.Clone(); //need clone because we will move caret
            counter = 0;
            maxIterations = maxBracketSearchIterations;
            do
            {
                if (range.CharAfterStart == LeftBracket) counter++;
                if (range.CharAfterStart == RightBracket) counter--;
                if (counter == -1)
                {
                    //highlighting
                    range.End = new Place(range.Start.iChar + 1, range.Start.iLine);
                    rightBracketPosition = range;
                    break;
                }
                //
                maxIterations--;
                if (maxIterations <= 0) break;
            } while (range.GoRightThroughFolded()); //move caret right

            if (oldLeftBracketPosition != leftBracketPosition ||
                oldRightBracketPosition != rightBracketPosition)
                Invalidate();
        }

        public virtual void OnSyntaxHighlight(TextChangedEventArgs args)
        {
#if debug
            Stopwatch sw = Stopwatch.StartNew();
            #endif

            Range range;

            switch (HighlightingRangeType)
            {
                case HighlightingRangeType.VisibleRange:
                    range = VisibleRange.GetUnionWith(args.ChangedRange);
                    break;
                case HighlightingRangeType.AllTextRange:
                    range = Range;
                    break;
                default:
                    range = args.ChangedRange;
                    break;
            }

            if (SyntaxHighlighter != null)
            {
                if (Language == Language.Custom && !string.IsNullOrEmpty(DescriptionFile))
                    SyntaxHighlighter.HighlightSyntax(DescriptionFile, range);
                else
                    SyntaxHighlighter.HighlightSyntax(Language, range);
            }

#if debug
            Console.WriteLine("OnSyntaxHighlight: "+ sw.ElapsedMilliseconds);
            #endif
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            //
            // FastColoredTextBox
            //
            Name = "FastColoredTextBox";
            ResumeLayout(false);
        }

        /// <summary>
        /// Prints range of text
        /// </summary>
        public void Print(Range range, PrintDialogSettings settings)
        {
            var wb = new WebBrowser();
            settings.printRange = range;
            wb.Tag = settings;
            wb.Visible = false;
            wb.Location = new Point(-1000, -1000);
            wb.Parent = this;
            wb.Navigate("about:blank");
            wb.Navigated += ShowPrintDialog;
        }

        /// <summary>
        /// Prints all text
        /// </summary>
        public void Print(PrintDialogSettings settings)
        {
            Print(Range, settings);
        }

        /// <summary>
        /// Prints all text, without any dialog windows
        /// </summary>
        public void Print()
        {
            Print(Range,
                  new PrintDialogSettings
                      {ShowPageSetupDialog = false, ShowPrintDialog = false, ShowPrintPreviewDialog = false});
        }

        private void ShowPrintDialog(object sender, WebBrowserNavigatedEventArgs e)
        {
            var wb = sender as WebBrowser;
            var settings = wb.Tag as PrintDialogSettings;
            //prepare export with wordwrapping
            var exporter = new ExportToHTML();
            exporter.UseBr = true;
            exporter.UseForwardNbsp = true;
            exporter.UseNbsp = false;
            exporter.UseStyleTag = false;
            //generate HTML
            string HTML = exporter.GetHtml(settings.printRange);
            //show print dialog
            wb.Document.Body.InnerHtml = HTML;
            if (settings.ShowPrintPreviewDialog)
                wb.ShowPrintPreviewDialog();
            else
            {
                if (settings.ShowPageSetupDialog)
                    wb.ShowPageSetupDialog();

                if (settings.ShowPrintDialog)
                    wb.ShowPrintDialog();
                else
                    wb.Print();
            }
            //destroy webbrowser
            wb.Parent = null;
            wb.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (SyntaxHighlighter != null)
                    SyntaxHighlighter.Dispose();
                timer.Dispose();
                timer2.Dispose();

                if (findForm != null)
                    findForm.Dispose();

                if (replaceForm != null)
                    replaceForm.Dispose();

                if (Font != null)
                    Font.Dispose();

                if (TextSource != null)
                    TextSource.Dispose();
            }
        }

        protected virtual void OnPaintLine(PaintLineEventArgs e)
        {
            if (PaintLine != null)
                PaintLine(this, e);
        }

        internal void OnLineInserted(int index)
        {
            OnLineInserted(index, 1);
        }

        internal void OnLineInserted(int index, int count)
        {
            if (LineInserted != null)
                LineInserted(this, new LineInsertedEventArgs(index, count));
        }

        internal void OnLineRemoved(int index, int count, List<int> removedLineIds)
        {
            if (count > 0)
                if (LineRemoved != null)
                    LineRemoved(this, new LineRemovedEventArgs(index, count, removedLineIds));
        }

        /// <summary>
        /// Open file binding mode
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="enc"></param>
        public void OpenBindingFile(string fileName, Encoding enc)
        {
            try
            {
                var fts = new FileTextSource(this);
                InitTextSource(fts);
                fts.OpenFile(fileName, enc);
                IsChanged = false;
                OnVisibleRangeChanged();
            }
            catch
            {
                InitTextSource(CreateTextSource());
                lines.InsertLine(0, TextSource.CreateLine());
                IsChanged = false;
                throw;
            }
        }

        /// <summary>
        /// Close file binding mode
        /// </summary>
        public void CloseBindingFile()
        {
            if (lines is FileTextSource)
            {
                var fts = lines as FileTextSource;
                fts.CloseFile();

                InitTextSource(CreateTextSource());
                lines.InsertLine(0, TextSource.CreateLine());
                IsChanged = false;
                Invalidate();
            }
        }

        /// <summary>
        /// Save text to the file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="enc"></param>
        public void SaveToFile(string fileName, Encoding enc)
        {
            lines.SaveToFile(fileName, enc);
            IsChanged = false;
            OnVisibleRangeChanged();
        }

        /// <summary>
        /// Set VisibleState of line
        /// </summary>
        public void SetVisibleState(int iLine, VisibleState state)
        {
            LineInfo li = lineInfos[iLine];
            li.VisibleState = state;
            lineInfos[iLine] = li;
            needRecalc = true;
        }

        /// <summary>
        /// Returns VisibleState of the line
        /// </summary>
        public VisibleState GetVisibleState(int iLine)
        {
            return lineInfos[iLine].VisibleState;
        }

        /// <summary>
        /// Shows Goto dialog form
        /// </summary>
        public void ShowGoToDialog()
        {
            var form = new GoToForm();
            form.TotalLineCount = LinesCount;
            form.SelectedLineNumber = Selection.Start.iLine + 1;

            if (form.ShowDialog() == DialogResult.OK)
            {
                int line = Math.Min(LinesCount - 1, Math.Max(0, form.SelectedLineNumber - 1));
                Selection = new Range(this, 0, line, 0, line);
                DoSelectionVisible();
            }
        }

        /// <summary>
        /// Occurs when undo/redo stack is changed
        /// </summary>
        public void OnUndoRedoStateChanged()
        {
            if (UndoRedoStateChanged != null)
                UndoRedoStateChanged(this, EventArgs.Empty);
        }

        #region Nested type: LineYComparer

        private class LineYComparer : IComparer<LineInfo>
        {
            private readonly int Y;

            public LineYComparer(int Y)
            {
                this.Y = Y;
            }

            #region IComparer<LineInfo> Members

            public int Compare(LineInfo x, LineInfo y)
            {
                if (x.startY == -10)
                    return -y.startY.CompareTo(Y);
                else
                    return x.startY.CompareTo(Y);
            }

            #endregion
        }

        #endregion
    }

    public class PaintLineEventArgs : PaintEventArgs
    {
        public PaintLineEventArgs(int iLine, Rectangle rect, Graphics gr, Rectangle clipRect) : base(gr, clipRect)
        {
            LineIndex = iLine;
            LineRect = rect;
        }

        public int LineIndex { get; private set; }
        public Rectangle LineRect { get; private set; }
    }

    public class LineInsertedEventArgs : EventArgs
    {
        public LineInsertedEventArgs(int index, int count)
        {
            Index = index;
            Count = count;
        }

        /// <summary>
        /// Inserted line index
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Count of inserted lines
        /// </summary>
        public int Count { get; private set; }
    }

    public class LineRemovedEventArgs : EventArgs
    {
        public LineRemovedEventArgs(int index, int count, List<int> removedLineIds)
        {
            Index = index;
            Count = count;
            RemovedLineUniqueIds = removedLineIds;
        }

        /// <summary>
        /// Removed line index
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Count of removed lines
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// UniqueIds of removed lines
        /// </summary>
        public List<int> RemovedLineUniqueIds { get; private set; }
    }

    /// <summary>
    /// TextChanged event argument
    /// </summary>
    public class TextChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TextChangedEventArgs(Range changedRange)
        {
            ChangedRange = changedRange;
        }

        /// <summary>
        /// This range contains changed area of text
        /// </summary>
        public Range ChangedRange { get; set; }
    }

    public class TextChangingEventArgs : EventArgs
    {
        public string InsertingText { get; set; }
        public bool IsHandled { get; set; }
    }

    public class TabChangingEventArgs : EventArgs
    {
        public TabChangingEventArgs(TabChangingReason reason)
        {
            Reason = reason;
        }

        public TabChangingReason Reason { get; private set; }
        public bool Cancel { get; set; }
    }

    public enum TabChangingReason
    {
        Programm,
        User
    }

    public enum WordWrapMode
    {
        /// <summary>
        /// Word wrapping by control width
        /// </summary>
        WordWrapControlWidth,

        /// <summary>
        /// Word wrapping by preferred line width (PreferredLineWidth)
        /// </summary>
        WordWrapPreferredWidth,

        /// <summary>
        /// Char wrapping by control width
        /// </summary>
        CharWrapControlWidth,

        /// <summary>
        /// Char wrapping by preferred line width (PreferredLineWidth)
        /// </summary>
        CharWrapPreferredWidth
    }

    public class PrintDialogSettings
    {
        public bool ShowPageSetupDialog;
        public bool ShowPrintDialog;
        public bool ShowPrintPreviewDialog = true;
        internal Range printRange;
    }

    public class AutoIndentEventArgs : EventArgs
    {
        public AutoIndentEventArgs(int iLine, string lineText, string prevLineText, int tabLength)
        {
            this.iLine = iLine;
            LineText = lineText;
            PrevLineText = prevLineText;
            TabLength = tabLength;
        }

        public int iLine { get; internal set; }
        public int TabLength { get; internal set; }
        public string LineText { get; internal set; }
        public string PrevLineText { get; internal set; }

        /// <summary>
        /// Additional spaces count for this line, relative to previous line
        /// </summary>
        public int Shift { get; set; }

        /// <summary>
        /// Additional spaces count for next line, relative to previous line
        /// </summary>
        public int ShiftNextLines { get; set; }
    }

    /// <summary>
    /// Type of highlighting
    /// </summary>
    public enum HighlightingRangeType
    {
        /// <summary>
        /// Highlight only changed range of text. Highest performance.
        /// </summary>
        ChangedRange,

        /// <summary>
        /// Highlight visible range of text. Middle performance.
        /// </summary>
        VisibleRange,

        /// <summary>
        /// Highlight all (visible and invisible) text. Lowest performance.
        /// </summary>
        AllTextRange
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Windows.Forms.VisualStyles;
using Irony.Parsing;
using Irony.WinForms.Highlighter;
using Irony.WinForms.Proxies;
using FastColoredTextBoxNS;
using FctbHighlighter = Irony.WinForms.Highlighter.FastColoredTextBoxHighlighter;

namespace Irony.WinForms {
  /// <summary>
  /// TextBox with syntax highlighting support based on Irony toolkit.
  /// </summary>
  public partial class IronyTextBox : UserControl {
    BorderStyleEx _borderStyleEx;
    FctbHighlighter _highlighter;
    LanguageData _languageData;
    TextBox _textBoxProxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="IronyTextBox" /> class.
    /// </summary>
    public IronyTextBox() {
      InitializeComponent();
      BorderStyleEx = BorderStyleEx.VisualStyle;
    }

    /// <summary>
    /// Gets or sets <see cref="LanguageData"/> for syntax highlighting.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public LanguageData Language {
      get { return _languageData; }
      set {
        if (_languageData != value) {
          _languageData = value;
          HighlightingEnabled = _languageData != null;
        }
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether syntax highlighting is enabled.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool HighlightingEnabled {
      get { return _highlighter != null; }
      set {
        if (HighlightingEnabled != value) {
          if (value)
            StartHighlighter();
          else
            StopHighlighter();
        }
      }
    }

    private void StartHighlighter() {
      if (_highlighter != null)
        StopHighlighter();
      if (!Language.CanParse()) return;
      _highlighter = new FctbHighlighter(FastColoredTextBox, Language);
      _highlighter.Adapter.Activate();
    }

    private void StopHighlighter() {
      if (_highlighter == null) return;
      _highlighter.Dispose();
      _highlighter = null;
      ClearHighlighting();
    }

    private void ClearHighlighting() {
      var selectedRange = FastColoredTextBox.Selection;
      var visibleRange = FastColoredTextBox.VisibleRange;
      var firstVisibleLine = Math.Min(visibleRange.Start.iLine, visibleRange.End.iLine);

      var txt = FastColoredTextBox.Text;
      FastColoredTextBox.Clear();
      FastColoredTextBox.Text = txt; //remove all old highlighting

      FastColoredTextBox.SetVisibleState(firstVisibleLine, VisibleState.Visible);
      FastColoredTextBox.Selection = selectedRange;
    }

    /// <summary>
    /// Gets or sets the text associated with this <see cref="IronyTextBox"/>.
    /// </summary>
    [DefaultValue(""), Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Localizable(true)]
    [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public override string Text {
      get { return FastColoredTextBox.Text; }
      set {
        if (Text != value) {
          FastColoredTextBox.ClearUndo();
          FastColoredTextBox.ClearStylesBuffer();
          FastColoredTextBox.Text = value;
          FastColoredTextBox.SetVisibleState(0, FastColoredTextBoxNS.VisibleState.Visible);
          FastColoredTextBox.Selection = FastColoredTextBox.GetRange(0, 0);
          FastColoredTextBox.DoCaretVisible();
        }
      }
    }

    /// <summary>
    /// Occurs when Text property is changed.
    /// </summary>
    public event EventHandler TextChanged;

    protected virtual void OnTextChanged(EventArgs args) {
      if (TextChanged != null)
        TextChanged(this, args);
    }

    private void FastColoredTextBox_TextChanged(object sender, TextChangedEventArgs args) {
      OnTextChanged(args);
    }

    /// <summary>
    /// Gets or sets the border style of the control.
    /// </summary>
    [DefaultValue(BorderStyleEx.VisualStyle), Browsable(true)]
    public BorderStyleEx BorderStyleEx {
      get { return _borderStyleEx; }
      set {
        if (_borderStyleEx != value) {
          _borderStyleEx = value;
          if (_borderStyleEx != BorderStyleEx.VisualStyle) {
            BorderStyle = (BorderStyle)value;
            Padding = new Padding(0);
          } else {
            BorderStyle = BorderStyle.None;
            if (Application.RenderWithVisualStyles)
              Padding = new Padding(1);
            else
              Padding = new Padding(2);
          }
          Invalidate();
        }
      }
    }

    /// <summary>
    /// Hide the inherited BorderStyle property.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new BorderStyle BorderStyle {
      get { return base.BorderStyle; }
      set { base.BorderStyle = value; }
    }

    /// <summary>
    /// Hide the inherited Padding property.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Padding Padding {
      get { return base.Padding; }
      set { base.Padding = value; }
    }

    /// <summary>
    /// Raises the <see cref="E:Paint" /> event.
    /// </summary>
    /// <param name="args">The <see cref="PaintEventArgs" /> instance containing the event data.</param>
    protected override void OnPaint(PaintEventArgs args) {
      base.OnPaint(args);
      if (Application.RenderWithVisualStyles)
        ControlPaint.DrawVisualStyleBorder(args.Graphics, new Rectangle(0, 0, Width - 1, Height - 1));
      else {
        ControlPaint.DrawBorder3D(args.Graphics, new Rectangle(0, 0, Width, Height), Border3DStyle.SunkenOuter);
        ControlPaint.DrawBorder3D(args.Graphics, new Rectangle(1, 1, Width - 2, Height - 2), Border3DStyle.SunkenInner);
      }
    }

    /// <summary>
    /// Selects a range of text in the text box.
    /// </summary>
    /// <param name="start">The starting position.</param>
    /// <param name="length">The length of the selection.</param>
    public void Select(int start, int length) {
      FastColoredTextBox.SelectionStart = start;
      FastColoredTextBox.SelectionLength = length;
      FastColoredTextBox.DoCaretVisible();
    }

    /// <summary>
    /// Gets or sets the starting point of the text selected in the text box.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual int SelectionStart {
      get { return FastColoredTextBox.SelectionStart; }
      set { FastColoredTextBox.SelectionStart = value; }
    }

    /// <summary>
    /// Gets or sets the number of characters selected in the text box.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual int SelectionLength {
      get { return FastColoredTextBox.SelectionLength; }
      set { FastColoredTextBox.SelectionLength = value; }
    }

    /// <summary>
    /// Scrolls the contents of the control to the current caret position.
    /// </summary>
    public void ScrollToCaret() {
      FastColoredTextBox.DoCaretVisible();
    }

    /// <summary>
    /// Gets or sets the TextBox proxy object.
    /// </summary>
    private TextBox TextBoxProxy {
      get {
        if (_textBoxProxy == null)
          _textBoxProxy = new TextBoxProxy(FastColoredTextBox).Instance;
        return _textBoxProxy;
      }
    }

    /// <summary>
    /// Allows converting IronyTextBox into TextBox implicitly.
    /// </summary>
    /// <param name="ironyTextBox">The irony text box.</param>
    public static implicit operator TextBox(IronyTextBox ironyTextBox) {
      return ironyTextBox.TextBoxProxy;
    }
  }
}

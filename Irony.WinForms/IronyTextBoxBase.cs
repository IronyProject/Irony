using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using Irony.WinForms.Proxies;
using SWFBorderStyle = System.Windows.Forms.BorderStyle;

namespace Irony.WinForms {
  /// <summary>
  /// Common ancestor for Irony.WinForms text boxes.
  /// Allows implicit conversion to the System.Windows.Forms.TextBox.
  /// </summary>
  [ToolboxItem(false)]
  public partial class IronyTextBoxBase : UserControl {
    BorderStyleEx _borderStyle;
    TextBox _textBoxProxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="IronyTextBoxBase" /> class.
    /// </summary>
    public IronyTextBoxBase() {
      InitializeComponent();
      BorderStyle = BorderStyleEx.VisualStyle;
    }

    /// <summary>
    /// Gets or sets the text associated with this <see cref="IronyTextBoxBase"/>.
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
          FastColoredTextBox.SelectionStart = 0;
          FastColoredTextBox.SelectionLength = 0;
          FastColoredTextBox.SetVisibleState(0, FastColoredTextBoxNS.VisibleState.Visible);
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
    public BorderStyleEx BorderStyle {
      get { return _borderStyle; }
      set {
        if (_borderStyle != value) {
          _borderStyle = value;
          if (_borderStyle != BorderStyleEx.VisualStyle) {
            base.BorderStyle = (SWFBorderStyle)value;
            Padding = new Padding(0);
          } else {
            base.BorderStyle = SWFBorderStyle.None;
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
    /// Allows converting IronyTextBoxBase into TextBox implicitly.
    /// </summary>
    /// <param name="IronyTextBoxBase">The irony text box.</param>
    public static implicit operator TextBox(IronyTextBoxBase IronyTextBoxBase) {
      return IronyTextBoxBase.TextBoxProxy;
    }
  }
}

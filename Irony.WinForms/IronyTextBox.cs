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
using Irony.WinForms.Proxies;

namespace Irony.WinForms {
  /// <summary>
  /// TextBox with syntax highlighting support based on Irony toolkit.
  /// </summary>
  public partial class IronyTextBox : UserControl {
    BorderStyleEx _borderStyleEx = BorderStyleEx.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="IronyTextBox" /> class.
    /// </summary>
    public IronyTextBox() {
      InitializeComponent();
      TextBoxProxy = new Lazy<TextBox>(() => new TextBoxProxy(FastColoredTextBox).Instance);
      BorderStyleEx = BorderStyleEx.VisualStyle;
    }

    /// <summary>
    /// Gets or sets the TextBox proxy object.
    /// </summary>
    private Lazy<TextBox> TextBoxProxy { get; set; }

    /// <summary>
    /// Allows converting IronyTextBox into TextBox implicitly.
    /// </summary>
    /// <param name="ironyTextBox">The irony text box.</param>
    public static implicit operator TextBox(IronyTextBox ironyTextBox) {
      return ironyTextBox.TextBoxProxy.Value;
    }

    /// <summary>
    /// Gets or sets the text associated with this <see cref="IronyTextBox"/>.
    /// </summary>
    [DefaultValue(null), Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Localizable(true)]
    [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public override string Text {
      get { return FastColoredTextBox.Text; }
      set {
        FastColoredTextBox.Text = value;
        FastColoredTextBox.SelectionLength = 0;
        FastColoredTextBox.SelectionStart = 0;
        FastColoredTextBox.DoCaretVisible();
      }
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
  }
}

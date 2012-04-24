using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Irony.Interpreter;
using Irony.Parsing;

namespace Irony.WinForms {
  using FastColoredTextBox = FastColoredTextBoxNS.FastColoredTextBox;
  using FctbConsoleTextBox = FastColoredTextBoxNS.ConsoleTextBox;

  /// <summary>
  /// TextBox with for console emulation.
  /// </summary>
  [ToolboxItem(true)]
  public partial class ConsoleTextBox : IronyTextBoxBase, IConsoleAdaptor {
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleTextBox" /> class.
    /// </summary>
    public ConsoleTextBox() {
      InitializeComponent();
    }

    protected override FastColoredTextBox CreateFastColoredTextBox() {
      var textBox = new FctbConsoleTextBox {
        BackColor = Color.Black,
        IndentBackColor = Color.Black,
        PaddingBackColor = Color.Black,
        LineNumberColor = Color.Gold,
        ForeColor = Color.White,
        CaretColor = Color.White,
        PreferredLineWidth = 80,
        WordWrap = true,
        WordWrapMode = FastColoredTextBoxNS.WordWrapMode.CharWrapPreferredWidth
      };

      return textBox;
    }

    private FctbConsoleTextBox Console {
      get { return (FctbConsoleTextBox)FastColoredTextBox; }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string Text {
      get { return base.Text; }
      set { base.Text = value; }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Canceled {
      get;
      set;
    }

    public void Write(string text) {
      if (!InvokeRequired) {
        Console.WriteLine(text);
        return;
      }
      Invoke(new Action<string>(Console.WriteLine), text);
    }

    public void WriteLine(string text) {
      Write(text + Environment.NewLine);
    }

    public void SetTextStyle(ConsoleTextStyle style) {
      Console.ForeColor = style == ConsoleTextStyle.Normal ? Color.White : Color.Red;
    }

    public int Read() {
      return 0;
    }

    public string ReadLine() {
      if (!InvokeRequired)
        return Console.ReadLine();
      return Invoke(new Func<string>(Console.ReadLine)) as string;
    }

    public void SetTitle(string title) {
      throw new NotImplementedException();
    }
  }
}

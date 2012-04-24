using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Irony.Interpreter;
using Irony.Parsing;

namespace Irony.WinForms {
  using FastColoredTextBox = FastColoredTextBoxNS.FastColoredTextBox;
  using FctbConsoleTextBox = FastColoredTextBoxNS.ConsoleTextBox;
  using Style = FastColoredTextBoxNS.Style;
  using StyleIndex = FastColoredTextBoxNS.StyleIndex;
  using TextChangedEventArgs = FastColoredTextBoxNS.TextChangedEventArgs;
  using TextStyle = FastColoredTextBoxNS.TextStyle;

  /// <summary>
  /// TextBox with for console emulation.
  /// Implements <see cref="IConsoleAdaptor"/> interface.
  /// </summary>
  /// <remarks><see cref="IConsoleAdaptor"/> implementation is thread-safe.</remarks>
  [ToolboxItem(true)]
  public partial class ConsoleTextBox : IronyTextBoxBase, IConsoleAdaptor {
    private bool _canceled;
    private Style _normalStyle = new TextStyle(Brushes.White, null, FontStyle.Regular);
    private Style _errorStyle = new TextStyle(Brushes.Red, null, FontStyle.Bold);
    private Style _currentStyle;

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

      textBox.TextChanged += textBox_TextChanged;
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

    protected override void OnHandleDestroyed(EventArgs e) {
      base.OnHandleDestroyed(e);
      Canceled = true;
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Canceled {
      get { return _canceled; }
      set {
        _canceled = value;
        if (Canceled)
          Console.IsReadLineMode = false;
      }
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
      CurrentStyle = style == ConsoleTextStyle.Normal ? _normalStyle : _errorStyle;
    }

    private Style CurrentStyle {
      get { return _currentStyle ?? (_currentStyle = _normalStyle); }
      set { _currentStyle = value; }
    }

    private void textBox_TextChanged(object sender, TextChangedEventArgs args) {
      args.ChangedRange.SetStyle(CurrentStyle);
    }

    public int Read() {
      throw new NotSupportedException();
    }

    public string ReadLine() {
      if (!InvokeRequired)
        return Console.ReadLine();
      return Invoke(new Func<string>(Console.ReadLine)) as string;
    }

    public void SetTitle(string title) {
      throw new NotSupportedException();
    }
  }
}

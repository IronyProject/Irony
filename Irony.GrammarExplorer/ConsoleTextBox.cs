using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Irony.Interpreter;
using Irony.Parsing;

namespace Irony.GrammarExplorer {

  /// <summary>
  /// TextBox with for console emulation.
  /// Implements <see cref="IConsoleAdapter"/> interface.
  /// </summary>
  /// <remarks><see cref="IConsoleAdapter"/> implementation is thread-safe.</remarks>
  [ToolboxItem(true)]
  public partial class ConsoleTextBox : TextBox, IConsoleAdapter {
    private bool _canceled;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleTextBox" /> class.
    /// </summary>
    public ConsoleTextBox() {
      InitializeComponent();
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
      }
    }

    public void Write(string text) {
      this.Text += Environment.NewLine + text;
    }

    public void WriteLine(string text) {
      Write(text + Environment.NewLine);
    }

    public void SetTextStyle(ConsoleTextStyle style) {
    }

    public int Read() {
      throw new NotSupportedException();
    }

    public string ReadLine() {
      if (!InvokeRequired) {
        Focus();
        return Console.ReadLine();
      }
      return Invoke(new Func<string>(Console.ReadLine)) as string;
    }

    public void SetTitle(string title) {
      throw new NotSupportedException();
    }

    public string GetOutput() {
      return Text;
    }
  }
}

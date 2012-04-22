using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using FastColoredTextBoxNS;

namespace Irony.WinForms.Proxies {
  /// <summary>
  /// Proxy class to convert <see cref="FastColoredTextBox"/> instance to a standard <see cref="TextBox"/>.
  /// </summary>
  public class TextBoxProxy : DuckTypingProxy<TextBox> {
    /// <summary>
    /// Initializes a new instance of the <see cref="TextBoxProxy"/> class.
    /// </summary>
    public TextBoxProxy(FastColoredTextBox textBox) : base(textBox) {
      TextBox = textBox;
    }

    private FastColoredTextBox TextBox { get; set; }

    protected override object InvokeMethod(MethodInfo method, object target, object[] arguments) {
      var result = base.InvokeMethod(method, target, arguments);
      TextBox.Invalidate();
      return result;
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Irony.WinForms
{
  public interface ITextBox {
    string Text { get; set; }
    int SelectionStart { get; set; }
    int SelectionLength { get; set; }
    void ScrollToCaret();
    bool Focus();
  }

  public class TextBoxAdapter : ITextBox {
    public TextBoxAdapter(TextBoxBase textBox) {
      TextBox = textBox;
    }
    private TextBoxBase TextBox;
    public string Text { get { return TextBox.Text; } set { TextBox.Text = value; } }
    public int SelectionStart { get { return TextBox.SelectionStart; } set { TextBox.SelectionStart = value; } }
    public int SelectionLength { get { return TextBox.SelectionLength; } set { TextBox.SelectionLength = value; } }
    public void ScrollToCaret() {
      TextBox.ScrollToCaret();
    }
    public bool Focus() {
      return TextBox.Focus();
    }
  }

  public static class TextBoxHelpers {
    public static ITextBox AsITextBox(this TextBoxBase textBox) {
      return new TextBoxAdapter(textBox);
    }
  }
}

namespace Irony.WinForms {
  partial class IronyTextBox {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.FastColoredTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
      this.SuspendLayout();
      // 
      // FastColoredTextBox
      // 
      this.FastColoredTextBox.AutoScrollMinSize = new System.Drawing.Size(25, 15);
      this.FastColoredTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.FastColoredTextBox.Location = new System.Drawing.Point(1, 1);
      this.FastColoredTextBox.Name = "FastColoredTextBox";
      this.FastColoredTextBox.Size = new System.Drawing.Size(261, 148);
      this.FastColoredTextBox.TabIndex = 0;
      this.FastColoredTextBox.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.FastColoredTextBox_TextChanged);
      // 
      // IronyTextBox
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.FastColoredTextBox);
      this.Name = "IronyTextBox";
      this.Padding = new System.Windows.Forms.Padding(1);
      this.Size = new System.Drawing.Size(263, 150);
      this.ResumeLayout(false);

    }

    #endregion

    public FastColoredTextBoxNS.FastColoredTextBox FastColoredTextBox;

  }
}

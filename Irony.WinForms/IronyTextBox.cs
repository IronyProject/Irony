using System;
using System.ComponentModel;
using FastColoredTextBoxNS;
using Irony.Parsing;

namespace Irony.WinForms {
  using FctbHighlighter = Highlighter.FastColoredTextBoxHighlighter;

  /// <summary>
  /// TextBox with syntax highlighting support based on Irony toolkit.
  /// </summary>
  [ToolboxItem(true)]
  public partial class IronyTextBox : IronyTextBoxBase {
    FctbHighlighter _highlighter;
    LanguageData _languageData;

    /// <summary>
    /// Initializes a new instance of the <see cref="IronyTextBox" /> class.
    /// </summary>
    public IronyTextBox() {
      InitializeComponent();
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
      if (Language == null || !Language.CanParse()) return;
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
      var selectedRange = FastColoredTextBox.Selection.Clone();
      FastColoredTextBox.ClearStyle(StyleIndex.All); //remove all old highlighting
      FastColoredTextBox.Selection = selectedRange;
      FastColoredTextBox.Invalidate();
    }
  }
}

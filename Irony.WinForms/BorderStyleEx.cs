using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Irony.WinForms {
  /// <summary>
  /// Specifies the border style for the control.
  /// </summary>
  public enum BorderStyleEx {
    /// <summary>
    /// No border.
    /// </summary>
    None = BorderStyle.None,
    /// <summary>
    /// Single-line border.
    /// </summary>
    FixedSingle = BorderStyle.FixedSingle,
    /// <summary>
    /// A three-dimensional border.
    /// </summary>
    Fixed3D = BorderStyle.Fixed3D,
    /// <summary>
    /// A border supplied by system theme.
    /// </summary>
    VisualStyle = 100,
  }
}

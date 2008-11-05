using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Irony.GrammarExplorer {
  public partial class fmSelectGrammars : Form {
    public fmSelectGrammars() {
      InitializeComponent();
    }

    public static GrammarItemList SelectGrammars(GrammarItemList fromGrammars) {
      fmSelectGrammars form = new fmSelectGrammars();
      var listbox = form.lstGrammars;
      foreach(GrammarItem item in fromGrammars) {
        listbox.Items.Add(item);
        listbox.SetItemChecked(listbox.Items.Count - 1, true);
      }
      if (form.ShowDialog() != DialogResult.OK) return null;
      GrammarItemList result = new GrammarItemList();
      for (int i = 0; i < listbox.Items.Count; i++) {
        if (listbox.GetItemChecked(i))
          result.Add(listbox.Items[i] as GrammarItem); 
      }
      return result;
    }

  }//class
}

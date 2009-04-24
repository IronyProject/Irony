#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

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
        if (listbox.GetItemChecked(i)) {
          var item = listbox.Items[i] as GrammarItem;
          item._loading = false; 
          result.Add(item);
        }
      }
      return result;
    }

    private void btnCheckUncheck_Click(object sender, EventArgs e) {
      bool check = sender == btnCheckAll;
      for (int i = 0; i < lstGrammars.Items.Count; i++)
        lstGrammars.SetItemChecked(i, check); 
    }

  }//class
}

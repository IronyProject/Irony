using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml; 
using System.Windows.Forms;
using System.Reflection;
using Irony.Compiler; 

namespace Irony.GrammarExplorer {

  //Helper classes for supporting showing grammar list in top combo, saving list on exit and loading on start
  public class GrammarItem {
    public readonly string Caption;
    public readonly string Location; //location of assembly containing the grammar
    public readonly string TypeName; //full type name
    public GrammarItem(string name, string location, string typeName) {
      Caption = name;
      Location = location;
      TypeName = typeName;
    }
    public GrammarItem(Type grammarClass, string assemblyLocation) {
      Location = assemblyLocation;
      TypeName = grammarClass.FullName;
      //Get language name from Language attribute
      Caption = grammarClass.Name; //default caption
      object[] attrs = grammarClass.GetCustomAttributes(typeof(LanguageAttribute), true);
      if (attrs != null && attrs.Length > 0) {
        LanguageAttribute la = attrs[0] as LanguageAttribute;
        if (la != null) {
          Caption = la.LanguageName;
          if (!string.IsNullOrEmpty(la.Version))
            Caption += ", version " + la.Version;
        }
      }
    }

    public GrammarItem(XmlElement element) {
      Caption = element.GetAttribute("Caption");
      Location = element.GetAttribute("Location");
      TypeName = element.GetAttribute("TypeName");
    }
    public void Save(XmlElement toElement) {
      toElement.SetAttribute("Caption", Caption);
      toElement.SetAttribute("Location", Location);
      toElement.SetAttribute("TypeName", TypeName);
    }
    public Grammar CreateGrammar() {
      Assembly asm = Assembly.LoadFrom(Location); 
      Type type = asm.GetType(TypeName, true, true);
      var grammar = (Grammar) Activator.CreateInstance(type);
      return grammar; 
    }
    public override string  ToString() {
 	    return Caption; 
    }
  
  }//class

  public class GrammarItemList : List<GrammarItem> {
    public static GrammarItemList FromXml(string xml) {
      GrammarItemList list = new GrammarItemList();
      XmlDocument xdoc = new XmlDocument();
      xdoc.LoadXml(xml);
      XmlNodeList xlist = xdoc.SelectNodes("//Grammar");
      foreach (XmlElement xitem in xlist) {
        GrammarItem item = new GrammarItem(xitem);
        list.Add(item); 
      }
      return list; 
    }
    public static GrammarItemList FromCombo(ComboBox combo) {
      GrammarItemList list = new GrammarItemList();
      foreach (GrammarItem item in combo.Items)
        list.Add(item);
      return list;
    }

    public string ToXml() {
      XmlDocument xdoc = new XmlDocument();
      XmlElement xlist = xdoc.CreateElement("Grammars");
      xdoc.AppendChild(xlist);
      foreach (GrammarItem item in this) {
        XmlElement xitem = xdoc.CreateElement("Grammar");
        xlist.AppendChild(xitem);
        item.Save(xitem); 
      } //foreach
      return xdoc.OuterXml; 
    }//method

    public void ShowIn(ComboBox combo) {
      combo.Items.Clear();
      foreach (GrammarItem item in this)
        combo.Items.Add(item); 
    }

  }//class
}

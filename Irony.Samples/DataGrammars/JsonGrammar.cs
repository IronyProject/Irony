using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Irony.Samples.Json {
  [Language("JSON", "1.0", "JSON data format")]
  public class JsonGrammar : Grammar {
    public JsonGrammar() {
      //Terminals
      var jstring = new StringLiteral("string", "\"");
      var jnumber = new NumberLiteral("number");
      var comma = ToTerm(","); 
      
      //Nonterminals
      var jobject = new NonTerminal("Object");
      var jarray = new NonTerminal("Array");
      var jvalue = new NonTerminal("Value");
      var jprop = new NonTerminal("Property"); 
      var jproplist = new NonTerminal("PropertyList"); 
      var jlist = new NonTerminal("List"); 

      //Rules
      jvalue.Rule = jstring | jnumber | jobject | jarray | "true" | "false" | "null";
      jobject.Rule = "{" + jproplist + "}";
      jproplist.Rule = MakeStarRule(jproplist, comma, jprop);
      jprop.Rule = jstring + ":" + jvalue;
      jarray.Rule = "[" + jlist + "]";
      jlist.Rule = MakeStarRule(jlist, comma, jvalue);

      //Set grammar root
      this.Root = jvalue;
      RegisterPunctuation("{", "}", "[", "]", ":", ",");
      this.MarkTransient(jvalue, jlist, jproplist);

    }//constructor
  }//class
}//namespace

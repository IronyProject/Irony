using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;
using System.Globalization;

namespace Irony.Samples.TestBed {
  [Language("ServerGrammar", "1", "Http Server name Grammar")]
  public class ServerGrammar : Irony.Parsing.Grammar {
    public ServerGrammar()  : base() {
      var serverNamevalue = new FreeTextLiteral("serverNameValue", FreeTextOptions.None, " ", ";");
      var semicolon = ToTerm(";");
      var httpCoreServerName = new NonTerminal("httpCoreServerName");
      var httpCoreServerNameItems = new NonTerminal("httpCoreServerNameItems");

      httpCoreServerName.Rule = "server_name" + httpCoreServerNameItems + semicolon;
      httpCoreServerNameItems.Rule = MakePlusRule(httpCoreServerNameItems, serverNamevalue);

      this.Root = httpCoreServerName;

    }

  }
}


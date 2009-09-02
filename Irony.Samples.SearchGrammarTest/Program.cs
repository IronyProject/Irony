using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Samples.FullTextSearch;

namespace Irony.Samples.SearchGrammarTest {
  //A simple console app for testing FullTextSearch grammar.
  // Does not execute the queries against database, only converts them.
  class Program {
    static void Main(string[] args) {
      Console.WriteLine("SearchGrammar test console.");
      Console.WriteLine("Enter '/q' to exit.");
      var g = new SearchGrammar();
      var parser = new Parser(g); 
      while(true) {
        Console.WriteLine();
        Console.WriteLine();
        Console.Write("Enter query>"); 
        var input = Console.ReadLine();
        if (!string.IsNullOrEmpty(input) && input == "/q") return; 
        try {
          var outputTree = parser.Parse(input);
          if (outputTree.Root == null || outputTree.Errors.Count > 0) {
            foreach (var err in outputTree.Errors)
              Console.WriteLine("Error: " + err.Message + " at " + err.Location);
            continue; 
          }
          var sql = SearchGrammar.ConvertQuery(outputTree.Root);
          Console.WriteLine("Result: ");
          Console.WriteLine(sql); 
        } catch (Exception ex) {
          Console.WriteLine(ex.ToString()); 
        }

      }

    }
  }
}

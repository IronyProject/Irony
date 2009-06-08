using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.CompilerServices;
//using System.Data.SqlClient;
//using System.Data;

namespace Irony.Samples.Apress.Examples
{
  [Language("SearchGrammar", "1.0", "Google-to-SQL query converter")]
  public class SearchGrammar : Grammar
    {
        public SearchGrammar() : base(false)   {
          this.GrammarComments = 
            "Google-to-SQL full-text query format converter. Based on original project by Michael Coles.\r\n" +
            "http://www.sqlservercentral.com/articles/Full-Text+Search+(2008)/64248/ \r\n" +
            "Slightly revised to work with latest version of Irony. "; 
          
          // Terminals
          var Term = new IdentifierTerminal("Term", "!@#$%^*_'.?", "!@#$%^*_'.?0123456789");
          var Phrase = new StringLiteral("Phrase");

          // NonTerminals
          var OrExpression = new NonTerminal("OrExpression");
          var OrOperator = new NonTerminal("OrOperator");
          var AndExpression = new NonTerminal("AndExpression");
          var AndOperator = new NonTerminal("AndOperator");
          var ExcludeOperator = new NonTerminal("ExcludeOperator");
          var PrimaryExpression = new NonTerminal("PrimaryExpression");
          var ThesaurusExpression = new NonTerminal("ThesaurusExpression");
          var ThesaurusOperator = new NonTerminal("ThesaurusOperator");
          var ExactOperator = new NonTerminal("ExactOperator");
          var ExactExpression = new NonTerminal("ExactExpression");
          var ParenthesizedExpression = new NonTerminal("ParenthesizedExpression");
          var ProximityExpression = new NonTerminal("ProximityExpression");
          var ProximityList = new NonTerminal("ProximityList");

          this.Root = OrExpression;
          OrExpression.Rule = AndExpression
                            | OrExpression + OrOperator + AndExpression;
          OrOperator.Rule = Symbol("or") | "|";
          AndExpression.Rule = PrimaryExpression
                             | AndExpression + AndOperator + PrimaryExpression;
          AndOperator.Rule = Empty
                           | "and"
                           | "&"
                           | ExcludeOperator;
          ExcludeOperator.Rule = Symbol("-");
          PrimaryExpression.Rule = Term
                                 | ThesaurusExpression
                                 | ExactExpression
                                 | ParenthesizedExpression
                                 | Phrase
                                 | ProximityExpression;
          ThesaurusExpression.Rule = ThesaurusOperator + Term;
          ThesaurusOperator.Rule = Symbol("~");
          ExactExpression.Rule = ExactOperator + Term
                               | ExactOperator + Phrase;
          ExactOperator.Rule = Symbol("+");
          ParenthesizedExpression.Rule = "(" + OrExpression + ")";
          ProximityExpression.Rule = "<" + ProximityList + ">";

          MakePlusRule(ProximityList, Term);

          RegisterPunctuation("<", ">", "(", ")");
        }

        public enum TermType
        {
            Inflectional = 1,
            Thesaurus = 2,
            Exact = 3
        }

        public static string ConvertQuery(ParseTreeNode node, TermType type) {
          string result = "";
          // Note that some NonTerminals don't actually get into the AST tree, 
          // because of some Irony's optimizations - punctuation stripping and 
          // node bubbling. For example, ParenthesizedExpression - parentheses 
          // symbols get stripped off as punctuation, and child expression node 
          // (parenthesized content) replaces the parent ParExpr node (the 
          // child is "bubbled up").
          switch (node.Term.Name) {
            case "OrExpression":
              if (node.ChildNodes.Count == 1) {
                result = ConvertQuery(node.ChildNodes[0], TermType.Inflectional);
                break;
              }
              result = "(" + ConvertQuery(node.ChildNodes[0], type) + " OR " +
                  ConvertQuery(node.ChildNodes[2], type) + ")";
              break;

            case "AndExpression":
              if (node.ChildNodes.Count == 1) {
                result = ConvertQuery(node.ChildNodes[0], TermType.Inflectional);
                break; 
              }
              string opSym = string.Empty;
              var opNode = node.ChildNodes[1]; 
              string opName = (opNode.ChildNodes.Count > 0 ? opNode.ChildNodes[0].Token.Text : string.Empty);
              string andop = "";

              if (opName == "-") {
                andop += " AND NOT ";
              } else {
                andop = " AND ";
                type = TermType.Inflectional;
              }
              result = "(" + ConvertQuery(node.ChildNodes[0], type) + andop +
                  ConvertQuery(node.ChildNodes[2], type) + ")";
              type = TermType.Inflectional;
              break;

            case "PrimaryExpression":
              result = "(" + ConvertQuery(node.ChildNodes[0], type) + ")";
              break;

            case "ProximityList":
              string[] tmp = new string[node.ChildNodes.Count];
              type = TermType.Exact;
              for (int i = 0; i < node.ChildNodes.Count; i++) {
                tmp[i] = ConvertQuery(node.ChildNodes[i], type);
              }
              result = "(" + string.Join(" NEAR ", tmp) + ")";
              type = TermType.Inflectional;
              break;

            case "Phrase":
              result = '"' + node.Token.ValueString + '"';
              break;

            case "ThesaurusExpression":
              result = " FORMSOF (THESAURUS, " +
                  node.ChildNodes[1].Token.ValueString + ") ";
              break;

            case "ExactExpression":
              result = " \"" + node.ChildNodes[1].Token.ValueString + "\" ";
              break;

            case "Term":
              switch (type) {
                case TermType.Inflectional:
                  result = node.Token.ValueString;
                  if (result.EndsWith("*"))
                    result = "\"" + result + "\"";
                  else
                    result = " FORMSOF (INFLECTIONAL, " + result + ") ";
                  break;
                case TermType.Exact:
                  result = node.Token.ValueString;

                  break;
              }
              break;

            // This should never happen, even if input string is garbage
            default:
              throw new ApplicationException("Converter failed: unexpected term: " +
                  node.Term.Name + ". Please investigate.");

          }
          return result;
        }
    /*      
            private static string connectionString = "DATA SOURCE=(local);INITIAL CATALOG=AdventureWorks;INTEGRATED SECURITY=SSPI;";

            //commented out, to avoid referencing extra Data/SQL namespaces 
            public static DataTable ExecuteQuery(string ftsQuery) {
              SqlDataAdapter da = null;
              DataTable dt = null;
              try {
                dt = new DataTable();
                da = new SqlDataAdapter
                    (
                        "SELECT ROW_NUMBER() OVER (ORDER BY DocumentId) AS Number, " +
                        "   Title, " +
                        "   DocumentSummary " +
                        "FROM Production.Document " +
                        "WHERE CONTAINS(*, @ftsQuery);",
                        connectionString
                    );
                da.SelectCommand.Parameters.Add("@ftsQuery", SqlDbType.NVarChar, 4000).Value = ftsQuery;
                da.Fill(dt);
                da.Dispose();
              } catch (Exception ex) {
                if (da != null)
                  da.Dispose();
                if (dt != null)
                  dt.Dispose();
                throw (ex);
              }
              return dt;
            }

     */
  }

}

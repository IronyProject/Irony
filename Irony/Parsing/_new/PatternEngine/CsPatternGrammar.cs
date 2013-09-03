using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irony.Parsing._new {

  public class SampleClass<T1, T2> : List<T1> where T1 : IDisposable {

  }
  public class where {

  }
  public class WhereList : List<where> { }

  public class CsPatternGrammar : Grammar {
    public const bool IsTrue = 1 > 0;

    public CsPatternGrammar() {
      var comma = ToTerm(",");
      var identifier = new IdentifierTerminal("id");

      var classPttn = new Pattern("class");
      var classDef = new NonTerminal("classDef");
      var classBases = new NonTerminal("classBases");
      var typeRestr = new NonTerminal("whereRestr");
      var typeRef = new NonTerminal("typeRef");
      var classBody = new NonTerminal("classBody");

      typeRef.Pattern = 

      classDef.Pattern = /*Keywords +*/
        "class" + identifier.Aliased("className")
        + MayBe(":" + classBases.FromPattern(Patterns.ListOf(comma, typeRef.Pattern) + ""))
        + MayBe("where" + typeRestr.FromPattern(Patterns.Any() + ""))
        + ("{" + classBody + "}");




/*


      var pUsingBody = new Pattern("pUsingBody");
      var pNsFullName = new Pattern("pNsFullName");
      var pNsBody = new Pattern("pNsBody");
      var pClass = new Pattern("pClass");
      var tPrmList = new ListPattern("tPrmList");
      var tStmtList = new ListPattern("tStmtList");
      var tFuncDef = new Pattern("tFuncDef");
      var tTypeArgWhere = new Pattern("tTypeArgWhere");
      var tTypeArgs = new Pattern("tTypeArgs");
      var tAttrDeclList = Patterns.CreateList("tAttrDeclList");
      var tAttrList = new ListPattern("tAttrList");
      var tCompoundId = new ListPattern("tCompoundId");
      var typeRef = new Pattern("typeRef", tCompoundId + MayBe(tTypeArgs));
      var typeRefList = new ListPattern("typeRefList", ",", typeRef + "");

      var pUsingList = new ListPattern("pUsingList", null, "using" + Infer(pUsingBody) + ";");
      var pNsList = new ListPattern("pNsList", null, "namespace" + Infer(pNsFullName) + "{" + 
                                    Infer(pUsingList + pClass.Star()) +  
                                    "}");
      var pFile = new Pattern("pFile", pUsingList + pNsList);

      var pClassOptions = new Pattern("pClassOptions");
      var pClassBody = new Pattern("pClassBody");
      pClass.Rule = pClassOptions + "class" + identifier + tTypeArgs + pBases("") + "{" + Infer(pClassBody) + "}";
      var pClasses = new ListPattern("tClasses", null, pClass.Rule);
      pNsBody.Rule = pUsingList + pClasses; 


      tFuncDef.Rule = Any() + identifier + MayBe(tTypeArgs) +
        "(" + tPrmList + ")" + MayBe(tTypeArgWhere) +
        "{" + Infer(tStmtList) + "}";
      tTypeArgs.Rule = "<" + typeRefList + ">";
      typeRef.ElementRule = typeRef + string.Empty;
      typeRef.Rule = "" + ListOf(identifier, ToTerm("."), ToTerm("::"));
 * */
    }

    public static BnfExpression MayBe(BnfTerm term,  BnfExpression pattern) {
      return pattern;
    }
    public static BnfExpression MayBe(BnfExpression term) {
      return term + "";
    }
    /*
        x = a < b > c.Me();
  
     */
    //Helper methods
  }

  public static class Patterns {
    public static BnfTerm Aliased(this BnfTerm term, string alias) {
      return term;
    }
    public static BnfTerm FromPattern(this BnfTerm term, BnfExpression pattern) {
      return term;
    }

    
    public static Pattern CreateList(string name) {
      return null;
    }
    
    public static BnfTerm Any() {
      return null;
    }
    public static BnfTerm MayBe(BnfTerm term) {
      return null;
    }
    public static BnfTerm OneOf(params BnfTerm[] items) {
      return null;
    }
    public static BnfTerm ListOf(params BnfTerm[] items) {
      return null;
    }
    public static BnfTerm Infer(BnfTerm term) {
      return null;
    }
    public static BnfTerm InferAnyOf(params BnfTerm[] term) {
      return null;
    }
    public static BnfTerm Star(this BnfTerm term) {
      return null;
    }
    public static BnfTerm Plus(this BnfTerm term) {
      return null;
    }
  }

}

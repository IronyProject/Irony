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
using System.Text;
using Irony.Compiler;

namespace Irony.Samples.Scheme {
  public class SchemeGrammar : Grammar {
    // It is loosely based on R6RS specs.  
    // See Grammar Errors tab in GrammarExplorer for remaining conflicts.
    public SchemeGrammar() {

      base.Ops = new SchemeOps();


      #region Terminals
      ConstantTerminal Constant = new ConstantTerminal("Constant");
      Constant.Add("#T", 1);
      Constant.Add("#t", 1);
      Constant.Add("#F", null);
      Constant.Add("#f", null);
      Constant.Add("'()", null);
      Constant.Add(@"#\nul", '\u0000');
      Constant.Add(@"#\alarm", '\u0007');
      Constant.Add(@"#\backspace", '\b');
      Constant.Add(@"#\tab", '\t');
      Constant.Add(@"#\linefeed", '\n');
      Constant.Add(@"#\vtab", '\v');
      Constant.Add(@"#\page", '\f');
      Constant.Add(@"#\return", '\r');
      Constant.Add(@"#\esc", '\u001B');
      Constant.Add(@"#\space", ' ');
      Constant.Add(@"#\delete", '\u007F');

      // TODO: build SchemeCharLiteral
      // the following is nonsense, just to put something there
      StringLiteral charLiteral = new StringLiteral("Char", "'", ScanFlags.None); 
      Terminal stringLiteral = new StringLiteral("String", "\"", ScanFlags.AllowAllEscapes);
      //Identifiers. Note: added "-", just to allow IDs starting with "->" 
      IdentifierTerminal SimpleIdentifier = new IdentifierTerminal("SimpleIdentifier", "_+-*/.@?!<>=", "_+-*/.@?!<>=$%&:^~");
      //                                                           name                extraChars      extraFirstChars  
      Terminal Number = TerminalFactory.CreateSchemeNumber("Number");
      Terminal Byte = Number; 

      //Comments
      Terminal Comment = new CommentTerminal("Comment", "#|", "|#");
      Terminal LineComment = new CommentTerminal("LineComment", ";", "\n");
      NonGrammarTerminals.Add(Comment); //add comments explicitly to this list as it is not reachable from Root
      NonGrammarTerminals.Add(LineComment);
      #endregion

      #region NonTerminals
      var Module = new NonTerminal("Module");
      var Library = new NonTerminal("Library");
      var Script = new NonTerminal("Script");

      var Abbreviation = new NonTerminal("Abbreviation");
      var Vector = new NonTerminal("Vector");
      var ByteVector = new NonTerminal("ByteVector");
      var Datum = new NonTerminal("Datum"); //Datum in R6RS terms
      var DatumList = new NonTerminal("Datum+", typeof(DatumListNode));
      var DatumListOpt = new NonTerminal("Datum*", typeof(DatumListNode));
      var Statement = new NonTerminal("Statement");
      var Atom = new NonTerminal("Atom");
      var CompoundDatum = new NonTerminal("CompoundDatum");
      var AbbrevPrefix = new NonTerminal("AbbrevPrefix");

      var LibraryName = new NonTerminal("LibraryName");
      var ImportSection = new NonTerminal("ImportSection");
      var ExportSection = new NonTerminal("ExportSection");
      var ImportSpec = new NonTerminal("ImportSpec");
      var ExportSpec = new NonTerminal("ExportSpec");
      var LP = new NonTerminal("LP"); //actually is "(" or "["
      var RP = new NonTerminal("RP"); // ")" or "]"
      var Identifier = new NonTerminal("Identifier", typeof(IdentifierNode));
      var IdentifierList = new NonTerminal("IdentifierList");
      var IdentifierListOpt = new NonTerminal("IdentifierListOpt");
      var PeculiarIdentifier = new NonTerminal("PeculiarIdentifier");
      var LibraryVersion = new NonTerminal("LibraryVersion");

      var FunctionCall = new NonTerminal("FunctionCall", CreateFunctionCallNode);
      var FunctionRef = new NonTerminal("FunctionRef");
      var SpecialForm = new NonTerminal("SpecialForm");
      var DefineVarForm = new NonTerminal("DefineVarForm", CreateDefineVarNode);
      var DefineFunForm = new NonTerminal("DefineFunForm", CreateDefineFunNode);
      var LambdaForm = new NonTerminal("LambdaForm", CreateLambdaNode);
      var IfForm = new NonTerminal("IfForm", CreateIfThenElseNode);
      var CondForm = new NonTerminal("CondForm", CreateCondFormNode);
      var CondClause = new NonTerminal("CondClause", CreateCondClauseNode);
      var CondElse = new NonTerminal("CondElse");
      var BeginForm = new NonTerminal("BeginForm", CreateBeginNode);
      var LetForm = new NonTerminal("LetForm"); //not implemented
      var LetRecForm = new NonTerminal("LetRecForm"); //not implemented
      var LetPair = new NonTerminal("LetPair");
      #endregion

      #region Rules
      base.Root = Module;

      Module.Rule = Library.Plus() + Script | Script;
      Script.Rule = ImportSection + DatumList | DatumList;
      LP.Rule = Symbol("(") | "[";  //R6RS allows mix & match () and []
      RP.Rule = Symbol(")") | "]";

      //Library
      Library.Rule = LP + "library" + LibraryName + ExportSection.Q() + ImportSection.Q() + DatumListOpt + RP;
      LibraryName.Rule = LP + IdentifierList + LibraryVersion.Q() + RP;
      LibraryVersion.Rule = LP + Number.Star() + RP; //zero or more subversion numbers
      ExportSection.Rule = LP + "export" + ExportSpec.Plus() + RP;
      ImportSection.Rule = LP + "import" + ImportSpec.Plus() + RP;
      ExportSpec.Rule = Identifier | LP + "rename"  +  LP + Identifier + Identifier + RP + RP;
      ImportSpec.Rule = LP + Identifier + RP;   //FRI - much more complex in R6RS

      //Datum
      Datum.Rule = Atom | CompoundDatum;
      DatumList.Rule = MakePlusRule(DatumList, null, Datum);
      DatumListOpt.Rule = MakeStarRule(DatumListOpt, null, Datum);
      Atom.Rule = Number | Identifier | stringLiteral | Constant | charLiteral | ".";
      CompoundDatum.Rule = Statement | Abbreviation | Vector | ByteVector;
      Identifier.Rule = SimpleIdentifier | PeculiarIdentifier;
      IdentifierList.Rule = MakePlusRule(IdentifierList, null, Identifier);
      IdentifierListOpt.Rule = MakeStarRule(IdentifierListOpt, null, Identifier);

      //TODO: create PeculiarIdentifier custom terminal instead of var 
      // or just custom SchemeIdentifier terminal
      PeculiarIdentifier.Rule = Symbol("+") | "-" | "..."; // |"->" + subsequent; (should be!) 
      Abbreviation.Rule = AbbrevPrefix + Datum;
      AbbrevPrefix.Rule = Symbol("'") | "`" | ",@" | "," | "#'" | "#`" | "#,@" | "#,";
      Vector.Rule = "#(" + DatumListOpt + ")";
      ByteVector.Rule = "#vu8(" + Byte.Star() + ")";

      Statement.Rule = FunctionCall | SpecialForm;

      FunctionCall.Rule = LP + FunctionRef + DatumListOpt + RP;
      FunctionRef.Rule = Identifier | Statement;

      SpecialForm.Rule = DefineVarForm | DefineFunForm | LambdaForm | IfForm | CondForm | BeginForm | LetForm | LetRecForm;
      DefineVarForm.Rule = LP + "define" + Identifier + Datum + RP;
      DefineFunForm.Rule = LP + "define" + LP + Identifier + IdentifierListOpt + RP + DatumList + RP;
      LambdaForm.Rule = LP + "lambda" + LP + IdentifierListOpt + RP + DatumList + RP;
      IfForm.Rule = LP + "if" + Datum + Datum + Datum.Q() + RP;
      CondForm.Rule = LP + "cond" + CondClause.Plus() + CondElse.Q() + RP;
      CondClause.Rule = LP + Datum + DatumList + RP;
      CondElse.Rule = LP + "else" + DatumList + RP;
      LetForm.Rule = LP + "let" + LP + LetPair.Plus() + RP + DatumList + RP;
      LetRecForm.Rule = LP + "letrec" + LP + LetPair.Plus() + RP + DatumList + RP;
      BeginForm.Rule = LP + "begin" + DatumList + RP;
      LetPair.Rule = LP + Identifier + Datum + RP;


      #endregion 

      //Register brace  pairs
      RegisterBracePair("(", ")");
      RegisterBracePair("[", "]");

      //Filters and other stuff
      BraceMatchFilter filter = new BraceMatchFilter();
      TokenFilters.Add(filter);

      RegisterPunctuation(LP, RP);

    }//constructor

    private AstNode CreateFunctionCallNode(AstNodeArgs args) {
      string name = args.GetContent(0);
      AstNodeList funArgs = args.ChildNodes[1].ChildNodes;
      return new FunctionCallNode(args, name, funArgs); 
    }
    private AstNode CreateDefineVarNode(AstNodeArgs args) {
      //we skip the "define" keyword so the indexes are 1,2 - compare to CreateLetPairNode
      return new SetVarNode(args, args.ChildNodes[1] as IdentifierNode, args.ChildNodes[2]);
    }
    private AstNode CreateDefineFunNode(AstNodeArgs args) {
      //"define" keyword is at index 0
      IdentifierNode funName = args.ChildNodes[1] as IdentifierNode; 
      AstNode funParams = args.ChildNodes[2]; 
      AstNode funBody = args.ChildNodes[3];
      AstNode fun = new FunctionDefNode(args, funName, funParams, funBody);
      return fun;
    }
    private AstNode CreateLambdaNode(AstNodeArgs args) {
      AstNode funParams = args.ChildNodes[1];
      AstNode funBody = args.ChildNodes[2];
      return new AnonFunctionNode(args, funParams, funBody); 
    }
    private AstNode CreateIfThenElseNode(AstNodeArgs args) {
      AstNode test = args.ChildNodes[1];
      AstNode ifTrue = args.ChildNodes[2];
      AstNode ifFalse = args.ChildNodes[3];
      return new IfThenElseNode(args,test, ifTrue, ifFalse);
    }
    
    private AstNode CreateCondFormNode(AstNodeArgs args) {
      AstNodeList condClauses = args.ChildNodes[1].ChildNodes;
      AstNode elseNode = args.ChildNodes[2];
      AstNode elseCommands = (elseNode.IsEmpty()? null : elseNode.ChildNodes[1]);
      return new CondFormNode(args, condClauses, elseCommands);
    }
    private AstNode CreateCondClauseNode(AstNodeArgs args) {
      AstNode test = args.ChildNodes[0];
      DatumListNode command = args.ChildNodes[1] as DatumListNode;
      return new CondClauseNode(args, test, command);
    }

    private AstNode CreateBeginNode(AstNodeArgs args) {
      return new DatumListNode(args, args.ChildNodes[1].ChildNodes);
    }

  }//class

  


}//namespace

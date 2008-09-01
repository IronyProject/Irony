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
using Irony.Compiler.AST;

namespace Irony.Samples.Scheme {
  public class SchemeGrammar : Grammar {
    // It is loosely based on R6RS specs.  
    // See Grammar Errors tab in GrammarExplorer for remaining conflicts.
    public SchemeGrammar() {

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
      var charLiteral = new StringLiteral("Char", "'", ScanFlags.None); 
      var stringLiteral = new StringLiteral("String", "\"", ScanFlags.AllowAllEscapes);
      //Identifiers. Note: added "-", just to allow IDs starting with "->" 
      var SimpleIdentifier = new IdentifierTerminal("SimpleIdentifier", "_+-*/.@?!<>=", "_+-*/.@?!<>=$%&:^~");
      //                                                           name                extraChars      extraFirstChars  
      var Number = TerminalFactory.CreateSchemeNumber("Number");
      var Byte = new NumberLiteral("Byte", TermOptions.NumberIntOnly); 

      //Comments
      Terminal Comment = new CommentTerminal("Comment", "#|", "|#");
      Terminal LineComment = new CommentTerminal("LineComment", ";", "\n");
      NonGrammarTerminals.Add(Comment); //add comments explicitly to this list as it is not reachable from Root
      NonGrammarTerminals.Add(LineComment);
      #endregion

      #region NonTerminals
      var Module = new NonTerminal("Module");
      var Library = new NonTerminal("Library");
      var LibraryList = new NonTerminal("Library+");
      var Script = new NonTerminal("Script");

      var Abbreviation = new NonTerminal("Abbreviation");
      var Vector = new NonTerminal("Vector");
      var ByteList = new NonTerminal("ByteList");
      var ByteVector = new NonTerminal("ByteVector");
      var Datum = new NonTerminal("Datum"); //Datum in R6RS terms
      var DatumOpt = new NonTerminal("DatumOpt"); //Datum in R6RS terms
      var DatumList = new NonTerminal("Datum+", typeof(StatementListNode));
      var DatumListOpt = new NonTerminal("Datum*", typeof(StatementListNode));
      var Statement = new NonTerminal("Statement");
      var Atom = new NonTerminal("Atom");
      var CompoundDatum = new NonTerminal("CompoundDatum");
      var AbbrevPrefix = new NonTerminal("AbbrevPrefix");

      var LibraryName = new NonTerminal("LibraryName");
      var LibraryBody = new NonTerminal("LibraryBody");
      var ImportSection = new NonTerminal("ImportSection");
      var ExportSection = new NonTerminal("ExportSection");
      var ImportSpec = new NonTerminal("ImportSpec");
      var ImportSpecList = new NonTerminal("ImportSpecList");
      var ExportSpec = new NonTerminal("ExportSpec");
      var ExportSpecList = new NonTerminal("ExportSpecList");
      var LP = new NonTerminal("LP"); //"(" or "["
      var RP = new NonTerminal("RP"); // ")" or "]"
      var Identifier = new NonTerminal("Identifier", typeof(IdentifierNode));
      var IdentifierList = new NonTerminal("IdentifierList");
      var IdentifierListOpt = new NonTerminal("IdentifierListOpt");
      var PeculiarIdentifier = new NonTerminal("PeculiarIdentifier");
      var LibraryVersion = new NonTerminal("LibraryVersion");
      var VersionListOpt = new NonTerminal("VersionListOpt");

      var FunctionCall = new NonTerminal("FunctionCall", CreateFunctionCallNode);
      var FunctionRef = new NonTerminal("FunctionRef");
      var SpecialForm = new NonTerminal("SpecialForm");
      var DefineVarForm = new NonTerminal("DefineVarForm", CreateDefineVarNode);
      var DefineFunForm = new NonTerminal("DefineFunForm", CreateDefineFunNode);
      var LambdaForm = new NonTerminal("LambdaForm", CreateLambdaNode);
      var IfForm = new NonTerminal("IfForm", CreateIfThenElseNode);
      var CondForm = new NonTerminal("CondForm", CreateCondFormNode);
      var CondClause = new NonTerminal("CondClause", CreateCondClauseNode);
      var CondClauseList = new NonTerminal("CondClauseList");
      var CondElseOpt = new NonTerminal("CondElseOpt");
      var BeginForm = new NonTerminal("BeginForm", CreateBeginNode);
      var LetForm = new NonTerminal("LetForm"); //not implemented
      var LetRecForm = new NonTerminal("LetRecForm"); //not implemented
      var LetPair = new NonTerminal("LetPair");
      var LetPairList = new NonTerminal("LetPairList");
      #endregion

      #region Rules
      //
      // Using optional elements in Scheme grammar brings some nasty conflicts - by default the parser selects "shift" over reduce
      // which leads to failure to parse simple programs without libraries and export/import sections. 
      // This trouble comes from that the fact that Scheme has soooooo many parenthesis. Therefore, using a single next symbol 
      // as a lookahead (as it happens in LALR parsing) doesn't help much - the next symbol is almost always a parenthesis, 
      // not some meaningful symbol. That's why in the following expressions I had to use explicit listing of variants, 
      // instead of simply marking some elements as optional (see Module, Script, LibraryBody elements) - this clears the conflicts
      // but would make node construction more difficult.
      base.Root = Module; 

      LP.Rule = Symbol("(") | "[";  //R6RS allows mix & match () and []
      RP.Rule = Symbol(")") | "]";

      // Module.Rule = LibraryListOpt + Script; -- this brings conflicts
      Module.Rule = LibraryList + Script | Script;
      LibraryList.Rule = MakePlusRule(LibraryList, Library);
      Script.Rule = ImportSection + DatumList | DatumList; 

      //Library
      // the following doesn't work - brings conflicts that incorrectly resolved by default shifting
      //Library.Rule = LP + "library" + LibraryName + ExportSectionOpt + ImportSectionOpt + DatumListOpt + RP;
      Library.Rule = LP + "library" + LibraryName + LibraryBody + RP;
      //Note - we should be using DatumListOpt, but that brings 2 conflicts, so for now it is just DatumList
      //Note that the following style of BNF expressions is strongly discouraged - all productions should be of the same length, 
      // so that the process of mapping child nodes to parent's properties is straightforward.
      LibraryBody.Rule = ExportSection + ImportSection + DatumList
                       | ExportSection + DatumList
                       | ImportSection + DatumList
                       | DatumList; 
      LibraryName.Rule = LP + IdentifierList + LibraryVersion.Q() + RP;
      LibraryVersion.Rule = LP + VersionListOpt + RP; //zero or more subversion numbers
      VersionListOpt.Rule = MakeStarRule(VersionListOpt, Number);
      ExportSection.Rule = LP + "export" + ExportSpecList + RP;
      ImportSection.Rule = LP + "import" + ImportSpecList + RP;
      ExportSpecList.Rule = MakePlusRule(ExportSpecList, ExportSpec);
      ImportSpecList.Rule = MakePlusRule(ImportSpecList, ImportSpec);
      ExportSpec.Rule = Identifier | LP + "rename"  +  LP + Identifier + Identifier + RP + RP;
      ImportSpec.Rule = LP + Identifier + RP;   // - much more complex in R6RS

      //Datum
      Datum.Rule = Atom | CompoundDatum;
      DatumOpt.Rule = Empty | Datum;
      DatumList.Rule = MakePlusRule(DatumList, Datum);
      DatumListOpt.Rule = MakeStarRule(DatumListOpt, Datum);
      Atom.Rule = Number | Identifier | stringLiteral | Constant | charLiteral | ".";
      CompoundDatum.Rule = Statement | Abbreviation | Vector | ByteVector;
      Identifier.Rule = SimpleIdentifier | PeculiarIdentifier;
      IdentifierList.Rule = MakePlusRule(IdentifierList, Identifier);
      IdentifierListOpt.Rule = MakeStarRule(IdentifierListOpt, Identifier);

      //TODO: create PeculiarIdentifier custom terminal instead of var 
      // or just custom SchemeIdentifier terminal
      PeculiarIdentifier.Rule = Symbol("+") | "-" | "..."; // |"->" + subsequent; (should be!) 
      Abbreviation.Rule = AbbrevPrefix + Datum;
      AbbrevPrefix.Rule = Symbol("'") | "`" | ",@" | "," | "#'" | "#`" | "#,@" | "#,";
      Vector.Rule = "#(" + DatumListOpt + ")";
      ByteVector.Rule = "#vu8(" + ByteList + ")";
      ByteList.Rule = MakeStarRule(ByteList, Byte);

      Statement.Rule = FunctionCall | SpecialForm;

      FunctionCall.Rule = LP + FunctionRef + DatumListOpt + RP;
      FunctionRef.Rule = Identifier | Statement;

      SpecialForm.Rule = DefineVarForm | DefineFunForm | LambdaForm | IfForm | CondForm | BeginForm | LetForm | LetRecForm;
      DefineVarForm.Rule = LP + "define" + Identifier + Datum + RP;
      DefineFunForm.Rule = LP + "define" + LP + Identifier + IdentifierListOpt + RP + DatumList + RP;
      LambdaForm.Rule = LP + "lambda" + LP + IdentifierListOpt + RP + DatumList + RP;
      IfForm.Rule = LP + "if" + Datum + Datum + DatumOpt + RP;

      CondForm.Rule = LP + "cond" + CondClauseList + CondElseOpt + RP;
      CondClauseList.Rule = MakePlusRule(CondClauseList, CondClause);
      CondClause.Rule = LP + Datum + DatumList + RP;
      CondElseOpt.Rule = Empty | LP + "else" + DatumList + RP;
      LetForm.Rule = LP + "let" + LP + LetPairList + RP + DatumList + RP;
      LetRecForm.Rule = LP + "letrec" + LP + LetPairList + RP + DatumList + RP;
      BeginForm.Rule = LP + "begin" + DatumList + RP;
      LetPairList.Rule = MakePlusRule(LetPairList, LetPair);
      LetPair.Rule = LP + Identifier + Datum + RP;
      #endregion 

      //Register brace  pairs
      RegisterBracePair("(", ")");
      RegisterBracePair("[", "]");

      RegisterPunctuation(LP, RP);

      //Filters and other stuff
      BraceMatchFilter filter = new BraceMatchFilter();
      TokenFilters.Add(filter);


    }//constructor

    public override Irony.Runtime.LanguageRuntime CreateRuntime() {
      return new SchemeRuntime();
    }

    private string[] Operators = new string[] { "+", "-", "*", "/", "<", "=", ">" };
    private bool IsOperator(string op) {
      foreach (string s in Operators)
        if (s == op) return true;
      return false; 
    }
    private AstNode CreateFunctionCallNode(NodeArgs args) {
      IdentifierNode id = args.ChildNodes[0] as IdentifierNode;
      AstNodeList funArgs = args.ChildNodes[1].ChildNodes;
      if (IsOperator(id.Name)) {
        return new BinExprNode(args, funArgs[0], id.Name, funArgs[1]);
      } else {
        return new FunctionCallNode(args, id, funArgs);
      }
    }
    private AstNode CreateDefineVarNode(NodeArgs args) {
      //we skip the "define" keyword so the indexes are 1,2
      return new AssigmentNode(args, args.ChildNodes[1] as IdentifierNode, args.ChildNodes[2]);
    }
    private AstNode CreateDefineFunNode(NodeArgs args) {
      //"define" keyword is at index 0
      IdentifierNode funNameNode = args.ChildNodes[1] as IdentifierNode;
      funNameNode.Flags |= AstNodeFlags.AllocateSlot;
      AstNode funParams = args.ChildNodes[2]; 
      AstNode funBody = args.ChildNodes[3];
      AstNode anonFun = new AnonFunctionNode(args, funParams, funBody);
      AssigmentNode function = new AssigmentNode(args, funNameNode, anonFun);
      return function;
    }
    private AstNode CreateLambdaNode(NodeArgs args) {
      AstNode funParams = args.ChildNodes[1];
      AstNode funBody = args.ChildNodes[2];
      return new AnonFunctionNode(args, funParams, funBody); 
    }
    private AstNode CreateIfThenElseNode(NodeArgs args) {
      AstNode test = args.ChildNodes[1];
      AstNode ifTrue = args.ChildNodes[2];
      AstNode ifFalse = args.ChildNodes[3];
      return new IfNode(args, test, ifTrue, ifFalse);
    }
    
    private AstNode CreateCondFormNode(NodeArgs args) {
      AstNodeList condClauses = args.ChildNodes[1].ChildNodes;
      AstNode elseNode = args.ChildNodes[2];
      AstNode elseCommands = (elseNode.IsEmpty()? null : elseNode.ChildNodes[1]);
      return new CondFormNode(args, condClauses, elseCommands);
    }
    private AstNode CreateCondClauseNode(NodeArgs args) {
      AstNode test = args.ChildNodes[0];
      StatementListNode command = args.ChildNodes[1] as StatementListNode;
      return new CondClauseNode(args, test, command);
    }

    private AstNode CreateBeginNode(NodeArgs args) {
      return new StatementListNode(args, args.ChildNodes[1].ChildNodes);
    }

  }//class

  


}//namespace

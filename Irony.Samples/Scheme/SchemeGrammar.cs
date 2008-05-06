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

      #region Terminals
      ConstantTerminal Constant = new ConstantTerminal("Constant");
      Constant.Add("#T", true);
      Constant.Add("#t", true);
      Constant.Add("#F", false);
      Constant.Add("#f", false);
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
      Constant.Add("'()", null);

      // TODO: build SchemeCharLiteral
      // the following is nonsense, just to put something there
      StringLiteral charLiteral = new StringLiteral("Char", "'", ScanFlags.None); 
      Terminal stringLiteral = new StringLiteral("String", "\"", ScanFlags.AllowAllEscapes);
      //Identifiers. Note: added "-", just to allow IDs starting with "->" 
      IdentifierTerminal SimpleIdentifier = new IdentifierTerminal("SimpleIdentifier", "_+-*/.@?!<>=", "_+-*/.@?!<>=$%&:^~");
      //                                                           name                extraChars      extraFirstChars  
      Terminal Number = new NumberLiteral("Number");
      Terminal Byte = Number; 

      //Comments
      Terminal Comment = new CommentTerminal("Comment", "#|", "|#");
      Terminal LineComment = new CommentTerminal("LineComment", ";", "\n");
      NonGrammarTerminals.Add(Comment); //add comments explicitly to this list as it is not reachable from Root
      NonGrammarTerminals.Add(LineComment);
      #endregion

      #region NonTerminals
      NonTerminal Module = new NonTerminal("Module");
      NonTerminal Library = new NonTerminal("Library");
      NonTerminal Script = new NonTerminal("Script");

      NonTerminal Abbreviation = new NonTerminal("Abbreviation");
      NonTerminal Vector = new NonTerminal("Vector");
      NonTerminal ByteVector = new NonTerminal("ByteVector");
      NonTerminal Datum = new NonTerminal("Datum"); //Datum in R6RS terms
      NonTerminal DatumList = new NonTerminal("Datum+", typeof(DatumListNode));
      NonTerminal DatumListOpt = new NonTerminal("Datum*", typeof(DatumListNode));
      NonTerminal Statement = new NonTerminal("Statement");
      NonTerminal Atom = new NonTerminal("Atom");
      NonTerminal CompoundDatum = new NonTerminal("CompoundDatum");
      NonTerminal AbbrevPrefix = new NonTerminal("AbbrevPrefix");

      NonTerminal LibraryName = new NonTerminal("LibraryName");
      NonTerminal ImportSection = new NonTerminal("ImportSection");
      NonTerminal ExportSection = new NonTerminal("ExportSection");
      NonTerminal ImportSpec = new NonTerminal("ImportSpec");
      NonTerminal ExportSpec = new NonTerminal("ExportSpec");
      NonTerminal LP = new NonTerminal("LP"); //actually is "(" or "["
      NonTerminal RP = new NonTerminal("RP"); // ")" or "]"
      NonTerminal Identifier = new NonTerminal("Identifier", typeof(IdentifierNode));
      NonTerminal IdentifierList = new NonTerminal("IdentifierList");
      NonTerminal IdentifierListOpt = new NonTerminal("IdentifierListOpt");
      NonTerminal PeculiarIdentifier = new NonTerminal("PeculiarIdentifier");
      NonTerminal LibraryVersion = new NonTerminal("LibraryVersion");

      NonTerminal FunctionCall = new NonTerminal("FunctionCall", CreateFunctionCallNode);
      NonTerminal FunctionRef = new NonTerminal("FunctionRef");
      NonTerminal SpecialForm = new NonTerminal("SpecialForm");
      NonTerminal DefineVarForm = new NonTerminal("DefineVarForm", CreateDefineVarNode);
      NonTerminal DefineFunForm = new NonTerminal("DefineFunForm", CreateDefineFunNode);
      NonTerminal LambdaForm = new NonTerminal("LambdaForm", CreateLambdaNode);
      NonTerminal IfForm = new NonTerminal("IfForm", CreateIfThenElseNode);
      NonTerminal CondForm = new NonTerminal("CondForm", CreateCondFormNode);
      NonTerminal CondClause = new NonTerminal("CondClause", CreateCondClauseNode);
      NonTerminal CondElse = new NonTerminal("CondElse");
      NonTerminal BeginForm = new NonTerminal("BeginForm", CreateBeginNode);
      NonTerminal LetForm = new NonTerminal("LetForm"); //not implemented
      NonTerminal LetRecForm = new NonTerminal("LetRecForm"); //not implemented
      NonTerminal LetPair = new NonTerminal("LetPair");
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

      //TODO: create PeculiarIdentifier custom terminal instead of NonTerminal 
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

      base.Ops = new SchemeOps();

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

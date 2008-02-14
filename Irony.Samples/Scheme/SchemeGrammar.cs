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

    // Note that this is a sample grammar, not real Scheme production-quality grammar. 
    // It is loosely based on R6RS specs.  
    // See Grammar Errors tab in GrammarExplorer for remaining conflicts.
    public SchemeGrammar() {

      #region Terminals
      ConstantSetTerminal Constants = new ConstantSetTerminal("Constants");
      Constants.Add("#T", true);
      Constants.Add("#t", true);
      Constants.Add("#F", false);
      Constants.Add("#f", false);
      Constants.Add(@"#\nul", '\u0000');
      Constants.Add(@"#\alarm", '\u0007');
      Constants.Add(@"#\backspace", '\b');
      Constants.Add(@"#\tab", '\t');
      Constants.Add(@"#\linefeed", '\n');
      Constants.Add(@"#\vtab", '\v');
      Constants.Add(@"#\page", '\f');
      Constants.Add(@"#\return", '\r');
      Constants.Add(@"#\esc", '\u001B');
      Constants.Add(@"#\space", ' ');
      Constants.Add(@"#\delete", '\u007F');
      Constants.Add("'()", null);

      // the following probably doesn't work correctly
      // TODO: build SchemeCharLiteral
      Terminal charLiteral = new CharLiteral("CharLiteral", @"#\", string.Empty); 
      Terminal stringLiteral = new StringLiteral();
      //Identifiers. Note: added "-", just to allow IDs starting with "->" 
      IdentifierTerminal SimpleIdentifier = new IdentifierTerminal("SimpleIdentifier", "_+-*/.@?!<>=", "_!$%&*/:<=>?^~" + "+-");
      //                                                           name                extraChars      extraFirstChars  
      Terminal Number = new NumberTerminal("Number");
      Terminal Comment = new CommentTerminal("Comment", ";", "#|", "|#");
      Terminal Byte = Number; // new NumberTerminal("Byte"); //u8 in R6RS notation
      ExtraTerminals.Add(Comment); //add comment explicitly to this list as it is not reachable from Root
      #endregion

      #region NonTerminals
      NonTerminal Module = new NonTerminal("Module");
      NonTerminal Library = new NonTerminal("Library");
      NonTerminal Script = new NonTerminal("Script");

      NonTerminal Abbreviation = new NonTerminal("Abbreviation");
      NonTerminal Vector = new NonTerminal("Vector");
      NonTerminal ByteVector = new NonTerminal("ByteVector");
      NonTerminal Datum = new NonTerminal("Datum"); //Datum in R6RS terms
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
      NonTerminal Identifier = new NonTerminal("Identifier");
      NonTerminal PeculiarIdentifier = new NonTerminal("PeculiarIdentifier");
      NonTerminal LibraryVersion = new NonTerminal("LibraryVersion");

      NonTerminal FunctionCall = new NonTerminal("FunctionCall");
      NonTerminal SpecialForm = new NonTerminal("SpecialForm");
      NonTerminal DefineForm = new NonTerminal("DefineForm");
      NonTerminal DefineFunForm = new NonTerminal("DefineFunForm");
      NonTerminal LambdaForm = new NonTerminal("LambdaForm");
      NonTerminal IfForm = new NonTerminal("IfForm", typeof(IfNode));
      NonTerminal CondForm = new NonTerminal("CondForm");
      NonTerminal CondCase = new NonTerminal("CondCase");
      NonTerminal CondElse = new NonTerminal("CondElse");
      NonTerminal BeginForm = new NonTerminal("BeginForm");
      NonTerminal LetForm = new NonTerminal("LetForm");
      NonTerminal LetPair = new NonTerminal("LetPair");
      #endregion

      #region Rules
      base.Root = Module;

      Module.Rule = Library.Plus() + Script | Script;
      Script.Rule = ImportSection + Datum.Plus() | Datum.Plus();
      LP.Rule = Symbol("(") | "[";  //R6RS allows mix & match () and []; exact match is enforced by token filter
      RP.Rule = Symbol(")") | "]";

      //Library
      Library.Rule = LP + "library" + LibraryName + ExportSection.Q() + ImportSection.Q() + Statement.Star() + RP;
      LibraryName.Rule = LP + Identifier.Plus() + LibraryVersion.Q() + RP;
      LibraryVersion.Rule = LP + Number.Star() + RP; //zero or more subversion numbers
      ExportSection.Rule = LP + "export" + ExportSpec.Plus() + RP;
      ImportSection.Rule = LP + "import" + ImportSpec.Plus() + RP;
      ExportSpec.Rule = Identifier | LP + "rename"  +  LP + Identifier + Identifier + RP + RP;
      ImportSpec.Rule = LP + Identifier + RP;   //FRI - much more complex in R6RS

      //Datum
      Datum.Rule = Atom | CompoundDatum;
      Atom.Rule = Number | Identifier | stringLiteral | Constants | charLiteral | ".";
      CompoundDatum.Rule = Statement | Abbreviation | Vector | ByteVector;
      Identifier.Rule = SimpleIdentifier | PeculiarIdentifier;
      //TODO: create PeculiarIdentifier custom terminal instead of NonTerminal 
      // or just custom SchemeIdentifier terminal
      PeculiarIdentifier.Rule = Symbol("+") | "-" | "..."; // |"->" + subsequent; (should be!) 
      Abbreviation.Rule = AbbrevPrefix + Datum;
      AbbrevPrefix.Rule = Symbol("'") | "`" | ",@" | "," | "#'" | "#`" | "#,@" | "#,";
      Vector.Rule = "#(" + Datum.Star() + ")";
      ByteVector.Rule = "#vu8(" + Byte.Star() + ")";

      Statement.Rule = FunctionCall | SpecialForm;
      FunctionCall.Rule = LP + Datum.Plus() + RP; 
      SpecialForm.Rule = DefineForm | DefineFunForm | LambdaForm | IfForm | CondForm | BeginForm | LetForm;
      DefineForm.Rule = LP + "define" + Identifier + Datum + RP;
      DefineFunForm.Rule = LP + "define" + LP + Identifier + Identifier.Star() + RP + Statement.Plus() + RP;
      LambdaForm.Rule = LP + "lambda" + LP + Identifier.Star() + RP + Statement.Plus() + RP;
      IfForm.Rule = LP + "if" + Datum + Datum + Datum.Q() + RP;
      CondForm.Rule = LP + "cond" + CondCase.Plus() + CondElse.Q() + RP;
      CondCase.Rule = LP + Datum + Datum + RP;
      CondElse.Rule = LP + "else" + Datum.Plus() + RP;
      LetForm.Rule = LP + "let" + LP + LetPair.Plus() + RP + Datum.Plus() + RP;
      BeginForm.Rule = LP + "begin" + Datum.Plus() + RP;
      LetPair.Rule = LP + Identifier + Datum + RP;


      #endregion 

      //Register brace  pairs
      RegisterBracePair("(", ")");
      RegisterBracePair("[", "]");

      //Filters and other stuff
      BraceMatchFilter filter = new BraceMatchFilter();
      TokenFilters.Add(filter);

      //RegisterPunctuation( LP, RP); -- we could do this, but doesn't work because raw symbols ()[] get "bubbled up" -
      // see Parser.CreateNode method
      RegisterPunctuation("(", ")", "[", "]");

      //this.CaseSensitive = false; -- just for testing case-insensitive parser

    }//constructor


  }//class

  


}//namespace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Irony.Samples {

    [Language("IronAsm2", "1.0", "x86 MASM assembler")]
    public class AsmGrammar2 : Grammar
    {
        public AsmGrammar2() : base(false)
        {
            GrammarComments = @"Simple MASM parser test.";

            // ----------------------------------------------------------------------------------------------------------
            // Tokens
            // ----------------------------------------------------------------------------------------------------------
            var NUMBER = new NumberLiteral("number");
            NUMBER.DefaultIntTypes = new TypeCode[] { TypeCode.Single, TypeCode.Int32, TypeCode.Int64 };

            var TEXT_LITERAL = new StringLiteral("String");
            TEXT_LITERAL.AddStartEnd("<", ">", StringFlags.AllowsAllEscapes);

            var IDENTIFIER = new IdentifierTerminal("Identifier", "_$?@", "_$?@");

            var COMMA = ToTerm(",","Comma");

            var POINT = ToTerm(".", "Point");

            var WS = new NonTerminal("Whitespace");
            WS.Rule = ToTerm(" ") | "\t";
            var WSP = WS.Plus();
            var WSS = WS.Star();

            var LBR = ToTerm("[");
            LBR.Options = TermOptions.IsOpenBrace;

            var RBR = ToTerm("]");
            RBR.Options = TermOptions.IsCloseBrace;

            var LBP = ToTerm("(");
            LBP.Options = TermOptions.IsOpenBrace;

            var RBP = ToTerm(")");
            RBP.Options = TermOptions.IsCloseBrace;
            
            var COLON = ToTerm(":", "Colon");

            // NewLine is explicitly handle in the grammar
            var NL = NewLine;

            // Comment
            var comment = new CommentTerminal("comment", ";", "\n", "\r", "\u2085", "\u2028", "\u2029");
            NonGrammarTerminals.Add(comment);

            // Set Whitespace no empty string. We handle whitspace in grammar to keep the unstructured tokens organized
            WhitespaceChars = "";

            // No delimiters (same for unstructured tokens)
            Delimiters = "";

            // ----------------------------------------------------------------------------------------------------------
            // Non Terminals
            // ----------------------------------------------------------------------------------------------------------

            //var PreprocessDir = new NonTerminal("PreprocessDir");
            // var TextLine = new NonTerminal("TextLine");
            var MacroDir = new NonTerminal("MacroDir");
            var MacroParmList = new NonTerminal("MacroParmList");
            var MacroParmListOpt = new NonTerminal("MacroParmListOpt");
            var MacroBody = new NonTerminal("MacroBody");
            var MacroParm = new NonTerminal("MacroParm");
            var MacroStmtList = new NonTerminal("MacroStmtList");
            var MacroStmt = new NonTerminal("MacroStmt");
            var ParmTypeOptional = new NonTerminal("ParmTypeOptional");
            var ParmType = new NonTerminal("ParmType");
            var Exitm = new NonTerminal("Exitm");
            var ExitmOptional = new NonTerminal("ExitmOptional");
            //var MacroCall = new NonTerminal("MacroCall");
            //var MacroArgList = new NonTerminal("MacroArgList");
            //var MacroArgListOptional = new NonTerminal("MacroArgListOptional");
            //var MacroArg = new NonTerminal("MacroArg");
            var NonMacroKeywords = new NonTerminal("OtherKeywords");
            var PreprocessLine = new NonTerminal("ProcLine");
            var PreprocessList = new NonTerminal("PreprocessList");
            var UnStructuredToken = new NonTerminal("UnStructuredToken");
            var UnStructuredTokens = new NonTerminal("UnStructuredTokens");
            var UnStructuredTokenLine = new NonTerminal("UnStructuredTokenLine");

            // ----------------------------------------------------------------------------------------------------------
            // BNF Rules
            // ----------------------------------------------------------------------------------------------------------
            PreprocessList.Rule = MakeStarRule(PreprocessList, null, PreprocessLine);

            PreprocessLine.Rule = MacroDir | UnStructuredTokenLine;

            UnStructuredToken.Rule = IDENTIFIER
                            | WS
                            | NonMacroKeywords
                            | NUMBER
                            | LBR | RBR | LBP | RBP
                            | COMMA
                            | POINT
                            | COLON
                            ;
            UnStructuredTokens.Rule = MakeStarRule(UnStructuredTokens, null, UnStructuredToken);

            UnStructuredTokenLine.Rule = UnStructuredTokens + NL;

            // Partial test list of keywords
            NonMacroKeywords.Rule = ToTerm("mov") | "inc" | "eax" | "ebx" | "ecx" | "end" | "jmp" | ".code" | ".model" | ".data";

            //macroDir
            // id MACRO [[ macroParmList ]] ;;
            //macroBody
            //ENDM ;;
            MacroDir.Rule = IDENTIFIER + WSP + PreferShiftHere() + "MACRO" + MacroParmListOpt + WSS + NL
                //+ MacroBody
                + MacroStmtList
                + "ENDM" + WSS + NL;

            MacroParmListOpt.Rule = Empty | (WSP + MacroParmList);

            //macroParmList
            //    macroParm
            //    | macroParmList , [[ NL ]] macroParm 
            var comma_decl = new NonTerminal("comma_decl");
            comma_decl.Rule = COMMA + WSS + (Empty | NL) + WSS;  
            MacroParmList.Rule = MakeStarRule(MacroParmList, comma_decl, MacroParm);

            // macroParm := id [[ : parmType ]]
            MacroParm.Rule = IDENTIFIER + ParmTypeOptional;
            ParmTypeOptional.Rule = Empty | (COLON + ParmType);

            //parmType
            //    REQ
            //    | = textLiteral
            //    | VARARG
            ParmType.Rule = ToTerm("REQ")
                | "=" + TEXT_LITERAL
                | "VARARG";

            //macroBody
            //    [[ localList ]]
            //    macroStmtList 
            //MacroBody.Rule = MacroStmtList;

            //macroStmtList
            //    macroStmt ;;
            //    | macroStmtList macroStmt ;; 
            MacroStmtList.Rule = MakeStarRule(MacroStmtList, null, MacroStmt);

            // Uncomplete macroStmt (just using macroDir and exitM)
            //macroStmt
            //  directive
            //  | exitmDir
            //  | : macroLabel
            //  | GOTO macroLabel
            MacroStmt.Rule = MacroDir
                | Exitm
                | UnStructuredTokenLine;
            
            //exitmDir:
            //  EXITM
            //  | EXITM textItem
            //RegexBasedTerminal regBasedTerminal = new RegexBasedTerminal("exitm-content","<.*>", "<");

            Exitm.Rule = ToTerm("EXITM") + PreferShiftHere() + ExitmOptional + NL;

            ExitmOptional.Rule = Empty | WSS + TEXT_LITERAL;

            ////macroCall
            ////    id macroArgList ;;
            ////    | id ( macroArgList )
            //MacroCall.Rule = IDENTIFIER + PreferShiftHere() + MacroArgListOptional;

            //MacroArgListOptional.Rule = "(" + MacroArgList + ")" |  WSP + MacroArgList | Empty;

            //var comma_arg = new NonTerminal("comma_nl");
            //comma_arg.Rule = COMMA + WSS;
            //MacroArgList.Rule = MakeStarRule(MacroArgList, comma_arg, MacroArg);

            //MacroArg.Rule = (NUMBER | TEXT_LITERAL | IDENTIFIER) + WSS;

            Root = PreprocessList;       // Set grammar root

            //MarkTransient(ParmTypeOptional, MacroArgListOptional);

            //automatically add NL before EOF so that our BNF rules work correctly when there's no final line break in source
            LanguageFlags = LanguageFlags.NewLineBeforeEOF;

            MarkReservedWords("MACRO");
        }
    }

}

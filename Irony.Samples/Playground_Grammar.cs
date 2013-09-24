using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;
using System.Globalization;
using Irony.Interpreter;
using Irony.Interpreter.Ast;

namespace Irony.Samples.TestBed {
  public class HTMLGrammar : Irony.Parsing.Grammar {
    public HTMLGrammar() {
      KeyTerm leftAnguralBracket = new KeyTerm("<", "LeftAngularBarakcet");
      KeyTerm rightAnguralBracket = new KeyTerm(">", "RightAngularBarakcet");
      KeyTerm leftAngularBracketEndTag = new KeyTerm("</", "LeftAngularBracketEndTag");
      KeyTerm rightAngularBracketEndTag = new KeyTerm("/>", "RightAngularBracketEndTag");


      NonTerminal element = new NonTerminal("Element");
      NonTerminal emptyElementTag = new NonTerminal("EmptyElementTag");
      NonTerminal startTag = new NonTerminal("StartTag");
      NonTerminal content = new NonTerminal("Content");
      NonTerminal endTag = new NonTerminal("EndTag");
      RegexBasedTerminal name = new RegexBasedTerminal("Name", "\\w+");

      element.Rule = emptyElementTag | startTag + content + endTag;
      emptyElementTag.Rule = leftAnguralBracket + name + rightAngularBracketEndTag;
      startTag.Rule = leftAnguralBracket + name + rightAnguralBracket;
      endTag.Rule = leftAngularBracketEndTag + name + rightAnguralBracket;
      content.Rule = MakeStarRule(content, element);

      this.Root = element;
    }
  }
}


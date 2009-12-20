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
using System.Linq;
using System.Text;

namespace Irony.Parsing.Construction { 

  internal class GrammarDataBuilder {
    LanguageData _language;
    Grammar _grammar;
    GrammarData _grammarData;
    int _unnamedCount; //internal counter for generating names for unnamed non-terminals
    internal int _lastItemId; //each LR0Item gets its unique ID, last assigned (max) Id is kept in this field

    internal GrammarDataBuilder(LanguageData language) {
      _language = language;
      _grammar = _language.Grammar;
    }

    internal void Build() {
      _grammarData = _language.GrammarData;
      _grammarData.AugmentedRoot = new NonTerminal(_grammar.Root.Name + "'");
      _grammarData.AugmentedRoot.Rule = _grammar.Root + _grammar.Eof;
      CollectTermsFromGrammar();
      AssignWhitespaceAndDelimiters(); 
      InitTermLists(_grammarData);
      FillOperatorReportGroup(); 
      FindClosingBraces(); 
      CreateProductions();
      ComputeNonTerminalsNullability(_grammarData);
      ComputeTailsNullability(_grammarData);
      ValidateGrammar(); 
    }

    private void CollectTermsFromGrammar() {
      _unnamedCount = 0;
      _grammarData.AllTerms.Clear();
      //Start with NonGrammarTerminals, and set IsNonGrammar flag
      foreach (Terminal t in _grammarData.Grammar.NonGrammarTerminals) {
        t.SetOption(TermOptions.IsNonGrammar);
        _grammarData.AllTerms.Add(t);
      }
      CollectTermsRecursive(_grammarData.AugmentedRoot);
    }

    private void CollectTermsRecursive(BnfTerm term) {
      // Do not add pseudo terminals defined as static singletons in Grammar class (Empty, Eof, etc)
      //  We will never see these terminals in the input stream.
      //   Filter them by type - their type is exactly "Terminal", not derived class. 
      if (term.GetType() == typeof(Terminal)) return;
      if (_grammarData.AllTerms.Contains(term)) return;
      _grammarData.AllTerms.Add(term);
      NonTerminal nt = term as NonTerminal;
      if (nt == null) return;

      if (nt.Name == null) {
        if (nt.Rule != null && !string.IsNullOrEmpty(nt.Rule.Name))
          nt.Name = nt.Rule.Name;
        else
          nt.Name = "NT" + (_unnamedCount++);
      }
      if (nt.Rule == null)
        _language.Errors.AddAndThrow(GrammarErrorLevel.Error, null, Resources.ErrNtRuleIsNull, nt.Name);
      //check all child elements
      foreach (BnfTermList elemList in nt.Rule.Data)
        for (int i = 0; i < elemList.Count; i++) {
          BnfTerm child = elemList[i];
          if (child == null) {
            _language.Errors.Add(GrammarErrorLevel.Error, null, Resources.ErrRuleContainsNull, nt.Name, i);
            continue; //for i loop 
          }
          //Check for nested expression - convert to non-terminal
          BnfExpression expr = child as BnfExpression;
          if (expr != null) {
            child = new NonTerminal(null, expr);
            elemList[i] = child;
          }
          CollectTermsRecursive(child);
        }//for i
    }//method

    private void FillOperatorReportGroup() {
      foreach(var group in _grammar.TermReportGroups)
        if(group.GroupType == TermReportGroupType.Operator) {
          foreach(var term in _grammarData.Terminals)
            if (term.OptionIsSet(TermOptions.IsOperator))
              group.Terminals.Add(term); 
          return; 
        }
    }

    private void FindClosingBraces() {
      foreach(var term in _grammar.KeyTerms.Values) {
        if (term.OptionIsSet(TermOptions.IsCloseBrace))
          _grammarData.ClosingBraces.Add(term.Text);
      }
    }

    private void AssignWhitespaceAndDelimiters() {
      var delims = _grammar.Delimiters;
        //if it was not assigned by language creator, let's guess them
      if(delims == null) {
        delims = string.Empty;
        var commonDelims = ",;[](){}";  //chars usually used as delimiters in programming languages
        foreach(var delim in commonDelims) 
          if (_grammar.KeyTerms.ContainsKey(delim.ToString()))  //if language uses this char as a Term, then include it
            delims += delim; 
      }//if 
      _grammarData.WhitespaceAndDelimiters = _grammar.WhitespaceChars + delims 
           + "\n"  //in case if it is removed from whitespace chars by NewLineTerminal 
           + "\0"; //EOF: SourceStream returns this char when we reach end of file
    }


    private static void InitTermLists(GrammarData data) {
      //Collect terminals and NonTerminals
      foreach (BnfTerm term in data.AllTerms) {  //remember - we may have hints, so it's not only terminals and non-terminals
        if (term is NonTerminal) data.NonTerminals.Add((NonTerminal)term);
        if (term is Terminal) data.Terminals.Add((Terminal)term);
      }
      data.Terminals.Sort(Terminal.ByName);
      //Mark keywords - any "word" symbol directly mentioned in the grammar
      foreach (var term in data.Terminals) {
        var symTerm = term as KeyTerm;
        if (symTerm == null) continue;
        if (symTerm.Text.Length > 0 && char.IsLetter(symTerm.Text[0]))
          symTerm.SetOption(TermOptions.IsKeyword); 
      }//foreach term
      //Init all terms
      foreach (var term in data.AllTerms)
        term.Init(data);
    }//method

    private void CreateProductions() {
      _lastItemId = 0;
      //CheckWrapTailHints() method may add non-terminals on the fly, so we have to use for loop here (not foreach)
      for (int i = 0; i < _grammarData.NonTerminals.Count; i++) {
        var nt = _grammarData.NonTerminals[i];
        nt.Productions.Clear();
        //Get data (sequences) from both Rule and ErrorRule
        BnfExpressionData allData = new BnfExpressionData();
        allData.AddRange(nt.Rule.Data);
        if (nt.ErrorRule != null)
          allData.AddRange(nt.ErrorRule.Data);
        //actually create productions for each sequence
        foreach (BnfTermList prodOperands in allData) {
          Production prod = CreateProduction(nt, prodOperands);
          nt.Productions.Add(prod);
        }//foreach prodOperands
      }
    }

    private Production CreateProduction(NonTerminal lvalue, BnfTermList operands) {
      Production prod = new Production(lvalue);
      GrammarHintList hints = null;
      //create RValues list skipping Empty terminal and collecting grammar hints
      foreach (BnfTerm operand in operands) {
        if (operand ==  _grammar.Empty)
          continue;
        //Collect hints as we go - they will be added to the next non-hint element
        GrammarHint hint = operand as GrammarHint;
        if (hint != null) {
          if (hints == null) hints = new GrammarHintList();
          hints.Add(hint);
          continue;
        }
        //Add the operand and create LR0 Item
        prod.RValues.Add(operand);
        prod.LR0Items.Add(new LR0Item(_lastItemId++, prod, prod.RValues.Count - 1, hints));
        hints = null;
      }//foreach operand
      //set the flags
      if (prod.RValues.Count == 0)
        prod.Flags |= ProductionFlags.IsEmpty;
      //Add final LRItem
      ComputeProductionFlags(prod); 
      prod.LR0Items.Add(new LR0Item(_lastItemId++, prod, prod.RValues.Count, hints));
      return prod;
    }
    private void ComputeProductionFlags(Production production) {
      production.Flags = ProductionFlags.None;
      int transListCount = 0; 
      foreach (var rv in production.RValues) {
        //Check if it is a Terminal or Error element
        var t = rv as Terminal;
        if (t != null) {
          production.Flags |= ProductionFlags.HasTerminals;
          if (t.Category == TokenCategory.Error) production.Flags |= ProductionFlags.IsError;
        }
        if(rv.OptionIsSet(TermOptions.IsPunctuation)) continue;
        if (rv.OptionIsSet(TermOptions.IsTransient) && rv.OptionIsSet(TermOptions.IsList))
          transListCount++;
        else
          transListCount += 100; //so it will never be 1
      }//foreach
      //Set TransientListCopy flag
      if (transListCount == 1) //if only one transient list and some punctuation symbols
        production.Flags |= ProductionFlags.TransientListCopy;
      //Set IsListBuilder flag
      if (production.RValues.Count > 0 && production.RValues[0] == production.LValue
          && production.LValue.OptionIsSet(TermOptions.IsList))
        production.Flags |= ProductionFlags.IsListBuilder;
    }//method

    private static void ComputeNonTerminalsNullability(GrammarData data) {
      NonTerminalList undecided = data.NonTerminals;
      while (undecided.Count > 0) {
        NonTerminalList newUndecided = new NonTerminalList();
        foreach (NonTerminal nt in undecided)
          if (!ComputeNullability(nt))
            newUndecided.Add(nt);
        if (undecided.Count == newUndecided.Count) return;  //we didn't decide on any new, so we're done
        undecided = newUndecided;
      }//while
    }

    private static bool ComputeNullability(NonTerminal nonTerminal) {
      foreach (Production prod in nonTerminal.Productions) {
        if (prod.RValues.Count == 0) {
          nonTerminal.SetOption(TermOptions.IsNullable);
          return true; //decided, Nullable
        }//if 
        //If production has terminals, it is not nullable and cannot contribute to nullability
        if (prod.IsSet(ProductionFlags.HasTerminals)) continue;
        //Go thru all elements of production and check nullability
        bool allNullable = true;
        foreach (BnfTerm child in prod.RValues) {
          allNullable &= child.OptionIsSet(TermOptions.IsNullable);
        }//foreach child
        if (allNullable) {
          nonTerminal.SetOption(TermOptions.IsNullable);
          return true;
        }
      }//foreach prod
      return false; //cannot decide
    }

    private static void ComputeTailsNullability(GrammarData data) {
      foreach (var nt in data.NonTerminals) {
        foreach (var prod in nt.Productions) {
          var count = prod.LR0Items.Count;
          for (int i = count - 1; i >= 0; i--) {
            var item = prod.LR0Items[i];
            item.TailIsNullable = true;
            if (item.Current == null) continue;
            if (!item.Current.OptionIsSet(TermOptions.IsNullable))
              break; //for i
          }//for i
        }//foreach prod
      }
    }

    #region Grammar Validation
    private void ValidateGrammar() {
      //Check that if CreateAst flag is set then AstNodeType or AstNodeCreator is assigned on all non-transient nodes.
      if (_grammar.FlagIsSet(LanguageFlags.CreateAst)) {
        var errorSet = new BnfTermSet();
        foreach (var nt in _grammarData.NonTerminals)
          if (nt.AstNodeCreator == null && nt.AstNodeType == null 
                  && !nt.OptionIsSet(TermOptions.IsTransient) && nt != _grammarData.AugmentedRoot)
            errorSet.Add(nt);
        if (errorSet.Count > 0)
          this._language.Errors.Add(GrammarErrorLevel.Warning, null, Resources.ErrNodeTypeNotSetOn, errorSet.ToString());
      }
    }//method
    #endregion

  }//class
}

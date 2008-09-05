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


namespace Irony.Compiler.Lalr {
  // ParserControlData is a container for all information used by LALR Parser in input processing.
  // The state graph entry is InitialState state; the state graph encodes information usually contained 
  // in what is known in literature as transiton/goto tables.
  // The graph is built from the language grammar by ParserControlDataBuilder instance. 
  // See Dragon book or other book on compilers on details of LALR parsing and parsing tables construction. 

  public class ParserControlData {
    public Grammar Grammar;
    public NonTerminal AugmentedRoot;
    public ParserState InitialState;
    public ParserState FinalState;
    public readonly NonTerminalList NonTerminals = new NonTerminalList();

    public readonly ProductionList Productions = new ProductionList();
    public readonly ParserStateList States = new ParserStateList();
    
    public bool AnalysisCanceled;  //True if grammar analysis was canceled due to errors

    public ParserControlData(Grammar grammar) {
      Grammar = grammar;
    }
  }


  public enum ParserActionType {
    Shift,
    Reduce,
    Operator,  //shift or reduce depending on operator associativity and precedence
  }

  /// <summary>
  /// A container for LALR Parser-specific information for non-terminals
  /// about the term.
  /// </summary> 
  public class NtData {
    public NonTerminal NonTerminal;
    public bool Nullable;
    public readonly ProductionList Productions = new ProductionList();

    public readonly StringSet Firsts = new StringSet();
    public readonly NonTerminalList PropagateFirstsTo = new NonTerminalList();

    private NtData(NonTerminal  nonTerminal) {
      this.NonTerminal = nonTerminal;
      nonTerminal.ParserData = this;
    }
    public static NtData GetOrCreate(NonTerminal nonTerminal) {
      if (nonTerminal.ParserData != null)
        return nonTerminal.ParserData as NtData;
      else
        return new NtData(nonTerminal);
    }
  }//class

  public class NtDataList : List<NtData> {
    public void Add(NonTerminal term) {
      base.Add(NtData.GetOrCreate(term));
    }
  }

  public partial class ParserState {
    public readonly string Name;
    public readonly ActionRecordTable Actions = new ActionRecordTable();
    public readonly LRItemList Items = new LRItemList();

    public ParserState(string name, LRItem item) {
      Name = name;
      Items.Add(item);
    }
    public ParserState(string name, LR0ItemList coreItems) {
      Name = name;
      foreach (LR0Item coreItem in coreItems)
        Items.Add(new LRItem(this, coreItem));
    }
    public override string ToString() {
      return Name;
    }
  }//class

  public class ParserStateList : List<ParserState> { }
  public class ParserStateTable : Dictionary<string, ParserState> { } //hash table

  public class ActionRecord {
    public string Key;
    public ParserActionType ActionType = ParserActionType.Shift;
    public ParserState NewState;
    public readonly ProductionList ReduceProductions = new ProductionList(); //may be more than one, in case of conflict
    public readonly LRItemList ShiftItems = new LRItemList();
    public bool ConflictResolved; 

    internal ActionRecord(string key, ParserActionType type, ParserState newState, Production reduceProduction) {
      this.Key = key;
      this.ActionType = type;
      this.NewState = newState; 
      if (reduceProduction != null)
        ReduceProductions.Add(reduceProduction);
    }
    public ActionRecord CreateDerived(ParserActionType type, Production reduceProduction) {
      return new ActionRecord(this.Key, type, this.NewState, reduceProduction);
    }

    public Production Production { 
      get {return ReduceProductions.Count > 0? ReduceProductions[0] : null;}
    }
    public NonTerminal NonTerminal {
      get { return Production == null? null: Production.LValue; }
    }
    public int PopCount {
      get { return Production.RValues.Count;}
    }
    public bool HasConflict() {
      // This function is used by parser to determine if it needs to call OnActionConflict method in Grammar.
      // Even if conflict is resolved, we still need to return true to force parser to invoke method.
      // This is necessary to make parser work properly in situation like this: in c#, the "<" symbol is 
      // both operator symbol and opening brace for type parameter. When used purely as operator symbol, 
      //  it is involved in shift/reduced conflict resolved by operator precedence. Still, before parser starts 
      // acting based on precedence, a custom grammar method should decide - is it really an operator or type parameter
      // bracket.
      //if (ConflictResolved) return false; -- this should be commented out
      return ShiftItems.Count > 0 && ReduceProductions.Count > 0 ||
        ReduceProductions.Count > 1; 
    }
    public override string ToString() {
      string result = ActionType.ToString();
      if (ActionType == ParserActionType.Reduce && ReduceProductions.Count > 0)
        result += " on " + ReduceProductions[0];
      return result;
    }

  }//class ActionRecord

  public class ActionRecordTable : Dictionary<string, ActionRecord> { }

  [Flags]
  public enum ProductionFlags {
    None = 0,
    IsInitial = 0x01,    //is initial production
    HasTerminals = 0x02, //contains terminal
    IsError = 0x04,      //contains Error terminal
    IsEmpty = 0x08,
  }

  public class Production {
    public ProductionFlags Flags;
    public readonly NonTerminal LValue;                              // left-side element
    public readonly BnfTermList RValues = new BnfTermList(); //the right-side elements sequence
    public readonly GrammarHintList Hints = new GrammarHintList();
    public readonly LR0ItemList LR0Items = new LR0ItemList();        //LR0 items based on this production 
    public Production(NonTerminal lvalue) {
      LValue = lvalue;
    }//constructor

    public bool IsSet(ProductionFlags flag) {
      return (Flags & flag) != ProductionFlags.None;
    }

    public override string ToString() {
      return ToString(-1); //no dot
    }

    //Utility method used by Production and LR0Item
    internal string ToString(int dotPosition) {
      char dotChar = '\u00B7'; //dot in the middle of the line
      StringBuilder bld = new StringBuilder();
      bld.Append(LValue.Name);
      bld.Append(" -> ");
      for (int i = 0; i < RValues.Count; i++) {
        if (i == dotPosition)
          bld.Append(dotChar);
        bld.Append(RValues[i].Name);
        bld.Append(" ");
      }//for i
      if (dotPosition == RValues.Count)
        bld.Append(dotChar);
      return bld.ToString();
    }

  }//Production class

  public class ProductionList : List<Production> { }

  public class LRItem {
    public readonly ParserState State;
    public readonly LR0Item Core;
    public readonly LRItemList PropagateTargets = new LRItemList(); //used for lookaheads propagation
    public readonly StringSet Lookaheads = new StringSet();
    public readonly StringSet NewLookaheads = new StringSet();
    public LRItem(ParserState state, LR0Item core) {
      State = state;
      Core = core;
    }
    public override string ToString() {
      return Core.ToString() + "  LOOKAHEADS: " + TextUtils.Cleanup(Lookaheads.ToString(" "));
    }
  }//LRItem class

  public class LRItemList : List<LRItem> { }

  public partial class LR0Item {
    public readonly Production Production;
    public readonly int Position;

    public readonly StringSet TailFirsts = new StringSet(); //tail is a set of elements after the Current element
    public bool TailIsNullable = false;
    
    //automatically generated IDs - used for building keys for lists of kernel LR0Items
    // which in turn are used to quickly lookup parser states in hash
    internal readonly int ID;
    internal static int _maxID;
    private string _toString; //caches the ToString() value

    public LR0Item(Production production, int position) {
      Production = production;
      Position = position;
      ID = _maxID++;
    }
    //The after-dot element
    public BnfTerm Current {
      get {
        if (Position < Production.RValues.Count)
          return Production.RValues[Position];
        else
          return null;
      }
    }
    public bool IsKernel {
      get { return Position > 0 || (Production.IsSet(ProductionFlags.IsInitial) && Position == 0); }
    }
    public override string ToString() {
      if (_toString == null)
        _toString = Production.ToString(Position);
      return _toString;
    }
  }//LR0Item

  public class LR0ItemList : List<LR0Item> { }


}//namespace

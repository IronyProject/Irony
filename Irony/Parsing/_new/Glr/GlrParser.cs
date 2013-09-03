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
using Irony.Parsing.Construction;
using System.Diagnostics;

namespace Irony.Parsing {

  public partial class ParsingContext {
    public GlrStep CurrentStep;
  }

  public partial class ParserState {
    public Dictionary<BnfTerm, GlrParserAction> GlrActions;
  }

  public partial class Production {
    public ParseTreeNode ExecuteReduce(IEnumerable<ParseTreeNode> childNodes) {
      return null; 
    }
  }

  public class GlrParserAction {
    public List<LRItem> Reduces = new List<LRItem>();
    public List<LRItem> Shifts = new List<LRItem>();
    public void GetItems(ParsingContext context, out IList<LRItem> reduces, out IList<LRItem> shifts) {
      reduces = Reduces;
      shifts = Shifts;
    }
  }

  public class GlrStep {
    public ParseTreeNode Input;
    public List<GlrStackItem> ShiftBases = new List<GlrStackItem>(); // stack tops after all reduces, before shifts
    public Dictionary<ParserState, GlrStackItem> Shifts = new Dictionary<ParserState,GlrStackItem>(); //result stack tops after shift
    public List<GlrStackItem> Discarded = new List<GlrStackItem>(); // abandoned stacks
  }

  public class GlrStackItem {
    public GlrStep Step; 
    public ParserState State;
    public readonly List<GlrStackItem> Previous = new List<GlrStackItem>();
    public ParseTreeNode ParseNode;
    public GlrStackItem(ParserState state, ParseTreeNode parseNode, GlrStackItem previous) {
      State = state;
      ParseNode = parseNode;
      Previous.Add(previous);
    }
  }

  class GlrStackSlice {
    public int Length; 
    public GlrStackItem[] Items;
    //Previous items of item #0 in stack slice; same as Items[0].Previous. 
    public List<GlrStackItem> PrecedingItems;

    public GlrStackSlice(int length) {
      Length = length;
      Items = new GlrStackItem[length];
    }
  }

  public class GlrParser {
    // PT p382:
    // For each possible reduce in the state, a copy of the stack is made and reduce is applied to it.
    public void ProcessInput(ParsingContext context) {
      var prevStep = context.CurrentStep;
      context.CurrentStep = new GlrStep() { Input = context.CurrentParserInput }; 
      // 1. All possible reduces
      foreach (var top in prevStep.Shifts.Values)
        ExecuteReduces(context.CurrentStep, top);
      // 2. Shifts
      ExecuteShift(context.CurrentStep);
    }

    public void ExecuteReduces(GlrStep step, GlrStackItem stackTop) {
      var inputTerm = step.Input.Term;
      GlrParserAction actionInfo;
      if (!stackTop.State.GlrActions.TryGetValue(inputTerm, out actionInfo)) {
        step.Discarded.Add(stackTop); 
        return;
      }
      // if this top is elligible for shift on input, add it to step.ShiftBases 
      if (actionInfo.Shifts.Count > 0)
        step.ShiftBases.Add(stackTop);
      foreach (var reduceItem in actionInfo.Reduces) {
        if (!reduceItem.Lookaheads.Contains(inputTerm)) continue;
        //execute reduce
        var prod = reduceItem.Core.Production;
        var prodLen = prod.RValues.Count;
        var sliceBuffer = new GlrStackSlice(prodLen);
        foreach (var slice in EnumerateTopStackSlices(stackTop, sliceBuffer)) {
          // the slice contains top stack elements, we can now try to reduce
          var newNode = prod.ExecuteReduce(slice.Items.Select(el => el.ParseNode));
          //Shift over newNode from all stack elements preceding reduced segment
          foreach (var prev in sliceBuffer.PrecedingItems) {
            GlrParserAction actionFromPrev;
            if (!prev.State.GlrActions.TryGetValue(newNode.Term, out actionFromPrev) || actionFromPrev.Shifts.Count == 0)
              // TODO: possible it never happens in LALR, investigate!
              continue; // no shifts over newNode; 
            // find next state after shifting node
            var stateAfterShift = actionFromPrev.Shifts[0].ShiftedItem.State; 
            // do not check here if this state is compatible with input; if not it will be discarded inside the ExeduceReduces call that follows
            var itemAfterShift = new GlrStackItem(stateAfterShift, newNode, prev);
            ExecuteReduces(step, itemAfterShift);
          }// foreach prev
        }//foreach path
      }//foreach reduce
    }//method

    public void ExecuteShift(GlrStep step) {
      //execute shifts on ShiftBases over input. Merge stack elements with the same state - see merging suffixes in Parsing Techniques
      foreach (var shiftFrom in step.ShiftBases) {
        GlrParserAction action;
        if (!shiftFrom.State.GlrActions.TryGetValue(step.Input.Term, out action)) continue;
        // if (action.Shifts.Count == 0)  continue; // that never happens, step.ShiftBases has only items that have shifts over input
        var nextState = action.Shifts[0].ShiftedItem.State;
        GlrStackItem nextItem;
        if (step.Shifts.TryGetValue(nextState, out nextItem))
          nextItem.Previous.Add(shiftFrom); //add back link to existing stack elem with the same state
        else 
          // create new stack element
          step.Shifts[nextState] = new GlrStackItem(nextState, step.Input, shiftFrom);
      }
    }

    private IEnumerable<GlrStackSlice> EnumerateTopStackSlices(GlrStackItem current, GlrStackSlice slice) {
      if (slice.Length == 0) {
        // special case, no child elements - empty production; we generate a single path consisting of 0 elements and PrecedingItems with only 'current' element. 
        // Note that PrecedingItems are those items (states) that will be used as bases (popped states)
        // from which we will shift over created non-terminal. In case of empty production the base is the top element itself.
        slice.PrecedingItems = new List<GlrStackItem>();
        slice.PrecedingItems.Add(current);
        return new GlrStackSlice[] {slice};
      } else
        return EnumerateTopStackSlices(current, slice.Length - 1, slice);
    }

    // Goes recursively, decreasing currentLevel until 0.
    // Enumerator returns the same object for every yield, and it is the same object as its parameter 'path'. 
    // We do it to reuse a single StackPath instance in all iterations
    private IEnumerable<GlrStackSlice> EnumerateTopStackSlices(GlrStackItem current, int currentLevel, GlrStackSlice path) {
      path.Items[currentLevel] = current;
      if (currentLevel == 0) { // we reach the start of the path
        path.PrecedingItems = current.Previous;
        yield return path;
      } else {
        foreach (var prev in current.Previous)
          foreach (var subPath in EnumerateTopStackSlices(prev, currentLevel - 1, path)) {
            yield return path; //pass path from child
          }
      }
    }

  }//class
} //ns

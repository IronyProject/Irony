using System;
using System.Collections.Generic;

namespace FastColoredTextBoxNS
{
    /// <summary>
    /// Insert single char
    /// </summary>
    /// <remarks>This operation includes also insertion of new line and removing char by backspace</remarks>
    internal class InsertCharCommand: UndoableCommand
    {
        char c;
        char deletedChar = '\x0';

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tb">Underlaying textbox</param>
        /// <param name="c">Inserting char</param>
        public InsertCharCommand(TextSource ts, char c): base(ts)
        {
            this.c = c;
        }

        /// <summary>
        /// Undo operation
        /// </summary>
        public override void Undo()
        {
            ts.OnTextChanging();
            switch (c)
            {
                case '\n': MergeLines(sel.Start.iLine, ts); break;
                case '\r': break;
                case '\b':
                    ts.CurrentTB.Selection.Start = lastSel.Start;
                    char cc = '\x0';
                    if (deletedChar != '\x0')
                    {
                        ts.CurrentTB.ExpandBlock(ts.CurrentTB.Selection.Start.iLine);
                        InsertChar(deletedChar, ref cc, ts);
                    }
                    break;
                default:
                    ts.CurrentTB.ExpandBlock(sel.Start.iLine);
                    ts[sel.Start.iLine].RemoveAt(sel.Start.iChar);
                    ts.CurrentTB.Selection.Start = sel.Start;
                    break;
            }

            ts.NeedRecalc(new TextSource.TextChangedEventArgs(sel.Start.iLine, sel.Start.iLine));

            base.Undo();
        }

        /// <summary>
        /// Execute operation
        /// </summary>
        public override void Execute()
        {
            ts.CurrentTB.ExpandBlock(ts.CurrentTB.Selection.Start.iLine);
            string s = c.ToString();
            ts.OnTextChanging(ref s);
            if (s.Length == 1)
                c = s[0];

            if (ts.Count == 0)
                InsertLine(ts);
            InsertChar(c, ref deletedChar, ts);

            ts.NeedRecalc(new TextSource.TextChangedEventArgs(ts.CurrentTB.Selection.Start.iLine, ts.CurrentTB.Selection.Start.iLine));
            base.Execute();
        }

        internal static void InsertChar(char c, ref char deletedChar, TextSource ts)
        {
            var tb = ts.CurrentTB;

            switch (c)
            {
                case '\n':
                    if (ts.Count == 0)
                        InsertLine(ts);
                    InsertLine(ts);
                    break;
                case '\r': break;
                case '\b'://backspace
                    if (tb.Selection.Start.iChar == 0 && tb.Selection.Start.iLine == 0)
                        return;
                    if (tb.Selection.Start.iChar == 0)
                    {
                        if (tb.lineInfos[tb.Selection.Start.iLine - 1].VisibleState != VisibleState.Visible)
                            tb.ExpandBlock(tb.Selection.Start.iLine - 1);
                        deletedChar = '\n';
                        MergeLines(tb.Selection.Start.iLine - 1, ts);
                    }
                    else
                    {
                        deletedChar = ts[tb.Selection.Start.iLine][tb.Selection.Start.iChar - 1].c;
                        ts[tb.Selection.Start.iLine].RemoveAt(tb.Selection.Start.iChar - 1);
                        tb.Selection.Start = new Place(tb.Selection.Start.iChar - 1, tb.Selection.Start.iLine);
                    }
                    break;
                case '\t':
                    for (int i = 0; i < tb.TabLength;i++ )
                        ts[tb.Selection.Start.iLine].Insert(tb.Selection.Start.iChar, new Char(' '));
                    tb.Selection.Start = new Place(tb.Selection.Start.iChar + tb.TabLength, tb.Selection.Start.iLine);
                    break;
                default:
                    ts[tb.Selection.Start.iLine].Insert(tb.Selection.Start.iChar, new Char(c));
                    tb.Selection.Start = new Place(tb.Selection.Start.iChar + 1, tb.Selection.Start.iLine);
                    break;
            }
        }

        internal static void InsertLine(TextSource ts)
        {
            var tb = ts.CurrentTB;

            if (!tb.Multiline && tb.LinesCount > 0)
                return;

            if (ts.Count == 0)
                ts.InsertLine(tb.Selection.Start.iLine + 1, ts.CreateLine());
            else
                BreakLines(tb.Selection.Start.iLine, tb.Selection.Start.iChar, ts);

            tb.Selection.Start = new Place(0, tb.Selection.Start.iLine + 1);
            ts.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));
        }

        /// <summary>
        /// Merge lines i and i+1
        /// </summary>
        internal static void MergeLines(int i, TextSource ts)
        {
            var tb = ts.CurrentTB;

            if (i + 1 >= ts.Count)
                return;
            tb.ExpandBlock(i);
            tb.ExpandBlock(i + 1);
            int pos = ts[i].Count;
            //
            if(ts[i].Count == 0)
                ts.RemoveLine(i);
            else
            if (ts[i + 1].Count == 0)
                ts.RemoveLine(i + 1);
            else
            {
                ts[i].AddRange(ts[i + 1]);
                ts.RemoveLine(i + 1);
            }
            tb.Selection.Start = new Place(pos, i);
            ts.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));
        }

        internal static void BreakLines(int iLine, int pos, TextSource ts)
        {
            Line newLine = ts.CreateLine();
            for(int i=pos;i<ts[iLine].Count;i++)
                newLine.Add(ts[iLine][i]);
            ts[iLine].RemoveRange(pos, ts[iLine].Count - pos);
            //
            ts.InsertLine(iLine+1, newLine);
        }
    }

    /// <summary>
    /// Insert text
    /// </summary>
    internal class InsertTextCommand : UndoableCommand
    {
        string insertedText;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tb">Underlaying textbox</param>
        /// <param name="insertedText">Text for inserting</param>
        public InsertTextCommand(TextSource ts, string insertedText): base(ts)
        {
            this.insertedText = insertedText;
        }

        /// <summary>
        /// Undo operation
        /// </summary>
        public override void Undo()
        {
            ts.CurrentTB.Selection.Start = sel.Start;
            ts.CurrentTB.Selection.End = lastSel.Start;
            ts.OnTextChanging();
            ClearSelectedCommand.ClearSelected(ts);
            base.Undo();
        }

        /// <summary>
        /// Execute operation
        /// </summary>
        public override void Execute()
        {
            ts.OnTextChanging(ref insertedText);
            InsertText(insertedText, ts);
            base.Execute();
        }

        internal static void InsertText(string insertedText, TextSource ts)
        {
            var tb = ts.CurrentTB;
            try
            {
                tb.Selection.BeginUpdate();
                char cc = '\x0';
                if (ts.Count == 0)
                    InsertCharCommand.InsertLine(ts);
                tb.ExpandBlock(tb.Selection.Start.iLine);
                foreach (char c in insertedText)
                    InsertCharCommand.InsertChar(c, ref cc, ts);
                ts.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));
            }
            finally {
                tb.Selection.EndUpdate();
            }
        }
    }

    /// <summary>
    /// Insert text into given ranges
    /// </summary>
    internal class ReplaceTextCommand : UndoableCommand
    {
        string insertedText;
        List<Range> ranges;
        List<string> prevText = new List<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tb">Underlaying textbox</param>
        /// <param name="ranges">List of ranges for replace</param>
        /// <param name="insertedText">Text for inserting</param>
        public ReplaceTextCommand(TextSource ts, List<Range> ranges, string insertedText)
            : base(ts)
        {
            //sort ranges by place
            ranges.Sort((r1, r2)=>
                {
                    if (r1.Start.iLine == r2.Start.iLine)
                        return r1.Start.iChar.CompareTo(r2.Start.iChar);
                    return r1.Start.iLine.CompareTo(r2.Start.iLine);
                });
            //
            this.ranges = ranges;
            this.insertedText = insertedText;
            lastSel = sel = new RangeInfo(ts.CurrentTB.Selection);
        }

        /// <summary>
        /// Undo operation
        /// </summary>
        public override void Undo()
        {
            var tb = ts.CurrentTB;

            ts.OnTextChanging();

            tb.Selection.BeginUpdate();
            for (int i = 0; i<ranges.Count; i++)
            {
                tb.Selection.Start = ranges[i].Start;
                for (int j = 0; j < insertedText.Length; j++)
                    tb.Selection.GoRight(true);
                ClearSelectedCommand.ClearSelected(ts);
                InsertTextCommand.InsertText(prevText[prevText.Count - i - 1], ts);
                ts.OnTextChanged(ranges[i].Start.iLine, ranges[i].Start.iLine);
            }
            tb.Selection.EndUpdate();

            ts.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));
        }

        /// <summary>
        /// Execute operation
        /// </summary>
        public override void Execute()
        {
            var tb = ts.CurrentTB;
            prevText.Clear();

            ts.OnTextChanging(ref insertedText);

            tb.Selection.BeginUpdate();
            for (int i = ranges.Count - 1; i >= 0; i--)
            {
                tb.Selection.Start = ranges[i].Start;
                tb.Selection.End = ranges[i].End;
                prevText.Add(tb.Selection.Text);
                ClearSelectedCommand.ClearSelected(ts);
                InsertTextCommand.InsertText(insertedText, ts);
                ts.OnTextChanged(ranges[i].Start.iLine, ranges[i].End.iLine);
            }
            tb.Selection.EndUpdate();
            ts.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));

            lastSel = new RangeInfo(tb.Selection);
        }
    }

    /// <summary>
    /// Clear selected text
    /// </summary>
    internal class ClearSelectedCommand : UndoableCommand
    {
        string deletedText;

        /// <summary>
        /// Construstor
        /// </summary>
        /// <param name="tb">Underlaying textbox</param>
        public ClearSelectedCommand(TextSource ts): base(ts)
        {
        }

        /// <summary>
        /// Undo operation
        /// </summary>
        public override void Undo()
        {
            ts.CurrentTB.Selection.Start = new Place(sel.FromX, Math.Min(sel.Start.iLine, sel.End.iLine));
            ts.OnTextChanging();
            InsertTextCommand.InsertText(deletedText, ts);
            ts.OnTextChanged(sel.Start.iLine, sel.End.iLine);
            ts.CurrentTB.Selection.Start = sel.Start;
            ts.CurrentTB.Selection.End = sel.End;
        }

        /// <summary>
        /// Execute operation
        /// </summary>
        public override void Execute()
        {
            var tb = ts.CurrentTB;

            ts.OnTextChanging();
            deletedText = tb.Selection.Text;
            ClearSelected(ts);
            lastSel = new RangeInfo(tb.Selection);
            ts.OnTextChanged(lastSel.Start.iLine, lastSel.Start.iLine);
        }

        internal static void ClearSelected(TextSource ts)
        {
            var tb = ts.CurrentTB;

            Place start = tb.Selection.Start;
            Place end = tb.Selection.End;
            int fromLine = Math.Min(end.iLine, start.iLine);
            int toLine = Math.Max(end.iLine, start.iLine);
            int fromChar = tb.Selection.FromX;
            int toChar = tb.Selection.ToX;
            if (fromLine < 0) return;
            //
            if (fromLine == toLine)
                ts[fromLine].RemoveRange(fromChar, toChar - fromChar);
            else
            {
                ts[fromLine].RemoveRange(fromChar, ts[fromLine].Count - fromChar);
                ts[toLine].RemoveRange(0, toChar);
                ts.RemoveLine(fromLine + 1, toLine - fromLine - 1);
                InsertCharCommand.MergeLines(fromLine, ts);
            }
            //
            tb.Selection.Start = new Place(fromChar, fromLine);
            //
            ts.NeedRecalc(new TextSource.TextChangedEventArgs(fromLine, toLine));
        }
    }

    /// <summary>
    /// Insert texts into given ranges
    /// </summary>
    internal class ReplaceMultipleTextCommand : UndoableCommand
    {
        List<ReplaceRange> ranges;
        List<string> prevText = new List<string>();

        public class ReplaceRange
        {
            public Range ReplacedRange { get; set; }
            public String ReplaceText { get; set; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ts">Underlaying textsource</param>
        /// <param name="ranges">List of ranges for replace</param>
        public ReplaceMultipleTextCommand(TextSource ts, List<ReplaceRange> ranges)
            : base(ts)
        {
            //sort ranges by place
            ranges.Sort((r1, r2) =>
            {
                if (r1.ReplacedRange.Start.iLine == r2.ReplacedRange.Start.iLine)
                    return r1.ReplacedRange.Start.iChar.CompareTo(r2.ReplacedRange.Start.iChar);
                return r1.ReplacedRange.Start.iLine.CompareTo(r2.ReplacedRange.Start.iLine);
            });
            //
            this.ranges = ranges;
            lastSel = sel = new RangeInfo(ts.CurrentTB.Selection);
        }

        /// <summary>
        /// Undo operation
        /// </summary>
        public override void Undo()
        {
            var tb = ts.CurrentTB;

            ts.OnTextChanging();

            tb.Selection.BeginUpdate();
            for (int i = 0; i < ranges.Count; i++)
            {
                tb.Selection.Start = ranges[i].ReplacedRange.Start;
                for (int j = 0; j < ranges[i].ReplaceText.Length; j++)
                    tb.Selection.GoRight(true);
                ClearSelectedCommand.ClearSelected(ts);
                var prevTextIndex = ranges.Count - 1 - i;
                InsertTextCommand.InsertText(prevText[prevTextIndex], ts);
                ts.OnTextChanged(ranges[i].ReplacedRange.Start.iLine, ranges[i].ReplacedRange.Start.iLine);
            }
            tb.Selection.EndUpdate();

            ts.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));
        }

        /// <summary>
        /// Execute operation
        /// </summary>
        public override void Execute()
        {
            var tb = ts.CurrentTB;
            prevText.Clear();

            ts.OnTextChanging();

            tb.Selection.BeginUpdate();
            for (int i = ranges.Count - 1; i >= 0; i--)
            {
                tb.Selection.Start = ranges[i].ReplacedRange.Start;
                tb.Selection.End = ranges[i].ReplacedRange.End;
                prevText.Add(tb.Selection.Text);
                ClearSelectedCommand.ClearSelected(ts);
                InsertTextCommand.InsertText(ranges[i].ReplaceText, ts);
                ts.OnTextChanged(ranges[i].ReplacedRange.Start.iLine, ranges[i].ReplacedRange.End.iLine);
            }
            tb.Selection.EndUpdate();
            ts.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));

            lastSel = new RangeInfo(tb.Selection);
        }
    }
}

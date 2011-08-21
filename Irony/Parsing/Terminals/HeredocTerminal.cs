#region License
/* **********************************************************************************
 * Copyright (c) Michael Tindal
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
using System.Globalization;

namespace Irony.Parsing {
    
    [Flags]
    public enum HereDocOptions {
        None = 0,
        AllowIndentedEndToken = 0x01,
    }

    public class HereDocTerminal : Terminal {

        #region HereDocSubType
        class HereDocSubType {
            internal readonly string Start;
            internal readonly HereDocOptions Flags;

            internal HereDocSubType(string start, HereDocOptions flags) {
                Start = start;
                Flags = flags;
            }

            internal static int LongerStartFirst(HereDocSubType x, HereDocSubType y) {
                try {//in case any of them is null
                    if (x.Start.Length > y.Start.Length) return -1;
                } catch { }
                return 0;
            }
        }
        class HereDocSubTypeList : List<HereDocSubType> {
            internal void Add(string start, HereDocOptions flags) {
                base.Add(new HereDocSubType(start, flags));
            }
        }
        #endregion

        private readonly HereDocSubTypeList _subtypes = new HereDocSubTypeList();

        #region Constructors and Initialization
        public HereDocTerminal(string name)
            : base(name) {
            base.SetFlag(TermFlags.IsLiteral);
        }

        public HereDocTerminal(string name, string start, HereDocOptions options)
            : this(name) {
            _subtypes.Add(start, options);
        }

        public void AddStart(string startSymbol, HereDocOptions hereDocOptions) {
            _subtypes.Add(startSymbol, hereDocOptions);
        }
        #endregion

        public override IList<string> GetFirsts() {
            List<string> firsts = new List<string>();
            foreach (var subtype in _subtypes) {
                firsts.Add(subtype.Start);
            }
            return firsts;
        }

        private char[] endOfLineMarker = { '\n', '\r' };

        private char[] endOfTokenMarker = { ' ', '\n', '\r' };

        struct HereDocConsumedLine {
            public int offset;
            public string text;
        }

        public override Token TryMatch(ParsingContext context, ISourceStream source) {
            List<HereDocConsumedLine> consumedLines;
            if (context.Values.ContainsKey("HereDocConsumedLines")) {
                consumedLines = context.Values["HereDocConsumedLines"] as List<HereDocConsumedLine>;
            } else {
                consumedLines = new List<HereDocConsumedLine>();
                context.Values["HereDocConsumedLines"] = consumedLines;
            }

            // First determine which subtype we are working with:
            _subtypes.Sort(HereDocSubType.LongerStartFirst);
            HereDocSubType subtype = null;
            foreach (HereDocSubType s in _subtypes) {
                if (source.Text.IndexOf(s.Start, source.PreviewPosition) == source.PreviewPosition) {
                    subtype = s;
                    break;
                }
            }
            if (subtype == null) {
                return null;
            }
            //Find the end of the heredoc token
            var newPos = source.Text.IndexOfAny(endOfTokenMarker, source.PreviewPosition + subtype.Start.Length);
            //We could not find the end of the token
            if (newPos == -1)
                return source.CreateErrorToken(Resources.ErrNoEndForHeredoc);// "No end symbol for heredoc." 
            source.PreviewPosition = newPos;
            var tagLength = source.PreviewPosition - source.Location.Position - subtype.Start.Length;
            var tag = source.Text.Substring(source.PreviewPosition - tagLength, tagLength);
            newPos = source.Text.IndexOfAny(endOfLineMarker, source.PreviewPosition);
            if (newPos == -1)
                return source.CreateErrorToken(Resources.ErrNoEndForHeredoc);
            source.PreviewPosition = newPos;

            source.PreviewPosition++;
            var value = new StringBuilder();
            var endFound = false;
            while (!endFound) {
                var eolPos = source.Text.IndexOfAny(endOfLineMarker, source.PreviewPosition);
                if (eolPos == -1)
                    break;
                var nextEol = source.Text.IndexOfAny(endOfLineMarker, eolPos + 1);
                var line = nextEol == -1 ? source.Text.Substring(eolPos + 1) : source.Text.Substring(eolPos + 1, nextEol - eolPos - 1);
                if (subtype.Flags.HasFlag(HereDocOptions.AllowIndentedEndToken)) {
                    var endPos = source.Text.IndexOf(tag, eolPos + 1);
                    if ((endPos != -1 && nextEol != -1 && endPos < nextEol) || (nextEol == -1 && endPos != -1)) {
                        HereDocConsumedLine cline = new HereDocConsumedLine();
                        cline.offset = eolPos + 1;
                        cline.text = line;
                        consumedLines.Add(cline);
                        endFound = true;
                    }
                } else {
                    if (line.Length >= tag.Length && line.Substring(0, tag.Length) == tag) {
                        HereDocConsumedLine cline = new HereDocConsumedLine();
                        cline.offset = eolPos + 1;
                        cline.text = line;
                        consumedLines.Add(cline);
                        endFound = true;
                    }
                }
                if (!endFound) {
                    bool foundLine = false;
                    foreach (HereDocConsumedLine consumed in consumedLines) {
                        if (consumed.text == line && consumed.offset == eolPos + 1) {
                            foundLine = true;
                            break;
                        }
                    }
                    if (!foundLine) {
                        value.Append(line);
                        value.Append('\n');
                        HereDocConsumedLine cline = new HereDocConsumedLine();
                        cline.offset = eolPos + 1;
                        cline.text = line;
                        consumedLines.Add(cline);
                        source.PreviewPosition = nextEol + 1;
                    }
                }
            }
            if (endFound) {
                Token token = source.CreateToken(this.OutputTerminal);
                token.Value = value.ToString();
                return token;
            } else
                return source.CreateErrorToken(Resources.ErrNoEndForHeredoc);
        }
    }
}

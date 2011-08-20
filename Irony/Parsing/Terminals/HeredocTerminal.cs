// -----------------------------------------------------------------------
// <copyright file="HeredocTerminal.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Irony.Parsing {
    
    [Flags]
    public enum HeredocFlags {
        None = 0,
        AllowIndentedEndToken = 0x01,
    }

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class HeredocTerminal : CompoundTerminalBase {
        private HeredocFlags _flags;

        public string StartSymbol = "<<";

        public HeredocTerminal(string name)
            : base(name) {
            _flags = HeredocFlags.None;
            base.SetFlag(TermFlags.IsLiteral);
        }

        public HeredocTerminal(string name, string startSymbol, HeredocFlags flags)
            : base(name) {
            StartSymbol = startSymbol;
            _flags = flags;
        }

        public override void Init(GrammarData grammarData) {
            base.Init(grammarData);
        }

        public override IList<string> GetFirsts() {
            var result = new StringList();
            result.Add(StartSymbol.ToString());
            return result;
        }

        private char[] _continueProcessing = { '\n', '\r' };

        private char[] _endOfToken = { ' ', '\n', '\r' };

        private static Terminal _HeredocString = new StringLiteral("HeredocString");

        public override Token TryMatch(ParsingContext context, ISourceStream source) {
            var startPos = source.PreviewPosition;
            //Find the end of the heredoc token
            var newPos = source.Text.IndexOfAny(_endOfToken, source.PreviewPosition + StartSymbol.Length);
            //We could not find the end of the token
            if (newPos == -1)
                return source.CreateErrorToken(Resources.ErrNoEndForHeredoc);// "No end symbol for heredoc." 
            source.PreviewPosition = newPos;
            var tokenLength = source.PreviewPosition - source.Location.Position - StartSymbol.Length;
            var _token = source.Text.Substring(source.PreviewPosition - tokenLength, tokenLength);
            newPos = source.Text.IndexOfAny(_continueProcessing, source.PreviewPosition);
            if (newPos == -1)
                return source.CreateErrorToken(Resources.ErrNoEndForHeredoc);
            source.PreviewPosition = newPos;

            source.PreviewPosition++;
            var heredocStart = source.PreviewPosition;
            var value = new StringBuilder();
            while (true) {
                var eolPos = source.Text.IndexOfAny(_continueProcessing, source.PreviewPosition);
                if (eolPos == -1)
                    break;
                if (_flags == HeredocFlags.AllowIndentedEndToken) {
                    var nextEol = source.Text.IndexOfAny(_continueProcessing, eolPos + 1);
                    var line = nextEol == -1 ? source.Text.Substring(eolPos + 1) : source.Text.Substring(eolPos + 1, nextEol - eolPos - 1);
                    var endPos = source.Text.IndexOf(_token, eolPos + 1);
                    if ((endPos != -1 && nextEol != -1 && endPos < nextEol) || (nextEol == -1 && endPos != -1)) {
                        var finalPos = endPos + _token.Length;
                        var newSource = source.Text.Remove(newPos, finalPos - newPos);
                        source.PreviewPosition = newPos;
                        (source as SourceStream).SetText(newSource, source.PreviewPosition, true);
                        Token token = source.CreateToken(_HeredocString.OutputTerminal);
                        token.Value = value.ToString();
                        return token;
                    } else {
                        value.Append(line);
                        value.Append('\n');
                        source.PreviewPosition = nextEol + 1;
                    }
                } else {
                    var nextEol = source.Text.IndexOfAny(_continueProcessing, eolPos + 1);
                    var line = nextEol == -1 ? source.Text.Substring(eolPos + 1) : source.Text.Substring(eolPos + 1, nextEol - eolPos - 1);
                    if (line.Length >= _token.Length && line.Substring(0, _token.Length) == _token) {
                        var finalPos = nextEol == -1 ? eolPos + _token.Length + 1 : nextEol + _token.Length;
                        var newSource = source.Text.Remove(newPos, finalPos - newPos);
                        source.PreviewPosition = newPos;
                        (source as SourceStream).SetText(newSource, source.PreviewPosition, true);
                        Token token = source.CreateToken(_HeredocString.OutputTerminal);
                        token.Value = value.ToString();
                        return token;
                    } else {
                        value.Append(line);
                        value.Append('\n');
                        source.PreviewPosition = nextEol + 1;
                    }
                }
            }
            return source.CreateErrorToken(Resources.ErrNoEndForHeredoc);
        }
    }
}

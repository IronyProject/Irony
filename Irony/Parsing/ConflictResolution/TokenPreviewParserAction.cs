using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {
  public class TokenPreviewParserAction: ConditionalParserAction {
  
  }

  public class TokenPreviewHint : GrammarHint {
    private PreferredActionType _actionType; 
    private string _firstString;
    private StringSet _beforeStrings = new StringSet();
    private Terminal _firstTerminal;
    private TerminalSet _beforeTerminals = new TerminalSet();
    
    public TokenPreviewHint(PreferredActionType actionType, string thisSymbol, params string[] comesBefore) {
      _actionType = actionType; 
      _firstString = thisSymbol;
      _beforeStrings.AddRange(comesBefore);
    }
    public TokenPreviewHint(PreferredActionType actionType, Terminal thisTerm, params Terminal[] comesBefore) {
      _actionType = actionType;
      _firstTerminal = thisTerm;
      _beforeTerminals.UnionWith(comesBefore);
    }


    public override void Init(GrammarData grammarData) {
      base.Init(grammarData);
      // convert strings to terminals, if needed
      _firstTerminal = _firstTerminal ?? Grammar.ToTerm(_firstString);
      if (_beforeStrings.Count > 0)
        _beforeTerminals.UnionWith(_beforeStrings.Select(s => Grammar.ToTerm(s)));
    }

  }//class  
  /*

   // Semi-automatic conflict resolution hint
  public class TokenPreviewHint : GrammarHint {
    public int MaxPreviewTokens = 200;
    private string _firstString;
    private StringSet _beforeStrings = new StringSet();
    private Terminal _firstTerminal;
    private TerminalSet _beforeTerminals = new TerminalSet();

    private TokenPreviewHint(ParserActionType action) : base(action) {
      _firstString = String.Empty;
      _firstTerminal = null;
      MaxPreviewTokens = 0;
    }

    public TokenPreviewHint(ParserActionType action, string first, string[] comesBefore) : this(action) {
      _firstString = first;
      _beforeStrings.AddRange(comesBefore); 
    }

    public TokenPreviewHint(ParserActionType action, Terminal first, Terminal[] comesBefore) : this(action) {
      _firstTerminal = first;
    }

    public override bool Match(ConflictResolutionArgs args) {
      try {
        args.Scanner.BeginPreview();

        var count = 0;
        var token = args.Scanner.GetToken();
        while (token != null && token.Terminal != args.Context.Language.Grammar.Eof) {
          if (token.Terminal == _firstTerminal)
            return true;
          if (_beforeTerminals.Contains(token.Terminal))
            return false;
          if (++count > MaxPreviewTokens && MaxPreviewTokens > 0)
            return false;
          token = args.Scanner.GetToken();
        }
        return false;
      }
      finally {
        args.Scanner.EndPreview(true);
      }
    }
  }
 
     // Base class for custom grammar hints
  public abstract class CustomGrammarHint : GrammarHint {
    public ParserActionType Action { get; private set; }
    public CustomGrammarHint(ParserActionType action) : base(HintType.Custom, null) {
      Action = action;
    }
    public abstract bool Match(ConflictResolutionArgs args);
  }
 
   */
}

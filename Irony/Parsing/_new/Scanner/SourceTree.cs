using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing.New {

  public class SourceTree {
    public SourceSegment Root;
    public List<TextSegment> NotScannedSegments = new List<TextSegment>();
    //public void AddPart(SourceSegment parent, SourceSegment child, int atIndex = -1);
    //public void Expand(SourceSegment segment, SourceSegment expansion);
  }

  public enum SegmentOrigin {
    SourceFile, 
    Fragment,
    Expansion,
    Token, 
  }

  public class SourceSpan {
    public TextSegment Source;
    public int Positon;
    public int Line; 
    public int Column;
    public int Length; 
  }

  public class SourceSegment {
    public SegmentOrigin Kind {get; protected set;}
    public SourceSegment Parent;
    public SourceSegment Expansion; 
    public IList<SourceSegment> Parts = new List<SourceSegment>();

    public IEnumerable<SourceSegment> GetAll() {
      if (Expansion != null)
        foreach (var child in Expansion.GetAll())
          yield return child;
      else if (Parts.Count == 0)
        yield return this;
      else foreach (var part in Parts)
          foreach (var subPart in part.GetAll())
            yield return subPart;         
    }//get all
  }

  public class TextSegment : SourceSegment {
    public SourceSpan Span;
    string _text;
    public bool IsScanned;

    public TextSegment(string text) {
      _text = text;
    }
    public override string ToString() {
      return _text;
    }

    public ISourceStream GetSourceStream(int start = 0) {
      return new SourceStream(_text, true, 8, new SourceLocation(start, 0, 0));
    }
  } //class

  public class FileSourceSegment : TextSegment {
    public string Path; 

  }

  public class Token : SourceSegment {

  }

}
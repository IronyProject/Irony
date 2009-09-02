using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Interpreter {
  public class DataStack {

    List<object> _data = new List<object>(16);
    object _lastPushedItem;
    object _unassigned; //Unassigned singleton value

    public void Init(object unassigned) {
      _unassigned = unassigned;
      _lastPushedItem = unassigned;
      _data.Clear(); 
    }

    public int Count {
      get {return _data.Count;}
    }
    public object this[int index] {
      get {return _data[_data.Count - 1 - index]; }
    }
    public object Top {
      get { return _data[_data.Count - 1]; }
    }
    public object Pop() {
      if (Count == 0)
        throw new RuntimeException("Interpreter error, DataStack.Pop() operation failed - stack is empty."); 
      var result = Top;
      _data.RemoveAt(_data.Count - 1);
      return result; 
    }
    public void Pop(int count) {
      _data.RemoveRange(_data.Count - count, count);
    }
    public void PopUntil(int toCount) {
      if (toCount >= _data.Count) return;
      Pop(_data.Count - toCount); 
    }
    public void Push(object item) {
      _lastPushedItem = item; 
      _data.Add(item);
    }
    public void Replace(int removeCount, object item) {
      Pop(removeCount);
      Push(item); 
    }
    public object LastPushedItem {
      get { return _lastPushedItem; }
    }
    public override string ToString() {
      return this.GetType().Name + "(Count=" + Count + ")";
    }

  }//class
}//namespace

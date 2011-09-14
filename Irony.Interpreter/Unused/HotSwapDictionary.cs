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

namespace Irony.Interpreter {
  // This class was created for use in Interpreter, but currently not used, getting along without it. 
  // May need in the future

  /// <summary> Fast-read, thread-safe dictionary. </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TValue">Value type.</typeparam>
  /// <remarks>
  /// The dictionary maintains 2 internal dictionaries with almost identical key/value sets. 
  /// Front-end dictionary is read-only, so it can be read safely without locking. 
  /// Back-end dictionary is a read/write dictionary that must be accessed with thread lock (done internally by HotSwapDictionary).
  /// New items are added initially to backend dictionary only, so there's some lag between backend and frontend (frontend contains 
  /// less items). The frontend dictionary is eventually "refreshed" - replaced by a copy of the backend dictionary. 
  /// </remarks>
  public class HotSwapDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
    // Readonly dictionary with free, no-lock access; _frontEnd is a copy of _backEnd, with maybe a few missing items
    // that were recently added. 
    IDictionary<TKey, TValue> _frontEnd;
    // Read/Write back-end dictionary with locked access
    IDictionary<TKey, TValue> _backEnd;
    object _lockObject;
    int _maxFrontEndMisses;  // number of _frontEnd misses that trigger _frontEnd refresh
    int _maxFrontEndLag;  // max number of items that _frontEnd is missing compared to _backEnd; when this number is reached, 
                          // the _frontEnd is refreshed.
    // Current counts
    int _currentLag = 0; //Note that we can't simply compare Count properties of frontend and backend - items can be added and deleted,
                         // so Count diff might be misleading
    int _missCount = 0;
    
    public HotSwapDictionary(int maxFrontEndMisses = 5, int maxFrontEndLag = 5, int capacity = 100) {
      _maxFrontEndMisses = maxFrontEndMisses;
      _maxFrontEndLag = maxFrontEndLag;
      _frontEnd = new Dictionary<TKey, TValue>(capacity);
      _backEnd = new Dictionary<TKey, TValue>(capacity);
      _lockObject = new object();
    }

    #region Utility methods - updating counts and refreshing front-end. 
    // "Locked" suffix identifies that methods assume the thread already holds a lock on _lockObject
    private void IncrementLagLocked() {
      _currentLag++;
      if (_currentLag > _maxFrontEndLag)
        RefreshFrontEndLocked(); 
    }

    private void IncrementMissCountLocked() {
      _missCount++;
      if (_missCount > _maxFrontEndMisses)
        RefreshFrontEndLocked();
    }

    private void RefreshFrontEndLocked() {
      // Create a copy of backend and put it into frontend.
      var newCopy = new Dictionary<TKey, TValue>(_backEnd.Count);
      foreach (var kvp in _backEnd)
        newCopy.Add(kvp.Key, kvp.Value);
      System.Threading.Interlocked.Exchange(ref _frontEnd, newCopy);
      //Clear the counters
      _currentLag = 0;
      _missCount = 0;
    }
    #endregion

    #region IDictionary implementation
    public bool TryGetValue(TKey key, out TValue value) {
      if (_frontEnd.TryGetValue(key, out value))
        return true; 
      // We have front-end miss
      lock (_lockObject) {
        if (_backEnd.TryGetValue(key, out value)) {
          IncrementMissCountLocked(); 
          return true;
        }//if
        return false; 
      }//lock
    }

    public void Add(TKey key, TValue value) {
      lock (_lockObject) {
        _backEnd.Add(key, value);
        IncrementLagLocked(); 
      }
    }

    public bool ContainsKey(TKey key) {
      lock (_lockObject) {
        return _backEnd.ContainsKey(key);
      }
    }

    public ICollection<TKey> Keys {
      get {
        lock (_lockObject) {
          return new List<TKey>(_backEnd.Keys); 
        }
      }
    }

    public bool Remove(TKey key) {
      lock (_lockObject) {
        var result = _backEnd.Remove(key);
        IncrementLagLocked();
        return result; 
      }
    }

    public ICollection<TValue> Values {
      get {
        lock (_lockObject) {
          return new List<TValue>(_backEnd.Values);
        }
      }
    }

    public TValue this[TKey key] {
      get {
        TValue value;
        if (TryGetValue(key, out value))
          return value;
        throw new ArgumentOutOfRangeException(key.ToString());        
      }
      set {
        lock (_lockObject) {
          _backEnd[key] = value;
          IncrementLagLocked();
        }
      }
    }

    public void Add(KeyValuePair<TKey, TValue> item) {
      lock (_lockObject) {
        _backEnd.Add(item);
        IncrementLagLocked();
      }
    }

    public void Clear() {
      lock (_lockObject) {
        _backEnd.Clear();
        RefreshFrontEndLocked();
      }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) {
      lock (_lockObject) {
        return _backEnd.Contains(item);
      }
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
      lock (_lockObject) {
        _backEnd.CopyTo(array, arrayIndex);
      }
    }

    public int Count {
      get {
        lock (_lockObject) {
          return _backEnd.Count;
        }
      }
    }

    public bool IsReadOnly {
      get { return false; }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) {
      lock (_lockObject) {
        var result = _backEnd.Remove(item);
        IncrementLagLocked();
        return result;
      }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
      var list = new List<KeyValuePair<TKey, TValue>>();
      lock (_lockObject) {
        foreach (var kvp in _backEnd)
          list.Add(kvp);
      }
      return list.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return GetEnumerator(); 
    }
    #endregion
  }
}

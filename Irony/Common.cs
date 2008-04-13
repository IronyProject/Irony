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

namespace Irony {
  //Some common classes

  public class StringList : List<string> { }
  public class StringDictionary : Dictionary<string, string> { }
  public class CharList : List<char> { }

  //string list with no duplicates
  public class KeyList : List<string> {
    Dictionary<string, byte> _hash = new Dictionary<string, byte>();

    public KeyList() { }
    public KeyList(params string[] keys) {
      this.AddRange(keys);
    }
    //overwrite Add and AddRange to disallow repetitions
    public new void Add(string key) {
      if (!Contains(key)) {
        base.Add(key);
        _hash.Add(key, 1);
      }
    }

    public new void AddRange(IEnumerable<string> keys) {
      foreach (string key in keys)
        this.Add(key);
    }
    public new void Remove(string key) {
      base.Remove(key);
      _hash.Remove(key);
    }
    public new bool Contains(string key) {
      return _hash.ContainsKey(key);
    }
    public override string ToString() {
      return ToString(" ");
    }
    public string ToString(string separator) {
      string[] arr = new string[this.Count];
      this.CopyTo(arr);
      //Clean-up \b suffix
      for(int i = 0; i < arr.Length; i++) {
        string key = arr[i];
        if(key.EndsWith("\b"))
          arr[i] = key.Substring(0, key.Length - 1);
      }
      return string.Join(separator, arr);
    }
    public new void Clear() {
      base.Clear();
      _hash.Clear();
    }

    //Used in sorting suffixes and prefixes; longer strings must come first in sort order
    public static int LongerFirst(string x, string y) {
      try {//in case any of them is null
        if (x.Length > y.Length) return -1;
      } catch { }
      return 0;
    }

  }//KeyList class

}

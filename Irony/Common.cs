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

  //string list with no duplicates
  public class KeyList : List<string> {
    //overwrite Add and AddRange to disallow repetitions
    public new void Add(string key) {
      if (!Contains(key))
        base.Add(key);
    }

    public new void AddRange(IEnumerable<string> keys) {
      foreach (string key in keys)
        this.Add(key);
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
  }//KeyList class


}

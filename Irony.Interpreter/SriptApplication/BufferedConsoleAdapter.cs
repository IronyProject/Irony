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
using System.Threading;
using Irony.Parsing;

namespace Irony.Interpreter {

  // Buffered implementation of IConsoleAdapter with StringBuilder output.
  public class BufferedConsoleAdapter : IConsoleAdapter {
    private StringBuilder OutputBuffer = new StringBuilder();
    private object _lockObject = new object();

    public bool Canceled { get; set; }

    public void Write(string text) {
      lock(_lockObject) {
        OutputBuffer.Append(text);
      }
    }

    public void WriteLine(string text) {
      lock(_lockObject) {
        OutputBuffer.AppendLine(text);
      }
    }

    public void SetTextStyle(ConsoleTextStyle style) {
      // not supported
    }

    public int Read() {
      return 0; // not supported
    }

    public virtual string ReadLine() {
      return null; // not supported
    }

    public void SetTitle(string title) {
      // not supported
    }

    public void Clear() {
      lock (_lockObject) {
        OutputBuffer.Clear();
      }
    }

    public string GetOutput() {
      lock (_lockObject) {
        return OutputBuffer.ToString();
      }
    }
  }
}

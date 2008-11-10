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

// Aknowledgments
// This module borrows code and ideas from TinyPG framework by Herre Kuijpers,
// specifically TextMarker.cs and TextHighlighter.cs classes.
// http://www.codeproject.com/KB/recipes/TinyPG.aspx
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Runtime.InteropServices;
using Irony.Compiler;
using Irony.EditorServices;
using System.Diagnostics;

namespace Irony.GrammarExplorer {

  public class TokenColorTable : Dictionary<TokenColor, Color> { }

  public class RichTextBoxHighligter : NativeWindow, IDisposable {
    public RichTextBox TextBox;
    public readonly TokenColorTable TokenColors = new TokenColorTable();
    EditorViewAdapter _viewAdapter;

    private IntPtr _savedEventMask = IntPtr.Zero;
    bool _colorizing;
    bool _disposed;

    #region constructor, initialization and disposing
    public RichTextBoxHighligter(RichTextBox textBox, EditorViewAdapter adapter) {
      TextBox = textBox;  
      _viewAdapter = adapter;
      InitColorTable();
      Connect();
      UpdateViewRange();
      _viewAdapter.SetNewText(TextBox.Text);
    }
    private void Connect() {
      TextBox.MouseMove += TextBox_MouseMove;
      TextBox.TextChanged += TextBox_TextChanged;
      TextBox.KeyDown += TextBox_KeyDown;
      TextBox.VScroll += TextBox_ScrollResize;
      TextBox.HScroll += TextBox_ScrollResize;
      TextBox.SizeChanged += TextBox_ScrollResize;
      TextBox.Disposed += TextBox_Disposed;
      _viewAdapter.ColorizeTokens += Adapter_ColorizeTokens;
      this.AssignHandle(TextBox.Handle);
    }

    private void Disconnect() {
      if (TextBox != null) {
        TextBox.MouseMove -= TextBox_MouseMove;
        TextBox.TextChanged -= TextBox_TextChanged;
        TextBox.KeyDown -= TextBox_KeyDown;
        TextBox.Disposed -= TextBox_Disposed;
        TextBox.VScroll -= TextBox_ScrollResize;
        TextBox.HScroll -= TextBox_ScrollResize;
        TextBox.SizeChanged -= TextBox_ScrollResize;
      }
      TextBox = null;
    }

    public void Dispose() {
      _disposed = true; 
      Disconnect();
      this.ReleaseHandle();
      GC.SuppressFinalize(this);
    }
    private void InitColorTable() {
      TokenColors[TokenColor.Comment] = Color.Green;
      TokenColors[TokenColor.Identifier] = Color.Black;
      TokenColors[TokenColor.Keyword] = Color.Blue;
      TokenColors[TokenColor.Number] = Color.DarkRed;
      TokenColors[TokenColor.String] = Color.DarkSlateGray; 
      TokenColors[TokenColor.Text] = Color.Black;

    }
    #endregion

    #region TextBox event handlers

    void TextBox_MouseMove(object sender, MouseEventArgs e) {
      //TODO: implement showing tip
    }

    void TextBox_KeyDown(object sender, KeyEventArgs e) {
      //TODO: implement showing intellisense hints or drop-downs
    }

    void TextBox_TextChanged(object sender, EventArgs e) {
      //if we are here while colorizing, it means the "change" event is a result of our coloring action
      if (_colorizing) return; 
      _viewAdapter.SetNewText(TextBox.Text);
    }
    void TextBox_ScrollResize(object sender, EventArgs e) {
      UpdateViewRange();
    }


    void TextBox_Disposed(object sender, EventArgs e) {
      Dispose();
    }
    private void UpdateViewRange() {
      int minpos = TextBox.GetCharIndexFromPosition(new Point(0, 0));
      int maxpos = TextBox.GetCharIndexFromPosition(new Point(TextBox.ClientSize.Width, TextBox.ClientSize.Height));
      _viewAdapter.SetViewRange(minpos, maxpos);
    }
    #endregion

    #region WinAPI
    // some winapís required
    [DllImport("user32", CharSet = CharSet.Auto)]
    private extern static IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool PostMessageA(IntPtr hWnd, int nBar, int wParam, int lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetScrollPos(int hWnd, int nBar);

    [DllImport("user32.dll")]
    private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

    private const int WM_SETREDRAW = 0x000B;
    private const int WM_USER = 0x400;
    private const int EM_GETEVENTMASK = (WM_USER + 59);
    private const int EM_SETEVENTMASK = (WM_USER + 69);
    private const int SB_HORZ = 0x0;
    private const int SB_VERT = 0x1;
    private const int WM_HSCROLL = 0x114;
    private const int WM_VSCROLL = 0x115;
    private const int SB_THUMBPOSITION = 4;
    const int WM_PAINT = 0x000F;

    private int HScrollPos {
      get {
        //sometimes explodes with null reference exception
        return GetScrollPos((int)TextBox.Handle, SB_HORZ); 
      }
      set {
        SetScrollPos((IntPtr)TextBox.Handle, SB_HORZ, value, true);
        PostMessageA((IntPtr)TextBox.Handle, WM_HSCROLL, SB_THUMBPOSITION + 0x10000 * value, 0);
      }
    }

    private int VScrollPos {
      get {
        return GetScrollPos((int)TextBox.Handle, SB_VERT); 
      }
      set {
        SetScrollPos((IntPtr)TextBox.Handle, SB_VERT, value, true);
        PostMessageA((IntPtr)TextBox.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * value, 0);
      }
    }
    #endregion

    #region overrides: WndProc
    protected override void WndProc(ref Message m) {
      // pre-process the text control's messages
      switch (m.Msg) {
        case WM_PAINT:
          //force recolorising when window is about to paint - this would likely cause another WM_PAINT message
          if (_viewAdapter.WantsColorize) {
            UpdateViewRange();
            _viewAdapter.TryInvokeColorize();
          } 
          break; 
      }
      base.WndProc(ref m);
    }
    #endregion

    #region Colorizing tokens
    public void LockTextBox() {
      // Stop redrawing:  
      SendMessage(TextBox.Handle, WM_SETREDRAW, 0, IntPtr.Zero );
      // Stop sending of events:  
      _savedEventMask = SendMessage(TextBox.Handle, EM_GETEVENTMASK, 0,  IntPtr.Zero);
      //SendMessage(TextBox.Handle, EM_SETEVENTMASK, 0, IntPtr.Zero);
    }

    public void UnlockTextBox() {
        // turn on events  
        SendMessage(TextBox.Handle, EM_SETEVENTMASK, 0, _savedEventMask);
        // turn on redrawing  
        SendMessage(TextBox.Handle, WM_SETREDRAW, 1, IntPtr.Zero);
    }

    void Adapter_ColorizeTokens(object sender, ColorizeEventArgs args) {
      if (_disposed) return; 
      //Debug.WriteLine("Coloring " + args.Tokens.Count + " tokens.");
      _colorizing = true;
      
      int hscroll = HScrollPos;
      int vscroll = VScrollPos;
      int selstart = TextBox.SelectionStart;
      int selLength = TextBox.SelectionLength;
      LockTextBox();
      try {
        foreach (Token t in args.Tokens) {
          TextBox.Select(t.Location.Position, t.Span.Length);
          TextBox.SelectionColor = GetTokenColor(t);
        }
      } finally {
        TextBox.Select(selstart, selLength);
        HScrollPos = hscroll;
        VScrollPos = vscroll;
        UnlockTextBox();
        _colorizing = false;
      }
      TextBox.Invalidate();
    }

    private Color GetTokenColor(Token token) {
      Color result;
      if (token.EditorInfo != null && TokenColors.TryGetValue(token.EditorInfo.Color, out result))
        return result;
      return Color.Black;
    }
    #endregion

  }//class

}//namespace 

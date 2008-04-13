using System;
using Text = System.Text;

/* 
  sample program * /
   
*/

namespace MyNamespace {
    extern alias Abc;
    using Xml = System.Xml;

    /// <summary>
    /// </summary>
    [Flags()]
    internal enum MyFlags : int {
       Flag3,
       Flag1 = 0x01,    //Flag1 comment
       Flag2 = /* Flag 2 comment */ 0x02,    
    }   

    [return: Optional( a, b)]
    public static class Class1  {
      private int GetInt(string prm) {
      }
      public Class1(SomeType name) : base() {
      }
      protected PropType PropName {
         get {}
         internal set {}
      }//prop
      
      int MyInterface.GetInt() {
      }
      
    }//class

    public delegate void MyDelegate(int prm1);

    internal struct MyPoint<T> : Point<float>, ISomeInterface  {
       public Ptr<T> _value = 1;
    }
    public interface IMyInt {
      void DoStuff();
      string Name {get; [Attr()] set;}
    }
}//namespace


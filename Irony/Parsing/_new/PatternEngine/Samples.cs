using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irony.Parsing._new {

  public class Foo {

    public static Foo operator <(Foo x, Foo y) {
      return x;
    }
    public static Foo operator >(Foo x, Foo y) {
      return x;
    }
  }
  public class Foo<T1, T2> : Foo { }
  
  class PatternSamples {
    static void Test() {
      Foo f1 = null; Foo f2 = null; Foo f3 = null;
      Foo f4 = f1 < f2 > f3;

      //Foo<string, int> foo;
      //foo = null;

      doStuff(f1 < f2, f2 > f3, new Foo<string, int>());
    }

    static void doStuff(object x, object y, object z) {

    }
  }//class
}

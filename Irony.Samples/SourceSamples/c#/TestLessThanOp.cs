//Simple test for "<" operator, to check it is properly distinguished from "<" in type argument.
// NLALR method is capable to distinguish it automatically, while LALR requires custom token preview
namespace Test {
  public class TestClass {
    public void DoStuff(int arg) {
      List<T> x;
      if (myGenType<T>.Prop < arg)
        arg = 5;
    }
  }//class
}//namespace
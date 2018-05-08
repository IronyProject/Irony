using System;

namespace Irony.Utilities
{
#if NET40
  internal static class ReflectionExtensions
  {
    public static Type GetTypeInfo(this Type type)
    {
      return type;
    }
  }
#endif
}
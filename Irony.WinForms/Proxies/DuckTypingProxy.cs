using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Proxies;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace Irony.WinForms.Proxies {
  /// <summary>
  /// Proxy class implementing duck typing semantics of the target object.
  /// </summary>
  /// <typeparam name="T">Proxied type.</typeparam>
  public class DuckTypingProxy<T> : RealProxy {
    /// <summary>
    /// Initializes a new instance of the <see cref="DuckTypingProxy`1" /> class.
    /// </summary>
    /// <param name="target">The target instance.</param>
    /// <param name="strict">if set to <c>true</c>, all methods of type T are required to be supported by target.</param>
    public DuckTypingProxy(object target, bool strict = false)
      : base(typeof(T)) {
      Target = target;
      Strict = strict;
      BuildMethodTable();
    }

    /// <summary>
    /// Gets the target instance.
    /// </summary>
    public object Target { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="DuckTypingProxy`1" /> supports all methods of type T.
    /// </summary>
    public bool Strict { get; private set; }

    private Dictionary<string, MethodInfo> ProxyMethods = new Dictionary<string, MethodInfo>();

    private Dictionary<string, MethodInfo> TargetMethods = new Dictionary<string, MethodInfo>();

    private BindingFlags Flags = BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public;

    private void BuildMethodTable() {
      var targetType = Target.GetType();
      var proxyType = GetType();
      var methods = typeof(T).GetMethods(Flags);
      var proxyFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

      foreach (var method in methods) {
        // first, look for proxy method with the same signature
        var parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
        var foundMethod = proxyType.GetMethod(method.Name, proxyFlags, null, parameters, null);
        if (foundMethod != null) {
          ProxyMethods[method.ToString()] = foundMethod;
          continue;
        }

        // next, look for target method with the same signature
        foundMethod = targetType.GetMethod(method.Name, Flags, null, parameters, null);
        if (foundMethod != null) {
          TargetMethods[method.ToString()] = foundMethod;
          continue;
        }

        if (Strict) {
          throw new MissingMethodException(method.Name);
        }
      }
    }

    /// <summary>
    /// Invokes the specified remoting message.
    /// </summary>
    /// <param name="message">The message to invoke.</param>
    public override IMessage Invoke(IMessage message) {
      // note: we don't support constructor calls
      var mcm = (IMethodCallMessage)message;
      var minfo = (MethodInfo)mcm.MethodBase;
      return InvokeMessage(mcm, minfo);
    }

    private ReturnMessage InvokeMessage(IMethodCallMessage mcm, MethodInfo minfo) {
      try {
        object target = this;
        MethodInfo method = null;
        string key = minfo.ToString();

        if (!ProxyMethods.TryGetValue(key, out method)) {
          target = Target;
          if (!TargetMethods.TryGetValue(key, out method)) {
            throw new MissingMethodException(minfo.ToString());
          }
        }

        var result = InvokeMethod(method, target, mcm.Args);
        return new ReturnMessage(result, null, 0, null, mcm);
      } catch (Exception ex) {
        return new ReturnMessage(ex, mcm);
      }
    }

    /// <summary>
    /// Invokes the specified method on the specified target instance.
    /// </summary>
    /// <param name="method">The method to invoke.</param>
    /// <param name="target">The target instance.</param>
    /// <param name="arguments">The arguments to pass.</param>
    /// <returns>Method return value.</returns>
    protected virtual object InvokeMethod(MethodInfo method, object target, object[] arguments) {
      return method.Invoke(target, arguments);
    }

    /// <summary>
    /// Sets the strong-typed proxy instance.
    /// </summary>
    public T Instance {
      get { return (T)GetTransparentProxy(); }
    }

    /// <summary>
    /// Converts the specified proxy to the target type implicitly.
    /// </summary>
    /// <param name="proxy">The proxy to convert.</param>
    public static implicit operator T(DuckTypingProxy<T> proxy) {
      return proxy.Instance;
    }
  }
}

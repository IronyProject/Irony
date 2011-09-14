using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Irony.Interpreter {

  public enum ImportType {
    ScriptModule, 
    ClrNamespace,
    ClrClassMembers,
  }

  public class ImportSpecList : List<ImportSpec> {
    public ImportSpec AddStaticMembers(Type fromType) {
      var import = new ClrClassMembersImport(fromType);
      Add(import);
      return import;
    }
    public ImportSpec AddInstanceMembers(object instance) {
      var import = new ClrClassMembersImport(instance.GetType(), instance);
      Add(import);
      return import;
    }
    public ImportSpec AddInstanceMember(string member, object instance) {
      var import = new ClrClassMembersImport(instance.GetType(), instance, member);
      Add(import);
      return import;
    }
  }

  public class ImportSpec {
    public ImportType Type;
    public ImportSpec(ImportType type) {
      Type = type; 
    }
    public virtual Binding Bind(BindingRequest request) {
      return null; 
    }
  }

  public class ModuleImport: ImportSpec {
    public ModuleInfo Module;
    public ModuleImport(ModuleInfo module) : base(ImportType.ScriptModule) {
      Type = ImportType.ScriptModule;
      Module = module; 
    }

    public override Binding Bind(BindingRequest request) {
      return  request.Thread.Runtime.BindSymbol(request, Module); 
    }
  }

  //TODO: this is very slow! cache all symbols from loaded assemblies
  public class ClrNamespaceImport : ImportSpec {
    public string Namespace;
    public ClrNamespaceImport(string ns) : base(ImportType.ClrNamespace) {
      Namespace = ns; 
    }

    public override Binding Bind(BindingRequest request) {
      var fullName = Namespace + "." + request.Symbol;
      var assemblies = request.Thread.App.GetImportAssemblies(); 
      // Name inside namespace might be either Type, or child namespace. Try to find type first
      foreach(var asm in assemblies) {
        var type = asm.GetType(fullName);
        if (type != null)
          return new ClrTypeBinding(type);
      }
      // Try to find namespace. Assembly class does not provide a list of all namespaces explicitly, we have to derive it.
      // Go thru all types, and if there's a type with namespace that starts with this.Namespace, then 
      // return Namespace binding
      var targetNs = Namespace + "." + request.Symbol;
      var targetNsWithDot = targetNs + ".";
      foreach (var asm in assemblies)
        foreach (var type in asm.GetTypes())
          if (type.Namespace == targetNs || type.Namespace.StartsWith(targetNsWithDot))
            return new ClrNamespaceBinding(targetNs);
      return null; 
    }//method

      
  }//class

  public class ClrClassMembersImport : ImportSpec {
    public Type TargetType;
    public object Instance;
    public string MemberFilter; 
    public ClrClassMembersImport(Type targetType, object instance = null, string memberFilter = null) : base(ImportType.ClrClassMembers) {
      TargetType = targetType;
      Instance = instance;
      MemberFilter = memberFilter;
    }

    public override Binding Bind(BindingRequest request) {
      var flags = BindingFlags.Public;
      if (Instance == null)
        flags |= BindingFlags.Static;
      else
        flags |= BindingFlags.Instance;

      if (request.IgnoreCase)
        flags |= BindingFlags.IgnoreCase;
      if (!string.IsNullOrEmpty(MemberFilter)) {
        if (string.Compare(request.Symbol, MemberFilter, request.IgnoreCase) != 0)
          return null; 
      }
      var members = TargetType.GetMember(request.Symbol, flags);
      if (members != null && members.Length > 0) {
        return new ClrMemberBinding(request.Options, members, Instance);
      }
      return null; 
    }
  }


}

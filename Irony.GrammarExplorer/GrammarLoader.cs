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
// This file and all functionality of dynamic assembly reloading was contributed by Alexey Yakovlev (yallie)
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Irony.Parsing;
using System.IO;
using System.Threading;

namespace Irony.GrammarExplorer {
  /// <summary>
  /// Maintains grammar assemblies, reloads updated files automatically.
  /// </summary>
  class GrammarLoader {
    private TimeSpan _autoRefreshDelay = TimeSpan.FromMilliseconds(500);
    private Dictionary<string, CachedAssembly> _cachedGrammarAssemblies = new Dictionary<string, CachedAssembly>();
    private static HashSet<string> _probingPaths = new HashSet<string>();
    private static Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>();
    private static bool _enableAssemblyResolution = false;

    static GrammarLoader() {
      AppDomain.CurrentDomain.AssemblyLoad += (s, args) => _loadedAssemblies[args.LoadedAssembly.FullName] = args.LoadedAssembly;
      AppDomain.CurrentDomain.AssemblyResolve += (s, args) => _enableAssemblyResolution ? FindAssembly(args.Name) : null;
    }

    static Assembly FindAssembly(string assemblyName) {
      if (_loadedAssemblies.ContainsKey(assemblyName)) {
        return _loadedAssemblies[assemblyName];
      }
      // use probing paths to look for assemblies
      var fileName = assemblyName.Split(',').First() + ".dll";
      foreach (var path in _probingPaths) {
        var fullName = Path.Combine(path, fileName);
        if (File.Exists(fullName)) {
          try {
            return _loadedAssemblies[assemblyName] = Assembly.LoadFrom(fullName);
          }
          catch {
            // the file seems to be bad, let's try to find another one
          }
        }
      }
      // assembly not found, don't search for it again
      return _loadedAssemblies[assemblyName] = null;
    }

    class CachedAssembly {
      public long FileSize;
      public DateTime LastWriteTime;
      public FileSystemWatcher Watcher;
      public Assembly Assembly;
    }

    public event EventHandler AssemblyUpdated;

    public GrammarItem SelectedGrammar { get; set; }

    public Grammar CreateGrammar() {
      if (SelectedGrammar == null)
        return null;

      // resolve dependencies while loading and creating grammars
      _enableAssemblyResolution = true;
      try {
        var type = SelectedGrammarAssembly.GetType(SelectedGrammar.TypeName, true, true);
        return Activator.CreateInstance(type) as Grammar;
      }
      finally {
        _enableAssemblyResolution = false;
      }
    }

    Assembly SelectedGrammarAssembly {
      get {
        if (SelectedGrammar == null)
          return null;

        // create assembly cache entry as needed
        var location = SelectedGrammar.Location;
        if (!_cachedGrammarAssemblies.ContainsKey(location)) {
          var fileInfo = new FileInfo(location);
          _cachedGrammarAssemblies[location] =
            new CachedAssembly {
              LastWriteTime = fileInfo.LastWriteTime,
              FileSize = fileInfo.Length,
              Assembly = null
            };

          // set up file system watcher
          _cachedGrammarAssemblies[location].Watcher = CreateFileWatcher(location);
        }

        // get loaded assembly from cache if possible
        var assembly = _cachedGrammarAssemblies[location].Assembly;
        if (assembly == null) {
          assembly = LoadAssembly(location);
          _cachedGrammarAssemblies[location].Assembly = assembly;
        }

        return assembly;
      }
    }

    private FileSystemWatcher CreateFileWatcher(string location) {
      var folder = Path.GetDirectoryName(location);
      var watcher = new FileSystemWatcher(folder);
      watcher.Filter = Path.GetFileName(location);

      watcher.Changed += (s, args) => {
        if (args.ChangeType != WatcherChangeTypes.Changed)
          return;

        // check if assembly was changed indeed to work around multiple FileSystemWatcher event firing
        var cacheEntry = _cachedGrammarAssemblies[location];
        var fileInfo = new FileInfo(location);
        if (cacheEntry.LastWriteTime == fileInfo.LastWriteTime && cacheEntry.FileSize == fileInfo.Length)
          return;

        // clear cached assembly and save last file update time
        cacheEntry.LastWriteTime = fileInfo.LastWriteTime;
        cacheEntry.FileSize = fileInfo.Length;
        cacheEntry.Assembly = null;

        // delay auto-refresh for safety reasons
        ThreadPool.QueueUserWorkItem(_ => {
          Thread.Sleep(_autoRefreshDelay);
          OnAssemblyUpdated(location);
        });
      };

      watcher.EnableRaisingEvents = true;
      return watcher;
    }

    private void OnAssemblyUpdated(string location) {
      if (AssemblyUpdated == null || SelectedGrammar == null || SelectedGrammar.Location != location)
        return;
      AssemblyUpdated(this, EventArgs.Empty);
    }

    public static Assembly LoadAssembly(string fileName) {
      // save assembly path for dependent assemblies probing
      var path = Path.GetDirectoryName(fileName);
      _probingPaths.Add(path);
      // 1. Assembly.Load doesn't block the file
      // 2. Assembly.Load doesn't check if the assembly is already loaded in the current AppDomain
      return Assembly.Load(File.ReadAllBytes(fileName));
    }
  }
}

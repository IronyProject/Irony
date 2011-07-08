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
    private object _lockObject = new object();
    private TimeSpan _autoUpdateDelay = TimeSpan.FromMilliseconds(500);
    private GrammarItem _selectedGrammar;
    private Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>();
    private Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();

    public event EventHandler AssemblyUpdated;

    public GrammarItem SelectedGrammar {
      get { return _selectedGrammar; }
      set { _selectedGrammar = value; }
    }

    public Grammar CreateGrammar() {
      if (SelectedGrammar == null)
        return null;

      var type = SelectedGrammarAssembly.GetType(SelectedGrammar.TypeName, true, true);
      return Activator.CreateInstance(type) as Grammar;
    }

    Assembly SelectedGrammarAssembly {
      get {
        if (SelectedGrammar == null)
          return null;

        var location = SelectedGrammar.Location;
        if (!_loadedAssemblies.ContainsKey(location) || _loadedAssemblies[location] == null) {
          _loadedAssemblies[location] = LoadAssembly(location);
          _watchers[location] = CreateFileWatcher(location);
        }
        return _loadedAssemblies[location];
      }
    }

    private FileSystemWatcher CreateFileWatcher(string location) {
      var folder = Path.GetDirectoryName(location);
      var watcher = new FileSystemWatcher(folder);
      watcher.Filter = Path.GetFileName(location);
      watcher.Changed += (s, e) => {
        _loadedAssemblies[location] = null; // clear cached assembly
        ThreadPool.QueueUserWorkItem(_ => {
          Thread.Sleep(_autoUpdateDelay);
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

    Assembly LoadAssembly(string fileName) {
      // 1. Assembly.Load doesn't block the file
      // 2. Assembly.Load doesn't check if the assembly is already loaded in the current AppDomain
      return Assembly.Load(File.ReadAllBytes(fileName));
    }
  }
}

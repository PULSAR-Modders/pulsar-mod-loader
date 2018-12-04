using PulsarPluginLoader.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PulsarPluginLoader
{
    public class PluginManager
    {
        public delegate void PluginLoaded(string name, PulsarPlugin plugin);
        public event PluginLoaded OnPluginSuccessfullyLoaded;

        private readonly Dictionary<string, PulsarPlugin> activePlugins;
        private readonly HashSet<string> pluginDirectories;

        private static PluginManager _instance = null;

        public static PluginManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PluginManager();
                }

                return _instance;
            }
        }

        public PluginManager()
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Logger.Info($"Starting {fvi.ProductName} v{fvi.FileVersion}");

            activePlugins = new Dictionary<string, PulsarPlugin>();
            pluginDirectories = new HashSet<string>();

            // Add plugins directories to AppDomain so plugins referencing other as-yet-unloaded plugins don't fail to find assemblies
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolvePluginsDirectory);

            // Force Photon's static constructor to run so patching its methods doesn't fail
            RuntimeHelpers.RunClassConstructor(typeof(PhotonNetwork).TypeHandle);
        }

        public PulsarPlugin GetPlugin(string name)
        {
            if (activePlugins.TryGetValue(name, out PulsarPlugin plugin))
            {
                return plugin;
            }
            else
            {
                return null;
            }
        }

        public bool IsPluginLoaded(string name)
        {
            return activePlugins.ContainsKey(name);
        }

        public IEnumerable<PulsarPlugin> GetAllPlugins()
        {
            return activePlugins.Values;
        }

        public void LoadPluginsDirectory(string pluginsDir)
        {
            Logger.Info($"Attempting to load plugins from {pluginsDir}");

            // Manage directories
            if (!Directory.Exists(pluginsDir))
            {
                Directory.CreateDirectory(pluginsDir);
            }
            pluginDirectories.Add(pluginsDir);

            // Load plugins
            foreach (string assemblyPath in Directory.GetFiles(pluginsDir, "*.dll"))
            {
                if (Path.GetFileName(assemblyPath) != "0Harmony.dll")
                {
                    LoadPlugin(assemblyPath);
                }
            }

            Logger.Info($"Finished loading {activePlugins.Count} plugins!");
        }

        private Assembly ResolvePluginsDirectory(object sender, ResolveEventArgs args)
        {
            // Search for dependency in every plugins directory loaded so far
            foreach (string pluginsDir in pluginDirectories)
            {
                string assemblyPath = Path.Combine(pluginsDir, new AssemblyName(args.Name).Name + ".dll");

                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
            }

            // Failed to find dependency!  Assemblies missing from plugins directory?
            return null;
        }

        public PulsarPlugin LoadPlugin(string assemblyPath)
        {

            if (!File.Exists(assemblyPath))
            {
                throw new IOException($"Couldn't find file: {assemblyPath}");
            }

            Assembly asm = Assembly.LoadFile(assemblyPath);
            Type pluginType = asm.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(PulsarPlugin)));

            if (pluginType != null)
            {
                Logger.Info($"Loading plugin: {pluginType.AssemblyQualifiedName}");

                PulsarPlugin plugin = Activator.CreateInstance(pluginType) as PulsarPlugin;
                activePlugins.Add(plugin.Name, plugin);
                OnPluginSuccessfullyLoaded?.Invoke(plugin.Name, plugin);

                return plugin;
            }
            else
            {
                Logger.Info($"Skipping {Path.GetFileName(assemblyPath)}; couldn't find plugin entry point.");

                return null;
            }
        }
    }
}

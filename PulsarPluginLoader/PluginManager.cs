using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using System.Runtime.InteropServices;
using PulsarPluginLoader.Chat.Commands;

namespace PulsarPluginLoader
{
    public class PluginManager
    {
        public const string VERSION = "0.10.0";

        public delegate void PluginLoaded(string name, PulsarPlugin plugin);
        public delegate void PluginUnloaded(PulsarPlugin plugin);
        public event PluginLoaded OnPluginSuccessfullyLoaded;
        public event PluginUnloaded OnPluginUnloaded;

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
            Logger.Info($"Starting PulsarPluginLoader v{VERSION}");

            activePlugins = new Dictionary<string, PulsarPlugin>();
            pluginDirectories = new HashSet<string>();

            // Add plugins directories to AppDomain so plugins referencing other as-yet-unloaded plugins don't fail to find assemblies
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolvePluginsDirectory);
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

        public bool IsPluginLoaded(string name) => activePlugins.ContainsKey(name);

        public IEnumerable<PulsarPlugin> GetAllPlugins() => activePlugins.Values;

        public void LoadPluginsDirectory(string pluginsDir)
        {
            OnPluginSuccessfullyLoaded += Events.EventHelper.RegisterEventHandlers;
            Logger.Info($"Attempting to load plugins from {pluginsDir}");

            // Manage directories
            if (!Directory.Exists(pluginsDir))
            {
                Directory.CreateDirectory(pluginsDir);
            }
            pluginDirectories.Add(pluginsDir);

            // Load plugins
            foreach (string plugin in Directory.GetFiles(pluginsDir, "*.dll", SearchOption.AllDirectories))
                LoadPlugin(plugin);

            Logger.Info($"Finished loading {activePlugins.Count.ToString()} plugin{(activePlugins.Count == 1 ? string.Empty : 's')}!"); // C# 9.0 feature
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

            try
            {
                Assembly asm = Assembly.LoadFile(assemblyPath);
                Type pluginType = asm.GetTypes().AsParallel().FirstOrDefault(t => t.IsSubclassOf(typeof(PulsarPlugin)));

                if (pluginType != null)
                {
                    PulsarPlugin plugin = Activator.CreateInstance(pluginType) as PulsarPlugin;
                    activePlugins.Add(plugin.Name, plugin);
                    OnPluginSuccessfullyLoaded?.Invoke(plugin.Name, plugin);

                    Logger.Info($"Loaded Plugin: {plugin.Name} Version {plugin.Version} Author: {plugin.Author}");
                    return plugin;
                }
                else
                {
                    Logger.Info($"Skipping {Path.GetFileName(assemblyPath)}; couldn't find plugin entry point.");

                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.Info($"Failed to load plugin: {Path.GetFileName(assemblyPath)}\n{e}");

                return null;
            }
        }
        
        internal void UnloadPlugin(PulsarPlugin plugin, ref Harmony harmony)
        {
            activePlugins.Remove(plugin.Name); // Removes selected plugin from activePlugins
            harmony.UnpatchAll(plugin.HarmonyIdentifier()); // Removes all patches from selected plugin
            OnPluginUnloaded?.Invoke(plugin);
            Logger.Info($"Unloaded plugin: {plugin.Name} Version {plugin.Version} Author: {plugin.Author}");
            GC.Collect();
        }
    }
}

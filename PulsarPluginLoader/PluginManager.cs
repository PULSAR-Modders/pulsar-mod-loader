using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PulsarModLoader
{
    public class PluginManager
    {
        public delegate void PluginLoaded(string name, PulsarMod plugin);
        public delegate void PluginUnloaded(PulsarMod plugin);
        public event PluginLoaded OnPluginSuccessfullyLoaded;
        public event PluginUnloaded OnPluginUnloaded;
        public FileVersionInfo PPLVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

        private readonly Dictionary<string, PulsarMod> activePlugins;
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
            Logger.Info($"Starting {PPLVersionInfo.ProductName} v{PPLVersionInfo.FileVersion}");

            activePlugins = new Dictionary<string, PulsarMod>();
            pluginDirectories = new HashSet<string>();

            // Add plugins directories to AppDomain so plugins referencing other as-yet-unloaded plugins don't fail to find assemblies
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolvePluginsDirectory);

            // Force Photon's static constructor to run so patching its methods doesn't fail
            RuntimeHelpers.RunClassConstructor(typeof(PhotonNetwork).TypeHandle);
        }

        public PulsarMod GetPlugin(string name)
        {
            if (activePlugins.TryGetValue(name, out PulsarMod plugin))
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

        public IEnumerable<PulsarMod> GetAllPlugins()
        {
            return activePlugins.Values;
        }

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

        public PulsarMod LoadPlugin(string assemblyPath)
        {

            if (!File.Exists(assemblyPath))
            {
                throw new IOException($"Couldn't find file: {assemblyPath}");
            }

            try
            {
                Assembly asm = Assembly.LoadFile(assemblyPath);
                Type pluginType = asm.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(PulsarMod)));

                if (pluginType != null)
                {
                    PulsarMod plugin = Activator.CreateInstance(pluginType) as PulsarMod;
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

        internal void UnloadPlugin(PulsarMod plugin, ref Harmony harmony)
        {
            activePlugins.Remove(plugin.Name); // Removes selected plugin from activePlugins
            harmony.UnpatchAll(plugin.HarmonyIdentifier()); // Removes all patches from selected plugin
            OnPluginUnloaded?.Invoke(plugin);
            Logger.Info($"Unloaded plugin: {plugin.Name} Version {plugin.Version} Author: {plugin.Author}");
            GC.Collect();
        }
    }
}

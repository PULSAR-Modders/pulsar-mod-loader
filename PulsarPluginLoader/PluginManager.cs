using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PulsarPluginLoader
{
    public class PluginManager
    {
        public delegate void PluginLoaded(string name, PulsarPlugin plugin);
        public event PluginLoaded OnPluginSuccessfullyLoaded;

        private readonly Dictionary<string, PulsarPlugin> activePlugins;
        private readonly HashSet<string> pluginDirectories;

        public readonly Dictionary<string, Assembly> activeManagedLibs;
        public readonly Dictionary<string, IntPtr> activeUnmanagedLibs;
        private readonly HashSet<string> libDirectories;

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

            activeManagedLibs = new Dictionary<string, Assembly>();
            activeUnmanagedLibs = new Dictionary<string, IntPtr>();
            libDirectories = new HashSet<string>();
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

        public PulsarPlugin LoadPlugin(string assemblyPath)
        {

            if (!File.Exists(assemblyPath))
            {
                throw new IOException($"Couldn't find file: {assemblyPath}");
            }

            try
            {
                Assembly asm = Assembly.LoadFile(assemblyPath);
                Type pluginType = asm.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(PulsarPlugin)));

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

        public void LoadLibrariesDirectory(string LibDir)
        {
            #region init...
            System.Diagnostics.Stopwatch timer = new Stopwatch();
            timer.Start();
            int counter = 0;
            Logger.Info($"Attempting to load libraries from {LibDir}");

            if (!Directory.Exists(LibDir))
            {
                Logger.Info($"{LibDir} directory not found! Creating...");
                Directory.CreateDirectory(LibDir);
            }
            libDirectories.Add(LibDir);

            var unmanaged_directory = Path.Combine(LibDir, "Unmanaged");
            if (!Directory.Exists(unmanaged_directory))
            {
                Logger.Info($"{unmanaged_directory} directory not found! Creating...");
                Directory.CreateDirectory(unmanaged_directory);
            }
            var managed_directory = Path.Combine(LibDir, "Managed");
            if (!Directory.Exists(managed_directory))
            {
                Logger.Info($"{managed_directory} directory not found! Creating...");
                Directory.CreateDirectory(managed_directory);
            }
            #endregion

            Logger.Info($"Loading unmanaged libs...");

            foreach (string assemblyPath in Directory.GetFiles(unmanaged_directory, "*.dll"))
            {
                if (!activeUnmanagedLibs.ContainsKey(Path.GetFileName(assemblyPath)))
                {
                    if (LoadUnmanagedLib(assemblyPath) != IntPtr.Zero) counter += 1;
                }
            }

            Logger.Info($"All unmanaged libs loaded!");
            Logger.Info($"Loading managed libs...");

            foreach (string assemblyPath in Directory.GetFiles(managed_directory, "*.dll"))
            {
                if (!activeUnmanagedLibs.ContainsKey(Path.GetFileName(assemblyPath)))
                {
                    if (LoadManagedLib(assemblyPath) != null) counter += 1;
                }
            }

            Logger.Info($"All managed libs loaded!");
            timer.Stop();
            Logger.Info($"Loaded {counter} libs in {timer.Elapsed}!");
        }

        public Assembly LoadManagedLib(string dllpath)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(dllpath);
            }
            catch (Exception e)
            {
                Logger.Info($"Error loading library from {dllpath} !");
            }
            if(assembly != null)
            {
                Logger.Info($"Loaded managed (C#) lib - {assembly.FullName}!");

                activeManagedLibs.Add(assembly.FullName, assembly);
            }

            return assembly;
        }

        public IntPtr LoadUnmanagedLib(string dllpath)
        {
            IntPtr lib = LoadLibrary(dllpath);

            var dllname = Path.GetFileName(dllpath);

            if (lib != IntPtr.Zero) // If lib == IntPtr.Zero, then lib wasnt loaded (lib 32bit or without Entry Point)
            {
                Logger.Info($"Loaded unmanaged (C/C++) lib - {dllname}!");
                activeUnmanagedLibs.Add(dllname, lib);
                return lib;
            }
            else { Logger.Info($"Error loading {dllname}!") ; return IntPtr.Zero; }
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);
    }
}

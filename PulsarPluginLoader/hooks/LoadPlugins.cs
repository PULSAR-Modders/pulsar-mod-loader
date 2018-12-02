using Harmony;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PulsarPluginLoader.hooks
{
    [HarmonyPatch(typeof(PLGlobal))]
    [HarmonyPatch("Start")]
    class LoadPlugins
    {
        private static bool pluginsLoaded = false;
        private static readonly string pluginsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Plugins");

        static void Prefix()
        {
            if (!pluginsLoaded)
            {
                LoadPluginsDirectory();
                pluginsLoaded = true;
            }
        }

        private static void LoadPluginsDirectory()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            
            Loader.Log($"Starting {fvi.ProductName} v{fvi.FileVersion}");
            Loader.Log($"Attempting to load plugins from {pluginsDir}");

            if (!Directory.Exists(pluginsDir))
            {
                Directory.CreateDirectory(pluginsDir);
            }

            // Add plugins folder to AppDomain so plugins referencing other as-yet-unloaded plugins don't fail to find assemblies
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolvePluginsDirectory);
            // Force PhotonNetwork's static constructor to run so patches of its methods don't fail
            RuntimeHelpers.RunClassConstructor(typeof(PhotonNetwork).TypeHandle);

            int LoadedPluginCounter = 0;
            foreach (string assemblyPath in Directory.GetFiles(pluginsDir, "*.dll"))
            {
                if (Path.GetFileName(assemblyPath) != "0Harmony.dll")
                {
                    bool isLoaded = LoadPlugin(assemblyPath);

                    if (isLoaded)
                    {
                        LoadedPluginCounter += 1;
                    }
                }
            }

            Loader.Log($"Finished loading {LoadedPluginCounter} plugins!");
        }

        private static Assembly ResolvePluginsDirectory(object sender, ResolveEventArgs args)
        {
            string assemblyPath = Path.Combine(pluginsDir, new AssemblyName(args.Name).Name + ".dll");
            Loader.Log(assemblyPath);
            if (!File.Exists(assemblyPath))
            {
                return null;
            }
            else
            {
                return Assembly.LoadFrom(assemblyPath);
            }
        }

        private static bool LoadPlugin(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
            {
                throw new IOException($"Couldn't find file: {assemblyPath}");
            }

            Loader.Log($"Scanning {Path.GetFileName(assemblyPath)} for plugin entry point...");

            bool pluginLoaded = LoadPluginBySubclass(assemblyPath);

            // Couldn't detect plugin by subclass; old style plugin?
            // TODO: Remove deprecated plugin style some day.
            if (!pluginLoaded)
            {
                pluginLoaded = LoadPluginByAttribute(assemblyPath);
            }

            if (!pluginLoaded)
            {
                Loader.Log($"Skipping {Path.GetFileName(assemblyPath)}; couldn't find plugin entry point.");
            }

            return pluginLoaded;
        }

        private static bool LoadPluginBySubclass(string assemblyPath)
        {
            Assembly asm = Assembly.LoadFile(assemblyPath);
            Type pluginType = asm.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(PulsarPlugin)));

            if (pluginType != null)
            {
                Loader.Log($"Loading plugin: {pluginType.AssemblyQualifiedName}");

                PulsarPlugin plugin = Activator.CreateInstance(pluginType) as PulsarPlugin;

                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool LoadPluginByAttribute(string assemblyPath)
        {
            Assembly asm = Assembly.LoadFrom(assemblyPath);
            foreach (Type t in asm.GetTypes())
            {
                foreach (MethodInfo m in t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
#pragma warning disable 612, 618
                    object[] attrs = m.GetCustomAttributes(typeof(PluginEntryPoint), inherit: false);
#pragma warning restore 612, 618

                    if (attrs != null && attrs.Length > 0)
                    {
                        Loader.Log($"Loading old-style plugin via {m.Name}: via {t.AssemblyQualifiedName}");
                        Loader.Log("Warning!  Plugin uses old attribute-style initialization.  Please upgrade to subclass-style initialization ASAP.");
                        m.Invoke(null, null);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

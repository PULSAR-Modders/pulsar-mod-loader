using Harmony;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PulsarPluginLoader.hooks
{
    [HarmonyPatch(typeof(PLGlobal))]
    [HarmonyPatch("Awake")]
    class LoadPlugins
    {
        private static bool pluginsLoaded = false;
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
            string pluginsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Plugins");

            Loader.Log(String.Format("Attempting to load plugins from {0}", pluginsDir));

            if (!Directory.Exists(pluginsDir))
            {
                Directory.CreateDirectory(pluginsDir);
            }

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

            Loader.Log(string.Format("Finished loading {0} plugins!", LoadedPluginCounter));
        }

        private static bool LoadPlugin(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
            {
                throw new IOException(string.Format("Couldn't find file: {0}", assemblyPath));
            }

            Loader.Log(string.Format("Scanning {0} for plugin entry point...", Path.GetFileName(assemblyPath)));

            bool pluginLoaded = LoadPluginBySubclass(assemblyPath);

            // Couldn't detect plugin by subclass; old style plugin?
            // TODO: Remove deprecated plugin style some day.
            if (!pluginLoaded)
            {
                pluginLoaded = LoadPluginByAttribute(assemblyPath);
            }

            if (!pluginLoaded)
            {
                Loader.Log(string.Format("Skipping {0}; couldn't find plugin entry point.", Path.GetFileName(assemblyPath)));
            }

            return pluginLoaded;
        }

        private static bool LoadPluginBySubclass(string assemblyPath)
        {
            Assembly asm = Assembly.LoadFile(assemblyPath);
            Type pluginType = asm.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(PulsarPlugin)));

            if (pluginType != null)
            {
                Loader.Log(string.Format("Loading plugin: {0}", pluginType.AssemblyQualifiedName));

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
                    object[] attrs = m.GetCustomAttributes(typeof(PluginEntryPoint), inherit: false);
                    if (attrs != null && attrs.Length > 0)
                    {
                        Loader.Log(string.Format("Loading old-style plugin via {0}: via {1}", m.Name, t.AssemblyQualifiedName));
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

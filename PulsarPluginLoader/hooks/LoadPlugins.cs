using Harmony;
using System;
using System.IO;
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

            /* Find methods labeled as the plugin's entry point */
            Loader.Log(string.Format("Searching for plugin entry point in {0}", Path.GetFileName(assemblyPath)));

            Assembly asm = Assembly.LoadFrom(assemblyPath);
            foreach (Type t in asm.GetTypes())
            {
                foreach (MethodInfo m in t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    object[] attrs = m.GetCustomAttributes(typeof(PluginEntryPoint), inherit: false);
                    if (attrs != null && attrs.Length > 0)
                    {
                        Loader.Log(string.Format("Loading plugin via {0}", m.Name));
                        m.Invoke(null, null);
                        return true;
                    }
                }
            }

            Loader.Log(string.Format("Skipping {0}; couldn't find plugin entry point.", Path.GetFileName(assemblyPath)));
            return false;
        }
    }
}

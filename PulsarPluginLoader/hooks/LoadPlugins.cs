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

            Assembly asm = Assembly.LoadFile(assemblyPath);
            Type pluginType = asm.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(PulsarPlugin)));

            if (pluginType != null)
            {
                Loader.Log(string.Format("Loading {0}", pluginType.AssemblyQualifiedName));

                PulsarPlugin plugin = Activator.CreateInstance(pluginType) as PulsarPlugin;

                return true;
            }
            else
            {
                Loader.Log(string.Format("Skipping {0}; couldn't find plugin entry point.", Path.GetFileName(assemblyPath)));
                return false;
            }
        }
    }
}

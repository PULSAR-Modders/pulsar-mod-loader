using Harmony;
using System.IO;
using System.Reflection;

namespace PulsarPluginLoader.Patches
{
    [HarmonyPatch(typeof(PLGlobal), "Start")]
    class LoadPlugins
    {
        private static bool pluginsLoaded = false;

        static void Prefix()
        {
            if (!pluginsLoaded)
            {
                string pluginsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Plugins");
                PluginManager.Instance.LoadPluginsDirectory(pluginsDir);
                pluginsLoaded = true;
            }
        }
    }
}

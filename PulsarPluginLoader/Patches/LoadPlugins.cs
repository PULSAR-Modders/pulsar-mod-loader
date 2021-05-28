using HarmonyLib;
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
                string LibsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Libraries");
                PluginManager.Instance.LoadLibrariesDirectory(LibsDir);

                string pluginsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Mods");
                PluginManager.Instance.LoadPluginsDirectory(pluginsDir);
                pluginsLoaded = true;
            }
        }
    }
}

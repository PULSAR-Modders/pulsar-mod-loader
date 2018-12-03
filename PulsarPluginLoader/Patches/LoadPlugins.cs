using Harmony;
using PulsarPluginLoader.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PulsarPluginLoader.Patches
{
    [HarmonyPatch(typeof(PLGlobal), "Start")]
    class LoadPlugins
    {
        private static readonly PluginManager pluginManager = new PluginManager();
        private static bool pluginsLoaded = false;

        static void Prefix()
        {
            if (!pluginsLoaded)
            {
                string pluginsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Plugins");
                pluginManager.LoadPluginsDirectory(pluginsDir);
                pluginsLoaded = true;
            }
        }
    }
}

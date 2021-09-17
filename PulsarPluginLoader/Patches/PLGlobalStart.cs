using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace PulsarModLoader.Patches
{
    [HarmonyPatch(typeof(PLGlobal), "Start")]
    class PLGlobalStart
    {
        private static bool pluginsLoaded = false;

        static void Prefix()
        {
            if (!pluginsLoaded)
            {
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/PulsarPluginLoaderConfig.json";
                if (!File.Exists(path))
                    PPLConfig.CreateDefaultConfig(path, true);
                else
                    PPLConfig.CreateConfigFromFile(path);


                new GameObject("ModManager", typeof(CustomGUI.GUIMain)) { hideFlags = HideFlags.HideAndDontSave };
                
                string pluginsDir = Path.Combine(Directory.GetCurrentDirectory(), "Mods");
                PluginManager.Instance.LoadPluginsDirectory(pluginsDir);
                pluginsLoaded = true;
            }
        }
    }
}

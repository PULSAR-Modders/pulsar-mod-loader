using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace PulsarModLoader.Patches
{
    [HarmonyPatch(typeof(PLGlobal), "Start")]
    class PLGlobalStart
    {
        private static bool modsLoaded = false;

        static void Prefix()
        {
            if (!modsLoaded)
            {
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/PulsarModLoaderConfig.json";
                if (!File.Exists(path))
                    PMLConfig.CreateDefaultConfig(path, true);
                else
                    PMLConfig.CreateConfigFromFile(path);


                new GameObject("ModManager", typeof(CustomGUI.GUIMain)) { hideFlags = HideFlags.HideAndDontSave };
                
                string modsDir = Path.Combine(Directory.GetCurrentDirectory(), "Mods");
                ModManager.Instance.LoadModsDirectory(modsDir);
                modsLoaded = true;
            }
        }
    }
}

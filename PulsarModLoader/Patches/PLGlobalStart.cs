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
                //DebugModeSetting
                if (bool.TryParse(PLXMLOptionsIO.Instance.CurrentOptions.GetStringValue("PMLDebugMode"), out bool result))
                {
                    Chat.Commands.DebugModeCommand.DebugMode = result;
                }

                //PML Config
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/PulsarModLoaderConfig.json";
                if (!File.Exists(path))
                    PMLConfig.CreateDefaultConfig(path, true);
                else
                    PMLConfig.CreateConfigFromFile(path);

                //Modmanager GUI Init.
                new GameObject("ModManager", typeof(CustomGUI.GUIMain)) { hideFlags = HideFlags.HideAndDontSave };

                //SaveDataManager Init()
                new SaveData.SaveDataManager();

                //ModLoading
                string modsDir = Path.Combine(Directory.GetCurrentDirectory(), "Mods");
                ModManager.Instance.LoadModsDirectory(modsDir);
                modsLoaded = true;
            }
        }
    }
}

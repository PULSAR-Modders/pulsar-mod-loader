using HarmonyLib;
using System.IO;
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
                //Events Init
                new PulsarModLoader.Events();

                //Modmanager GUI Init.
                new GameObject("ModManager", typeof(CustomGUI.GUIMain)) { hideFlags = HideFlags.HideAndDontSave };

                //SaveDataManager Init()
                new SaveData.SaveDataManager();

                //KeybindManager Init()
                _ = PulsarModLoader.Keybinds.KeybindManager.Instance;

                //MP Mod Checks
                new MPModChecks.MPModCheckManager();

                //ModLoading
                string modsDir = Path.Combine(Directory.GetCurrentDirectory(), "Mods");
                ModManager.Instance.LoadModsDirectory(modsDir);

                modsLoaded = true;
            }
        }
    }
}

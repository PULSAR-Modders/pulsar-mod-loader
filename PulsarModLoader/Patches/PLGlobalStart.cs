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
                //Modmanager GUI Init.
                new GameObject("ModManager", typeof(CustomGUI.GUIMain)) { hideFlags = HideFlags.HideAndDontSave };

                //SaveDataManager Init()
                new SaveData.SaveDataManager();

                //ModLoading
                string modsDir = Path.Combine(Directory.GetCurrentDirectory(), "Mods");
                ModManager.Instance.LoadModsDirectory(modsDir);

                //MP Mod Checks
                new MPModChecks.MPModCheckManager();

                modsLoaded = true;
            }
        }
    }
}

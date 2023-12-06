using UnityEngine;

namespace PulsarModLoader.Patches
{
    //Called by Entrypoint
    //[HarmonyPatch(typeof(PLGlobal), "Start")]
    class PLGlobalStart
    {
        private static bool modsLoaded = false;

        internal static void Prefix()
        {
            if (!modsLoaded)
            {
                //Logging adjustments
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

                //Patch Everything
                ModManager.Harmony.PatchAll();

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
                ModManager.Instance.LoadModsDirectory(ModManager.GetModsDir());

                modsLoaded = true;
            }
        }
    }
}

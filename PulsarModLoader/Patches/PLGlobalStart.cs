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
                if (BepinPlugin.instance != null)
                {
                    //Logging adjustments
                    Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                    Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

                    //Patch Everything
                    var harmony = new Harmony("wiki.pulsar.pml");
                    harmony.PatchAll(Assembly.GetExecutingAssembly());
                }

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

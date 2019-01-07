using Harmony;
using System.Diagnostics;
using System.Reflection;
using UnityEngine.UI;

namespace PulsarPluginLoader.Patches
{
    [HarmonyPatch(typeof(PLInGameUI), "Update")]
    class GameVersion
    {
        static void Postfix(PLNetworkManager __instance, Text ___CurrentVersionLabel)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            PLGlobal.SafeLabelSetText(___CurrentVersionLabel, $"{___CurrentVersionLabel.text}\nPPL {fvi.FileVersion}");
        }
    }
}

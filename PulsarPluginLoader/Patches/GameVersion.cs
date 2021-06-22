using HarmonyLib;
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
            PLGlobal.SafeLabelSetText(___CurrentVersionLabel, $"{___CurrentVersionLabel.text}\nPPL {PluginManager.VERSION}");
        }
    }
}

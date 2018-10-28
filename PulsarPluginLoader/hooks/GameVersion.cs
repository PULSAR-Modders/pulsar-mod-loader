using Harmony;
using System;

namespace PulsarPluginLoader.hooks
{
    [HarmonyPatch(typeof(PLNetworkManager))]
    [HarmonyPatch("Start")]
    class GameVersion
    {
        static void Postfix(PLNetworkManager __instance)
        {
            __instance.VersionString += "\n(PPL)";
        }

    }
}

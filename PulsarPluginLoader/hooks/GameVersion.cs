using Harmony;

namespace PulsarPluginLoader.hooks
{
    class GameVersion
    {
        [HarmonyPatch(typeof(PLNetworkManager))]
        [HarmonyPatch("Start")]
        class VersionModifier
        {
            static void Postfix(PLNetworkManager __instance)
            {
                __instance.VersionString += "\n(PPL)";
            }
        }
    }
}

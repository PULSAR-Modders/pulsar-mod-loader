using HarmonyLib;

namespace PulsarPluginLoader.Chat.Extensions
{
    [HarmonyPatch(typeof(PLServer), "Start")]
    class HarmonyServerStart
    {
        static void Postfix()
        {
            HarmonyNetworkUpdate.publicCached = false;
            HandlePublicCommands.RequestPublicCommands();
        }
    }
}

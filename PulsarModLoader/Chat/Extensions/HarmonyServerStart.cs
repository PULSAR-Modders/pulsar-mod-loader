using HarmonyLib;

namespace PulsarModLoader.Chat.Extensions
{
    [HarmonyPatch(typeof(PLServer), "Start")]
    class HarmonyServerStart
    {
        static void Postfix()
        {
            ChatHelper.publicCached = false;
            HandlePublicCommands.RequestPublicCommands();
        }
    }
}

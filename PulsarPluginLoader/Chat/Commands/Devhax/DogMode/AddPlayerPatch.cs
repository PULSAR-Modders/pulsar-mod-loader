using Harmony;

namespace PulsarPluginLoader.Chat.Commands.Devhax.DogMode
{
    [HarmonyPatch(typeof(PLServer), "AddPlayer")]
    class AddPlayerPatch
    {
        static void Postfix(PLPlayer inPlayer)
        {
            inPlayer.IsGodModeActive = DogModeCommand.IsEnabled;
        }
    }
}
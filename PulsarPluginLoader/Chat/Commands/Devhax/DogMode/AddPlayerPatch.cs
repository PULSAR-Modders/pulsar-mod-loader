using HarmonyLib;

namespace PulsarPluginLoader.Chat.Commands.Devhax.DogMode
{
    [HarmonyPatch(typeof(PLServer), "AddPlayer")]
    class AddPlayerPatch
    {
        static void Postfix(PLPlayer inPlayer)
        {
            if (inPlayer != null && inPlayer.TeamID == 0)
            {
                inPlayer.IsGodModeActive = DogModeCommand.IsEnabled;
            }
        }
    }
}
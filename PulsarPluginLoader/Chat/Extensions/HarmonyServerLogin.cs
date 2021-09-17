using HarmonyLib;
using PulsarModLoader.Chat.Commands;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Chat.Extensions
{
    [HarmonyPatch(typeof(PLServer), "StartPlayer")]
    class HarmonyServerLogin
    {
        static void Postfix(PLServer __instance, int inID)
        {
            if (PhotonNetwork.isMasterClient && ChatCommandRouter.Instance.getPublicCommandAliases().Length > 1)
            {
                PLPlayer player = __instance.GetPlayerFromPlayerID(inID);
                if (player != null && player.GetPhotonPlayer() != null)
                {
                    Messaging.Echo(player, $"[&%~[C0 Welcome ]&%~] {player.GetPlayerName()}!");
                    Messaging.Echo(player, "This game has some commands available.");
                    Messaging.Echo(player, "Type [&%~[C2 !help ]&%~] for more information.");
                }
            }
        }
    }
}

using PulsarPluginLoader.Chat.Commands;

namespace PulsarPluginLoader.Chat.Extensions
{
    class HandlePublicCommands : ModMessage
    {
        private static readonly string harmonyIdentifier = "";
        private static readonly string handlerIdentifier = "PulsarPluginLoader.Chat.Extensions.HandlePublicCommands";

        private static string[] GetPublicCommands()
        {
            return ChatCommandRouter.Instance.getPublicCommandAliases();
        }

        public static void RequestPublicCommands()
        {
            SendRPC(harmonyIdentifier, handlerIdentifier, PhotonTargets.MasterClient, new object[] { true });
        }

        public override void HandleRPC(object[] arguments, PhotonMessageInfo sender)
        {
            if ((bool)arguments[0])
            {
                if (PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer().IsMasterClient)
                {
                    string[] aliases = GetPublicCommands();
                    SendRPC(harmonyIdentifier, handlerIdentifier, sender.sender, new object[] { false, aliases });
                }
            }
            else
            {
                if (((string[])arguments[1]).Length > 0)
                {
                    HarmonyNetworkUpdate.publicCommands = (string[])arguments[1];
                    HarmonyNetworkUpdate.publicCached = true;
                    if (PLNetworkManager.Instance.IsTyping && PLNetworkManager.Instance.CurrentChatText.StartsWith("!"))
                    {
                        PLNetworkManager.Instance.CurrentChatText = HarmonyNetworkUpdate.AutoComplete(PLNetworkManager.Instance.CurrentChatText, (string[])arguments[1]);
                    }
                }
            }
        }
    }
}

using PulsarPluginLoader.Chat.Commands;
using PulsarPluginLoader.Utilities;
using System;

namespace PulsarPluginLoader.Chat.Extensions
{
    class HandlePublicCommands : ModMessage
    {
        private static readonly string harmonyIdentifier = "";
        private static readonly string handlerIdentifier = "PulsarPluginLoader.Chat.Extensions.HandlePublicCommands";
        private static readonly int version = 1;

        private static string[] GetPublicCommands()
        {
            return ChatCommandRouter.Instance.getPublicCommandAliases();
        }

        public static void RequestPublicCommands()
        {
            SendRPC(harmonyIdentifier, handlerIdentifier, PhotonTargets.MasterClient, new object[] { true, version });
        }

        public override void HandleRPC(object[] arguments, PhotonMessageInfo sender)
        {
            if ((int)arguments[1] != version)
            {
                Logger.Info($"Recieved invalid version for command list. Expected {version}, got {(int)arguments[1]}");
                return;
            }
            if ((bool)arguments[0])
            {
                if (PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer().IsMasterClient)
                {
                    string[] aliases = GetPublicCommands();
                    Tuple<string, string[][]>[] t = new Tuple<string, string[][]>[aliases.Length];
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        t[i] = new Tuple<string, string[][]>(aliases[i], ChatCommandRouter.Instance.GetPublicCommand(aliases[i]).Item1.Arguments());
                    }
                    SendRPC(harmonyIdentifier, handlerIdentifier, sender.sender, new object[] { false, version, t });
                }
            }
            else
            {
                if (((Tuple<string, string[][]>[])arguments[2]).Length > 0)
                {
                    ChatHelper.publicCommands = (Tuple<string, string[][]>[])arguments[2];
                    ChatHelper.publicCached = true;
                    if (PLNetworkManager.Instance.IsTyping && PLNetworkManager.Instance.CurrentChatText.StartsWith("!"))
                    {
                        string chatText = ChatHelper.AutoComplete(PLNetworkManager.Instance.CurrentChatText, ChatHelper.cursorPos);
                        if (chatText != PLNetworkManager.Instance.CurrentChatText)
                        {
                            PLNetworkManager.Instance.CurrentChatText = chatText;
                            ChatHelper.cursorPos2 = -1;
                        }
                    }
                }
            }
        }
    }
}

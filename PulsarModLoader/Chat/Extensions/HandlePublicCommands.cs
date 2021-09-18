using PulsarModLoader.Chat.Commands;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;

namespace PulsarModLoader.Chat.Extensions
{
    class HandlePublicCommands : ModMessage
    {
        private static readonly string harmonyIdentifier = "";
        private static readonly string handlerIdentifier = "PulsarModLoader.Chat.Extensions.HandlePublicCommands";
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
                    if (aliases.Length <= 1)
                    {
                        return;
                    }
                    string[][][] commandArguments = new string[aliases.Length][][];
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        commandArguments[i] = ChatCommandRouter.Instance.GetPublicCommand(aliases[i]).Item1.Arguments();
                    }
                    List<object> o = new List<object>();
                    o.Add(false);
                    o.Add(version);
                    o.Add(aliases);
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        o.Add(commandArguments[i]);
                    }
                    SendRPC(harmonyIdentifier, handlerIdentifier, sender.sender, o.ToArray());
                }
            }
            else
            {
                if (((string[])arguments[2]).Length > 0)
                {
                    string[] aliases = (string[])arguments[2];
                    ChatHelper.publicCommands = new Tuple<string, string[][]>[aliases.Length];
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        ChatHelper.publicCommands[i] = new Tuple<string, string[][]>(aliases[i], (string[][])arguments[i + 3]);
                    }
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

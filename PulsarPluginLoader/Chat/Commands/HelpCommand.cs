using PulsarPluginLoader.Chat.Commands;
using PulsarPluginLoader.Utilities;
using System;
using System.Linq;

namespace PulsarPluginLoader.Chat.Commands
{
    class HelpCommand : IChatCommand
    {
        public string[] CommandAliases()
        {
            return new string[] { "help", "?" };
        }

        public string Description()
        {
            return "Displays help text for a command, or the list of commands if none specified.";
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]} [command]";
        }

        public bool Execute(string arguments, int SenderID)
        {
            if (arguments != string.Empty)
            {
                string alias = arguments.Split(' ')[0];

                IChatCommand cmd = ChatCommandRouter.Instance.GetCommand(alias);
                if (cmd != null)
                {
                    Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"[&%~[C0 /{cmd.CommandAliases()[0]} ]&%~] - {cmd.Description()}");
                    Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Aliases: /{string.Join(", /", cmd.CommandAliases())}");
                    Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Usage: {cmd.UsageExample()}");
                }
                else
                {
                    Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Command /{alias} not found");
                }
            }
            else
            {
                string commandList = string.Join(Environment.NewLine, ChatCommandRouter.Instance.GetCommands().Select(cmd => $"/{cmd.CommandAliases()[0]} - {cmd.Description()}").ToArray());

                Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), "[&%~[C0 Command List: ]&%~]");
                Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), commandList);
            }

            return false;
        }
        public bool PublicCommand()
        {
            return false;
        }
    }
}

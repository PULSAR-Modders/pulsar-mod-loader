using PulsarPluginLoader.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            return $"/{CommandAliases()[0]} [command], /{CommandAliases()[0]} [page number]";
        }

        public bool Execute(string arguments, int SenderID)
        {
            int page = 1;
            if (arguments != string.Empty)
            {
                string alias = arguments.Split(' ')[0];
                if (!int.TryParse(alias, out page))
                {
                    IChatCommand cmd = ChatCommandRouter.Instance.GetCommand(alias);
                    if (cmd != null)
                    {
                        string name = "Pulsar Plugin Loader";
                        foreach (PulsarPlugin plugin in PluginManager.Instance.GetAllPlugins())
                        {
                            if (plugin.GetType() == cmd.GetType().Assembly.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(PulsarPlugin))))
                            {
                                name = plugin.Name;
                                break;
                            }
                        }
                        string prefix = cmd.PublicCommand() ? "!" : "/";
                        Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"[&%~[C0 {prefix}{cmd.CommandAliases()[0]} ]&%~] - {cmd.Description()} <color=#ff6600ff>[{name}]</color>");
                        Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Aliases: {prefix}{string.Join($", {prefix}", cmd.CommandAliases())}");
                        Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Usage: {cmd.UsageExample()}");
                    }
                    else
                    {
                        Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Command /{alias} not found");
                    }
                    return false;
                }
            }

            int commandsPerPage = (PLXMLOptionsIO.Instance.CurrentOptions.GetStringValueAsInt("ChatNumLines") * 5 + 10) - 1;
            IEnumerable<IChatCommand> commands = ChatCommandRouter.Instance.GetCommands();
            int pages = Mathf.CeilToInt(commands.Count()/(float)commandsPerPage);
            page--; //Pages start from 1
            if (page < 0)
            {
                page = 0;
            }

            Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"[&%~[C0 Command List: ]&%~] Page {page + 1} : {pages}");
            for (int i = 0; i < commandsPerPage; i++)
            {
                int index = i + page * commandsPerPage;
                if (i + page*commandsPerPage >= commands.Count())
                    break;
                IChatCommand command = commands.ElementAt(index);
                string prefix = command.PublicCommand() ? "!" : "/";
                Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"{prefix}{command.CommandAliases()[0]} - {command.Description()}");
                
            }

            return false;
        }
        public bool PublicCommand()
        {
            return false;
        }
    }
}

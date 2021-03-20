using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PulsarPluginLoader.Chat.Commands
{
    class PublicHelpCommand : IChatCommand
    {
        public string[] CommandAliases()
        {
            return new string[] { "commands", "command" };
        }

        public string Description()
        {
            return "Send a list of available public commands to everyone";
        }

        public bool Execute(string arguments, int SenderID)
        {
            if (PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer().IsMasterClient)
            {
                IEnumerable<IChatCommand> commands = ChatCommandRouter.Instance.GetCommands();
                List<IChatCommand> publicCommands = new List<IChatCommand>();
                foreach (IChatCommand command in commands)
                {
                    if (command.PublicCommand())
                    {
                        publicCommands.Add(command);
                    }
                }

                if (publicCommands.Count <= 1)
                {
                    return false;
                }

                PLPlayer sender = PLServer.Instance.GetPlayerFromPlayerID(SenderID);
                int page = 1;
                if (!string.IsNullOrWhiteSpace(arguments))
                {
                    if (!int.TryParse(arguments, out page))
                    {
                        if (arguments.StartsWith("!"))
                        {
                            arguments = arguments.Substring(1);
                        }
                        IChatCommand cmd = ChatCommandRouter.Instance.GetCommand(arguments);
                        if (cmd != null && cmd.PublicCommand())
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
                            Messaging.Echo(sender, $"[&%~[C3 !{cmd.CommandAliases()[0]} ]&%~] - {cmd.Description()} <color=#ff6600ff>[{name}]</color>");
                            Messaging.Echo(sender, $"Aliases: !{string.Join($", !", cmd.CommandAliases())}");
                            Messaging.Echo(sender, $"Usage: {cmd.UsageExample()}");
                        }
                        else
                        {
                            Messaging.Echo(sender, $"Command !{arguments} not found");
                        }
                        return false;
                    }
                }

                int commandsPerPage = 14 /*(PLXMLOptionsIO.Instance.CurrentOptions.GetStringValueAsInt("ChatNumLines") * 5 + 10) - 1*/; //Minimum value
                int pages = Mathf.CeilToInt(publicCommands.Count() / (float)commandsPerPage);
                
                page--; //Pages start from 1
                if (page < 0)
                {
                    page = 0;
                }

                string header = pages == 1 ? $"[&%~[C3 Available Commands: ]&%~]" : $"[&%~[C3 Available Commands: ]&%~] Page {page + 1} : {pages}";
                Messaging.Echo(sender, header);
                for (int i = 0; i < commandsPerPage; i++)
                {
                    int index = i + page * commandsPerPage;
                    if (i + page * commandsPerPage >= publicCommands.Count())
                        break;
                    IChatCommand command = publicCommands.ElementAt(index);
                    Messaging.Echo(sender, $"!{command.CommandAliases()[0]} - {command.Description()}");

                }
            }
            return false;
        }

        public bool PublicCommand()
        {
            return true;
        }

        public string UsageExample()
        {
            return $"!{CommandAliases()[0]}";
        }
    }
}

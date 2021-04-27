using PulsarPluginLoader.Chat.Commands.CommandRouter;
using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PulsarPluginLoader.Chat.Commands
{
    class HelpCommand : ChatCommand
    {
        public override string[] CommandAliases()
        {
            return new string[] { "help", "?" };
        }

        public override string Description()
        {
            return "Displays help text for a command, or the list of commands if none specified.";
        }

        public override string[] UsageExamples()
        {
            return new List<string>(base.UsageExamples()).Concat(
                new string[] { $"/{CommandAliases()[0]} clear", $"/{CommandAliases()[0]} 3" }
                ).ToArray();
        }

        public override string[][] Arguments()
        {
            return new string[][] { new string[] { "%command", "%page_number" } };
        }

        public override void Execute(string arguments)
        {
            int page = 1;
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                string alias = arguments.Split(' ')[0];
                if (!int.TryParse(alias, out page))
                {
                    Tuple<ChatCommand, PulsarPlugin> t = ChatCommandRouter.Instance.GetCommand(alias);
                    if (t != null)
                    {
                        ChatCommand cmd = t.Item1;
                        string name = t.Item2 != null ? t.Item2.Name : "Pulsar Plugin Loader";

                        Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"[&%~[C0 /{cmd.CommandAliases()[0]} ]&%~] - {cmd.Description()} <color=#ff6600ff>[{name}]</color>");
                        Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Aliases: /{string.Join($", /", cmd.CommandAliases())}");
                        Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Usage: {cmd.UsageExamples()[0]}");
                        for (int i = 1; i < cmd.UsageExamples().Length; i++)
                        {
                            Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"       {cmd.UsageExamples()[i]}");
                        }
                    }
                    else
                    {
                        Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Command /{alias} not found");
                    }
                    return;
                }
            }

            int commandsPerPage = (PLXMLOptionsIO.Instance.CurrentOptions.GetStringValueAsInt("ChatNumLines") * 5 + 10) - 1;
            IOrderedEnumerable<Tuple<ChatCommand, PulsarPlugin>> commands = ChatCommandRouter.Instance.GetCommands();
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
                ChatCommand command = commands.ElementAt(index).Item1;
                Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"/{command.CommandAliases()[0]} - {command.Description()}");
                
            }
        }
    }
}

using PulsarModLoader.Chat.Commands.CommandRouter;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PulsarModLoader.Chat.Commands
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
            PhotonPlayer player = PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer();
            int page = 1;
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                if (!int.TryParse(arguments, out page))
                {
                    if (arguments[0] == '/')
                    {
                        arguments = arguments.Substring(1);
                    }
                    Tuple<ChatCommand, PulsarMod> t = ChatCommandRouter.Instance.GetCommand(arguments);
                    if (t != null)
                    {
                        ChatCommand cmd = t.Item1;
                        string name = t.Item2 != null ? t.Item2.Name : "Pulsar Mod Loader";

                        Messaging.Echo(player, $"[&%~[C0 /{cmd.CommandAliases()[0]} ]&%~] - {cmd.Description()} <color=#ff6600ff>[{name}]</color>");
                        Messaging.Echo(player, $"Aliases: /{string.Join($", /", cmd.CommandAliases())}");
                        Messaging.Echo(player, $"Usage: {cmd.UsageExamples()[0]}");
                        for (int i = 1; i < cmd.UsageExamples().Length; i++)
                        {
                            Messaging.Echo(player, $"       {cmd.UsageExamples()[i]}");
                        }
                    }
                    else
                    {
                        Messaging.Echo(player, $"Command /{arguments} not found");
                    }
                    return;
                }
            }

            int commandsPerPage = (PLXMLOptionsIO.Instance.CurrentOptions.GetStringValueAsInt("ChatNumLines") * 5 + 10) - 2;
            IOrderedEnumerable<Tuple<ChatCommand, PulsarMod>> commands = ChatCommandRouter.Instance.GetCommands();
            int pages = Mathf.CeilToInt(commands.Count()/(float)commandsPerPage);
            page--; //Pages start from 1
            if (page < 0)
            {
                page = 0;
            }

            Messaging.Echo(player, pages == 1 && page == 0 ? "[&%~[C0 Command List: ]&%~] :" : $"[&%~[C0 Command List: ]&%~] Page {page + 1} : {pages}");
            for (int i = 0; i < commandsPerPage; i++)
            {
                int index = i + page * commandsPerPage;
                if (i + page*commandsPerPage >= commands.Count())
                    break;
                ChatCommand command = commands.ElementAt(index).Item1;
                Messaging.Echo(player, $"/{command.CommandAliases()[0]} - {command.Description()}");
                
            }
            Messaging.Echo(player, "Use [&%~[C2 /help <command> ]&%~] for details about a specific command");
        }
    }
}

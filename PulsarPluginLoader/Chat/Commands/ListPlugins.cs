using PulsarPluginLoader.Chat.Commands.CommandRouter;
using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PulsarPluginLoader.Chat.Commands
{
    class ListPlugins : ChatCommand
    {
        public override string[] CommandAliases()
        {
            return new string[] { "plugin", "plugins", "listPlugins" };
        }

        public override string Description()
        {
            return "Displays information about a plugin, or the list of plugins if none specified";
        }

        public override string[][] Arguments()
        {
            return new string[][] { new string[] { "%plugin_name", "%plugin_identifier" } };
        }

        public override void Execute(string arguments)
        {
            PhotonPlayer player = PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer();
            int page = 1;
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                if (!int.TryParse(arguments, out page))
                {
                    PulsarPlugin plugin = PluginManager.Instance.GetPlugin(arguments);
                    if (plugin == null)
                    {
                        foreach (PulsarPlugin p in PluginManager.Instance.GetAllPlugins())
                        {
                            if (p.HarmonyIdentifier().ToLower() == arguments.ToLower())
                            {
                                plugin = p;
                                break;
                            }
                        }
                    }
                    if (plugin != null)
                    {
                        Messaging.Echo(player, $"[&%~[C4 {plugin.Name} ]&%~] - {plugin.ShortDescription}");
                        Messaging.Echo(player, $"Version: {plugin.Version}");
                        if (!string.IsNullOrWhiteSpace(plugin.LongDescription))
                        {
                            Messaging.Echo(player, plugin.LongDescription);
                        }
                    }
                    else
                    {
                        Messaging.Echo(player, $"Plugin {arguments} not found");
                    }
                    return;
                }
            }

            int pluginsPerPage = (PLXMLOptionsIO.Instance.CurrentOptions.GetStringValueAsInt("ChatNumLines") * 5 + 10) - 2;
            IOrderedEnumerable<PulsarPlugin> plugins = PluginManager.Instance.GetAllPlugins().OrderBy(t => t.Name);
            int pages = Mathf.CeilToInt(plugins.Count() / (float)pluginsPerPage);
            page--; //Pages start from 1
            if (page < 0)
            {
                page = 0;
            }

            Messaging.Echo(player, pages == 1 && page == 0 ? "[&%~[C4 Plugin List: ]&%~] :" : $"[&%~[C4 Plugin List: ]&%~] Page {page + 1} : {pages}");
            for (int i = 0; i < pluginsPerPage; i++)
            {
                int index = i + page * pluginsPerPage;
                if (i + page * pluginsPerPage >= plugins.Count())
                    break;
                PulsarPlugin plugin = plugins.ElementAt(index);
                Messaging.Echo(player, $"{plugin.Name} - {plugin.ShortDescription}");
            }
            Messaging.Echo(player, "Use [&%~[C2 /plugin <plugin> ]&%~] for details about a specific plugin");
        }
    }
}

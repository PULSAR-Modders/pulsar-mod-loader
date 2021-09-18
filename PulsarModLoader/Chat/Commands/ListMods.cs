using PulsarModLoader.Chat.Commands.CommandRouter;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PulsarModLoader.Chat.Commands
{
    class ListMods : ChatCommand
    {
        public override string[] CommandAliases()
        {
            return new string[] { "mod", "mods", "listMods" };
        }

        public override string Description()
        {
            return "Displays information about a mod, or the list of mods if none specified";
        }

        public override string[][] Arguments()
        {
            return new string[][] { new string[] { "%mod_name", "%mod_identifier" } };
        }

        public override void Execute(string arguments)
        {
            PhotonPlayer player = PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer();
            int page = 1;
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                if (!int.TryParse(arguments, out page))
                {
                    PulsarMod mod = ModManager.Instance.GetMod(arguments);
                    if (mod == null)
                    {
                        foreach (PulsarMod p in ModManager.Instance.GetAllMods())
                        {
                            if (p.HarmonyIdentifier().ToLower() == arguments.ToLower())
                            {
                                mod = p;
                                break;
                            }
                        }
                    }
                    if (mod != null)
                    {
                        Messaging.Echo(player, $"[&%~[C4 {mod.Name} ]&%~] - {mod.ShortDescription}");
                        Messaging.Echo(player, $"Version: {mod.Version}");
                        if (!string.IsNullOrWhiteSpace(mod.LongDescription))
                        {
                            Messaging.Echo(player, mod.LongDescription);
                        }
                    }
                    else
                    {
                        Messaging.Echo(player, $"Mod {arguments} not found");
                    }
                    return;
                }
            }

            int modsPerPage = (PLXMLOptionsIO.Instance.CurrentOptions.GetStringValueAsInt("ChatNumLines") * 5 + 10) - 2;
            IOrderedEnumerable<PulsarMod> mods = ModManager.Instance.GetAllMods().OrderBy(t => t.Name);
            int pages = Mathf.CeilToInt(mods.Count() / (float)modsPerPage);
            page--; //Pages start from 1
            if (page < 0)
            {
                page = 0;
            }

            Messaging.Echo(player, pages == 1 && page == 0 ? "[&%~[C4 Mod List: ]&%~] :" : $"[&%~[C4 Mod List: ]&%~] Page {page + 1} : {pages}");
            for (int i = 0; i < modsPerPage; i++)
            {
                int index = i + page * modsPerPage;
                if (i + page * modsPerPage >= mods.Count())
                    break;
                PulsarMod mod = mods.ElementAt(index);
                Messaging.Echo(player, $"{mod.Name} - {mod.ShortDescription}");
            }
            Messaging.Echo(player, "Use [&%~[C2 /mod <mod> ]&%~] for details about a specific mod");
        }
    }
}

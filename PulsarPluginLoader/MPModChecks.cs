using HarmonyLib;
using PulsarPluginLoader.Utilities;

namespace PulsarPluginLoader
{
    class MPModChecks
    {
        public static string GetModList()
        {
            string modlist = string.Empty;
            foreach (PulsarPlugin plugin in PluginManager.Instance.GetAllPlugins())
            {
                if (plugin.MPFunctionality == (int)MPFunction.All || plugin.MPFunctionality == (int)MPFunction.HostApproved)
                {
                    modlist += $"{plugin.Name} {plugin.Version} MPF{plugin.MPFunctionality}\n";
                }
            }
            return modlist;
        }
        public static string GetMPModList(RoomInfo room)
        {
            if (room.customProperties.ContainsKey("modList"))
            {
                return room.customProperties["modList"].ToString();
            }
            return string.Empty;
        }
    }
    [HarmonyPatch(typeof(PLUIPlayMenu), "ActuallyJoinRoom")]
    class JoinRoomPatch
    {
        static bool Prefix(ref RoomInfo room)
        {
            string LocalMods = MPModChecks.GetModList();
            string MPMods = MPModChecks.GetMPModList(room);
            Logger.Info($"Joining room: {room.name} MPmodlist: {room.customProperties["modList"]} Localmodlist: {LocalMods}");
            if (!string.IsNullOrEmpty(LocalMods))
            {
                Logger.Info("Modlist != NullOrEmpty");
                if (MPMods != LocalMods)
                {
                    string missingmods = string.Empty;
                    string[] localmodlist = LocalMods.Split('\n');
                    foreach (string plugin in localmodlist)
                    {
                        Logger.Info("Checking client mod " + plugin);
                        if (!string.IsNullOrEmpty(plugin) && !MPMods.Contains(plugin))
                        {
                            missingmods += plugin + "\n";
                        }
                    }
                    string[] MPmodlist = MPMods.Split('\n');
                    if (missingmods != string.Empty)
                    {
                        Logger.Info("Client mods good, checking server mods");
                        foreach (string plugin in MPmodlist)
                        {
                            Logger.Info("Checking Server mod " + plugin);
                            if (!string.IsNullOrEmpty(plugin) && !LocalMods.Contains(plugin) && plugin.Contains("MPF3"))
                            {
                                missingmods += plugin + "\n";
                            }
                        }
                        if(missingmods != string.Empty)
                        {
                            Logger.Info("Server plugin list is not equal to local plugin list");
                            PLNetworkManager.Instance.MainMenu.AddActiveMenu(new PLErrorMessageMenu($"Failed to join crew! The Server is missing the following mods or is not up to date (try uninstalling/updating): {missingmods}"));
                            return false;
                        }
                    }
                    PLNetworkManager.Instance.MainMenu.AddActiveMenu(new PLErrorMessageMenu($"Failed to join crew! You are missing the following mods or the mods are not up to date: {missingmods}"));

                    Logger.Info("Local Plugin list is not equal to Server plugin list");
                    return false;
                }
            }
            Logger.Info("Modcheck passed, proceding ondwards");
            return true;
        }
    }
}

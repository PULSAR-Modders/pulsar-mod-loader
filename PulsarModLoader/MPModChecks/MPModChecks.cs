using HarmonyLib;
using PulsarModLoader.Utilities;
using Steamworks;
using System.Collections.Generic;

namespace PulsarModLoader.MPModChecks
{
    class MPModChecks
    {
        public static string ConvertModlist(List<string> Missinglist)
        {
            string Missing = string.Empty;
            for (int i = 0; i < Missinglist.Count; i++)
            {
                if (i < Missinglist.Count - 1)
                {
                    Missing += $"{Missinglist[i]},\n";
                }
                else
                {
                    Missing += Missinglist[i];
                }
            }
            return Missing;
        }
        public static string GetMPModList()
        {
            string modlist = string.Empty;
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                if (mod.MPFunctionality == (int)MPFunction.All || mod.MPFunctionality == (int)MPFunction.HostRequired)
                {
                    modlist += $"{mod.Name} {mod.Version} MPF{mod.MPFunctionality}\n";
                }
            }
            return modlist;
        }
        public static string GetModList()
        {
            string modlist = string.Empty;
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                modlist += $"{mod.Name} {mod.Version} MPF{mod.MPFunctionality}\n";
            }
            return modlist;
        }
        public static string GetHostModList(RoomInfo room)
        {
            if (room.CustomProperties.ContainsKey("modList"))
            {
                return room.CustomProperties["modList"].ToString();
            }
            return string.Empty;
        }
    }
    [HarmonyPatch(typeof(PLUIPlayMenu), "ActuallyJoinRoom")]
    class JoinRoomPatch
    {
        static bool Prefix(ref RoomInfo room)
        {
            //overall basic description: checks if it is possible to join room based on mods installed locally and on the server
            string LocalMods = MPModChecks.GetMPModList();
            string MPMods = MPModChecks.GetHostModList(room);
            Logger.Info($"Joining room: {room.Name} MPmodlist: {room.CustomProperties["modList"]} Localmodlist: {LocalMods}");
            if (!string.IsNullOrEmpty(LocalMods))
            {
                Logger.Info("Modlist != NullOrEmpty");
                if (MPMods != LocalMods)
                {
                    List<string> missingmods = new List<string>();
                    string[] localmodlist = LocalMods.Split('\n');
                    foreach (string mod in localmodlist)
                    {
                        //Logger.Info("Checking client mod " + mod);
                        if (!string.IsNullOrEmpty(mod) && !MPMods.Contains(mod))
                        {
                            missingmods.Add(mod);
                        }
                    }
                    string[] MPmodlist = MPMods.Split('\n');
                    if (missingmods.Count > 0)
                    {
                        Logger.Info("Client mods good, checking server mods");
                        foreach (string mod in MPmodlist)
                        {
                            //Logger.Info("Checking Server mod " + mod);
                            if (!string.IsNullOrEmpty(mod) && !LocalMods.Contains(mod) && mod.Contains("MPF3"))
                            {
                                missingmods.Add(mod);
                            }
                        }
                        if (missingmods.Count > 0)
                        {
                            Logger.Info("Server mod list is not equal to local mod list");
                            PLNetworkManager.Instance.MainMenu.AddActiveMenu(new PLErrorMessageMenu($"Failed to join crew! The Server is missing the following mods or is not up to date (try uninstalling/updating):\n{MPModChecks.ConvertModlist(missingmods)}"));
                            return false;
                        }
                    }
                    PLNetworkManager.Instance.MainMenu.AddActiveMenu(new PLErrorMessageMenu($"Failed to join crew! You are missing the following mods or the mods are not up to date:\n{MPModChecks.ConvertModlist(missingmods)}"));

                    Logger.Info("Local mod list is not equal to Server mod list");
                    return false;
                }
            }
            Logger.Info("Modcheck passed, proceding ondwards");
            return true;
        }
    }
    [HarmonyPatch(typeof(PLServer), "AttemptGetVerified")]
    class PrefixServerOnClientJoin
    {
        static void Prefix(PhotonMessageInfo pmi)
        {
            if (PhotonNetwork.isMasterClient)
            {
                Logger.Info("Sending Ping (asking for mod list/Pong)");
                ModMessageHelper.Instance.photonView.RPC("SendConnectionMessage", pmi.sender, new object[0]);
            }
        }
    }

    [HarmonyPatch(typeof(PLServer), "AttemptGetVerified")]
    class VerifyModlistBeforeConnection
    {
        static bool Prefix(ref PhotonMessageInfo pmi)
        {
            //Utilities.Logger.Info("About to check if containskey. isMasterClient: " + PhotonNetwork.isMasterClient.ToString());
            bool foundplayer = false;
            if (ModMessageHelper.Instance.GetPlayerMods(pmi.sender) != "NoPlayer")
            { //checks if server has received mod list from client. request for mod list is sent in the class 'PrefixServerOnClientJoin'
                foundplayer = true;
            }
            Utilities.Logger.Info("tried finding player, returned " + foundplayer.ToString());
            //Checks mod list
            if (foundplayer) //If server received mod list from client
            {
                List<string> missingmods = new List<string>();
                string LocalMods = MPModChecks.GetMPModList();
                string clientmods = ModMessageHelper.Instance.GetPlayerMods(pmi.sender);
                Logger.Info($"Starting Serverside Mod check");
                if (clientmods != LocalMods) //if the client's modlist isn't equal to the local mod list
                {
                    Logger.Info($"Checking if client is missing required mods");
                    string[] localmodlist = LocalMods.Split('\n');
                    foreach (string mod in localmodlist) //check local multiplayer mods to see if the client has required mods
                    {
                        if (!string.IsNullOrWhiteSpace(mod) && !clientmods.Contains(mod) && mod.Contains("MPF3"))
                        {
                            missingmods.Add(mod);
                        }
                    }
                    if (missingmods.Count == 0) //if nothing was added to the missing mod list check if the client needs something the server doesn't.
                    {
                        Logger.Info($"Client isn't missing mods, checking if client has mods that require server installation");
                        string[] clientmodlist = clientmods.Split('\n');
                        foreach (string mod in clientmodlist)
                        {
                            if (!string.IsNullOrWhiteSpace(mod) && !LocalMods.Contains(mod) && (mod.Contains("MPF2") || mod.Contains("MPF3")))
                            {
                                missingmods.Add(mod);
                            }
                        }
                        if (missingmods.Count > 0) //Client has non-server mods
                        {
                            Logger.Info("Client has non-server multiplayer mods");
                            string message = $"You have been disconnected for having the following mods (try removing them):\n{MPModChecks.ConvertModlist(missingmods)}";
                            ModMessageHelper.Instance.photonView.RPC("RecieveErrorMessage", pmi.sender, new object[] { message });
                            if (SteamManager.Initialized && pmi.sender.SteamID != CSteamID.Nil)
                            {
                                SteamUser.EndAuthSession(pmi.sender.SteamID);
                            }
                            PhotonNetwork.CloseConnection(pmi.sender);
                            return false;
                        }
                    }
                    else //client is missing server mods
                    {
                        Logger.Info("client is missing server mods");
                        string message = $"You have been disconnected for not having the following mods (try installing them):\n{MPModChecks.ConvertModlist(missingmods)}";
                        ModMessageHelper.Instance.photonView.RPC("RecieveErrorMessage", pmi.sender, new object[] { message });
                        if (SteamManager.Initialized && pmi.sender.SteamID != CSteamID.Nil)
                        {
                            SteamUser.EndAuthSession(pmi.sender.SteamID);
                        }
                        PhotonNetwork.CloseConnection(pmi.sender);
                        return false;
                    }

                }
                Logger.Info("Modcheck passed, proceding onwards");
            }
            else //client wasn't found in mod list
            {
                if (ModMessageHelper.ServerHasMPMods) //small vulnerability: if a client with mods disables the pong message, they can still connect with their multiplayer mods
                {
                    Utilities.Logger.Info("Didn't receive message or proper modlist. proceeding to kick PhotonPlayer");
                    string message = $"You have been disconnected for not having the mod loader installed";
                    ModMessageHelper.Instance.photonView.RPC("RecieveErrorMessage", pmi.sender, new object[] { message });
                    if (SteamManager.Initialized && pmi.sender.SteamID != CSteamID.Nil)
                    {
                        SteamUser.EndAuthSession(pmi.sender.SteamID);
                    }
                    PhotonNetwork.CloseConnection(pmi.sender);
                    return false;
                }
                Utilities.Logger.Info("Didn't receive message or proper modlist, but the server doesn't have multiplayer explicit mods. Proceeding onwards");
            }
            return true;
        }
    }
}

using HarmonyLib;
using PulsarModLoader.Patches;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Logger = PulsarModLoader.Utilities.Logger;

namespace PulsarModLoader.MPModChecks
{
    /// <summary>
    /// Manages Mod Checks.
    /// </summary>
    public class MPModCheckManager
    {
        /// <summary>
        /// Instantiates the ModCheckManager.
        /// </summary>
        public MPModCheckManager()
        {
            Instance = this;
            ModManager.Instance.OnModUnloaded += RefreshData;
            ModManager.Instance.OnModSuccessfullyLoaded += RefreshData;
            ModManager.Instance.OnAllModsLoaded += RefreshData;
        }

        /// <summary>
        /// Delays Refreshing of MPModList until all mods have been loaded.
        /// </summary>
        public void HoldMPModListRefresh()
        {
            HoldRefreshUntilAllModsLoaded = true;
        }

        private bool HoldRefreshUntilAllModsLoaded = false;

        /// <summary>
        /// Updates modlists
        /// </summary>
        public void RefreshData()
        {
            HoldRefreshUntilAllModsLoaded = true;
            UpdateMyModList();
            UpdateLobbyModList();
        }

        /// <summary>
        /// Calls normal RefreshData
        /// </summary>
        /// <param name="name"></param>
        /// <param name="mod"></param>
        private void RefreshData(string name, PulsarMod mod)
        {
            if (!HoldRefreshUntilAllModsLoaded)
            {
                RefreshData();
            }
        }

        /// <summary>
        /// Calls normal RefreshData
        /// </summary>
        /// <param name="mod"></param>
        private void RefreshData(PulsarMod mod = null)
        {
            RefreshData();
        }

        private void UpdateLobbyModList()  //Update Photon Lobby Listing with mod list
        {
            if (PhotonNetwork.isMasterClient && PhotonNetwork.inRoom && PLNetworkManager.Instance != null)
            {
                Room room = PhotonNetwork.room;
                Hashtable customProperties = room.CustomProperties;
                customProperties["modList"] = SerializeHashlessUserData();
                room.SetCustomProperties(customProperties);
            }
        }

        /// <summary>
        /// Static instance of the ModCheckManager
        /// </summary>
        public static MPModCheckManager Instance = null;

        private MPModDataBlock[] MyModList = null;

        /// <summary>
        /// Called after all mod checks finished HostSide
        /// </summary>
        public delegate void ModChecksFinishedHost(PhotonPlayer JoiningPhotonPlayer);

        /// <summary>
        /// Called after all mod checks finished HostSide
        /// </summary>
        public event ModChecksFinishedHost OnModChecksFinishedHost;

        /*
        /// <summary>
        /// Called after all mod checks finished ClientSide
        /// </summary>
        //public delegate void ModChecksFinishedClient();

        /// <summary>
        /// Called after all mod checks finished ClientSide
        /// </summary>
        //public event ModChecksFinishedClient OnModChecksFinishedClient;*/

        /// <summary>
        /// List of clients that have requested mod lists of other clients.
        /// </summary>
        public List<PhotonPlayer> RequestedModLists = new List<PhotonPlayer>();

        private Dictionary<PhotonPlayer, MPUserDataBlock> NetworkedPeersModLists = new Dictionary<PhotonPlayer, MPUserDataBlock>();

        private int HighestLevelOfMPMods = 0;

        /// <summary>
        /// Gets full mod list of Networked Peer.
        /// </summary>
        /// <param name="Photonplayer"></param>
        /// <returns></returns>
        public MPUserDataBlock GetNetworkedPeerMods(PhotonPlayer Photonplayer)
        {
            if (NetworkedPeersModLists.TryGetValue(Photonplayer, out MPUserDataBlock value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// Checks if given player has mod, checked by HarmonyID
        /// </summary>
        /// <param name="player"></param>
        /// <param name="HarmonyIdentifier"></param>
        /// <returns>Returns true if player has mod</returns>
        public bool NetworkedPeerHasMod(PhotonPlayer player, string HarmonyIdentifier)
        {
            MPUserDataBlock userData = GetNetworkedPeerMods(player);
            if(userData != null)
            {
                foreach(MPModDataBlock modData in userData.ModData)
                {
                    if(modData.HarmonyIdentifier == HarmonyIdentifier)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Finds all Networked Peers with a given harmony ID
        /// </summary>
        /// <param name="HarmonyIdentifier"></param>
        /// <returns>NetworkedPeers using given mod</returns>
        public List<PhotonPlayer> NetworkedPeersWithMod(string HarmonyIdentifier)
        {
            List<PhotonPlayer> playerList = new List<PhotonPlayer>();
            foreach(KeyValuePair<PhotonPlayer,MPUserDataBlock> userEntry in NetworkedPeersModLists)
            {
                foreach(MPModDataBlock modData in userEntry.Value.ModData)
                {
                    if(modData.HarmonyIdentifier == HarmonyIdentifier)
                    {
                        playerList.Add(userEntry.Key);
                    }
                }
            }
            return playerList;
        }

        /// <summary>
        /// Adds a player's mod list to the local NetworkedPeersModLists
        /// </summary>
        /// <param name="Photonplayer"></param>
        /// <param name="modList"></param>
        public void AddNetworkedPeerMods(PhotonPlayer Photonplayer, MPUserDataBlock modList)
        {
            if (NetworkedPeersModLists.ContainsKey(Photonplayer))
            {
                NetworkedPeersModLists[Photonplayer] = modList;
                return;
            }
            NetworkedPeersModLists.Add(Photonplayer, modList);
        }

        /// <summary>
        /// Clears player from NetworkedPeersModLists
        /// </summary>
        /// <param name="Photonplayer"></param>
        public void RemoveNetworkedPeerMods(PhotonPlayer Photonplayer)
        {
            NetworkedPeersModLists.Remove(Photonplayer);
        }

        private List<PulsarMod> GetMPModList()
        {
            HighestLevelOfMPMods = 0;
            List<PulsarMod> modList = new List<PulsarMod>();
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                if (mod.MPRequirements != (int)MPRequirement.HideFromServerList)
                {
                    if (mod.MPRequirements != (int)MPRequirement.MatchVersion && (mod.MPRequirements == (int)MPRequirement.Host || mod.MPRequirements == (int)MPRequirement.All) && mod.MPRequirements > HighestLevelOfMPMods)
                    {
                        HighestLevelOfMPMods = (mod.MPRequirements);
                    }
                    modList.Add(mod);
                }
            }
            return modList;
        }

        private void UpdateMyModList()
        {
            Logger.Info("Building MyModList");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<PulsarMod> UnprocessedMods = GetMPModList();
            MPModDataBlock[] ProcessedMods = new MPModDataBlock[UnprocessedMods.Count];
                for (int i = 0; i < UnprocessedMods.Count; i++)
                {
                    PulsarMod currentMod = UnprocessedMods[i];
                ProcessedMods[i] = new MPModDataBlock(currentMod.HarmonyIdentifier(), currentMod.Name, currentMod.Version, (MPRequirement)currentMod.MPRequirements, currentMod.VersionLink, currentMod.ModHash);
            }
            MyModList = ProcessedMods;
            stopwatch.Stop();
            Logger.Info("Finished Building MyModList, time elapsted: " + stopwatch.ElapsedMilliseconds.ToString());
        }



        /// <summary>
        /// Serilizes user data into a byte array for network transfer. Does not contain a hash
        /// </summary>
        /// <returns>Serilized User data (Hashless)</returns>
        public byte[] SerializeHashlessUserData()
        {
            MemoryStream dataStream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(dataStream))
            {
                //Datastream storage structure:
                writer.Write(Patches.GameVersion.PMLVersion);   //--Header--
				writer.Write(MyModList.Length);                 //string PMLVersion
                for (int i = 0; i < MyModList.Length; i++)      //int    modcount
                {                                               //
                    MPModDataBlock dataBlock = MyModList[i];    //--ModData--
					writer.Write(dataBlock.ModName);            //string mod name
                    writer.Write(dataBlock.HarmonyIdentifier);  //string harmony ident
                    writer.Write(dataBlock.Version);            //string mod version
                    writer.Write((byte)dataBlock.MPRequirement);//byte   MPRequirement
                    writer.Write(dataBlock.ModID);              //string ModID
                }
            }

			return dataStream.ToArray();
        }
        
        /// <summary>
        /// Serilizes user data into a byte array for network transfer. Contains a hash.
        /// </summary>
        /// <returns>Serilized User data (Hashfull)</returns>
        public byte[] SerializeHashfullUserData()
        {
            MemoryStream dataStream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(dataStream))
            {
                //Datastream storage structure:
                writer.Write(Patches.GameVersion.PMLVersion);   //--Header--
                writer.Write(MyModList.Length);                 //string PMLVersion
                for (int i = 0; i < MyModList.Length; i++)      //int    modcount
                {                                               //
                    MPModDataBlock dataBlock = MyModList[i];    //--ModData--
                    writer.Write(dataBlock.ModName);            //string mod name
                    writer.Write(dataBlock.HarmonyIdentifier);  //string harmony ident
                    writer.Write(dataBlock.Version);            //string mod version
                    writer.Write((byte)dataBlock.MPRequirement);//byte   MPRequirements
                    writer.Write(dataBlock.ModID);              //string ModID
                    writer.Write(dataBlock.Hash);               //byte[] Hash
                }
            }
            return dataStream.ToArray();
        }

        /// <summary>
        /// Deserializes bytes representing a serialized MPUserDataBlock which does not contain a hash.
        /// </summary>
        /// <param name="byteData"></param>
        /// <returns>MPUserDataBlock (Hashless)</returns>
        public static MPUserDataBlock DeserializeHashlessMPUserData(byte[] byteData)
        {
            MemoryStream memoryStream = new MemoryStream(byteData);
            memoryStream.Position = 0;
            try
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {

                    string PMLVersion = reader.ReadString();
                    int ModCount = reader.ReadInt32();
                    MPModDataBlock[] ModList = new MPModDataBlock[ModCount];
                    for (int i = 0; i < ModCount; i++)
                    {
                        string modname = reader.ReadString();
                        string HarmonyIdent = reader.ReadString();
                        string ModVersion = reader.ReadString();
                        MPRequirement MPRequirements = (MPRequirement)reader.ReadByte();
                        string ModID = reader.ReadString();
                        ModList[i] = new MPModDataBlock(HarmonyIdent, modname, ModVersion, MPRequirements, ModID);
                    }
                    return new MPUserDataBlock(PMLVersion, ModList);

                }
            }
            catch (Exception ex)
            {
                Logger.Info($"Failed to read mod list from Hashless MPUserData, returning null.\n{ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deserializes bytes representing a serialized MPUserDataBlock containing a hash.
        /// </summary>
        /// <param name="byteData"></param>
        /// <returns>MPUserDataBlock (Hashfull)</returns>
        public static MPUserDataBlock DeserializeHashfullMPUserData(byte[] byteData)
        {
            MemoryStream memoryStream = new MemoryStream(byteData);
            memoryStream.Position = 0;
            try
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {

                    string PMLVersion = reader.ReadString();
                    int ModCount = reader.ReadInt32();
                    MPModDataBlock[] ModList = new MPModDataBlock[ModCount];
                    for (int i = 0; i < ModCount; i++)
                    {
                        string modname = reader.ReadString();
                        string HarmonyIdent = reader.ReadString();
                        string ModVersion = reader.ReadString();
                        MPRequirement MPRequirements = (MPRequirement)reader.ReadByte();
                        string ModID = reader.ReadString();
                        byte[] Hash = reader.ReadBytes(32);
                        ModList[i] = new MPModDataBlock(HarmonyIdent, modname, ModVersion, MPRequirements, ModID, Hash);
                    }
                    return new MPUserDataBlock(PMLVersion, ModList);
                }
            }
            catch (Exception ex)
            {
                Logger.Info($"Failed to read mod list from Hashfull MPUserData, returning null.\n{ex.Message}");
                return null;
            }

        }

        /// <summary>
        /// Converts a ModDataBlock array to a string list, Usually for logging purposes. Starts with a new line
        /// </summary>
        /// <param name="ModDatas"></param>
        /// <returns>Converts ModDataBLocks to a string list.</returns>
        public static string GetModListAsString(MPModDataBlock[] ModDatas)
        {
            string ModList = string.Empty;
            foreach (MPModDataBlock DataBlock in ModDatas)
            {
                ModList += $"\n{DataBlock.ModName}";
            }
            return ModList;
        }

        private static MPUserDataBlock GetHostModList(RoomInfo room)
        {
            if (room.CustomProperties.ContainsKey("modList"))
            {
                try
                {
                    return DeserializeHashlessMPUserData((byte[])room.CustomProperties["modList"]);
                }
                catch
                {

                }
            }
            return new MPUserDataBlock();
        }

        private static void KickClient(PhotonPlayer client)
        {
            if (SteamManager.Initialized && client.SteamID != CSteamID.Nil)
            {
                SteamUser.EndAuthSession(client.SteamID);
            }
            PhotonNetwork.CloseConnection(client);
        }

        /// <summary>
        /// Upon attempting to join a room, the client checks if it is allowed to join based on mods installed locally and on the server
        /// </summary>
        /// <param name="room"></param>
        /// <returns>Returns true if client can connect to host.</returns>
        public bool ClientClickJoinRoom(RoomInfo room)
        {
            MPUserDataBlock HostModData = GetHostModList(room);
            if (HostModData.PMLVersion == string.Empty)
            {
                if (HighestLevelOfMPMods == (int)MPRequirement.Host || HighestLevelOfMPMods == (int)MPRequirement.All)
                {
                    PLNetworkManager.Instance.MainMenu.AddActiveMenu(new PLErrorMessageMenu($"<color=red>FAILED TO JOIN CREW!</color>\nMods requiring host installation or higher have been installed locally"));

                    Logger.Info("Mods requiring host installation or higher have been installed locally");
                    return false;
                }
                else
                {
                    return true;
                }
            }
            MPModDataBlock[] HostModList = HostModData.ModData;

            //Debug Logging
            string HostModListString = GetModListAsString(HostModList);
            string LocalModListString = GetModListAsString(MyModList);
            Logger.Info($"Joining room: {room.Name} ServerPMLVersion: {HostModData.PMLVersion}\n--Hostmodlist: {HostModListString}\n--Localmodlist: {LocalModListString}");

            //Variable Initiallization
            string hostMPLimitedMods = string.Empty;
            string localMPLimitedMods = string.Empty;
            string outdatedMods = string.Empty;
            int LocalModListLength = MyModList.Length;
            int HostModListLength = HostModList.Length;




            //Check all local mods and compare against host mods
            for (int a = 0; a < LocalModListLength; a++)
            {
                bool found = false;
                int b = 0;
                for (; b < HostModListLength; b++)
                {
                    if (HostModList[b].HarmonyIdentifier == MyModList[a].HarmonyIdentifier)
                    {
                        found = true;
                        break;
                    }
                }

                //Mod not found in host list, Check if mod mandates host installation via Host, All.
                if (!found)
                {
                    if (MyModList[a].MPRequirement == MPRequirement.Host || MyModList[a].MPRequirement == MPRequirement.All)
                    {
                        localMPLimitedMods += $"\n{MyModList[a].ModName}";
                    }
                }

                //Mod found in host list, check if mod versions match. -Should only reach this if mod was found in both lists. -Catches MPRequirements Host, All, or MatchVersion.
                else
                {
                    if (MyModList[a].MPRequirement != MPRequirement.None && MyModList[a].Version != HostModList[b].Version)
                    {
                        outdatedMods += $"\nLocal: {MyModList[a].ModName} {MyModList[a].Version} Host: {HostModList[b].ModName} {HostModList[b].Version}";
                    }
                }
            }




            //Check all host mods and compare against local mods (Ensures the host doesn't have a mod requiring client installation)
            for (int a = 0; a < HostModListLength; a++)
            {
                bool found = false;
                for (int b = 0; b < LocalModListLength; b++)
                {
                    if (HostModList[a].HarmonyIdentifier == MyModList[b].HarmonyIdentifier)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    if (HostModList[a].MPRequirement == MPRequirement.All)
                    {   //Host MP mod not found locally
                        hostMPLimitedMods += $"\n{HostModList[a].ModName}";
                    }
                }
            }




            string message = string.Empty;
            if (hostMPLimitedMods != string.Empty)
            {
                message += $"\n<color=yellow>YOU ARE MISSING THE FOLLOWING REQUIRED MODS</color>{hostMPLimitedMods}";
            }
            if (localMPLimitedMods != string.Empty)
            {
                message += $"\n<color=yellow>YOU CANNOT JOIN WITH THE FOLLOWING MODS INSTALLED</color>{localMPLimitedMods}";
            }
            if (outdatedMods != string.Empty)
            {
                message += $"\n<color=yellow>THE FOLLOWING MOD VERSIONS DO NOT MATCH</color>{outdatedMods}";
            }


            if (message != string.Empty)
            {
                PLNetworkManager.Instance.MainMenu.AddActiveMenu(new PLErrorMessageMenu($"<color=red>Failed to join crew!</color>{message}"));

                Logger.Info("Local mod list is not equal to Server mod list");
                return false;
            }
            else
            {
                Logger.Info("Modcheck passed, proceding ondwards");
                return true;
            }
        }

        /// <summary>
        /// Run by host on client connect to compare mod info.
        /// </summary>
        /// <param name="Player"></param>
        /// <returns>Returns true if client should be allowed to join.</returns>
        public bool HostOnClientJoined(PhotonPlayer Player)
        {
            MPModDataBlock[] ClientMods = null;
            bool foundplayer = false;
            if (NetworkedPeersModLists.ContainsKey(Player)) //checks if server has received mod list from client.
            {
                ClientMods = NetworkedPeersModLists[Player].ModData;
                foundplayer = true;
            }
            Logger.Info("HostOnClientJoined checking for player mods, returned " + foundplayer.ToString());


            //Checks mod list
            if (foundplayer) //If server received mod list from client
            {
                string missingMods = string.Empty;
                string clientMPLimitedMods = string.Empty;
                string outdatedMods = string.Empty;
                string incorrectHashMods = string.Empty;
                Logger.Info($"Starting Serverside Mod check");

                int localLength = MyModList.Length;
                int clientLength = ClientMods.Length;

                //Check mods installed locally against client
                for (int a = 0; a < localLength; a++)
                {
                    bool found = false;
                    int b = 0;
                    for (; b < clientLength; b++)
                    {
                        if (MyModList[a].HarmonyIdentifier == ClientMods[b].HarmonyIdentifier)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        if (MyModList[a].MPRequirement >= MPRequirement.Host) //limit these types of kicking to MPLimited mods.
                        {
                            if (MyModList[a].Version != ClientMods[b].Version)  //if mod versions don't match, add as kick reason
                            {
                                outdatedMods += $"\nLocal: {MyModList[a].ModName} {MyModList[a].Version} Client: {ClientMods[b].ModName} {ClientMods[b].Version}";
                            }
                            else if (Encoding.ASCII.GetString(MyModList[a].Hash) != Encoding.ASCII.GetString(ClientMods[b].Hash))   //if mod versions match but hash doesn't, add as kick reason
                            {
                                incorrectHashMods += $"\n{ClientMods[b].ModName}";
                                missingMods += $"\n{ClientMods[b].ModName}";
                                Logger.Info($"Client has bad hash for {MyModList[a].ModName}. Local: {Encoding.ASCII.GetString(MyModList[a].Hash)} Client: {Encoding.ASCII.GetString(ClientMods[b].Hash)}");
                            }
                        }
                    }
                    else
                    {
                        if (MyModList[a].MPRequirement == MPRequirement.All) //if client needs mod installed
                        {
                            missingMods += $"\n{MyModList[a].ModName}";
                        }
                    }
                }

                //Check mods installed on client against local
                for (int b = 0; b < clientLength; b++)
                {
                    bool found = false;
                    for (int a = 0; a < localLength; a++)
                    {
                        if (MyModList[a].HarmonyIdentifier == ClientMods[b].HarmonyIdentifier)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) //if client mod not installed locally requires host to have installed.
                    {
                        if (ClientMods[b].MPRequirement == MPRequirement.Host || ClientMods[b].MPRequirement == MPRequirement.All)
                        {
                            clientMPLimitedMods += $"\n{ClientMods[b].ModName}";
                        }
                    }
                }

                string message = string.Empty;
                if (missingMods != string.Empty)
                {
                    message += $"\n<color=yellow>You are missing the following required mods</color>{missingMods}";
                }
                if (clientMPLimitedMods != string.Empty)
                {
                    message += $"\n<color=yellow>You cannot join with the following mods installed</color>{clientMPLimitedMods}";
                }
                if (outdatedMods != string.Empty)
                {
                    message += $"\n<color=yellow>The following mod versions do not match</color>{outdatedMods}";
                }
                if (message != string.Empty)
                {
                    ModMessageHelper.Instance.photonView.RPC("RecieveErrorMessage", Player, new object[] { $"<color=red>Failed to join crew!</color>{message}" });
                    KickClient(Player);

                    Logger.Info("Kicked client for failing mod check with the following message:" + message);
                    return false;
                }
                else
                {
                    OnModChecksFinishedHost?.Invoke(Player);
                    Logger.Info("Modcheck passed, proceding onwards");
                }
            }
            else //client wasn't found in mod list
            {
                if (HighestLevelOfMPMods >= (int)MPRequirement.All)
                {
                    Utilities.Logger.Info("Didn't receive message or proper modlist. proceeding to kick PhotonPlayer");
                    string message = $"You have been disconnected for not having the mod loader installed";
                    ModMessageHelper.Instance.photonView.RPC("RecieveErrorMessage", Player, new object[] { message });
                    KickClient(Player);
                    return false;
                }
                Utilities.Logger.Info("Didn't receive message or proper modlist, but the server doesn't have multiplayer explicit mods. Proceeding onwards");
            }
            return true;
        }

        [HarmonyPatch(typeof(PLUIPlayMenu), "ActuallyJoinRoom")] //allow/disallow local client to join server.
        class JoinRoomPatch
        {
            static bool Prefix(RoomInfo room)
            {
                return Instance.ClientClickJoinRoom(room);
            }
        }

        [HarmonyPatch(typeof(PLServer), "AttemptGetVerified")]
        class AttemptGetVerifiedRecievePatch
        {
            static bool Prefix(PhotonMessageInfo pmi)
            {
                return Instance.HostOnClientJoined(pmi.sender);
            }
        }

        [HarmonyPatch(typeof(PLServer), "Update")]
        class AttemptGetVerifiedSendPatch
        {
            static void PatchMethod() //Client sends mod info just before requesting verification
            {
                Logger.Info("Sending 'RecieveConnectionMessage' RPC");
                ModMessageHelper.Instance.photonView.RPC("ServerRecieveModList", PhotonTargets.MasterClient, new object[]
                {
                    Instance.SerializeHashfullUserData()
                });
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> targetSequence = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Ldstr, "Attempting to get verified")
                };
                List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AttemptGetVerifiedSendPatch), "PatchMethod"))
                };
                return HarmonyHelpers.PatchBySequence(instructions, targetSequence, injectedSequence);
            }
        }

        [HarmonyPatch(typeof(PLNetworkManager), "OnPhotonPlayerDisconnected")]
        class RemovePlayerPatch
        {
            static void Postfix(PhotonPlayer photonPlayer)
            {
                Instance.RemoveNetworkedPeerMods(photonPlayer);
            }
        }

        [HarmonyPatch(typeof(PLNetworkManager), "OnPhotonPlayerConnected")]
        class AddPlayerPatch
        {
            static void Postfix(PhotonPlayer photonPlayer)
            {
                if(PhotonNetwork.isMasterClient)
                {
                    ModMessageHelper.Instance.photonView.RPC("ClientRecieveModList", photonPlayer, new object[]
                    {
                            Instance.SerializeHashlessUserData()
                    });
                    return;
                }
                Instance.RequestedModLists.Add(photonPlayer);
                ModMessageHelper.Instance.photonView.RPC("ClientRequestModList", photonPlayer, new object[] { });
            }
        }
    }
}

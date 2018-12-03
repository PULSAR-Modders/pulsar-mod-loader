using ExitGames.Client.Photon;
using Harmony;
using System;
using System.Linq;

namespace PulsarPluginLoader.Patches
{
    [HarmonyPatch(typeof(PhotonNetwork), "CreateRoom", new Type[] { typeof(string), typeof(RoomOptions), typeof(TypedLobby) })]
    class PhotonProperties
    {

        private static void Prefix(RoomOptions roomOptions)
        {
            Console.WriteLine("aaaaa PhotonNetwork.CreateRoom()");
            // Key-Value pairs attached to room as metadata
            roomOptions.customRoomProperties.Merge(new Hashtable() {
                { "isModded", true},
                { "playerList", ""} // "playerName\tclassName" -> "Test Name\tCaptain"
            });
            // Keys of metadata exposed to public game list
            roomOptions.customRoomPropertiesForLobby = roomOptions.customRoomPropertiesForLobby.AddRangeToArray(new string[] {
                "isModded",
                "playerList",
            });
        }

        public static void UpdatePlayerList()
        {
            if (PhotonNetwork.isMasterClient && PhotonNetwork.inRoom && PLNetworkManager.Instance != null)
            {
                Room room = PhotonNetwork.room;
                Hashtable customProperties = room.customProperties;

                customProperties["playerList"] = string.Join(
                    "\n",
                    PLServer.Instance.AllPlayers
                        .Where(player => player.TeamID == 0)
                        .Select(player => $"{player.GetPlayerName()}\t{player.GetClassName()}")
                        .ToArray()
                );

                room.SetCustomProperties(customProperties);
            }
        }
    }

    [HarmonyPatch(typeof(PLServer), "AddPlayer")]
    class PlayerJoined
    {
        private static void Postfix()
        {
            PhotonProperties.UpdatePlayerList();
        }
    }

    [HarmonyPatch(typeof(PLServer), "RemovePlayer")]
    class PlayerQuit
    {
        private static void Postfix()
        {
            PhotonProperties.UpdatePlayerList();
        }
    }

    [HarmonyPatch(typeof(PLPlayer), "SetClassID")]
    class ChangedClass
    {
        private static void Postfix()
        {
            PhotonProperties.UpdatePlayerList();
        }
    }
}

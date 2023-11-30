using ExitGames.Client.Photon;
using HarmonyLib;
using System;
using System.Linq;

namespace PulsarModLoader.Patches
{
    [HarmonyPatch(typeof(PhotonNetwork), "CreateRoom", new Type[] { typeof(string), typeof(RoomOptions), typeof(TypedLobby), typeof(string[]) })]
    public static class PhotonProperties
    {
        private static void Prefix(RoomOptions roomOptions)
        {
            // Key-Value pairs attached to room as metadata
            roomOptions.CustomRoomProperties.Merge(new Hashtable() {
                { "isModded", true },
                { "playerList", "" }, // "playerName\tclassName" -> "Test Name\tCaptain"
                { "modList", ""}
            });
            // Keys of metadata exposed to public game list
            roomOptions.CustomRoomPropertiesForLobby = roomOptions.CustomRoomPropertiesForLobby.AddRangeToArray(new string[] {
                "isModded",
                "playerList",
                "modList",
            });

            //MPModCheck
            roomOptions.CustomRoomProperties["modList"] = MPModChecks.MPModCheckManager.Instance.SerializeHashlessUserData();
        }

        /// <summary>
        /// Updates Player List for PhotonRoom Properties.
        /// </summary>
        public static void UpdatePlayerList()
        {
            if (PhotonNetwork.isMasterClient && PhotonNetwork.inRoom && PLNetworkManager.Instance != null)
            {
                Room room = PhotonNetwork.room;
                Hashtable customProperties = room.CustomProperties;

                customProperties["playerList"] = string.Join(
                    "\n",
                    PLServer.Instance.AllPlayers
                        .Where(player => player?.TeamID == 0)
                        .Select(player => $"{player.GetPlayerName()}\t{player.GetClassName()}")
                        .ToArray()
                );

                room.SetCustomProperties(customProperties);
            }
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

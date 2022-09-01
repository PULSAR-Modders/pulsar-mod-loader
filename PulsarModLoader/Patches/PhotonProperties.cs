using ExitGames.Client.Photon;
using HarmonyLib;
using PulsarModLoader.MPModChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

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
        }

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

    [HarmonyPatch(typeof(PLServer), "AddPlayer")]
    class PlayerJoined
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(PLPlayer), "ResetTalentPoints")),
            };

            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PhotonProperties), "UpdatePlayerList")),
            };

            return HarmonyHelpers.PatchBySequence(instructions, targetSequence, injectedSequence);
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

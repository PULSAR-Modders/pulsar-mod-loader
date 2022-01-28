using HarmonyLib;
using PulsarModLoader.Chat.Commands;
using UnityEngine;
using UnityEngine.UI;

namespace PulsarModLoader.Patches
{
    // broken due to ToString being called once. We can manually update CurrentVersionLabel, but is it necessary?
    /* [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(PLCachedFormatString<int, string, string>), "ToString", new[] { typeof(int), typeof(string), typeof(string) })]\
    class DebugReadout 
    {
        static void Prefix(ref string Obj3)
        {
            if (DebugModeCommand.DebugMode && PLServer.Instance != null && PLEncounterManager.Instance != null && PLNetworkManager.Instance != null)
            {
                Vector3 pos;
                if (PLNetworkManager.Instance.LocalPlayer != null)
                {
                    PLPawn localPawn = PLNetworkManager.Instance.LocalPlayer.GetPawn();
                    pos = localPawn != null ? localPawn.transform.position : Vector3.zero;
                }
                else pos = Vector3.zero;

                PLPersistantEncounterInstance encounter = PLEncounterManager.Instance.GetCurrentPersistantEncounterInstance();
                int levelID = encounter != null ? encounter.LevelID.GetDecrypted() : -1;

                PLSectorInfo sectorInfo = PLServer.GetCurrentSector();
                string visualType = sectorInfo != null ? sectorInfo.VisualIndication.ToString() : "--";
                int sector = sectorInfo != null ? sectorInfo.ID : -1;

                //Obj3 += $"\n\n\nPOS: {pos}, Level ID: {levelID}, Sector: {sector}, Visual: {visualType}";
                Obj3 += $"\nPOS: {pos}, Level ID: {levelID}, Sector: {sector}, Visual: {visualType}";
            }
        }
    } */
}

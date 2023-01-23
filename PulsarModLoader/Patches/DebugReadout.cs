using HarmonyLib;
using UnityEngine;

namespace PulsarModLoader.Patches
{
    [HarmonyPatch(typeof(PLInGameUI), "Update")]
    class DebugReadout
    {
        static void Postfix(PLInGameUI __instance)
        {
            if (PMLConfig.DebugMode && PLServer.Instance != null && PLEncounterManager.Instance != null && PLNetworkManager.Instance != null && GameVersion.Version != string.Empty)
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

                PLGlobal.SafeLabelSetText(__instance.CurrentVersionLabel, $"{GameVersion.Version}\nPOS: {pos}, Level ID: {levelID}, Sector: {sector}, Visual: {visualType}");
            }
        }
    }
}

using HarmonyLib;
using PulsarModLoader.Chat.Commands;
using UnityEngine;
using UnityEngine.UI;

namespace PulsarModLoader.Patches
{
    [HarmonyPatch(typeof(PLInGameUI), "Update")]
    class DebugReadout
    {
        static void Postfix(Text ___CurrentVersionLabel)
        {
            if (DebugModeCommand.DebugMode && PLServer.Instance != null && PLEncounterManager.Instance != null && PLNetworkManager.Instance != null && ___CurrentVersionLabel != null)
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

                PLGlobal.SafeLabelSetText(___CurrentVersionLabel, $"{___CurrentVersionLabel.text}\n\n\nPOS: {pos}, Level ID: {levelID}, Sector: {sector}, Visual: {visualType}");
            }
        }
    }
}

using Harmony;
using PulsarPluginLoader.Chat.Commands.Devhax;
using UnityEngine;
using UnityEngine.UI;

namespace PulsarPluginLoader.Patches
{
    [HarmonyPatch(typeof(PLInGameUI), "Update")]
    class DebugReadout
    {
        static void Postfix(PLNetworkManager __instance, Text ___CurrentVersionLabel)
        {
            if (DevhaxCommand.IsEnabled && PLServer.Instance != null && PLEncounterManager.Instance != null)
            {
                PLPawn localPawn = PLNetworkManager.Instance.LocalPlayer.GetPawn();
                Vector3 pos = localPawn != null ? localPawn.transform.position : Vector3.zero;

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
using Harmony;
using PulsarPluginLoader.Chat.Commands.Devhax;
using UnityEngine;
using UnityEngine.UI;
using Logger = PulsarPluginLoader.Utilities.Logger;

namespace PulsarPluginLoader.Patches
{
    [HarmonyPatch(typeof(PLInGameUI), "Update")]
    class DebugReadout
    {
        static void Postfix(PLNetworkManager __instance, Text ___CurrentVersionLabel)
        {
            if (DevhaxCommand.IsEnabled && PLServer.Instance != null && PLEncounterManager.Instance != null)
            {
                Vector3 pos = PLNetworkManager.Instance.LocalPlayer.GetPawn().transform.position;

                int levelID = PLEncounterManager.Instance.GetCurrentPersistantEncounterInstance().LevelID;

                PLSectorInfo sectorInfo = PLServer.GetCurrentSector();
                string visualType = sectorInfo.VisualIndication.ToString();
                int sector = sectorInfo.ID;

                PLGlobal.SafeLabelSetText(___CurrentVersionLabel, $"{___CurrentVersionLabel.text}\n\n\nPOS: {pos}, Level ID: {levelID}, Sector: {sector}, Visual: {visualType}");
            }
        }
    }
}
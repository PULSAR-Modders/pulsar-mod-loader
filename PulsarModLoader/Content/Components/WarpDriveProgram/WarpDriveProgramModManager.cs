using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace PulsarModLoader.Content.Components.WarpDriveProgram
{
    public class WarpDriveProgramModManager : ComponentModManager<WarpDriveProgramMod, EWarpDriveProgramType>
    {
        private static WarpDriveProgramModManager m_instance = null;
        public static WarpDriveProgramModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new WarpDriveProgramModManager();
                }
                return m_instance;
            }
        }

        WarpDriveProgramModManager() { }

        public static PLWarpDriveProgram CreateWarpDriveProgram(int Subtype, int level)
        {
            PLWarpDriveProgram InWarpDriveProgram;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InWarpDriveProgram = new PLWarpDriveProgram(EWarpDriveProgramType.SHIELD_BOOSTER, level);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    WarpDriveProgramMod WarpDriveProgramType = Instance.types[Subtype - Instance.VanillaMaxType];
                    InWarpDriveProgram.SubType = Subtype;
                    InWarpDriveProgram.Name = WarpDriveProgramType.Name;
                    InWarpDriveProgram.Desc = WarpDriveProgramType.Description;
                    InWarpDriveProgram.MaxLevelCharges = WarpDriveProgramType.MaxLevelCharges;
                    InWarpDriveProgram.m_IconTexture = WarpDriveProgramType.IconTexture;
                    InWarpDriveProgram.ShortName = WarpDriveProgramType.ShortName;
                    InWarpDriveProgram.ShieldBooster_BoostAmount = 0f;
                    InWarpDriveProgram.m_MarketPrice = WarpDriveProgramType.MarketPrice;
                    InWarpDriveProgram.CargoVisualPrefabID = WarpDriveProgramType.CargoVisualID;
                    InWarpDriveProgram.CanBeDroppedOnShipDeath = WarpDriveProgramType.CanBeDroppedOnShipDeath;
                    InWarpDriveProgram.Experimental = WarpDriveProgramType.Experimental;
                    InWarpDriveProgram.Unstable = WarpDriveProgramType.Unstable;
                    InWarpDriveProgram.Contraband = WarpDriveProgramType.Contraband;
                    InWarpDriveProgram.Price_LevelMultiplierExponent = WarpDriveProgramType.Price_LevelMultiplierExponent;
                    if (PhotonNetwork.isMasterClient)
                    {
                        InWarpDriveProgram.Level = InWarpDriveProgram.MaxLevelCharges;
                    }
                }
            }
            else
            {
                InWarpDriveProgram = new PLWarpDriveProgram((EWarpDriveProgramType)Subtype, level);
            }
            return InWarpDriveProgram;
        }

        //Converts hashes to WarpDrivePrograms.
        [HarmonyPatch(typeof(PLWarpDriveProgram), "CreateWarpDriveProgramFromHash")]
        class WarpDriveProgramHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                __result = WarpDriveProgramModManager.CreateWarpDriveProgram(inSubType, inLevel);
                return false;
            }
        }
        [HarmonyPatch(typeof(PLWarpDriveProgram), "FinalLateAddStats")]
        class WarpDriveProgramFinalLateAddStatsPatch
        {
            static void Postfix(PLWarpDriveProgram __instance)
            {
                int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.types.Count && Time.time - __instance.ShieldBooster_LastActivationTime < WarpDriveProgramModManager.Instance.types[subtypeformodded].ActiveTime)
                {
                    WarpDriveProgramModManager.Instance.types[subtypeformodded].FinalLateAddStats(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLWarpDriveProgram), "ExecuteBasedOnType")]
        class WarpDriveProgramExecuteBasedOnTypePatch
        {
            static void Prefix(PLWarpDriveProgram __instance)
            {
                int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.types.Count)
                {
                    if (WarpDriveProgramModManager.Instance.types[subtypeformodded].IsVirus)
                    {
                        PLServer.Instance.photonView.RPC("AddToSendQueue", PhotonTargets.All, new object[] {
                        __instance.ShipStats.Ship.ShipID,
                        __instance.ShipStats.Ship.VirusSendQueueCounter + 1,
                        WarpDriveProgramModManager.Instance.types[subtypeformodded].VirusSubtype,
                        PLServer.Instance.GetEstimatedServerMs()
                    });
                        PulsarModLoader.Utilities.Messaging.Notification($"{WarpDriveProgramModManager.Instance.types[subtypeformodded].VirusSubtype}");
                    }
                    else
                    {
                        __instance.ShieldBooster_LastActivationTime = Time.time;
                        WarpDriveProgramModManager.Instance.types[subtypeformodded].Execute(__instance);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(PLServer), "AddToSendQueue")]
        class WarpDriveProgramAddToSendQueuePatch
        {
            static bool Prefix(int shipID, int sendQueueID, int virusType, int serverTime)
            {
                Debug.Log("AddToSendQueue: shipID-" + shipID.ToString() + "   sendQueueID-" + sendQueueID.ToString());
                PLServer.Instance.StartCoroutine(LateAddToSendQueueReplacement(shipID, sendQueueID, virusType, serverTime));
                return false;
            }
            private static IEnumerator LateAddToSendQueueReplacement(int shipID, int sendQueueID, int virusType, int serverTime)
            {
                PLShipInfoBase ship = null;
                while (ship == null)
                {
                    ship = PLEncounterManager.Instance.GetShipFromID(shipID);
                    if (ship == null)
                    {
                        yield return new WaitForSeconds(0.05f);
                    }
                }
                if (!ship.VirusSendQueue.ForwardDictionary.ContainsKey(sendQueueID))
                {
                    PLVirus plvirus = Virus.VirusModManager.CreateVirus(virusType, 0);
                    plvirus.NetID = -1;
                    plvirus.InitialTime = serverTime;
                    ship.VirusSendQueue.Add(sendQueueID, plvirus);
                    plvirus.Sender = ship;
                    Debug.Log("adding virus from send queue: id-" + sendQueueID.ToString() + "   name-" + plvirus.Name);
                }
                yield break;
            }
        }
        [HarmonyPatch(typeof(PLWarpDriveProgram), "GetActiveTimerAlpha")]
        class WarpDriveProgramGetActiveTimerAlphaPatch
        {
            static void Postfix(PLWarpDriveProgram __instance, ref float __result)
            {
                int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.types.Count)
                {
                    __result = Mathf.Clamp01((Time.time - __instance.ShieldBooster_LastActivationTime) / WarpDriveProgramModManager.Instance.types[subtypeformodded].ActiveTime);
                }
            }
        }
    }
}

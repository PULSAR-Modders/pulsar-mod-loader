using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Logger = PulsarModLoader.Utilities.Logger;

namespace PulsarModLoader.Content.Components.WarpDriveProgram
{
    public class WarpDriveProgramModManager
    {
        public readonly int VanillaWarpDriveProgramMaxType = 0;
        private static WarpDriveProgramModManager m_instance = null;
        public readonly List<WarpDriveProgramMod> WarpDriveProgramTypes = new List<WarpDriveProgramMod>();
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

        WarpDriveProgramModManager()
        {
            VanillaWarpDriveProgramMaxType = Enum.GetValues(typeof(EWarpDriveProgramType)).Length;
            Logger.Info($"MaxTypeint = {VanillaWarpDriveProgramMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type WarpDriveProgramMod = typeof(WarpDriveProgramMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (WarpDriveProgramMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading WarpDriveProgram from assembly");
                        WarpDriveProgramMod WarpDriveProgramModHandler = (WarpDriveProgramMod)Activator.CreateInstance(t);
                        if (GetWarpDriveProgramIDFromName(WarpDriveProgramModHandler.Name) == -1)
                        {
                            WarpDriveProgramTypes.Add(WarpDriveProgramModHandler);
                            Logger.Info($"Added WarpDriveProgram: '{WarpDriveProgramModHandler.Name}' with ID '{GetWarpDriveProgramIDFromName(WarpDriveProgramModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add WarpDriveProgram from {mod.Name} with the duplicate name of '{WarpDriveProgramModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds WarpDriveProgram type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find WarpDriveProgram.
        /// </summary>
        /// <param name="WarpDriveProgramName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetWarpDriveProgramIDFromName(string WarpDriveProgramName)
        {
            for (int i = 0; i < WarpDriveProgramTypes.Count; i++)
            {
                if (WarpDriveProgramTypes[i].Name == WarpDriveProgramName)
                {
                    return i + VanillaWarpDriveProgramMaxType;
                }
            }
            return -1;
        }
        public static PLWarpDriveProgram CreateWarpDriveProgram(int Subtype, int level)
        {
            PLWarpDriveProgram InWarpDriveProgram;
            if (Subtype >= Instance.VanillaWarpDriveProgramMaxType)
            {
                InWarpDriveProgram = new PLWarpDriveProgram(EWarpDriveProgramType.SHIELD_BOOSTER, level);
                int subtypeformodded = Subtype - Instance.VanillaWarpDriveProgramMaxType;
                if (subtypeformodded <= Instance.WarpDriveProgramTypes.Count && subtypeformodded > -1)
                {
                    WarpDriveProgramMod WarpDriveProgramType = Instance.WarpDriveProgramTypes[Subtype - Instance.VanillaWarpDriveProgramMaxType];
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
            int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaWarpDriveProgramMaxType;
            if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.WarpDriveProgramTypes.Count && Time.time - __instance.ShieldBooster_LastActivationTime < WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].ActiveTime)
            {
                WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].FinalLateAddStats(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLWarpDriveProgram), "ExecuteBasedOnType")]
    class WarpDriveProgramExecuteBasedOnTypePatch
    {
        static void Prefix(PLWarpDriveProgram __instance)
        {
            int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaWarpDriveProgramMaxType;
            if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.WarpDriveProgramTypes.Count)
            {
                if (WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].IsVirus) 
                {
                    PLServer.Instance.photonView.RPC("AddToSendQueue", PhotonTargets.All, new object[] {
                        __instance.ShipStats.Ship.ShipID,
                        __instance.ShipStats.Ship.VirusSendQueueCounter + 1,
                        WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].VirusSubtype,
                        PLServer.Instance.GetEstimatedServerMs()
                    });
                    PulsarModLoader.Utilities.Messaging.Notification($"{WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].VirusSubtype}");
                }
                else
                {
                    __instance.ShieldBooster_LastActivationTime = Time.time;
                    WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].Execute(__instance);
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
            int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaWarpDriveProgramMaxType;
            if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.WarpDriveProgramTypes.Count)
            {
                __result = Mathf.Clamp01((Time.time - __instance.ShieldBooster_LastActivationTime) / WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].ActiveTime);
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "LateAddStats")]
    class WarpDriveProgramLateAddStatsPatch
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance)
        {
            int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaWarpDriveProgramMaxType;
            if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.WarpDriveProgramTypes.Count && inStats != null)
            {
                WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].LateAddStats(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "AddStats")]
    class WarpDriveProgramAddStats
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance) 
        {
            if(__instance is PLWarpDriveProgram) 
            {
                int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaWarpDriveProgramMaxType;
                if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.WarpDriveProgramTypes.Count && inStats != null)
                {
                    WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].AddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLWarpDriveProgram), "Tick")]
    class WarpDriveProgramTick
    {
        static void Postfix(PLWarpDriveProgram __instance)
        {
            int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaWarpDriveProgramMaxType;
            if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.WarpDriveProgramTypes.Count)
            {
                 WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].Tick(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "GetStatLineLeft")]
    class WarpDriveProgramGetStatLineLeft
    {
        static void Postfix(ref string __result, PLShipComponent __instance)
        {
            if (__instance is PLWarpDriveProgram)
            {
                int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaWarpDriveProgramMaxType;
                if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.WarpDriveProgramTypes.Count)
                {
                    __result = WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].GetStatLineLeft(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "GetStatLineRight")]
    class WarpDriveProgramGetStatLineRight
    {
        static void Postfix(ref string __result, PLShipComponent __instance)
        {
            if (__instance is PLWarpDriveProgram)
            {
                int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaWarpDriveProgramMaxType;
                if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.WarpDriveProgramTypes.Count)
                {
                    __result = WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].GetStatLineRight(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "OnWarp")]
    class WarpDriveProgramOnWarp
    {
        static void Postfix(PLShipComponent __instance)
        {
            if (__instance is PLWarpDriveProgram)
            {
                int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaWarpDriveProgramMaxType;
                if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.WarpDriveProgramTypes.Count)
                {
                    WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].OnWarp(__instance);
                }
            }
        }
    }
}

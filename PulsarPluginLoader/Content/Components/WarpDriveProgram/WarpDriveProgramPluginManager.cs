using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Logger = PulsarPluginLoader.Utilities.Logger;

namespace PulsarPluginLoader.Content.Components.WarpDriveProgram
{
    public class WarpDriveProgramPluginManager
    {
        public readonly int VanillaWarpDriveProgramMaxType = 0;
        private static WarpDriveProgramPluginManager m_instance = null;
        public readonly List<WarpDriveProgramPlugin> WarpDriveProgramTypes = new List<WarpDriveProgramPlugin>();
        public static WarpDriveProgramPluginManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new WarpDriveProgramPluginManager();
                }
                return m_instance;
            }
        }

        WarpDriveProgramPluginManager()
        {
            VanillaWarpDriveProgramMaxType = Enum.GetValues(typeof(EWarpDriveProgramType)).Length;
            Logger.Info($"MaxTypeint = {VanillaWarpDriveProgramMaxType - 1}");
            foreach (PulsarPlugin plugin in PluginManager.Instance.GetAllPlugins())
            {
                Assembly asm = plugin.GetType().Assembly;
                Type WarpDriveProgramPlugin = typeof(WarpDriveProgramPlugin);
                foreach (Type t in asm.GetTypes())
                {
                    if (WarpDriveProgramPlugin.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading WarpDriveProgram from assembly");
                        WarpDriveProgramPlugin WarpDriveProgramPluginHandler = (WarpDriveProgramPlugin)Activator.CreateInstance(t);
                        if (GetWarpDriveProgramIDFromName(WarpDriveProgramPluginHandler.Name) == -1)
                        {
                            WarpDriveProgramTypes.Add(WarpDriveProgramPluginHandler);
                            Logger.Info($"Added WarpDriveProgram: '{WarpDriveProgramPluginHandler.Name}' with ID '{GetWarpDriveProgramIDFromName(WarpDriveProgramPluginHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add WarpDriveProgram from {plugin.Name} with the duplicate name of '{WarpDriveProgramPluginHandler.Name}'");
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
                /*if (Global.DebugLogging)
                {
                    Logger.Info($"Subtype for modded is {subtypeformodded}");
                }*/
                if (subtypeformodded <= Instance.WarpDriveProgramTypes.Count && subtypeformodded > -1)
                {
                    /*if (Global.DebugLogging)
                    {
                        Logger.Info("Creating WarpDriveProgram from list info");
                    }*/
                    WarpDriveProgramPlugin WarpDriveProgramType = Instance.WarpDriveProgramTypes[Subtype - Instance.VanillaWarpDriveProgramMaxType];
                    InWarpDriveProgram.SubType = Subtype;
                    InWarpDriveProgram.Name = WarpDriveProgramType.Name;
                    InWarpDriveProgram.Desc = WarpDriveProgramType.Description;
                    InWarpDriveProgram.MaxLevelCharges = WarpDriveProgramType.MaxLevelCharges;
                    InWarpDriveProgram.GetType().GetField("m_IconTexture", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InWarpDriveProgram, WarpDriveProgramType.IconTexture);
                    InWarpDriveProgram.ShortName = WarpDriveProgramType.ShortName;
                    InWarpDriveProgram.GetType().GetField("ShieldBooster_BoostAmount", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InWarpDriveProgram, 0f);
                    InWarpDriveProgram.GetType().GetField("m_MarketPrice", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InWarpDriveProgram, (ObscuredInt)WarpDriveProgramType.MarketPrice);
                    InWarpDriveProgram.CargoVisualPrefabID = WarpDriveProgramType.CargoVisualID;
                    InWarpDriveProgram.CanBeDroppedOnShipDeath = WarpDriveProgramType.CanBeDroppedOnShipDeath;
                    InWarpDriveProgram.Experimental = WarpDriveProgramType.Experimental;
                    InWarpDriveProgram.Unstable = WarpDriveProgramType.Unstable;
                    InWarpDriveProgram.Contraband = WarpDriveProgramType.Contraband;
                    InWarpDriveProgram.GetType().GetField("Price_LevelMultiplierExponent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InWarpDriveProgram, (ObscuredFloat)WarpDriveProgramType.Price_LevelMultiplierExponent);
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
            __result = WarpDriveProgramPluginManager.CreateWarpDriveProgram(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLWarpDriveProgram), "FinalLateAddStats")]
    class WarpDriveProgramFinalLateAddStatsPatch
    {
        static void Postfix(PLWarpDriveProgram __instance)
        {
            int subtypeformodded = __instance.SubType - WarpDriveProgramPluginManager.Instance.VanillaWarpDriveProgramMaxType;
            if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramPluginManager.Instance.WarpDriveProgramTypes.Count && Time.time - (float)__instance.GetType().GetField("ShieldBooster_LastActivationTime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) < WarpDriveProgramPluginManager.Instance.WarpDriveProgramTypes[subtypeformodded].ActiveTime)
            {
                WarpDriveProgramPluginManager.Instance.WarpDriveProgramTypes[subtypeformodded].FinalLateAddStats(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLWarpDriveProgram), "ExecuteBasedOnType")]
    class WarpDriveProgramExecuteBasedOnTypePatch
    {
        static void Prefix(PLWarpDriveProgram __instance)
        {
            int subtypeformodded = __instance.SubType - WarpDriveProgramPluginManager.Instance.VanillaWarpDriveProgramMaxType;
            if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramPluginManager.Instance.WarpDriveProgramTypes.Count)
            {
                if (WarpDriveProgramPluginManager.Instance.WarpDriveProgramTypes[subtypeformodded].IsVirus) 
                {
                    PLServer.Instance.photonView.RPC("AddToSendQueue", PhotonTargets.All, new object[] {
                        __instance.ShipStats.Ship.ShipID,
                        __instance.ShipStats.Ship.VirusSendQueueCounter + 1,
                        WarpDriveProgramPluginManager.Instance.WarpDriveProgramTypes[subtypeformodded].VirusSubtype,
                        PLServer.Instance.GetEstimatedServerMs()
                    });
                    PulsarPluginLoader.Utilities.Messaging.Notification($"{WarpDriveProgramPluginManager.Instance.WarpDriveProgramTypes[subtypeformodded].VirusSubtype}");
                }
                else
                {
                    __instance.GetType().GetField("ShieldBooster_LastActivationTime", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, Time.time);
                    WarpDriveProgramPluginManager.Instance.WarpDriveProgramTypes[subtypeformodded].Execute(__instance);
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
                PLVirus plvirus = Virus.VirusPluginManager.CreateVirus(virusType, 0);
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
            int subtypeformodded = __instance.SubType - WarpDriveProgramPluginManager.Instance.VanillaWarpDriveProgramMaxType;
            if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramPluginManager.Instance.WarpDriveProgramTypes.Count)
            {
                __result = Mathf.Clamp01((Time.time - (float)__instance.GetType().GetField("ShieldBooster_LastActivationTime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance)) / WarpDriveProgramPluginManager.Instance.WarpDriveProgramTypes[subtypeformodded].ActiveTime);
            }
        }
    }
}

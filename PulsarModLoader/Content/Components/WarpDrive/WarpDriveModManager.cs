using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.WarpDrive
{
    public class WarpDriveModManager
    {
        public readonly int VanillaWarpDriveMaxType = 0;
        private static WarpDriveModManager m_instance = null;
        public readonly List<WarpDriveMod> WarpDriveTypes = new List<WarpDriveMod>();
        public static WarpDriveModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new WarpDriveModManager();
                }
                return m_instance;
            }
        }

        WarpDriveModManager()
        {
            VanillaWarpDriveMaxType = Enum.GetValues(typeof(EWarpDriveType)).Length;
            Logger.Info($"MaxTypeint = {VanillaWarpDriveMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type WarpDriveMod = typeof(WarpDriveMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (WarpDriveMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading WarpDrive from assembly");
                        WarpDriveMod WarpDriveModHandler = (WarpDriveMod)Activator.CreateInstance(t);
                        if (GetWarpDriveIDFromName(WarpDriveModHandler.Name) == -1)
                        {
                            WarpDriveTypes.Add(WarpDriveModHandler);
                            Logger.Info($"Added WarpDrive: '{WarpDriveModHandler.Name}' with ID '{GetWarpDriveIDFromName(WarpDriveModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add WarpDrive from {mod.Name} with the duplicate name of '{WarpDriveModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds WarpDrive type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find WarpDrive.
        /// </summary>
        /// <param name="WarpDriveName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetWarpDriveIDFromName(string WarpDriveName)
        {
            for (int i = 0; i < WarpDriveTypes.Count; i++)
            {
                if (WarpDriveTypes[i].Name == WarpDriveName)
                {
                    return i + VanillaWarpDriveMaxType;
                }
            }
            return -1;
        }
        public static PLWarpDrive CreateWarpDrive(int Subtype, int level, short SubTypeData)
        {
            PLWarpDrive InWarpDrive;
            if (Subtype >= Instance.VanillaWarpDriveMaxType)
            {
                InWarpDrive = new PLWarpDrive(EWarpDriveType.E_MAX, level, SubTypeData);
                int subtypeformodded = Subtype - Instance.VanillaWarpDriveMaxType;
                if (subtypeformodded <= Instance.WarpDriveTypes.Count && subtypeformodded > -1)
                {
                    WarpDriveMod WarpDriveType = Instance.WarpDriveTypes[Subtype - Instance.VanillaWarpDriveMaxType];
                    InWarpDrive.SubType = Subtype;
                    InWarpDrive.Name = WarpDriveType.Name;
                    InWarpDrive.Desc = WarpDriveType.Description;
                    InWarpDrive.m_IconTexture = WarpDriveType.IconTexture;
                    InWarpDrive.ChargeSpeed = WarpDriveType.ChargeSpeed;
                    InWarpDrive.WarpRange = WarpDriveType.WarpRange;
                    InWarpDrive.EnergySignatureAmt = WarpDriveType.EnergySignature;
                    InWarpDrive.NumberOfChargingNodes = WarpDriveType.NumberOfChargesPerFuel;
                    InWarpDrive.m_MaxPowerUsage_Watts = WarpDriveType.MaxPowerUsage_Watts;
                    InWarpDrive.m_MarketPrice = WarpDriveType.MarketPrice;
                    InWarpDrive.CargoVisualPrefabID = WarpDriveType.CargoVisualID;
                    InWarpDrive.CanBeDroppedOnShipDeath = WarpDriveType.CanBeDroppedOnShipDeath;
                    InWarpDrive.Experimental = WarpDriveType.Experimental;
                    InWarpDrive.Unstable = WarpDriveType.Unstable;
                    InWarpDrive.Contraband = WarpDriveType.Contraband;
                    InWarpDrive.Price_LevelMultiplierExponent = WarpDriveType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InWarpDrive = new PLWarpDrive((EWarpDriveType)Subtype, level,SubTypeData);
            }
            return InWarpDrive;
        }
    }
    //Converts hashes to WarpDrives.
    [HarmonyPatch(typeof(PLWarpDrive), "CreateWarpDriveFromHash")]
    class WarpDriveHashFix
    {
        static bool Prefix(int inSubType, int inLevel, short inSubTypeData, ref PLShipComponent __result)
        {
            __result = WarpDriveModManager.CreateWarpDrive(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLWarpDrive), "GetStatLineLeft")]
    class LeftDescFix
    {
        static void Postfix(PLWarpDrive __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - WarpDriveModManager.Instance.VanillaWarpDriveMaxType;
            if (subtypeformodded > -1 && subtypeformodded < WarpDriveModManager.Instance.WarpDriveTypes.Count && __instance.ShipStats != null)
            {
                __result = WarpDriveModManager.Instance.WarpDriveTypes[subtypeformodded].GetStatLineLeft(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLWarpDrive), "GetStatLineRight")]
    class RightDescFix
    {
        static void Postfix(PLWarpDrive __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - WarpDriveModManager.Instance.VanillaWarpDriveMaxType;
            if (subtypeformodded > -1 && subtypeformodded < WarpDriveModManager.Instance.WarpDriveTypes.Count && __instance.ShipStats != null)
            {
                __result = WarpDriveModManager.Instance.WarpDriveTypes[subtypeformodded].GetStatLineRight(__instance);
            }
        }
    }
}

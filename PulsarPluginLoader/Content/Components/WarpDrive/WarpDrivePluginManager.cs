using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Logger = PulsarPluginLoader.Utilities.Logger;

namespace PulsarPluginLoader.Content.Components.WarpDrive
{
    public class WarpDrivePluginManager
    {
        public readonly int VanillaWarpDriveMaxType = 0;
        private static WarpDrivePluginManager m_instance = null;
        public readonly List<WarpDrivePlugin> WarpDriveTypes = new List<WarpDrivePlugin>();
        public static WarpDrivePluginManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new WarpDrivePluginManager();
                }
                return m_instance;
            }
        }

        WarpDrivePluginManager()
        {
            VanillaWarpDriveMaxType = Enum.GetValues(typeof(EWarpDriveType)).Length;
            Logger.Info($"MaxTypeint = {VanillaWarpDriveMaxType - 1}");
            foreach (PulsarPlugin plugin in PluginManager.Instance.GetAllPlugins())
            {
                Assembly asm = plugin.GetType().Assembly;
                Type WarpDrivePlugin = typeof(WarpDrivePlugin);
                foreach (Type t in asm.GetTypes())
                {
                    if (WarpDrivePlugin.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading WarpDrive from assembly");
                        WarpDrivePlugin WarpDrivePluginHandler = (WarpDrivePlugin)Activator.CreateInstance(t);
                        if (GetWarpDriveIDFromName(WarpDrivePluginHandler.Name) == -1)
                        {
                            WarpDriveTypes.Add(WarpDrivePluginHandler);
                            Logger.Info($"Added WarpDrive: '{WarpDrivePluginHandler.Name}' with ID '{GetWarpDriveIDFromName(WarpDrivePluginHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add WarpDrive from {plugin.Name} with the duplicate name of '{WarpDrivePluginHandler.Name}'");
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
        public static PLWarpDrive CreateWarpDrive(int Subtype, int level)
        {
            PLWarpDrive InWarpDrive;
            if (Subtype >= Instance.VanillaWarpDriveMaxType)
            {
                InWarpDrive = new PLWarpDrive(EWarpDriveType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaWarpDriveMaxType;
                /*if (Global.DebugLogging)
                {
                    Logger.Info($"Subtype for modded is {subtypeformodded}");
                }*/
                if (subtypeformodded <= Instance.WarpDriveTypes.Count && subtypeformodded > -1)
                {
                    /*if (Global.DebugLogging)
                    {
                        Logger.Info("Creating WarpDrive from list info");
                    }*/
                    WarpDrivePlugin WarpDriveType = Instance.WarpDriveTypes[Subtype - Instance.VanillaWarpDriveMaxType];
                    InWarpDrive.SubType = Subtype;
                    InWarpDrive.Name = WarpDriveType.Name;
                    InWarpDrive.Desc = WarpDriveType.Description;
                    InWarpDrive.GetType().GetField("m_IconTexture", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InWarpDrive, WarpDriveType.IconTexture);
                    InWarpDrive.ChargeSpeed = WarpDriveType.ChargeSpeed;
                    InWarpDrive.WarpRange = WarpDriveType.WarpRange;
                    InWarpDrive.EnergySignatureAmt = WarpDriveType.EnergySignature;
                    InWarpDrive.NumberOfChargingNodes = WarpDriveType.NumberOfChargesPerFuel;
                    InWarpDrive.GetType().GetField("m_MarketPrice", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InWarpDrive, (ObscuredInt)WarpDriveType.MarketPrice);
                    InWarpDrive.CargoVisualPrefabID = WarpDriveType.CargoVisualID;
                    InWarpDrive.CanBeDroppedOnShipDeath = WarpDriveType.CanBeDroppedOnShipDeath;
                    InWarpDrive.Experimental = WarpDriveType.Experimental;
                    InWarpDrive.Unstable = WarpDriveType.Unstable;
                    InWarpDrive.Contraband = WarpDriveType.Contraband;
                    InWarpDrive.GetType().GetField("Price_LevelMultiplierExponent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InWarpDrive, (ObscuredFloat)WarpDriveType.Price_LevelMultiplierExponent);
                }
            }
            else
            {
                InWarpDrive = new PLWarpDrive((EWarpDriveType)Subtype, level);
            }
            return InWarpDrive;
        }
    }
    //Converts hashes to WarpDrives.
    [HarmonyPatch(typeof(PLWarpDrive), "CreateWarpDriveFromHash")]
    class WarpDriveHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = WarpDrivePluginManager.CreateWarpDrive(inSubType, inLevel);
            return false;
        }
    }
}

using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarPluginLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using Logger = PulsarPluginLoader.Utilities.Logger;

namespace PulsarPluginLoader.Content.Components.NuclearDevice
{
    public class NuclearDevicePluginManager
    {
        public readonly int VanillaNuclearDeviceMaxType = 0;
        private static NuclearDevicePluginManager m_instance = null;
        public readonly List<NuclearDevicePlugin> NuclearDeviceTypes = new List<NuclearDevicePlugin>();
        public static NuclearDevicePluginManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new NuclearDevicePluginManager();
                }
                return m_instance;
            }
        }

        NuclearDevicePluginManager()
        {
            VanillaNuclearDeviceMaxType = Enum.GetValues(typeof(ENuclearDeviceType)).Length;
            Logger.Info($"MaxTypeint = {VanillaNuclearDeviceMaxType - 1}");
            foreach (PulsarPlugin plugin in PluginManager.Instance.GetAllPlugins())
            {
                Assembly asm = plugin.GetType().Assembly;
                Type NuclearDevicePlugin = typeof(NuclearDevicePlugin);
                foreach (Type t in asm.GetTypes())
                {
                    if (NuclearDevicePlugin.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading NuclearDevice from assembly");
                        NuclearDevicePlugin NuclearDevicePluginHandler = (NuclearDevicePlugin)Activator.CreateInstance(t);
                        if (GetNuclearDeviceIDFromName(NuclearDevicePluginHandler.Name) == -1)
                        {
                            NuclearDeviceTypes.Add(NuclearDevicePluginHandler);
                            Logger.Info($"Added NuclearDevice: '{NuclearDevicePluginHandler.Name}' with ID '{GetNuclearDeviceIDFromName(NuclearDevicePluginHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add NuclearDevice from {plugin.Name} with the duplicate name of '{NuclearDevicePluginHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds NuclearDevice type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find NuclearDevice.
        /// </summary>
        /// <param name="NuclearDeviceName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetNuclearDeviceIDFromName(string NuclearDeviceName)
        {
            for (int i = 0; i < NuclearDeviceTypes.Count; i++)
            {
                if (NuclearDeviceTypes[i].Name == NuclearDeviceName)
                {
                    return i + VanillaNuclearDeviceMaxType;
                }
            }
            return -1;
        }
        public static PLNuclearDevice CreateNuclearDevice(int Subtype, int level)
        {
            PLNuclearDevice InNuclearDevice;
            if (Subtype >= Instance.VanillaNuclearDeviceMaxType)
            {
                InNuclearDevice = new PLNuclearDevice(ENuclearDeviceType.MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaNuclearDeviceMaxType;
                /*if (Global.DebugLogging)
                {
                    Logger.Info($"Subtype for modded is {subtypeformodded}");
                }*/
                if (subtypeformodded <= Instance.NuclearDeviceTypes.Count && subtypeformodded > -1)
                {
                    /*if (Global.DebugLogging)
                    {
                        Logger.Info("Creating NuclearDevice from list info");
                    }*/
                    NuclearDevicePlugin NuclearDeviceType = Instance.NuclearDeviceTypes[Subtype - Instance.VanillaNuclearDeviceMaxType];
                    InNuclearDevice.SubType = Subtype;
                    InNuclearDevice.Name = NuclearDeviceType.Name;
                    InNuclearDevice.Desc = NuclearDeviceType.Description;
                    InNuclearDevice.GetType().GetField("m_IconTexture", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InNuclearDevice, NuclearDeviceType.IconTexture);
                    InNuclearDevice.GetType().GetField("m_MaxDamage", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InNuclearDevice, NuclearDeviceType.MaxDamage);
                    InNuclearDevice.GetType().GetField("m_Range", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InNuclearDevice, NuclearDeviceType.Range);
                    InNuclearDevice.GetType().GetField("m_FuelBurnRate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InNuclearDevice, NuclearDeviceType.FuelBurnRate);
                    InNuclearDevice.GetType().GetField("m_TurnRate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InNuclearDevice, NuclearDeviceType.TurnRate);
                    InNuclearDevice.GetType().GetField("m_IntimidationBonus", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InNuclearDevice, NuclearDeviceType.IntimidationBonus);
                    InNuclearDevice.GetType().GetField("m_TurnRate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InNuclearDevice, NuclearDeviceType.TurnRate);
                    InNuclearDevice.GetType().GetField("m_Health", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InNuclearDevice, NuclearDeviceType.Health);
                    InNuclearDevice.GetType().GetField("m_MarketPrice", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InNuclearDevice, (ObscuredInt)NuclearDeviceType.MarketPrice);
                    InNuclearDevice.CargoVisualPrefabID = NuclearDeviceType.CargoVisualID;
                    InNuclearDevice.CanBeDroppedOnShipDeath = NuclearDeviceType.CanBeDroppedOnShipDeath;
                    InNuclearDevice.Experimental = NuclearDeviceType.Experimental;
                    InNuclearDevice.Unstable = NuclearDeviceType.Unstable;
                    InNuclearDevice.Contraband = NuclearDeviceType.Contraband;
                    InNuclearDevice.GetType().GetField("Price_LevelMultiplierExponent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InNuclearDevice, (ObscuredFloat)NuclearDeviceType.Price_LevelMultiplierExponent);
                }
            }
            else
            {
                InNuclearDevice = new PLNuclearDevice((ENuclearDeviceType)Subtype, level);
            }
            return InNuclearDevice;
        }
    }
    //Converts hashes to NuclearDevices.
    [HarmonyPatch(typeof(PLNuclearDevice), "CreateNuclearDeviceFromHash")]
    class NuclearDeviceHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = NuclearDevicePluginManager.CreateNuclearDevice(inSubType, inLevel);
            return false;
        }
    }
}

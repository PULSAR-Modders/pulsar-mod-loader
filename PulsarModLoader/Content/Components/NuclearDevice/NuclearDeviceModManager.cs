using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.NuclearDevice
{
    public class NuclearDeviceModManager
    {
        public readonly int VanillaNuclearDeviceMaxType = 0;
        private static NuclearDeviceModManager m_instance = null;
        public readonly List<NuclearDeviceMod> NuclearDeviceTypes = new List<NuclearDeviceMod>();
        public static NuclearDeviceModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new NuclearDeviceModManager();
                }
                return m_instance;
            }
        }

        NuclearDeviceModManager()
        {
            VanillaNuclearDeviceMaxType = Enum.GetValues(typeof(ENuclearDeviceType)).Length;
            Logger.Info($"MaxTypeint = {VanillaNuclearDeviceMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type NuclearDeviceMod = typeof(NuclearDeviceMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (NuclearDeviceMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading NuclearDevice from assembly");
                        NuclearDeviceMod NuclearDeviceModHandler = (NuclearDeviceMod)Activator.CreateInstance(t);
                        if (GetNuclearDeviceIDFromName(NuclearDeviceModHandler.Name) == -1)
                        {
                            NuclearDeviceTypes.Add(NuclearDeviceModHandler);
                            Logger.Info($"Added NuclearDevice: '{NuclearDeviceModHandler.Name}' with ID '{GetNuclearDeviceIDFromName(NuclearDeviceModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add NuclearDevice from {mod.Name} with the duplicate name of '{NuclearDeviceModHandler.Name}'");
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
                if (subtypeformodded <= Instance.NuclearDeviceTypes.Count && subtypeformodded > -1)
                {
                    NuclearDeviceMod NuclearDeviceType = Instance.NuclearDeviceTypes[Subtype - Instance.VanillaNuclearDeviceMaxType];
                    InNuclearDevice.SubType = Subtype;
                    InNuclearDevice.Name = NuclearDeviceType.Name;
                    InNuclearDevice.Desc = NuclearDeviceType.Description;
                    InNuclearDevice.m_IconTexture = NuclearDeviceType.IconTexture;
                    InNuclearDevice.m_MaxDamage = NuclearDeviceType.MaxDamage;
                    InNuclearDevice.m_Range = NuclearDeviceType.Range;
                    InNuclearDevice.m_FuelBurnRate = NuclearDeviceType.FuelBurnRate;
                    InNuclearDevice.m_TurnRate = NuclearDeviceType.TurnRate;
                    InNuclearDevice.m_IntimidationBonus = NuclearDeviceType.IntimidationBonus;
                    InNuclearDevice.m_TurnRate = NuclearDeviceType.TurnRate;
                    InNuclearDevice.m_Health = NuclearDeviceType.Health;
                    InNuclearDevice.m_MarketPrice = NuclearDeviceType.MarketPrice;
                    InNuclearDevice.CargoVisualPrefabID = NuclearDeviceType.CargoVisualID;
                    InNuclearDevice.CanBeDroppedOnShipDeath = NuclearDeviceType.CanBeDroppedOnShipDeath;
                    InNuclearDevice.Experimental = NuclearDeviceType.Experimental;
                    InNuclearDevice.Unstable = NuclearDeviceType.Unstable;
                    InNuclearDevice.Contraband = NuclearDeviceType.Contraband;
                    InNuclearDevice.Price_LevelMultiplierExponent = NuclearDeviceType.Price_LevelMultiplierExponent;
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
            __result = NuclearDeviceModManager.CreateNuclearDevice(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLNuclearDevice), "GetStatLineLeft")]
    class LeftDescFix
    {
        static void Postfix(PLNuclearDevice __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - NuclearDeviceModManager.Instance.VanillaNuclearDeviceMaxType;
            if (subtypeformodded > -1 && subtypeformodded < NuclearDeviceModManager.Instance.NuclearDeviceTypes.Count && __instance.ShipStats != null)
            {
                __result = NuclearDeviceModManager.Instance.NuclearDeviceTypes[subtypeformodded].GetStatLineLeft(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLNuclearDevice), "GetStatLineRight")]
    class RightDescFix
    {
        static void Postfix(PLNuclearDevice __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - NuclearDeviceModManager.Instance.VanillaNuclearDeviceMaxType;
            if (subtypeformodded > -1 && subtypeformodded < NuclearDeviceModManager.Instance.NuclearDeviceTypes.Count && __instance.ShipStats != null)
            {
                __result = NuclearDeviceModManager.Instance.NuclearDeviceTypes[subtypeformodded].GetStatLineRight(__instance);
            }
        }
    }
}

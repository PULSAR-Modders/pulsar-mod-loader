using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarPluginLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Logger = PulsarPluginLoader.Utilities.Logger;

namespace PulsarPluginLoader.ContentHelpers.Components.Shield
{
    public class ShieldPluginManager
    {
        public readonly int VanillaShieldMaxType = 0;
        private static ShieldPluginManager m_instance = null;
        public readonly List<ShieldPlugin> ShieldTypes = new List<ShieldPlugin>();
        public static ShieldPluginManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ShieldPluginManager();
                }
                return m_instance;
            }
        }

        ShieldPluginManager()
        {
            VanillaShieldMaxType = Enum.GetValues(typeof(EShieldGeneratorType)).Length;
            Logger.Info($"MaxTypeint = {VanillaShieldMaxType - 1}");
            foreach (PulsarPlugin plugin in PluginManager.Instance.GetAllPlugins())
            {
                Assembly asm = plugin.GetType().Assembly;
                Type ShieldPlugin = typeof(ShieldPlugin);
                foreach (Type t in asm.GetTypes())
                {
                    if (ShieldPlugin.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading Shield from assembly");
                        ShieldPlugin ShieldPluginHandler = (ShieldPlugin)Activator.CreateInstance(t);
                        if (GetShieldIDFromName(ShieldPluginHandler.Name) == -1)
                        {
                            ShieldTypes.Add(ShieldPluginHandler);
                            Logger.Info($"Added Shield: '{ShieldPluginHandler.Name}' with ID '{GetShieldIDFromName(ShieldPluginHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add Shield from {plugin.Name} with the duplicate name of '{ShieldPluginHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds Shield type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Shield.
        /// </summary>
        /// <param name="ShieldName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetShieldIDFromName(string ShieldName)
        {
            for (int i = 0; i < ShieldTypes.Count; i++)
            {
                if (ShieldTypes[i].Name == ShieldName)
                {
                    return i + VanillaShieldMaxType;
                }
            }
            return -1;
        }
        public static PLShieldGenerator CreateShield(int Subtype, int level)
        {
            PLShieldGenerator InShield;
            if (Subtype >= Instance.VanillaShieldMaxType)
            {
                InShield = new PLShieldGenerator(EShieldGeneratorType.E_SG_ID_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaShieldMaxType;
                /*if (Global.DebugLogging)
                {
                    Logger.Info($"Subtype for modded is {subtypeformodded}");
                }*/
                if (subtypeformodded <= Instance.ShieldTypes.Count && subtypeformodded > -1)
                {
                    /*if (Global.DebugLogging)
                    {
                        Logger.Info("Creating Shield from list info");
                    }*/
                    ShieldPlugin ShieldType = Instance.ShieldTypes[Subtype - Instance.VanillaShieldMaxType];
                    InShield.SubType = Subtype;
                    InShield.Name = ShieldType.Name;
                    InShield.Desc = ShieldType.Description;
                    InShield.GetType().GetField("m_IconTexture", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InShield, ShieldType.IconTexture);
                    InShield.Max = ShieldType.ShieldMax;
                    InShield.ChargeRateMax = ShieldType.ChargeRateMax;
                    InShield.RecoveryRate = ShieldType.RecoveryRate;
                    InShield.Deflection = ShieldType.Deflection;
                    InShield.MinIntegrityPercentForQuantumShield = ShieldType.MinIntegrityPercentForQuantumShield;
                    InShield.MinIntegrityAfterDamage = ShieldType.MinIntegrityAfterDamage;
                    InShield.GetType().GetField("m_MaxPowerUsage_Watts", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InShield, (ShieldType.MaxPowerUsage_Watts * 1.4f));
                    InShield.GetType().GetField("m_MarketPrice", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InShield, (ObscuredInt)ShieldType.MarketPrice);
                    InShield.CargoVisualPrefabID = ShieldType.CargoVisualID;
                    InShield.CanBeDroppedOnShipDeath = ShieldType.CanBeDroppedOnShipDeath;
                    InShield.Experimental = ShieldType.Experimental;
                    InShield.Unstable = ShieldType.Unstable;
                    InShield.Contraband = ShieldType.Contraband;
                    InShield.GetType().GetField("Price_LevelMultiplierExponent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InShield, (ObscuredFloat)ShieldType.Price_LevelMultiplierExponent);
                    if (InShield.MinIntegrityAfterDamage == -1)
                    {
                        InShield.MinIntegrityAfterDamage = Mathf.RoundToInt(InShield.Max * 0.15f);
                    }
                    InShield.MinIntegrityAfterDamage = Mathf.RoundToInt(InShield.MinIntegrityAfterDamage * (1f - Mathf.Clamp(0.05f * InShield.Level, 0f, 0.8f)));
                    InShield.CurrentMax = InShield.Max;
                    InShield.Current = InShield.Max;
                }
            }
            else
            {
                InShield = new PLShieldGenerator((EShieldGeneratorType)Subtype, level);
            }
            return InShield;
        }
    }
    //Converts hashes to Shields.
    [HarmonyPatch(typeof(PLShieldGenerator), "CreateShieldGeneratorFromHash")]
    class ShieldHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = ShieldPluginManager.CreateShield(inSubType, inLevel);
            return false;
        }
    }
    /*[HarmonyPatch(typeof(PLShieldGenerator), "Tick")]
    class TickPatch
    {
        static void Postfix(PLShieldGenerator __instance)
        {
            int subtypeformodded = __instance.SubType - ShieldPluginManager.Instance.VanillaShieldMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ShieldPluginManager.Instance.ShieldTypes.Count && __instance.ShipStats != null && __instance.ShipStats.ShieldTempMax != 0f)
            {
                ShieldPluginManager.Instance.ShieldTypes[subtypeformodded].ShieldPowerCode(__instance);
            }
        }
    }*/
}

using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Logger = PulsarModLoader.Utilities.Logger;

namespace PulsarModLoader.Content.Components.Shield
{
    public class ShieldModManager
    {
        public readonly int VanillaShieldMaxType = 0;
        private static ShieldModManager m_instance = null;
        public readonly List<ShieldMod> ShieldTypes = new List<ShieldMod>();
        public static ShieldModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ShieldModManager();
                }
                return m_instance;
            }
        }

        ShieldModManager()
        {
            VanillaShieldMaxType = Enum.GetValues(typeof(EShieldGeneratorType)).Length;
            Logger.Info($"MaxTypeint = {VanillaShieldMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type ShieldMod = typeof(ShieldMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (ShieldMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading Shield from assembly");
                        ShieldMod ShieldModHandler = (ShieldMod)Activator.CreateInstance(t);
                        if (GetShieldIDFromName(ShieldModHandler.Name) == -1)
                        {
                            ShieldTypes.Add(ShieldModHandler);
                            Logger.Info($"Added Shield: '{ShieldModHandler.Name}' with ID '{GetShieldIDFromName(ShieldModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add Shield from {mod.Name} with the duplicate name of '{ShieldModHandler.Name}'");
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
                if (subtypeformodded <= Instance.ShieldTypes.Count && subtypeformodded > -1)
                {
                    ShieldMod ShieldType = Instance.ShieldTypes[Subtype - Instance.VanillaShieldMaxType];
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
                    InShield.GetType().GetField("Price_LevelMultiplierExponent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InShield, ShieldType.Price_LevelMultiplierExponent);
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
            __result = ShieldModManager.CreateShield(inSubType, inLevel);
            return false;
        }
    }
    //Applies the Tick of the modded shields
    [HarmonyPatch(typeof(PLShieldGenerator), "Tick")]
    class TickPatch
    {
        static void Postfix(PLShieldGenerator __instance)
        {
            int subtypeformodded = __instance.SubType - ShieldModManager.Instance.VanillaShieldMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ShieldModManager.Instance.ShieldTypes.Count && __instance.ShipStats != null)
            {
                ShieldModManager.Instance.ShieldTypes[subtypeformodded].Tick(__instance);
            }
        }
    }
}

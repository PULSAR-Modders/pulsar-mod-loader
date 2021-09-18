using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PulsarModLoader.Content.Components.PolytechModule
{
    public class PolytechModuleModManager
    {
        public readonly int VanillaPolytechModuleMaxType = 0;
        private static PolytechModuleModManager m_instance = null;
        public readonly List<PolytechModuleMod> PolytechModuleTypes = new List<PolytechModuleMod>();
        public static PolytechModuleModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new PolytechModuleModManager();
                }
                return m_instance;
            }
        }

        PolytechModuleModManager()
        {
            VanillaPolytechModuleMaxType = Enum.GetValues(typeof(EPolytechModuleType)).Length;
            Logger.Info($"MaxTypeint = {VanillaPolytechModuleMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type PolytechModuleMod = typeof(PolytechModuleMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (PolytechModuleMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading PolytechModule from assembly");
                        PolytechModuleMod PolytechModuleModHandler = (PolytechModuleMod)Activator.CreateInstance(t);
                        if (GetPolytechModuleIDFromName(PolytechModuleModHandler.Name) == -1)
                        {
                            PolytechModuleTypes.Add(PolytechModuleModHandler);
                            Logger.Info($"Added PolytechModule: '{PolytechModuleModHandler.Name}' with ID '{GetPolytechModuleIDFromName(PolytechModuleModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add PolytechModule from {mod.Name} with the duplicate name of '{PolytechModuleModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds PolytechModule type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find PolytechModule.
        /// </summary>
        /// <param name="PolytechModuleName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetPolytechModuleIDFromName(string PolytechModuleName)
        {
            for (int i = 0; i < PolytechModuleTypes.Count; i++)
            {
                if (PolytechModuleTypes[i].Name == PolytechModuleName)
                {
                    return i + VanillaPolytechModuleMaxType;
                }
            }
            return -1;
        }
        public static PLPolytechModule CreatePolytechModule(int Subtype, int level)
        {
            PLPolytechModule InPolytechModule;
            if (Subtype >= Instance.VanillaPolytechModuleMaxType)
            {
                InPolytechModule = new PLPolytechModule(EPolytechModuleType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaPolytechModuleMaxType;
                if (subtypeformodded <= Instance.PolytechModuleTypes.Count && subtypeformodded > -1)
                {
                    PolytechModuleMod PolytechModuleType = Instance.PolytechModuleTypes[Subtype - Instance.VanillaPolytechModuleMaxType];
                    InPolytechModule.SubType = Subtype;
                    InPolytechModule.Name = PolytechModuleType.Name;
                    InPolytechModule.Desc = PolytechModuleType.Description;
                    InPolytechModule.GetType().GetField("m_IconTexture", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InPolytechModule, PolytechModuleType.IconTexture);
                    InPolytechModule.GetType().GetField("m_MarketPrice", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InPolytechModule, (ObscuredInt)PolytechModuleType.MarketPrice);
                    InPolytechModule.CargoVisualPrefabID = PolytechModuleType.CargoVisualID;
                    InPolytechModule.CanBeDroppedOnShipDeath = PolytechModuleType.CanBeDroppedOnShipDeath;
                    InPolytechModule.Experimental = PolytechModuleType.Experimental;
                    InPolytechModule.Unstable = PolytechModuleType.Unstable;
                    InPolytechModule.Contraband = PolytechModuleType.Contraband;
                    InPolytechModule.GetType().GetField("Price_LevelMultiplierExponent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InPolytechModule, PolytechModuleType.Price_LevelMultiplierExponent);
                    InPolytechModule.GetType().GetField("m_MaxPowerUsage_Watts", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InPolytechModule, PolytechModuleType.MaxPowerUsage_Watts);
                }
            }
            else
            {
                InPolytechModule = new PLPolytechModule((EPolytechModuleType)Subtype, level);
            }
            return InPolytechModule;
        }
    }
    //Converts hashes to PolytechModules.
    [HarmonyPatch(typeof(PLPolytechModule), "CreatePolytechModuleFromHash")]
    class HashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = PolytechModuleModManager.CreatePolytechModule(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLPolytechModule), "Tick")]
    class TickPatch
    {
        static void Postfix(PLPolytechModule __instance)
        {
            int subtypeformodded = __instance.SubType - PolytechModuleModManager.Instance.VanillaPolytechModuleMaxType;
            if (subtypeformodded > -1 && subtypeformodded < PolytechModuleModManager.Instance.PolytechModuleTypes.Count && __instance.ShipStats != null && __instance.IsEquipped)
            {
                PolytechModuleModManager.Instance.PolytechModuleTypes[subtypeformodded].Tick(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLPolytechModule), "FinalLateAddStats")]
    class FinalLateAddStatsPatch
    {
        static void Postfix(PLPolytechModule __instance)
        {
            int subtypeformodded = __instance.SubType - PolytechModuleModManager.Instance.VanillaPolytechModuleMaxType;
            if (subtypeformodded > -1 && subtypeformodded < PolytechModuleModManager.Instance.PolytechModuleTypes.Count && __instance.ShipStats != null)
            {
                PolytechModuleModManager.Instance.PolytechModuleTypes[subtypeformodded].FinalLateAddStats(__instance);
            }
        }
    }
}

using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.Thruster
{
    public class ThrusterModManager
    {
        public readonly int VanillaThrusterMaxType = 0;
        private static ThrusterModManager m_instance = null;
        public readonly List<ThrusterMod> ThrusterTypes = new List<ThrusterMod>();
        public static ThrusterModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ThrusterModManager();
                }
                return m_instance;
            }
        }

        ThrusterModManager()
        {
            VanillaThrusterMaxType = Enum.GetValues(typeof(EThrusterType)).Length;
            Logger.Info($"MaxTypeint = {VanillaThrusterMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type ThrusterMod = typeof(ThrusterMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (ThrusterMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading Thruster from assembly");
                        ThrusterMod ThrusterModHandler = (ThrusterMod)Activator.CreateInstance(t);
                        if (GetThrusterIDFromName(ThrusterModHandler.Name) == -1)
                        {
                            ThrusterTypes.Add(ThrusterModHandler);
                            Logger.Info($"Added Thruster: '{ThrusterModHandler.Name}' with ID '{GetThrusterIDFromName(ThrusterModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add Thruster from {mod.Name} with the duplicate name of '{ThrusterModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds Thruster type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Thruster.
        /// </summary>
        /// <param name="ThrusterName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetThrusterIDFromName(string ThrusterName)
        {
            for (int i = 0; i < ThrusterTypes.Count; i++)
            {
                if (ThrusterTypes[i].Name == ThrusterName)
                {
                    return i + VanillaThrusterMaxType;
                }
            }
            return -1;
        }
        public static PLThruster CreateThruster(int Subtype, int level)
        {
            PLThruster InThruster;
            if (Subtype >= Instance.VanillaThrusterMaxType)
            {
                InThruster = new PLThruster(EThrusterType.MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaThrusterMaxType;
                if (subtypeformodded <= Instance.ThrusterTypes.Count && subtypeformodded > -1)
                {
                    ThrusterMod ThrusterType = Instance.ThrusterTypes[Subtype - Instance.VanillaThrusterMaxType];
                    InThruster.SubType = Subtype;
                    InThruster.Name = ThrusterType.Name;
                    InThruster.Desc = ThrusterType.Description;
                    InThruster.m_IconTexture = ThrusterType.IconTexture;
                    InThruster.m_MaxOutput = ThrusterType.MaxOutput;
                    InThruster.m_BaseMaxPower = ThrusterType.MaxPowerUsage_Watts;
                    InThruster.m_MarketPrice = ThrusterType.MarketPrice;
                    InThruster.CargoVisualPrefabID = ThrusterType.CargoVisualID;
                    InThruster.CanBeDroppedOnShipDeath = ThrusterType.CanBeDroppedOnShipDeath;
                    InThruster.Experimental = ThrusterType.Experimental;
                    InThruster.Unstable = ThrusterType.Unstable;
                    InThruster.Contraband = ThrusterType.Contraband;
                    InThruster.UpdateMaxPowerWatts();
                    InThruster.Price_LevelMultiplierExponent = ThrusterType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InThruster = new PLThruster((EThrusterType)Subtype, level);
            }
            return InThruster;
        }
    }
    //Converts hashes to Thrusters.
    [HarmonyPatch(typeof(PLThruster), "CreateThrusterFromHash")]
    class ThrusterHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = ThrusterModManager.CreateThruster(inSubType, inLevel);
            return false;
        }
    }

    [HarmonyPatch(typeof(PLThruster), "Tick")]
    class TickPatch
    {
        static void Postfix(PLThruster __instance)
        {
            int subtypeformodded = __instance.SubType - ThrusterModManager.Instance.VanillaThrusterMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ThrusterModManager.Instance.ThrusterTypes.Count && __instance.ShipStats != null)
            {
                ThrusterModManager.Instance.ThrusterTypes[subtypeformodded].Tick(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLThruster), "GetStatLineLeft")]
    class LeftDescFix
    {
        static void Postfix(PLThruster __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - ThrusterModManager.Instance.VanillaThrusterMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ThrusterModManager.Instance.ThrusterTypes.Count && __instance.ShipStats != null)
            {
                __result = ThrusterModManager.Instance.ThrusterTypes[subtypeformodded].GetStatLineLeft(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLThruster), "GetStatLineRight")]
    class RightDescFix
    {
        static void Postfix(PLThruster __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - ThrusterModManager.Instance.VanillaThrusterMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ThrusterModManager.Instance.ThrusterTypes.Count && __instance.ShipStats != null)
            {
                __result = ThrusterModManager.Instance.ThrusterTypes[subtypeformodded].GetStatLineRight(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "LateAddStats")]
    class ThrusterLateAddStatsPatch
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance)
        {
            if(__instance is PLThruster)
            {
                int subtypeformodded = __instance.SubType - ThrusterModManager.Instance.VanillaThrusterMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ThrusterModManager.Instance.ThrusterTypes.Count && inStats != null)
                {
                    ThrusterModManager.Instance.ThrusterTypes[subtypeformodded].LateAddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "AddStats")]
    class ThrusterAddStats
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance) 
        {
            if(__instance is PLThruster) 
            {
                int subtypeformodded = __instance.SubType - ThrusterModManager.Instance.VanillaThrusterMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ThrusterModManager.Instance.ThrusterTypes.Count && inStats != null)
                {
                    ThrusterModManager.Instance.ThrusterTypes[subtypeformodded].AddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "FinalLateAddStats")]
    class ThrusterFinalLateAddStats
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance)
        {
            if (__instance is PLThruster)
            {
                int subtypeformodded = __instance.SubType - ThrusterModManager.Instance.VanillaThrusterMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ThrusterModManager.Instance.ThrusterTypes.Count && inStats != null)
                {
                    ThrusterModManager.Instance.ThrusterTypes[subtypeformodded].FinalLateAddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "OnWarp")]
    class ThrusterOnWarp
    {
        static void Postfix(PLShipComponent __instance)
        {
            if (__instance is PLThruster)
            {
                int subtypeformodded = __instance.SubType - ThrusterModManager.Instance.VanillaThrusterMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ThrusterModManager.Instance.ThrusterTypes.Count)
                {
                    ThrusterModManager.Instance.ThrusterTypes[subtypeformodded].OnWarp(__instance);
                }
            }
        }
    }
}

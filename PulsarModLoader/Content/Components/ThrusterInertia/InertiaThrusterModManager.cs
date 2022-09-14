﻿using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.InertiaThruster
{
    public class InertiaThrusterModManager
    {
        public readonly int VanillaInertiaThrusterMaxType = 0;
        private static InertiaThrusterModManager m_instance = null;
        public readonly List<InertiaThrusterMod> InertiaThrusterTypes = new List<InertiaThrusterMod>();
        public static InertiaThrusterModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new InertiaThrusterModManager();
                }
                return m_instance;
            }
        }

        InertiaThrusterModManager()
        {
            VanillaInertiaThrusterMaxType = Enum.GetValues(typeof(EInertiaThrusterType)).Length;
            Logger.Info($"MaxTypeint = {VanillaInertiaThrusterMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type InertiaThrusterMod = typeof(InertiaThrusterMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (InertiaThrusterMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading InertiaThruster from assembly");
                        InertiaThrusterMod InertiaThrusterModHandler = (InertiaThrusterMod)Activator.CreateInstance(t);
                        if (GetInertiaThrusterIDFromName(InertiaThrusterModHandler.Name) == -1)
                        {
                            InertiaThrusterTypes.Add(InertiaThrusterModHandler);
                            Logger.Info($"Added InertiaThruster: '{InertiaThrusterModHandler.Name}' with ID '{GetInertiaThrusterIDFromName(InertiaThrusterModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add InertiaThruster from {mod.Name} with the duplicate name of '{InertiaThrusterModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds InertiaThruster type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find InertiaThruster.
        /// </summary>
        /// <param name="InertiaThrusterName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetInertiaThrusterIDFromName(string InertiaThrusterName)
        {
            for (int i = 0; i < InertiaThrusterTypes.Count; i++)
            {
                if (InertiaThrusterTypes[i].Name == InertiaThrusterName)
                {
                    return i + VanillaInertiaThrusterMaxType;
                }
            }
            return -1;
        }
        public static PLInertiaThruster CreateInertiaThruster(int Subtype, int level)
        {
            PLInertiaThruster InInertiaThruster;
            if (Subtype >= Instance.VanillaInertiaThrusterMaxType)
            {
                InInertiaThruster = new PLInertiaThruster(EInertiaThrusterType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaInertiaThrusterMaxType;
                if (subtypeformodded <= Instance.InertiaThrusterTypes.Count && subtypeformodded > -1)
                {
                    InertiaThrusterMod InertiaThrusterType = Instance.InertiaThrusterTypes[Subtype - Instance.VanillaInertiaThrusterMaxType];
                    InInertiaThruster.SubType = Subtype;
                    InInertiaThruster.Name = InertiaThrusterType.Name;
                    InInertiaThruster.Desc = InertiaThrusterType.Description;
                    InInertiaThruster.m_IconTexture = InertiaThrusterType.IconTexture;
                    InInertiaThruster.m_MaxOutput = InertiaThrusterType.MaxOutput;
                    InInertiaThruster.m_BaseMaxPower = InertiaThrusterType.MaxPowerUsage_Watts;
                    InInertiaThruster.m_MarketPrice = InertiaThrusterType.MarketPrice; 
                    InInertiaThruster.CargoVisualPrefabID = InertiaThrusterType.CargoVisualID;
                    InInertiaThruster.CanBeDroppedOnShipDeath = InertiaThrusterType.CanBeDroppedOnShipDeath;
                    InInertiaThruster.Experimental = InertiaThrusterType.Experimental;
                    InInertiaThruster.Unstable = InertiaThrusterType.Unstable;
                    InInertiaThruster.Contraband = InertiaThrusterType.Contraband;
                    InInertiaThruster.UpdateMaxPowerWatts();
                    InInertiaThruster.Price_LevelMultiplierExponent = InertiaThrusterType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InInertiaThruster = new PLInertiaThruster((EInertiaThrusterType)Subtype, level);
            }
            return InInertiaThruster;
        }
    }
    //Converts hashes to InertiaThrusters.
    [HarmonyPatch(typeof(PLInertiaThruster), "CreateInertiaThrusterFromHash")]
    class InertiaThrusterHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = InertiaThrusterModManager.CreateInertiaThruster(inSubType, inLevel);
            return false;
        }
    }

    [HarmonyPatch(typeof(PLInertiaThruster), "Tick")]
    class TickPatch
    {
        static void Postfix(PLInertiaThruster __instance)
        {
            int subtypeformodded = __instance.SubType - InertiaThrusterModManager.Instance.VanillaInertiaThrusterMaxType;
            if (subtypeformodded > -1 && subtypeformodded < InertiaThrusterModManager.Instance.InertiaThrusterTypes.Count && __instance.ShipStats != null)
            {
                InertiaThrusterModManager.Instance.InertiaThrusterTypes[subtypeformodded].Tick(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLInertiaThruster), "GetStatLineLeft")]
    class LeftDescFix
    {
        static void Postfix(PLInertiaThruster __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - InertiaThrusterModManager.Instance.VanillaInertiaThrusterMaxType;
            if (subtypeformodded > -1 && subtypeformodded < InertiaThrusterModManager.Instance.InertiaThrusterTypes.Count && __instance.ShipStats != null)
            {
                __result = InertiaThrusterModManager.Instance.InertiaThrusterTypes[subtypeformodded].GetStatLineLeft(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLInertiaThruster), "GetStatLineRight")]
    class RightDescFix
    {
        static void Postfix(PLInertiaThruster __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - InertiaThrusterModManager.Instance.VanillaInertiaThrusterMaxType;
            if (subtypeformodded > -1 && subtypeformodded < InertiaThrusterModManager.Instance.InertiaThrusterTypes.Count && __instance.ShipStats != null)
            {
                __result = InertiaThrusterModManager.Instance.InertiaThrusterTypes[subtypeformodded].GetStatLineRight(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "LateAddStats")]
    class InertiaThrusterLateAddStatsPatch
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance)
        {
            if(__instance is PLInertiaThruster)
            {
                int subtypeformodded = __instance.SubType - InertiaThrusterModManager.Instance.VanillaInertiaThrusterMaxType;
                if (subtypeformodded > -1 && subtypeformodded < InertiaThrusterModManager.Instance.InertiaThrusterTypes.Count && inStats != null)
                {
                    InertiaThrusterModManager.Instance.InertiaThrusterTypes[subtypeformodded].LateAddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "AddStats")]
    class InertiaThrusterAddStats
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance) 
        {
            if(__instance is PLInertiaThruster) 
            {
                int subtypeformodded = __instance.SubType - InertiaThrusterModManager.Instance.VanillaInertiaThrusterMaxType;
                if (subtypeformodded > -1 && subtypeformodded < InertiaThrusterModManager.Instance.InertiaThrusterTypes.Count && inStats != null)
                {
                    InertiaThrusterModManager.Instance.InertiaThrusterTypes[subtypeformodded].AddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "FinalLateAddStats")]
    class InertiaThrusterFinalLateAddStats
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance)
        {
            if (__instance is PLInertiaThruster)
            {
                int subtypeformodded = __instance.SubType - InertiaThrusterModManager.Instance.VanillaInertiaThrusterMaxType;
                if (subtypeformodded > -1 && subtypeformodded < InertiaThrusterModManager.Instance.InertiaThrusterTypes.Count && inStats != null)
                {
                    InertiaThrusterModManager.Instance.InertiaThrusterTypes[subtypeformodded].FinalLateAddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "OnWarp")]
    class InertiaThrusterOnWarp
    {
        static void Postfix(PLShipComponent __instance)
        {
            if (__instance is PLInertiaThruster)
            {
                int subtypeformodded = __instance.SubType - InertiaThrusterModManager.Instance.VanillaInertiaThrusterMaxType;
                if (subtypeformodded > -1 && subtypeformodded < InertiaThrusterModManager.Instance.InertiaThrusterTypes.Count)
                {
                    InertiaThrusterModManager.Instance.InertiaThrusterTypes[subtypeformodded].OnWarp(__instance);
                }
            }
        }
    }
}

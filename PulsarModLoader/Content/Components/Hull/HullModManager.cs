using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.Hull
{
    public class HullModManager
    {
        public readonly int VanillaHullMaxType = 0;
        private static HullModManager m_instance = null;
        public readonly List<HullMod> HullTypes = new List<HullMod>();
        public static HullModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new HullModManager();
                }
                return m_instance;
            }
        }

        HullModManager()
        {
            VanillaHullMaxType = Enum.GetValues(typeof(EHullType)).Length;
            Logger.Info($"MaxTypeint = {VanillaHullMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type HullMod = typeof(HullMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (HullMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading Hull from assembly");
                        HullMod HullModHandler = (HullMod)Activator.CreateInstance(t);
                        if (GetHullIDFromName(HullModHandler.Name) == -1)
                        {
                            HullTypes.Add(HullModHandler);
                            Logger.Info($"Added Hull: '{HullModHandler.Name}' with ID '{GetHullIDFromName(HullModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add Hull from {mod.Name} with the duplicate name of '{HullModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds Hull type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Hull.
        /// </summary>
        /// <param name="HullName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetHullIDFromName(string HullName)
        {
            for (int i = 0; i < HullTypes.Count; i++)
            {
                if (HullTypes[i].Name == HullName)
                {
                    return i + VanillaHullMaxType;
                }
            }
            return -1;
        }
        public static PLHull CreateHull(int Subtype, int level)
        {
            PLHull InHull;
            if (Subtype >= Instance.VanillaHullMaxType)
            {
                InHull = new PLHull(EHullType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaHullMaxType;
                if (subtypeformodded <= Instance.HullTypes.Count && subtypeformodded > -1)
                {
                    HullMod HullType = Instance.HullTypes[Subtype - Instance.VanillaHullMaxType];
                    InHull.SubType = Subtype;
                    InHull.Name = HullType.Name;
                    InHull.Desc = HullType.Description;
                    InHull.m_IconTexture = HullType.IconTexture;
                    InHull.Max = HullType.HullMax;
                    InHull.Armor = HullType.Armor;
                    InHull.Defense = HullType.Defense;
                    InHull.m_MarketPrice = HullType.MarketPrice;
                    InHull.CargoVisualPrefabID = HullType.CargoVisualID;
                    InHull.CanBeDroppedOnShipDeath = HullType.CanBeDroppedOnShipDeath;
                    InHull.Experimental = HullType.Experimental;
                    InHull.Unstable = HullType.Unstable;
                    InHull.Contraband = HullType.Contraband;
                    InHull.Price_LevelMultiplierExponent = HullType.Price_LevelMultiplierExponent;
                    InHull.Max *= 2f;
                    InHull.Current = InHull.Max;
                }
            }
            else
            {
                InHull = new PLHull((EHullType)Subtype, level);
            }
            return InHull;
        }
    }
    //Converts hashes to Hulls.
    [HarmonyPatch(typeof(PLHull), "CreateHullFromHash")]
    class HullHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = HullModManager.CreateHull(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLHull), "Tick")]
    class TickPatch
    {
        static void Postfix(PLHull __instance)
        {
            int subtypeformodded = __instance.SubType - HullModManager.Instance.VanillaHullMaxType;
            if (subtypeformodded > -1 && subtypeformodded < HullModManager.Instance.HullTypes.Count && __instance.ShipStats != null)
            {
                HullModManager.Instance.HullTypes[subtypeformodded].Tick(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLHull), "GetStatLineLeft")]
    class LeftDescFix 
    {
        static void Postfix(PLHull __instance, ref string __result) 
        {
            int subtypeformodded = __instance.SubType - HullModManager.Instance.VanillaHullMaxType;
            if (subtypeformodded > -1 && subtypeformodded < HullModManager.Instance.HullTypes.Count && __instance.ShipStats != null)
            {
                __result = HullModManager.Instance.HullTypes[subtypeformodded].GetStatLineLeft(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLHull), "GetStatLineRight")]
    class RightDescFix
    {
        static void Postfix(PLHull __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - HullModManager.Instance.VanillaHullMaxType;
            if (subtypeformodded > -1 && subtypeformodded < HullModManager.Instance.HullTypes.Count && __instance.ShipStats != null)
            {
                __result = HullModManager.Instance.HullTypes[subtypeformodded].GetStatLineRight(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "LateAddStats")]
    class HullLateAddStatsPatch
    {
        static void Postfix(PLShipStats inStats, PLHull __instance)
        {
            if(__instance is PLHull) 
            {
                int subtypeformodded = __instance.SubType - HullModManager.Instance.VanillaHullMaxType;
                if (subtypeformodded > -1 && subtypeformodded < HullModManager.Instance.HullTypes.Count && inStats != null)
                {
                    HullModManager.Instance.HullTypes[subtypeformodded].LateAddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "AddStats")]
    class HullAddStats
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance) 
        {
            if(__instance is PLHull) 
            {
                int subtypeformodded = __instance.SubType - HullModManager.Instance.VanillaHullMaxType;
                if (subtypeformodded > -1 && subtypeformodded < HullModManager.Instance.HullTypes.Count && inStats != null)
                {
                    HullModManager.Instance.HullTypes[subtypeformodded].AddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "FinalLateAddStats")]
    class HullFinalLateAddStats
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance)
        {
            if (__instance is PLHull)
            {
                int subtypeformodded = __instance.SubType - HullModManager.Instance.VanillaHullMaxType;
                if (subtypeformodded > -1 && subtypeformodded < HullModManager.Instance.HullTypes.Count && inStats != null)
                {
                    HullModManager.Instance.HullTypes[subtypeformodded].FinalLateAddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "OnWarp")]
    class HullOnWarp
    {
        static void Postfix(PLShipComponent __instance)
        {
            if (__instance is PLHull)
            {
                int subtypeformodded = __instance.SubType - HullModManager.Instance.VanillaHullMaxType;
                if (subtypeformodded > -1 && subtypeformodded < HullModManager.Instance.HullTypes.Count)
                {
                    HullModManager.Instance.HullTypes[subtypeformodded].OnWarp(__instance);
                }
            }
        }
    }
}

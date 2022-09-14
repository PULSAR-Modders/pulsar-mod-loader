using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.Extractor
{
    public class ExtractorModManager
    {
        public readonly int VanillaExtractorMaxType = 0;
        private static ExtractorModManager m_instance = null;
        public readonly List<ExtractorMod> ExtractorTypes = new List<ExtractorMod>();
        public static ExtractorModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ExtractorModManager();
                }
                return m_instance;
            }
        }

        ExtractorModManager()
        {
            VanillaExtractorMaxType = Enum.GetValues(typeof(EExtractorType)).Length;
            Logger.Info($"MaxTypeint = {VanillaExtractorMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type ExtractorMod = typeof(ExtractorMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (ExtractorMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading Extractor from assembly");
                        ExtractorMod ExtractorModHandler = (ExtractorMod)Activator.CreateInstance(t);
                        if (GetExtractorIDFromName(ExtractorModHandler.Name) == -1)
                        {
                            ExtractorTypes.Add(ExtractorModHandler);
                            Logger.Info($"Added Extractor: '{ExtractorModHandler.Name}' with ID '{GetExtractorIDFromName(ExtractorModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add Extractor from {mod.Name} with the duplicate name of '{ExtractorModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds Extractor type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Extractor.
        /// </summary>
        /// <param name="ExtractorName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetExtractorIDFromName(string ExtractorName)
        {
            for (int i = 0; i < ExtractorTypes.Count; i++)
            {
                if (ExtractorTypes[i].Name == ExtractorName)
                {
                    return i + VanillaExtractorMaxType;
                }
            }
            return -1;
        }
        public static PLExtractor CreateExtractor(int Subtype, int level)
        {
            PLExtractor InExtractor;
            if (Subtype >= Instance.VanillaExtractorMaxType)
            {
                InExtractor = new PLExtractor(EExtractorType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaExtractorMaxType;
                if (subtypeformodded <= Instance.ExtractorTypes.Count && subtypeformodded > -1)
                {
                    ExtractorMod ExtractorType = Instance.ExtractorTypes[Subtype - Instance.VanillaExtractorMaxType];
                    InExtractor.SubType = Subtype;
                    InExtractor.Name = ExtractorType.Name;
                    InExtractor.Desc = ExtractorType.Description;
                    InExtractor.m_IconTexture = ExtractorType.IconTexture;
                    InExtractor.m_Stability = ExtractorType.Stability;
                    InExtractor.m_MarketPrice = ExtractorType.MarketPrice;
                    InExtractor.CargoVisualPrefabID = ExtractorType.CargoVisualID;
                    InExtractor.CanBeDroppedOnShipDeath = ExtractorType.CanBeDroppedOnShipDeath;
                    InExtractor.Experimental = ExtractorType.Experimental;
                    InExtractor.Unstable = ExtractorType.Unstable;
                    InExtractor.Contraband = ExtractorType.Contraband;
                    InExtractor.Price_LevelMultiplierExponent = ExtractorType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InExtractor = new PLExtractor((EExtractorType)Subtype, level);
            }
            return InExtractor;
        }
    }
    //Converts hashes to Extractors.
    [HarmonyPatch(typeof(PLExtractor), "CreateExtractorFromHash")]
    class ExtractorHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = ExtractorModManager.CreateExtractor(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLExtractor), "GetStatLineLeft")]
    class LeftDescFix
    {
        static void Postfix(PLExtractor __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - ExtractorModManager.Instance.VanillaExtractorMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ExtractorModManager.Instance.ExtractorTypes.Count && __instance.ShipStats != null)
            {
                __result = ExtractorModManager.Instance.ExtractorTypes[subtypeformodded].GetStatLineLeft(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLExtractor), "GetStatLineRight")]
    class RightDescFix
    {
        static void Postfix(PLExtractor __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - ExtractorModManager.Instance.VanillaExtractorMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ExtractorModManager.Instance.ExtractorTypes.Count && __instance.ShipStats != null)
            {
                __result = ExtractorModManager.Instance.ExtractorTypes[subtypeformodded].GetStatLineRight(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "LateAddStats")]
    class ExtractorLateAddStatsPatch
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance)
        {
            if(__instance is PLExtractor)
            {
                int subtypeformodded = __instance.SubType - ExtractorModManager.Instance.VanillaExtractorMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ExtractorModManager.Instance.ExtractorTypes.Count && inStats != null)
                {
                    ExtractorModManager.Instance.ExtractorTypes[subtypeformodded].LateAddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "AddStats")]
    class ExtractorAddStats
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance) 
        {
            if(__instance is PLExtractor) 
            {
                int subtypeformodded = __instance.SubType - ExtractorModManager.Instance.VanillaExtractorMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ExtractorModManager.Instance.ExtractorTypes.Count && inStats != null)
                {
                    ExtractorModManager.Instance.ExtractorTypes[subtypeformodded].AddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "Tick")]
    class ExtractorTick
    {
        static void Postfix(PLShipComponent __instance)
        {
            if (__instance is PLExtractor)
            {
                int subtypeformodded = __instance.SubType - ExtractorModManager.Instance.VanillaExtractorMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ExtractorModManager.Instance.ExtractorTypes.Count)
                {
                    ExtractorModManager.Instance.ExtractorTypes[subtypeformodded].Tick(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "FinalLateAddStats")]
    class ExtractorFinalLateAddStats
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance)
        {
            if (__instance is PLExtractor)
            {
                int subtypeformodded = __instance.SubType - ExtractorModManager.Instance.VanillaExtractorMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ExtractorModManager.Instance.ExtractorTypes.Count && inStats != null)
                {
                    ExtractorModManager.Instance.ExtractorTypes[subtypeformodded].FinalLateAddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "OnWarp")]
    class ExtractorOnWarp
    {
        static void Postfix(PLShipComponent __instance)
        {
            if (__instance is PLExtractor)
            {
                int subtypeformodded = __instance.SubType - ExtractorModManager.Instance.VanillaExtractorMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ExtractorModManager.Instance.ExtractorTypes.Count)
                {
                    ExtractorModManager.Instance.ExtractorTypes[subtypeformodded].OnWarp(__instance);
                }
            }
        }
    }
}

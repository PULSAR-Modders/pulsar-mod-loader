using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using Logger = PulsarModLoader.Utilities.Logger;

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
                    InExtractor.GetType().GetField("m_IconTexture", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InExtractor, ExtractorType.IconTexture);
                    InExtractor.GetType().GetField("m_Stability", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InExtractor, ExtractorType.Stability);
                    InExtractor.GetType().GetField("m_MarketPrice", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InExtractor, (ObscuredInt)ExtractorType.MarketPrice);
                    InExtractor.CargoVisualPrefabID = ExtractorType.CargoVisualID;
                    InExtractor.CanBeDroppedOnShipDeath = ExtractorType.CanBeDroppedOnShipDeath;
                    InExtractor.Experimental = ExtractorType.Experimental;
                    InExtractor.Unstable = ExtractorType.Unstable;
                    InExtractor.Contraband = ExtractorType.Contraband;
                    InExtractor.GetType().GetField("Price_LevelMultiplierExponent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InExtractor, ExtractorType.Price_LevelMultiplierExponent);
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
}

using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarPluginLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using Logger = PulsarPluginLoader.Utilities.Logger;

namespace PulsarPluginLoader.Content.Components.Extractor
{
    public class ExtractorPluginManager
    {
        public readonly int VanillaExtractorMaxType = 0;
        private static ExtractorPluginManager m_instance = null;
        public readonly List<ExtractorPlugin> ExtractorTypes = new List<ExtractorPlugin>();
        public static ExtractorPluginManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ExtractorPluginManager();
                }
                return m_instance;
            }
        }

        ExtractorPluginManager()
        {
            VanillaExtractorMaxType = Enum.GetValues(typeof(EExtractorType)).Length;
            Logger.Info($"MaxTypeint = {VanillaExtractorMaxType - 1}");
            foreach (PulsarPlugin plugin in PluginManager.Instance.GetAllPlugins())
            {
                Assembly asm = plugin.GetType().Assembly;
                Type ExtractorPlugin = typeof(ExtractorPlugin);
                foreach (Type t in asm.GetTypes())
                {
                    if (ExtractorPlugin.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading Extractor from assembly");
                        ExtractorPlugin ExtractorPluginHandler = (ExtractorPlugin)Activator.CreateInstance(t);
                        if (GetExtractorIDFromName(ExtractorPluginHandler.Name) == -1)
                        {
                            ExtractorTypes.Add(ExtractorPluginHandler);
                            Logger.Info($"Added Extractor: '{ExtractorPluginHandler.Name}' with ID '{GetExtractorIDFromName(ExtractorPluginHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add Extractor from {plugin.Name} with the duplicate name of '{ExtractorPluginHandler.Name}'");
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
                /*if (Global.DebugLogging)
                {
                    Logger.Info($"Subtype for modded is {subtypeformodded}");
                }*/
                if (subtypeformodded <= Instance.ExtractorTypes.Count && subtypeformodded > -1)
                {
                    /*if (Global.DebugLogging)
                    {
                        Logger.Info("Creating Extractor from list info");
                    }*/
                    ExtractorPlugin ExtractorType = Instance.ExtractorTypes[Subtype - Instance.VanillaExtractorMaxType];
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
            __result = ExtractorPluginManager.CreateExtractor(inSubType, inLevel);
            return false;
        }
    }
}

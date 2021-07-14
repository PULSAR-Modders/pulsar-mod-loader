using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Logger = PulsarPluginLoader.Utilities.Logger;

namespace PulsarPluginLoader.Content.Components.Virus
{
    public class VirusPluginManager
    {
        public readonly int VanillaVirusMaxType = 0;
        private static VirusPluginManager m_instance = null;
        public readonly List<VirusPlugin> VirusTypes = new List<VirusPlugin>();
        public static VirusPluginManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new VirusPluginManager();
                }
                return m_instance;
            }
        }

        VirusPluginManager()
        {
            VanillaVirusMaxType = Enum.GetValues(typeof(EVirusType)).Length - 1;
            Logger.Info($"MaxTypeint = {VanillaVirusMaxType - 1}");
            foreach (PulsarPlugin plugin in PluginManager.Instance.GetAllPlugins())
            {
                Assembly asm = plugin.GetType().Assembly;
                Type VirusPlugin = typeof(VirusPlugin);
                foreach (Type t in asm.GetTypes())
                {
                    if (VirusPlugin.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading Virus from assembly");
                        VirusPlugin VirusPluginHandler = (VirusPlugin)Activator.CreateInstance(t);
                        if (GetVirusIDFromName(VirusPluginHandler.Name) == -1)
                        {
                            VirusTypes.Add(VirusPluginHandler);
                            Logger.Info($"Added Virus: '{VirusPluginHandler.Name}' with ID '{GetVirusIDFromName(VirusPluginHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add Virus from {plugin.Name} with the duplicate name of '{VirusPluginHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds Virus type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Virus.
        /// </summary>
        /// <param name="VirusName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetVirusIDFromName(string VirusName)
        {
            for (int i = 0; i < VirusTypes.Count; i++)
            {
                if (VirusTypes[i].Name == VirusName)
                {
                    return i + VanillaVirusMaxType;
                }
            }
            return -1;
        }
        public static PLVirus CreateVirus(int Subtype, int level)
        {
            PLVirus InVirus;
            if (Subtype >= Instance.VanillaVirusMaxType)
            {
                InVirus = new PLVirus(EVirusType.NONE, level);
                int subtypeformodded = Subtype - Instance.VanillaVirusMaxType;
                if (subtypeformodded <= Instance.VirusTypes.Count && subtypeformodded > -1)
                {
                    VirusPlugin VirusType = Instance.VirusTypes[Subtype - Instance.VanillaVirusMaxType];
                    InVirus.SubType = Subtype;
                    InVirus.Name = VirusType.Name;
                    InVirus.Desc = VirusType.Description;
                    InVirus.GetType().GetField("m_IconTexture", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InVirus, VirusType.IconTexture);
                    InVirus.GetType().GetField("m_MarketPrice", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InVirus, (ObscuredInt)VirusType.MarketPrice);
                    InVirus.CargoVisualPrefabID = VirusType.CargoVisualID;
                    InVirus.CanBeDroppedOnShipDeath = VirusType.CanBeDroppedOnShipDeath;
                    InVirus.Experimental = VirusType.Experimental;
                    InVirus.Unstable = VirusType.Unstable;
                    InVirus.Contraband = VirusType.Contraband;
                    InVirus.GetType().GetField("Price_LevelMultiplierExponent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InVirus, VirusType.Price_LevelMultiplierExponent);
                }
            }
            else
            {
                InVirus = new PLVirus((EVirusType)Subtype, level);
            }
            return InVirus;
        }
    }
    //Converts hashes to Viruss.
    [HarmonyPatch(typeof(PLVirus), "CreateVirusFromHash")]
    class VirusHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = VirusPluginManager.CreateVirus(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLVirus), "FinalLateAddStats")]
    class VirusFinalLateAddStatsPatch
    {
        static void Postfix(PLVirus __instance)
        {
            int subtypeformodded = __instance.SubType - VirusPluginManager.Instance.VanillaVirusMaxType;
            if (subtypeformodded > -1 && subtypeformodded < VirusPluginManager.Instance.VirusTypes.Count)
            {
                VirusPluginManager.Instance.VirusTypes[subtypeformodded].FinalLateAddStats(__instance);
            }
        }
    }
}

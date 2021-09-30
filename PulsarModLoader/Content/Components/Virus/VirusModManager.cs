using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.Virus
{
    public class VirusModManager
    {
        public readonly int VanillaVirusMaxType = 0;
        private static VirusModManager m_instance = null;
        public readonly List<VirusMod> VirusTypes = new List<VirusMod>();
        public static VirusModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new VirusModManager();
                }
                return m_instance;
            }
        }

        VirusModManager()
        {
            VanillaVirusMaxType = Enum.GetValues(typeof(EVirusType)).Length - 1;
            Logger.Info($"MaxTypeint = {VanillaVirusMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type VirusMod = typeof(VirusMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (VirusMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading Virus from assembly");
                        VirusMod VirusModHandler = (VirusMod)Activator.CreateInstance(t);
                        if (GetVirusIDFromName(VirusModHandler.Name) == -1)
                        {
                            VirusTypes.Add(VirusModHandler);
                            Logger.Info($"Added Virus: '{VirusModHandler.Name}' with ID '{GetVirusIDFromName(VirusModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add Virus from {mod.Name} with the duplicate name of '{VirusModHandler.Name}'");
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
                    VirusMod VirusType = Instance.VirusTypes[Subtype - Instance.VanillaVirusMaxType];
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
            __result = VirusModManager.CreateVirus(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLVirus), "FinalLateAddStats")]
    class VirusFinalLateAddStatsPatch
    {
        static void Postfix(PLVirus __instance)
        {
            int subtypeformodded = __instance.SubType - VirusModManager.Instance.VanillaVirusMaxType;
            if (subtypeformodded > -1 && subtypeformodded < VirusModManager.Instance.VirusTypes.Count)
            {
                VirusModManager.Instance.VirusTypes[subtypeformodded].FinalLateAddStats(__instance);
            }
        }
    }
}

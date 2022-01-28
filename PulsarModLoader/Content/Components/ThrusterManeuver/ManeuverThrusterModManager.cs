using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.ManeuverThruster
{
    public class ManeuverThrusterModManager
    {
        public readonly int VanillaManeuverThrusterMaxType = 0;
        private static ManeuverThrusterModManager m_instance = null;
        public readonly List<ManeuverThrusterMod> ManeuverThrusterTypes = new List<ManeuverThrusterMod>();
        public static ManeuverThrusterModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ManeuverThrusterModManager();
                }
                return m_instance;
            }
        }

        ManeuverThrusterModManager()
        {
            VanillaManeuverThrusterMaxType = Enum.GetValues(typeof(EManeuverThrusterType)).Length;
            Logger.Info($"MaxTypeint = {VanillaManeuverThrusterMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type ManeuverThrusterMod = typeof(ManeuverThrusterMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (ManeuverThrusterMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading ManeuverThruster from assembly");
                        ManeuverThrusterMod ManeuverThrusterModHandler = (ManeuverThrusterMod)Activator.CreateInstance(t);
                        if (GetManeuverThrusterIDFromName(ManeuverThrusterModHandler.Name) == -1)
                        {
                            ManeuverThrusterTypes.Add(ManeuverThrusterModHandler);
                            Logger.Info($"Added ManeuverThruster: '{ManeuverThrusterModHandler.Name}' with ID '{GetManeuverThrusterIDFromName(ManeuverThrusterModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add ManeuverThruster from {mod.Name} with the duplicate name of '{ManeuverThrusterModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds ManeuverThruster type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find ManeuverThruster.
        /// </summary>
        /// <param name="ManeuverThrusterName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetManeuverThrusterIDFromName(string ManeuverThrusterName)
        {
            for (int i = 0; i < ManeuverThrusterTypes.Count; i++)
            {
                if (ManeuverThrusterTypes[i].Name == ManeuverThrusterName)
                {
                    return i + VanillaManeuverThrusterMaxType;
                }
            }
            return -1;
        }
        public static PLManeuverThruster CreateManeuverThruster(int Subtype, int level)
        {
            PLManeuverThruster InManeuverThruster;
            if (Subtype >= Instance.VanillaManeuverThrusterMaxType)
            {
                InManeuverThruster = new PLManeuverThruster(EManeuverThrusterType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaManeuverThrusterMaxType;
                if (subtypeformodded <= Instance.ManeuverThrusterTypes.Count && subtypeformodded > -1)
                {
                    ManeuverThrusterMod ManeuverThrusterType = Instance.ManeuverThrusterTypes[Subtype - Instance.VanillaManeuverThrusterMaxType];
                    InManeuverThruster.SubType = Subtype;
                    InManeuverThruster.Name = ManeuverThrusterType.Name;
                    InManeuverThruster.Desc = ManeuverThrusterType.Description;
                    InManeuverThruster.m_IconTexture = ManeuverThrusterType.IconTexture;
                    InManeuverThruster.m_MaxOutput = ManeuverThrusterType.MaxOutput;
                    InManeuverThruster.m_BaseMaxPower = ManeuverThrusterType.MaxPowerUsage_Watts;
                    InManeuverThruster.m_MarketPrice = ManeuverThrusterType.MarketPrice;
                    InManeuverThruster.CargoVisualPrefabID = ManeuverThrusterType.CargoVisualID;
                    InManeuverThruster.CanBeDroppedOnShipDeath = ManeuverThrusterType.CanBeDroppedOnShipDeath;
                    InManeuverThruster.Experimental = ManeuverThrusterType.Experimental;
                    InManeuverThruster.Unstable = ManeuverThrusterType.Unstable;
                    InManeuverThruster.Contraband = ManeuverThrusterType.Contraband;
                    InManeuverThruster.UpdateMaxPowerWatts();
                    InManeuverThruster.Price_LevelMultiplierExponent = ManeuverThrusterType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InManeuverThruster = new PLManeuverThruster((EManeuverThrusterType)Subtype, level);
            }
            return InManeuverThruster;
        }
    }
    //Converts hashes to ManeuverThrusters.
    [HarmonyPatch(typeof(PLManeuverThruster), "CreateManeuverThrusterFromHash")]
    class ManeuverThrusterHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = ManeuverThrusterModManager.CreateManeuverThruster(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLManeuverThruster), "Tick")]
    class TickPatch
    {
        static void Postfix(PLInertiaThruster __instance)
        {
            int subtypeformodded = __instance.SubType - ManeuverThrusterModManager.Instance.VanillaManeuverThrusterMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ManeuverThrusterModManager.Instance.ManeuverThrusterTypes.Count && __instance.ShipStats != null)
            {
                ManeuverThrusterModManager.Instance.ManeuverThrusterTypes[subtypeformodded].Tick(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLManeuverThruster), "GetStatLineLeft")]
    class LeftDescFix
    {
        static void Postfix(PLManeuverThruster __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - ManeuverThrusterModManager.Instance.VanillaManeuverThrusterMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ManeuverThrusterModManager.Instance.ManeuverThrusterTypes.Count && __instance.ShipStats != null)
            {
                __result = ManeuverThrusterModManager.Instance.ManeuverThrusterTypes[subtypeformodded].GetStatLineLeft(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLManeuverThruster), "GetStatLineRight")]
    class RightDescFix
    {
        static void Postfix(PLManeuverThruster __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - ManeuverThrusterModManager.Instance.VanillaManeuverThrusterMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ManeuverThrusterModManager.Instance.ManeuverThrusterTypes.Count && __instance.ShipStats != null)
            {
                __result = ManeuverThrusterModManager.Instance.ManeuverThrusterTypes[subtypeformodded].GetStatLineRight(__instance);
            }
        }
    }
}

using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PulsarModLoader.Content.Components.Reactor
{
    public class ReactorModManager
    {
        public readonly int VanillaReactorMaxType = 0;
        private static ReactorModManager m_instance = null;
        public readonly List<ReactorMod> ReactorTypes = new List<ReactorMod>();
        public static ReactorModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ReactorModManager();
                }
                return m_instance;
            }
        }

        ReactorModManager()
        {
            VanillaReactorMaxType = Enum.GetValues(typeof(EReactorType)).Length;
            Logger.Info($"MaxTypeint = {VanillaReactorMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type ReactorMod = typeof(ReactorMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (ReactorMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading reactor from assembly");
                        ReactorMod ReactorModHandler = (ReactorMod)Activator.CreateInstance(t);
                        if (GetReactorIDFromName(ReactorModHandler.Name) == -1)
                        {
                            ReactorTypes.Add(ReactorModHandler);
                            Logger.Info($"Added reactor: '{ReactorModHandler.Name}' with ID '{GetReactorIDFromName(ReactorModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add reactor from {mod.Name} with the duplicate name of '{ReactorModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds reactor type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find reactor.
        /// </summary>
        /// <param name="ReactorName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetReactorIDFromName(string ReactorName)
        {
            for (int i = 0; i < ReactorTypes.Count; i++)
            {
                if (ReactorTypes[i].Name == ReactorName)
                {
                    return i + VanillaReactorMaxType;
                }
            }
            return -1;
        }
        public static PLReactor CreateReactor(int Subtype, int level)
        {
            PLReactor InReactor;
            if (Subtype >= Instance.VanillaReactorMaxType)
            {
                InReactor = new PLReactor(EReactorType.E_REAC_ID_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaReactorMaxType;
                if (subtypeformodded <= Instance.ReactorTypes.Count && subtypeformodded > -1)
                {
                    ReactorMod ReactorType = Instance.ReactorTypes[Subtype - Instance.VanillaReactorMaxType];
                    InReactor.SubType = Subtype;
                    InReactor.Name = ReactorType.Name;
                    InReactor.Desc = ReactorType.Description;
                    InReactor.m_IconTexture = ReactorType.IconTexture;
                    InReactor.EnergyOutputMax = ReactorType.EnergyOutputMax;
                    InReactor.EnergySignatureAmt = ReactorType.EnergySignatureAmount;
                    InReactor.TempMax = ReactorType.MaxTemp;
                    InReactor.EmergencyCooldownTime = ReactorType.EmergencyCooldownTime;
                    InReactor.HeatOutput = ReactorType.HeatOutput;
                    InReactor.m_MarketPrice = ReactorType.MarketPrice;
                    InReactor.CargoVisualPrefabID = ReactorType.CargoVisualID;
                    InReactor.CanBeDroppedOnShipDeath = ReactorType.CanBeDroppedOnShipDeath;
                    InReactor.Experimental = ReactorType.Experimental;
                    InReactor.Unstable = ReactorType.Unstable;
                    InReactor.Contraband = ReactorType.Contraband;
                    InReactor.OriginalEnergyOutputMax = InReactor.EnergyOutputMax;
                    InReactor.Price_LevelMultiplierExponent = ReactorType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InReactor = new PLReactor((EReactorType)Subtype, level);
            }
            return InReactor;
        }
    }
    //Converts hashes to reactors.
    [HarmonyPatch(typeof(PLReactor), "CreateReactorFromHash")]
    class HashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = ReactorModManager.CreateReactor(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLReactor), "Tick")]
    class TickPatch
    {
        static void Postfix(PLReactor __instance)
        {
            int subtypeformodded = __instance.SubType - ReactorModManager.Instance.VanillaReactorMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ReactorModManager.Instance.ReactorTypes.Count && __instance.ShipStats != null && __instance.ShipStats.ReactorTempMax != 0f)
            {
                ReactorModManager.Instance.ReactorTypes[subtypeformodded].Tick(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLReactor), "GetStatLineLeft")]
    class LeftDescFix
    {
        static void Postfix(PLReactor __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - ReactorModManager.Instance.VanillaReactorMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ReactorModManager.Instance.ReactorTypes.Count && __instance.ShipStats != null)
            {
                __result = ReactorModManager.Instance.ReactorTypes[subtypeformodded].GetStatLineLeft(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLReactor), "GetStatLineRight")]
    class RightDescFix
    {
        static void Postfix(PLReactor __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - ReactorModManager.Instance.VanillaReactorMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ReactorModManager.Instance.ReactorTypes.Count && __instance.ShipStats != null)
            {
                __result = ReactorModManager.Instance.ReactorTypes[subtypeformodded].GetStatLineRight(__instance);
            }
        }
    }
}

using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarPluginLoader;
using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PulsarPluginLoader.ContentHelpers.Components.PolytechModule
{
    public class PolytechModulePluginManager
    {
        public readonly int VanillaPolytechModuleMaxType = 0;
        private static PolytechModulePluginManager m_instance = null;
        public readonly List<PolytechModulePlugin> PolytechModuleTypes = new List<PolytechModulePlugin>();
        public static PolytechModulePluginManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new PolytechModulePluginManager();
                }
                return m_instance;
            }
        }

        PolytechModulePluginManager()
        {
            VanillaPolytechModuleMaxType = Enum.GetValues(typeof(EPolytechModuleType)).Length;
            Logger.Info($"MaxTypeint = {VanillaPolytechModuleMaxType - 1}");
            foreach (PulsarPlugin plugin in PluginManager.Instance.GetAllPlugins())
            {
                Assembly asm = plugin.GetType().Assembly;
                Type PolytechModulePlugin = typeof(PolytechModulePlugin);
                foreach (Type t in asm.GetTypes())
                {
                    if (PolytechModulePlugin.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading PolytechModule from assembly");
                        PolytechModulePlugin PolytechModulePluginHandler = (PolytechModulePlugin)Activator.CreateInstance(t);
                        if (GetPolytechModuleIDFromName(PolytechModulePluginHandler.Name) == -1)
                        {
                            PolytechModuleTypes.Add(PolytechModulePluginHandler);
                            Logger.Info($"Added PolytechModule: '{PolytechModulePluginHandler.Name}' with ID '{GetPolytechModuleIDFromName(PolytechModulePluginHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add PolytechModule from {plugin.Name} with the duplicate name of '{PolytechModulePluginHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds PolytechModule type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find PolytechModule.
        /// </summary>
        /// <param name="PolytechModuleName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetPolytechModuleIDFromName(string PolytechModuleName)
        {
            for (int i = 0; i < PolytechModuleTypes.Count; i++)
            {
                if (PolytechModuleTypes[i].Name == PolytechModuleName)
                {
                    return i + VanillaPolytechModuleMaxType;
                }
            }
            return -1;
        }
        public static PLPolytechModule CreatePolytechModule(int Subtype, int level)
        {
            PLPolytechModule InPolytechModule;
            if (Subtype >= Instance.VanillaPolytechModuleMaxType)
            {
                InPolytechModule = new PLPolytechModule(EPolytechModuleType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaPolytechModuleMaxType;
                /*if (Global.DebugLogging)
                {
                    Logger.Info($"Subtype for modded is {subtypeformodded}");
                }*/
                if (subtypeformodded <= Instance.PolytechModuleTypes.Count && subtypeformodded > -1)
                {
                    /*if (Global.DebugLogging)
                    {
                        Logger.Info("Creating PolytechModule from list info");
                    }*/
                    PolytechModulePlugin PolytechModuleType = Instance.PolytechModuleTypes[Subtype - Instance.VanillaPolytechModuleMaxType];
                    InPolytechModule.SubType = Subtype;
                    InPolytechModule.Name = PolytechModuleType.Name;
                    InPolytechModule.Desc = PolytechModuleType.Description;
                    InPolytechModule.GetType().GetField("m_IconTexture", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InPolytechModule, PolytechModuleType.IconTexture);
                    InPolytechModule.GetType().GetField("m_MarketPrice", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InPolytechModule, (ObscuredInt)PolytechModuleType.MarketPrice);
                    InPolytechModule.CargoVisualPrefabID = PolytechModuleType.CargoVisualID;
                    InPolytechModule.CanBeDroppedOnShipDeath = PolytechModuleType.CanBeDroppedOnShipDeath;
                    InPolytechModule.Experimental = PolytechModuleType.Experimental;
                    InPolytechModule.Unstable = PolytechModuleType.Unstable;
                    InPolytechModule.Contraband = PolytechModuleType.Contraband;
                    InPolytechModule.GetType().GetField("Price_LevelMultiplierExponent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InPolytechModule, (ObscuredFloat)PolytechModuleType.Price_LevelMultiplierExponent);
                }
            }
            else
            {
                InPolytechModule = new PLPolytechModule((EPolytechModuleType)Subtype, level);
            }
            return InPolytechModule;
        }
    }
    //Converts hashes to PolytechModules.
    [HarmonyPatch(typeof(PLPolytechModule), "CreatePolytechModuleFromHash")]
    class HashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = PolytechModulePluginManager.CreatePolytechModule(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLPolytechModule), "Tick")]
    class TickPatch
    {
        static void Postfix(PLPolytechModule __instance)
        {
            int subtypeformodded = __instance.SubType - PolytechModulePluginManager.Instance.VanillaPolytechModuleMaxType;
            if (subtypeformodded > -1 && subtypeformodded < PolytechModulePluginManager.Instance.PolytechModuleTypes.Count && __instance.ShipStats != null && __instance.IsEquipped)
            {
                PolytechModulePluginManager.Instance.PolytechModuleTypes[subtypeformodded].Tick(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLPolytechModule), "FinalLateAddStats")]
    class FinalLateAddStatsPatch
    {
        static void Postfix(PLPolytechModule __instance)
        {
            int subtypeformodded = __instance.SubType - PolytechModulePluginManager.Instance.VanillaPolytechModuleMaxType;
            if (subtypeformodded > -1 && subtypeformodded < PolytechModulePluginManager.Instance.PolytechModuleTypes.Count && __instance.ShipStats != null)
            {
                PolytechModulePluginManager.Instance.PolytechModuleTypes[subtypeformodded].FinalLateAddStats(__instance);
            }
        }
    }
}

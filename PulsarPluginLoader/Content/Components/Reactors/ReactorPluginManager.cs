using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarPluginLoader;
using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PulsarPluginLoader.Content.Components.Reactor
{
    public class ReactorPluginManager
    {
        public readonly int VanillaReactorMaxType = 0;
        private static ReactorPluginManager m_instance = null;
        public readonly List<ReactorPlugin> ReactorTypes = new List<ReactorPlugin>();
        public static ReactorPluginManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ReactorPluginManager();
                }
                return m_instance;
            }
        }

        ReactorPluginManager()
        {
            VanillaReactorMaxType = Enum.GetValues(typeof(EReactorType)).Length;
            Logger.Info($"MaxTypeint = {VanillaReactorMaxType - 1}");
            foreach (PulsarPlugin plugin in PluginManager.Instance.GetAllPlugins())
            {
                Assembly asm = plugin.GetType().Assembly;
                Type ReactorPlugin = typeof(ReactorPlugin);
                foreach (Type t in asm.GetTypes())
                {
                    if (ReactorPlugin.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading reactor from assembly");
                        ReactorPlugin ReactorPluginHandler = (ReactorPlugin)Activator.CreateInstance(t);
                        if (GetReactorIDFromName(ReactorPluginHandler.Name) == -1)
                        {
                            ReactorTypes.Add(ReactorPluginHandler);
                            Logger.Info($"Added reactor: '{ReactorPluginHandler.Name}' with ID '{GetReactorIDFromName(ReactorPluginHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add reactor from {plugin.Name} with the duplicate name of '{ReactorPluginHandler.Name}'");
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
                /*if (Global.DebugLogging)
                {
                    Logger.Info($"Subtype for modded is {subtypeformodded}");
                }*/
                if (subtypeformodded <= Instance.ReactorTypes.Count && subtypeformodded > -1)
                {
                    /*if (Global.DebugLogging)
                    {
                        Logger.Info("Creating reactor from list info");
                    }*/
                    ReactorPlugin ReactorType = Instance.ReactorTypes[Subtype - Instance.VanillaReactorMaxType];
                    InReactor.SubType = Subtype;
                    InReactor.Name = ReactorType.Name;
                    InReactor.Desc = ReactorType.Description;
                    InReactor.GetType().GetField("m_IconTexture", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InReactor, ReactorType.IconTexture);
                    InReactor.EnergyOutputMax = ReactorType.EnergyOutputMax;
                    InReactor.EnergySignatureAmt = ReactorType.EnergySignatureAmount;
                    InReactor.TempMax = ReactorType.MaxTemp;
                    InReactor.EmergencyCooldownTime = ReactorType.EmergencyCooldownTime;
                    InReactor.HeatOutput = ReactorType.HeatOutput;
                    InReactor.GetType().GetField("m_MarketPrice", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InReactor, (ObscuredInt)ReactorType.MarketPrice);
                    InReactor.CargoVisualPrefabID = ReactorType.CargoVisualID;
                    InReactor.CanBeDroppedOnShipDeath = ReactorType.CanBeDroppedOnShipDeath;
                    InReactor.Experimental = ReactorType.Experimental;
                    InReactor.Unstable = ReactorType.Unstable;
                    InReactor.Contraband = ReactorType.Contraband;
                    InReactor.GetType().GetField("OriginalEnergyOutputMax", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InReactor, InReactor.EnergyOutputMax);
                    InReactor.GetType().GetField("Price_LevelMultiplierExponent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InReactor, ReactorType.Price_LevelMultiplierExponent);
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
            __result = ReactorPluginManager.CreateReactor(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLReactor), "Tick")]
    class TickPatch
    {
        static void Postfix(PLReactor __instance)
        {
            int subtypeformodded = __instance.SubType - ReactorPluginManager.Instance.VanillaReactorMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ReactorPluginManager.Instance.ReactorTypes.Count && __instance.ShipStats != null && __instance.ShipStats.ReactorTempMax != 0f)
            {
                ReactorPluginManager.Instance.ReactorTypes[subtypeformodded].Tick(__instance);
            }
        }
    }
}

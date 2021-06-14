using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarPluginLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using Logger = PulsarPluginLoader.Utilities.Logger;

namespace PulsarPluginLoader.ContentHelpers.Components.CaptainsChair
{
    public class CaptainsChairPluginManager
    {
        public readonly int VanillaCaptainsChairMaxType = 0;
        private static CaptainsChairPluginManager m_instance = null;
        public readonly List<CaptainsChairPlugin> CaptainsChairTypes = new List<CaptainsChairPlugin>();
        public static CaptainsChairPluginManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new CaptainsChairPluginManager();
                }
                return m_instance;
            }
        }

        CaptainsChairPluginManager()
        {
            VanillaCaptainsChairMaxType = Enum.GetValues(typeof(ECaptainsChairType)).Length;
            Logger.Info($"MaxTypeint = {VanillaCaptainsChairMaxType - 1}");
            foreach (PulsarPlugin plugin in PluginManager.Instance.GetAllPlugins())
            {
                Assembly asm = plugin.GetType().Assembly;
                Type CaptainsChairPlugin = typeof(CaptainsChairPlugin);
                foreach (Type t in asm.GetTypes())
                {
                    if (CaptainsChairPlugin.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading CaptainsChair from assembly");
                        CaptainsChairPlugin CaptainsChairPluginHandler = (CaptainsChairPlugin)Activator.CreateInstance(t);
                        if (GetCaptainsChairIDFromName(CaptainsChairPluginHandler.Name) == -1)
                        {
                            CaptainsChairTypes.Add(CaptainsChairPluginHandler);
                            Logger.Info($"Added CaptainsChair: '{CaptainsChairPluginHandler.Name}' with ID '{GetCaptainsChairIDFromName(CaptainsChairPluginHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add CaptainsChair from {plugin.Name} with the duplicate name of '{CaptainsChairPluginHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds CaptainsChair type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find CaptainsChair.
        /// </summary>
        /// <param name="CaptainsChairName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetCaptainsChairIDFromName(string CaptainsChairName)
        {
            for (int i = 0; i < CaptainsChairTypes.Count; i++)
            {
                if (CaptainsChairTypes[i].Name == CaptainsChairName)
                {
                    return i + VanillaCaptainsChairMaxType;
                }
            }
            return -1;
        }
        public static PLCaptainsChair CreateCaptainsChair(int Subtype, int level)
        {
            PLCaptainsChair InCaptainsChair;
            if (Subtype >= Instance.VanillaCaptainsChairMaxType)
            {
                InCaptainsChair = new PLCaptainsChair(ECaptainsChairType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaCaptainsChairMaxType;
                /*if (Global.DebugLogging)
                {
                    Logger.Info($"Subtype for modded is {subtypeformodded}");
                }*/
                if (subtypeformodded <= Instance.CaptainsChairTypes.Count && subtypeformodded > -1)
                {
                    /*if (Global.DebugLogging)
                    {
                        Logger.Info("Creating CaptainsChair from list info");
                    }*/
                    CaptainsChairPlugin CaptainsChairType = Instance.CaptainsChairTypes[Subtype - Instance.VanillaCaptainsChairMaxType];
                    InCaptainsChair.SubType = Subtype;
                    InCaptainsChair.Name = CaptainsChairType.Name;
                    InCaptainsChair.Desc = CaptainsChairType.Description;
                    InCaptainsChair.GetType().GetField("m_IconTexture", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InCaptainsChair, CaptainsChairType.IconTexture);
                    InCaptainsChair.GetType().GetField("m_MarketPrice", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InCaptainsChair, (ObscuredInt)CaptainsChairType.MarketPrice);
                    InCaptainsChair.CargoVisualPrefabID = CaptainsChairType.CargoVisualID;
                    InCaptainsChair.CanBeDroppedOnShipDeath = CaptainsChairType.CanBeDroppedOnShipDeath;
                    InCaptainsChair.Experimental = CaptainsChairType.Experimental;
                    InCaptainsChair.Unstable = CaptainsChairType.Unstable;
                    InCaptainsChair.Contraband = CaptainsChairType.Contraband;
                    InCaptainsChair.GetType().GetField("Price_LevelMultiplierExponent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InCaptainsChair, (ObscuredFloat)CaptainsChairType.Price_LevelMultiplierExponent);
                }
            }
            else
            {
                InCaptainsChair = new PLCaptainsChair((ECaptainsChairType)Subtype, level);
            }
            return InCaptainsChair;
        }
    }
    //Converts hashes to CaptainsChairs.
    [HarmonyPatch(typeof(PLCaptainsChair), "CreateCaptainsChairFromHash")]
    class CaptainsChairHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = CaptainsChairPluginManager.CreateCaptainsChair(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLCaptainsChair), "LateAddStats")]
    class CaptainsChairLateAddStatsPatch
    {
        static void Postfix(PLShipStats inStats, PLCaptainsChair __instance)
        {
            int subtypeformodded = __instance.SubType - CaptainsChairPluginManager.Instance.VanillaCaptainsChairMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CaptainsChairPluginManager.Instance.CaptainsChairTypes.Count && inStats != null)
            {
                CaptainsChairPluginManager.Instance.CaptainsChairTypes[subtypeformodded].LateAddStats(__instance);
            }
        }
    }
}

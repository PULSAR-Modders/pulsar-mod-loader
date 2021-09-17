using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using Logger = PulsarModLoader.Utilities.Logger;

namespace PulsarModLoader.Content.Components.CPU
{
    public class CPUPluginManager
    {
        public readonly int VanillaCPUMaxType = 0;
        private static CPUPluginManager m_instance = null;
        public readonly List<CPUPlugin> CPUTypes = new List<CPUPlugin>();
        public static CPUPluginManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new CPUPluginManager();
                }
                return m_instance;
            }
        }

        CPUPluginManager()
        {
            VanillaCPUMaxType = Enum.GetValues(typeof(ECPUClass)).Length;
            Logger.Info($"MaxTypeint = {VanillaCPUMaxType - 1}");
            foreach (PulsarMod plugin in PluginManager.Instance.GetAllPlugins())
            {
                Assembly asm = plugin.GetType().Assembly;
                Type CPUPlugin = typeof(CPUPlugin);
                foreach (Type t in asm.GetTypes())
                {
                    if (CPUPlugin.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading CPU from assembly");
                        CPUPlugin CPUPluginHandler = (CPUPlugin)Activator.CreateInstance(t);
                        if (GetCPUIDFromName(CPUPluginHandler.Name) == -1)
                        {
                            CPUTypes.Add(CPUPluginHandler);
                            Logger.Info($"Added CPU: '{CPUPluginHandler.Name}' with ID '{GetCPUIDFromName(CPUPluginHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add CPU from {plugin.Name} with the duplicate name of '{CPUPluginHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds CPU type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find CPU.
        /// </summary>
        /// <param name="CPUName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetCPUIDFromName(string CPUName)
        {
            for (int i = 0; i < CPUTypes.Count; i++)
            {
                if (CPUTypes[i].Name == CPUName)
                {
                    return i + VanillaCPUMaxType;
                }
            }
            return -1;
        }
        public static PLCPU CreateCPU(int Subtype, int level)
        {
            PLCPU InCPU;
            if (Subtype >= Instance.VanillaCPUMaxType)
            {
                InCPU = new PLCPU(ECPUClass.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaCPUMaxType;
                if (subtypeformodded <= Instance.CPUTypes.Count && subtypeformodded > -1)
                {
                    CPUPlugin CPUType = Instance.CPUTypes[Subtype - Instance.VanillaCPUMaxType];
                    InCPU.SubType = Subtype;
                    InCPU.Name = CPUType.Name;
                    InCPU.Desc = CPUType.Description;
                    InCPU.GetType().GetField("m_IconTexture", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InCPU, CPUType.IconTexture);
                    InCPU.GetType().GetField("m_MarketPrice", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InCPU, (ObscuredInt)CPUType.MarketPrice);
                    InCPU.GetType().GetField("m_MaxPowerUsage_Watts", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InCPU, (CPUType.MaxPowerUsage_Watts));
                    InCPU.CargoVisualPrefabID = CPUType.CargoVisualID;
                    InCPU.CanBeDroppedOnShipDeath = CPUType.CanBeDroppedOnShipDeath;
                    InCPU.Experimental = CPUType.Experimental;
                    InCPU.Unstable = CPUType.Unstable;
                    InCPU.Contraband = CPUType.Contraband;
                    InCPU.Speed = CPUType.Speed;
                    InCPU.GetType().GetField("m_Defense", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InCPU, CPUType.Defense);
                    InCPU.GetType().GetField("m_MaxCompUpgradeLevelBoost", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InCPU, CPUType.MaxCompUpgradeLevelBoost);
                    InCPU.GetType().GetField("m_MaxPawnItemUpgradeLevelBoost", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InCPU, CPUType.MaxItemUpgradeLevelBoost);
                    InCPU.GetType().GetField("Price_LevelMultiplierExponent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InCPU, CPUType.Price_LevelMultiplierExponent);
                }
            }
            else
            {
                InCPU = new PLCPU((ECPUClass)Subtype, level);
            }
            return InCPU;
        }
    }
    //Converts hashes to CPUs.
    [HarmonyPatch(typeof(PLCPU), "CreateCPUFromHash")]
    class CPUHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = CPUPluginManager.CreateCPU(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLCPU), "FinalLateAddStats")]
    class CPUFinalLateAddStatsPatch
    {
        static void Postfix(PLCPU __instance)
        {
            int subtypeformodded = __instance.SubType - CPUPluginManager.Instance.VanillaCPUMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CPUPluginManager.Instance.CPUTypes.Count)
            {
                CPUPluginManager.Instance.CPUTypes[subtypeformodded].FinalLateAddStats(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLCPU), "WhenProgramIsRun")]
    class CPWhenProgramIsRunPatch
    {
        static void Postfix(PLWarpDriveProgram inProgram, PLCPU __instance)
        {
            int subtypeformodded = __instance.SubType - CPUPluginManager.Instance.VanillaCPUMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CPUPluginManager.Instance.CPUTypes.Count && inProgram != null)
            {
                CPUPluginManager.Instance.CPUTypes[subtypeformodded].WhenProgramIsRun(inProgram);
            }
        }
    }
    [HarmonyPatch(typeof(PLCPU), "AddStats")]
    class CPUAddStatsPatch
    {
        static void Postfix(PLCPU __instance)
        {
            int subtypeformodded = __instance.SubType - CPUPluginManager.Instance.VanillaCPUMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CPUPluginManager.Instance.CPUTypes.Count)
            {
                CPUPluginManager.Instance.CPUTypes[subtypeformodded].AddStats(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLCPU), "Tick")]
    class CPUTickPatch
    {
        static void Postfix(PLCPU __instance)
        {
            int subtypeformodded = __instance.SubType - CPUPluginManager.Instance.VanillaCPUMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CPUPluginManager.Instance.CPUTypes.Count)
            {
                CPUPluginManager.Instance.CPUTypes[subtypeformodded].Tick(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLCPU), "GetStatLineRight")]
    class CPUGetStatLineRightPatch
    {
        static void Postfix(PLCPU __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - CPUPluginManager.Instance.VanillaCPUMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CPUPluginManager.Instance.CPUTypes.Count)
            {
                __result = CPUPluginManager.Instance.CPUTypes[subtypeformodded].GetStatLineRight(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLCPU), "GetStatLineLeft")]
    class CPUGetStatLineLeftPatch
    {
        static void Postfix(PLCPU __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - CPUPluginManager.Instance.VanillaCPUMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CPUPluginManager.Instance.CPUTypes.Count)
            {
                __result = CPUPluginManager.Instance.CPUTypes[subtypeformodded].GetStatLineLeft(__instance);
            }
        }
    }
}

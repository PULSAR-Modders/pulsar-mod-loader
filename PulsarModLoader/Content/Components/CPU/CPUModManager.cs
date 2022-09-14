using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.CPU
{
    public class CPUModManager
    {
        public readonly int VanillaCPUMaxType = 0;
        private static CPUModManager m_instance = null;
        public readonly List<CPUMod> CPUTypes = new List<CPUMod>();
        public static CPUModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new CPUModManager();
                }
                return m_instance;
            }
        }

        CPUModManager()
        {
            VanillaCPUMaxType = Enum.GetValues(typeof(ECPUClass)).Length;
            Logger.Info($"MaxTypeint = {VanillaCPUMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type CPUMod = typeof(CPUMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (CPUMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading CPU from assembly");
                        CPUMod CPUModHandler = (CPUMod)Activator.CreateInstance(t);
                        if (GetCPUIDFromName(CPUModHandler.Name) == -1)
                        {
                            CPUTypes.Add(CPUModHandler);
                            Logger.Info($"Added CPU: '{CPUModHandler.Name}' with ID '{GetCPUIDFromName(CPUModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add CPU from {mod.Name} with the duplicate name of '{CPUModHandler.Name}'");
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
                    CPUMod CPUType = Instance.CPUTypes[Subtype - Instance.VanillaCPUMaxType];
                    InCPU.SubType = Subtype;
                    InCPU.Name = CPUType.Name;
                    InCPU.Desc = CPUType.Description;
                    InCPU.m_IconTexture = CPUType.IconTexture;
                    InCPU.m_MarketPrice = CPUType.MarketPrice;
                    InCPU.m_MaxPowerUsage_Watts = CPUType.MaxPowerUsage_Watts;
                    InCPU.CargoVisualPrefabID = CPUType.CargoVisualID;
                    InCPU.CanBeDroppedOnShipDeath = CPUType.CanBeDroppedOnShipDeath;
                    InCPU.Experimental = CPUType.Experimental;
                    InCPU.Unstable = CPUType.Unstable;
                    InCPU.Contraband = CPUType.Contraband;
                    InCPU.Speed = CPUType.Speed;
                    InCPU.m_Defense = CPUType.Defense;
                    InCPU.m_MaxCompUpgradeLevelBoost = CPUType.MaxCompUpgradeLevelBoost;
                    InCPU.m_MaxPawnItemUpgradeLevelBoost = CPUType.MaxItemUpgradeLevelBoost;
                    InCPU.Price_LevelMultiplierExponent = CPUType.Price_LevelMultiplierExponent;
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
            __result = CPUModManager.CreateCPU(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLCPU), "FinalLateAddStats")]
    class CPUFinalLateAddStatsPatch
    {
        static void Postfix(PLCPU __instance)
        {
            int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaCPUMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.CPUTypes.Count)
            {
                CPUModManager.Instance.CPUTypes[subtypeformodded].FinalLateAddStats(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLCPU), "WhenProgramIsRun")]
    class CPWhenProgramIsRunPatch
    {
        static void Postfix(PLWarpDriveProgram inProgram, PLCPU __instance)
        {
            int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaCPUMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.CPUTypes.Count && inProgram != null)
            {
                CPUModManager.Instance.CPUTypes[subtypeformodded].WhenProgramIsRun(inProgram);
            }
        }
    }
    [HarmonyPatch(typeof(PLCPU), "AddStats")]
    class CPUAddStatsPatch
    {
        static void Postfix(PLCPU __instance)
        {
            int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaCPUMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.CPUTypes.Count)
            {
                CPUModManager.Instance.CPUTypes[subtypeformodded].AddStats(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "LateAddStats")]
    class CPULateAddStatsPatch
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance)
        {
            if(__instance is PLCPU)
            {
                int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaCPUMaxType;
                if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.CPUTypes.Count)
                {
                    CPUModManager.Instance.CPUTypes[subtypeformodded].LateAddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLCPU), "Tick")]
    class CPUTickPatch
    {
        static void Postfix(PLCPU __instance)
        {
            int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaCPUMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.CPUTypes.Count)
            {
                CPUModManager.Instance.CPUTypes[subtypeformodded].Tick(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLCPU), "OnWarp")]
    class CPUOnWarpPatch
    {
        static void Postfix(PLCPU __instance)
        {
            int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaCPUMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.CPUTypes.Count)
            {
                CPUModManager.Instance.CPUTypes[subtypeformodded].OnWarp(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLCPU), "GetStatLineRight")]
    class CPUGetStatLineRightPatch
    {
        static void Postfix(PLCPU __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaCPUMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.CPUTypes.Count)
            {
                __result = CPUModManager.Instance.CPUTypes[subtypeformodded].GetStatLineRight(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLCPU), "GetStatLineLeft")]
    class CPUGetStatLineLeftPatch
    {
        static void Postfix(PLCPU __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaCPUMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.CPUTypes.Count)
            {
                __result = CPUModManager.Instance.CPUTypes[subtypeformodded].GetStatLineLeft(__instance);
            }
        }
    }
}

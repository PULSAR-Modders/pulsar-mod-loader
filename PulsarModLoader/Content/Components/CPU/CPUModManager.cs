using HarmonyLib;

namespace PulsarModLoader.Content.Components.CPU
{
    /// <summary>
    /// Manages Modded CPUs
    /// </summary>
    public class CPUModManager : ComponentModManager<CPUMod, ECPUClass>
    {
        private static CPUModManager m_instance = null;

        /// <summary>
        /// Static Manager Instance
        /// </summary>
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

        CPUModManager() {}

        /// <summary>
        /// Creates a CPU based on input parameters.
        /// </summary>
        /// <param name="Subtype"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static PLCPU CreateCPU(int Subtype, int level)
        {
            PLCPU InCPU;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InCPU = new PLCPU(ECPUClass.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    CPUMod CPUType = Instance.types[Subtype - Instance.VanillaMaxType];
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
                int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.types.Count)
                {
                    CPUModManager.Instance.types[subtypeformodded].FinalLateAddStats(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLCPU), "WhenProgramIsRun")]
        class CPWhenProgramIsRunPatch
        {
            static void Postfix(PLWarpDriveProgram inProgram, PLCPU __instance)
            {
                int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.types.Count && inProgram != null)
                {
                    CPUModManager.Instance.types[subtypeformodded].WhenProgramIsRun(inProgram);
                }
            }
        }
        [HarmonyPatch(typeof(PLCPU), "AddStats")]
        class CPUAddStatsPatch
        {
            static void Postfix(PLCPU __instance)
            {
                int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.types.Count)
                {
                    CPUModManager.Instance.types[subtypeformodded].AddStats(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLCPU), "Tick")]
        class CPUTickPatch
        {
            static void Postfix(PLCPU __instance)
            {
                int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.types.Count)
                {
                    CPUModManager.Instance.types[subtypeformodded].Tick(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLCPU), "GetStatLineRight")]
        class CPUGetStatLineRightPatch
        {
            static void Postfix(PLCPU __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.types.Count)
                {
                    __result = CPUModManager.Instance.types[subtypeformodded].GetStatLineRight(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLCPU), "GetStatLineLeft")]
        class CPUGetStatLineLeftPatch
        {
            static void Postfix(PLCPU __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - CPUModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < CPUModManager.Instance.types.Count)
                {
                    __result = CPUModManager.Instance.types[subtypeformodded].GetStatLineLeft(__instance);
                }
            }
        }
    }
}

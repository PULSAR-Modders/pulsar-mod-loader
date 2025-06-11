using HarmonyLib;

namespace PulsarModLoader.Content.Components.PolytechModule
{
    public class PolytechModuleModManager : ComponentModManager<PolytechModuleMod, EPolytechModuleType>
    {
        private static PolytechModuleModManager m_instance = null;
        public static PolytechModuleModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new PolytechModuleModManager();
                }
                return m_instance;
            }
        }

        PolytechModuleModManager() {}

        public static PLPolytechModule CreatePolytechModule(int Subtype, int level)
        {
            PLPolytechModule InPolytechModule;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InPolytechModule = new PLPolytechModule(EPolytechModuleType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    PolytechModuleMod PolytechModuleType = Instance.types[Subtype - Instance.VanillaMaxType];
                    InPolytechModule.SubType = Subtype;
                    InPolytechModule.Name = PolytechModuleType.Name;
                    InPolytechModule.Desc = PolytechModuleType.Description;
                    InPolytechModule.m_IconTexture = PolytechModuleType.IconTexture;
                    InPolytechModule.m_MarketPrice = PolytechModuleType.MarketPrice;
                    InPolytechModule.CargoVisualPrefabID = PolytechModuleType.CargoVisualID;
                    InPolytechModule.CanBeDroppedOnShipDeath = PolytechModuleType.CanBeDroppedOnShipDeath;
                    InPolytechModule.Experimental = PolytechModuleType.Experimental;
                    InPolytechModule.Unstable = PolytechModuleType.Unstable;
                    InPolytechModule.Contraband = PolytechModuleType.Contraband;
                    InPolytechModule.Price_LevelMultiplierExponent = PolytechModuleType.Price_LevelMultiplierExponent;
                    InPolytechModule.m_MaxPowerUsage_Watts = PolytechModuleType.MaxPowerUsage_Watts;
                }
            }
            else
            {
                InPolytechModule = new PLPolytechModule((EPolytechModuleType)Subtype, level);
            }
            return InPolytechModule;
        }

        //Converts hashes to PolytechModules.
        [HarmonyPatch(typeof(PLPolytechModule), "CreatePolytechModuleFromHash")]
        class HashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                __result = PolytechModuleModManager.CreatePolytechModule(inSubType, inLevel);
                return false;
            }
        }
        [HarmonyPatch(typeof(PLPolytechModule), "Tick")]
        class TickPatch
        {
            static void Postfix(PLPolytechModule __instance)
            {
                int subtypeformodded = __instance.SubType - PolytechModuleModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < PolytechModuleModManager.Instance.types.Count && __instance.ShipStats != null && __instance.IsEquipped)
                {
                    PolytechModuleModManager.Instance.types[subtypeformodded].Tick(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLPolytechModule), "FinalLateAddStats")]
        class FinalLateAddStatsPatch
        {
            static void Postfix(PLPolytechModule __instance)
            {
                int subtypeformodded = __instance.SubType - PolytechModuleModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < PolytechModuleModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    PolytechModuleModManager.Instance.types[subtypeformodded].FinalLateAddStats(__instance);
                }
            }
        }
    }
}

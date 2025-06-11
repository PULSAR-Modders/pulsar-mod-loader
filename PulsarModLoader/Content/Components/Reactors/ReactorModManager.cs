using HarmonyLib;

namespace PulsarModLoader.Content.Components.Reactor
{
    public class ReactorModManager : ComponentModManager<ReactorMod, EReactorType>
    {
        private static ReactorModManager m_instance = null;
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

        ReactorModManager() {}

        public static PLReactor CreateReactor(int Subtype, int level)
        {
            PLReactor InReactor;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InReactor = new PLReactor(EReactorType.E_REAC_ID_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    ReactorMod ReactorType = Instance.types[Subtype - Instance.VanillaMaxType];
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
                int subtypeformodded = __instance.SubType - ReactorModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ReactorModManager.Instance.types.Count && __instance.ShipStats != null && __instance.ShipStats.ReactorTempMax != 0f)
                {
                    ReactorModManager.Instance.types[subtypeformodded].Tick(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLReactor), "GetStatLineLeft")]
        class LeftDescFix
        {
            static void Postfix(PLReactor __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - ReactorModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ReactorModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    __result = ReactorModManager.Instance.types[subtypeformodded].GetStatLineLeft(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLReactor), "GetStatLineRight")]
        class RightDescFix
        {
            static void Postfix(PLReactor __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - ReactorModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ReactorModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    __result = ReactorModManager.Instance.types[subtypeformodded].GetStatLineRight(__instance);
                }
            }
        }
    }
}

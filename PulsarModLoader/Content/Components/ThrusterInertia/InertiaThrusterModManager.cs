using HarmonyLib;

namespace PulsarModLoader.Content.Components.InertiaThruster
{
    public class InertiaThrusterModManager : ComponentModManager<InertiaThrusterMod, EInertiaThrusterType>
    {
        private static InertiaThrusterModManager m_instance = null;
        public static InertiaThrusterModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new InertiaThrusterModManager();
                }
                return m_instance;
            }
        }

        InertiaThrusterModManager() {}

        public static PLInertiaThruster CreateInertiaThruster(int Subtype, int level)
        {
            PLInertiaThruster InInertiaThruster;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InInertiaThruster = new PLInertiaThruster(EInertiaThrusterType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    InertiaThrusterMod InertiaThrusterType = Instance.types[Subtype - Instance.VanillaMaxType];
                    InInertiaThruster.SubType = Subtype;
                    InInertiaThruster.Name = InertiaThrusterType.Name;
                    InInertiaThruster.Desc = InertiaThrusterType.Description;
                    InInertiaThruster.m_IconTexture = InertiaThrusterType.IconTexture;
                    InInertiaThruster.m_MaxOutput = InertiaThrusterType.MaxOutput;
                    InInertiaThruster.m_BaseMaxPower = InertiaThrusterType.MaxPowerUsage_Watts;
                    InInertiaThruster.m_MarketPrice = InertiaThrusterType.MarketPrice;
                    InInertiaThruster.CargoVisualPrefabID = InertiaThrusterType.CargoVisualID;
                    InInertiaThruster.CanBeDroppedOnShipDeath = InertiaThrusterType.CanBeDroppedOnShipDeath;
                    InInertiaThruster.Experimental = InertiaThrusterType.Experimental;
                    InInertiaThruster.Unstable = InertiaThrusterType.Unstable;
                    InInertiaThruster.Contraband = InertiaThrusterType.Contraband;
                    InInertiaThruster.UpdateMaxPowerWatts();
                    InInertiaThruster.Price_LevelMultiplierExponent = InertiaThrusterType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InInertiaThruster = new PLInertiaThruster((EInertiaThrusterType)Subtype, level);
            }
            return InInertiaThruster;
        }

        //Converts hashes to InertiaThrusters.
        [HarmonyPatch(typeof(PLInertiaThruster), "CreateInertiaThrusterFromHash")]
        class InertiaThrusterHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                __result = InertiaThrusterModManager.CreateInertiaThruster(inSubType, inLevel);
                return false;
            }
        }

        [HarmonyPatch(typeof(PLInertiaThruster), "Tick")]
        class TickPatch
        {
            static void Postfix(PLInertiaThruster __instance)
            {
                int subtypeformodded = __instance.SubType - InertiaThrusterModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < InertiaThrusterModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    InertiaThrusterModManager.Instance.types[subtypeformodded].Tick(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLInertiaThruster), "GetStatLineLeft")]
        class LeftDescFix
        {
            static void Postfix(PLInertiaThruster __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - InertiaThrusterModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < InertiaThrusterModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    __result = InertiaThrusterModManager.Instance.types[subtypeformodded].GetStatLineLeft(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLInertiaThruster), "GetStatLineRight")]
        class RightDescFix
        {
            static void Postfix(PLInertiaThruster __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - InertiaThrusterModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < InertiaThrusterModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    __result = InertiaThrusterModManager.Instance.types[subtypeformodded].GetStatLineRight(__instance);
                }
            }
        }
    }
}

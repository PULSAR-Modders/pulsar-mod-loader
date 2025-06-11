using HarmonyLib;

namespace PulsarModLoader.Content.Components.ManeuverThruster
{
    public class ManeuverThrusterModManager : ComponentModManager<ManeuverThrusterMod, EManeuverThrusterType>
    {
        private static ManeuverThrusterModManager m_instance = null;
        public static ManeuverThrusterModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ManeuverThrusterModManager();
                }
                return m_instance;
            }
        }

        ManeuverThrusterModManager() { }

        public static PLManeuverThruster CreateManeuverThruster(int Subtype, int level)
        {
            PLManeuverThruster InManeuverThruster;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InManeuverThruster = new PLManeuverThruster(EManeuverThrusterType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    ManeuverThrusterMod ManeuverThrusterType = Instance.types[Subtype - Instance.VanillaMaxType];
                    InManeuverThruster.SubType = Subtype;
                    InManeuverThruster.Name = ManeuverThrusterType.Name;
                    InManeuverThruster.Desc = ManeuverThrusterType.Description;
                    InManeuverThruster.m_IconTexture = ManeuverThrusterType.IconTexture;
                    InManeuverThruster.m_MaxOutput = ManeuverThrusterType.MaxOutput;
                    InManeuverThruster.m_BaseMaxPower = ManeuverThrusterType.MaxPowerUsage_Watts;
                    InManeuverThruster.m_MarketPrice = ManeuverThrusterType.MarketPrice;
                    InManeuverThruster.CargoVisualPrefabID = ManeuverThrusterType.CargoVisualID;
                    InManeuverThruster.CanBeDroppedOnShipDeath = ManeuverThrusterType.CanBeDroppedOnShipDeath;
                    InManeuverThruster.Experimental = ManeuverThrusterType.Experimental;
                    InManeuverThruster.Unstable = ManeuverThrusterType.Unstable;
                    InManeuverThruster.Contraband = ManeuverThrusterType.Contraband;
                    InManeuverThruster.UpdateMaxPowerWatts();
                    InManeuverThruster.Price_LevelMultiplierExponent = ManeuverThrusterType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InManeuverThruster = new PLManeuverThruster((EManeuverThrusterType)Subtype, level);
            }
            return InManeuverThruster;
        }

        //Converts hashes to ManeuverThrusters.
        [HarmonyPatch(typeof(PLManeuverThruster), "CreateManeuverThrusterFromHash")]
        class ManeuverThrusterHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                __result = ManeuverThrusterModManager.CreateManeuverThruster(inSubType, inLevel);
                return false;
            }
        }
        [HarmonyPatch(typeof(PLManeuverThruster), "Tick")]
        class TickPatch
        {
            static void Postfix(PLInertiaThruster __instance)
            {
                int subtypeformodded = __instance.SubType - ManeuverThrusterModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ManeuverThrusterModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    ManeuverThrusterModManager.Instance.types[subtypeformodded].Tick(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLManeuverThruster), "GetStatLineLeft")]
        class LeftDescFix
        {
            static void Postfix(PLManeuverThruster __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - ManeuverThrusterModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ManeuverThrusterModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    __result = ManeuverThrusterModManager.Instance.types[subtypeformodded].GetStatLineLeft(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLManeuverThruster), "GetStatLineRight")]
        class RightDescFix
        {
            static void Postfix(PLManeuverThruster __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - ManeuverThrusterModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ManeuverThrusterModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    __result = ManeuverThrusterModManager.Instance.types[subtypeformodded].GetStatLineRight(__instance);
                }
            }
        }
    }
}

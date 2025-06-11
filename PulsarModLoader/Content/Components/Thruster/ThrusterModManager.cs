using HarmonyLib;

namespace PulsarModLoader.Content.Components.Thruster
{
    public class ThrusterModManager : ComponentModManager<ThrusterMod, EThrusterType>
    {
        private static ThrusterModManager m_instance = null;
        public static ThrusterModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ThrusterModManager();
                }
                return m_instance;
            }
        }

        ThrusterModManager() {}
        
        public static PLThruster CreateThruster(int Subtype, int level)
        {
            PLThruster InThruster;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InThruster = new PLThruster(EThrusterType.MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    ThrusterMod ThrusterType = Instance.types[Subtype - Instance.VanillaMaxType];
                    InThruster.SubType = Subtype;
                    InThruster.Name = ThrusterType.Name;
                    InThruster.Desc = ThrusterType.Description;
                    InThruster.m_IconTexture = ThrusterType.IconTexture;
                    InThruster.m_MaxOutput = ThrusterType.MaxOutput;
                    InThruster.m_BaseMaxPower = ThrusterType.MaxPowerUsage_Watts;
                    InThruster.m_MarketPrice = ThrusterType.MarketPrice;
                    InThruster.CargoVisualPrefabID = ThrusterType.CargoVisualID;
                    InThruster.CanBeDroppedOnShipDeath = ThrusterType.CanBeDroppedOnShipDeath;
                    InThruster.Experimental = ThrusterType.Experimental;
                    InThruster.Unstable = ThrusterType.Unstable;
                    InThruster.Contraband = ThrusterType.Contraband;
                    InThruster.UpdateMaxPowerWatts();
                    InThruster.Price_LevelMultiplierExponent = ThrusterType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InThruster = new PLThruster((EThrusterType)Subtype, level);
            }
            return InThruster;
        }
    }
    //Converts hashes to Thrusters.
    [HarmonyPatch(typeof(PLThruster), "CreateThrusterFromHash")]
    class ThrusterHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = ThrusterModManager.CreateThruster(inSubType, inLevel);
            return false;
        }
    }

    [HarmonyPatch(typeof(PLThruster), "Tick")]
    class TickPatch
    {
        static void Postfix(PLThruster __instance)
        {
            int subtypeformodded = __instance.SubType - ThrusterModManager.Instance.VanillaMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ThrusterModManager.Instance.types.Count && __instance.ShipStats != null)
            {
                ThrusterModManager.Instance.types[subtypeformodded].Tick(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLThruster), "GetStatLineLeft")]
    class LeftDescFix
    {
        static void Postfix(PLThruster __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - ThrusterModManager.Instance.VanillaMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ThrusterModManager.Instance.types.Count && __instance.ShipStats != null)
            {
                __result = ThrusterModManager.Instance.types[subtypeformodded].GetStatLineLeft(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLThruster), "GetStatLineRight")]
    class RightDescFix
    {
        static void Postfix(PLThruster __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - ThrusterModManager.Instance.VanillaMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ThrusterModManager.Instance.types.Count && __instance.ShipStats != null)
            {
                __result = ThrusterModManager.Instance.types[subtypeformodded].GetStatLineRight(__instance);
            }
        }
    }
}

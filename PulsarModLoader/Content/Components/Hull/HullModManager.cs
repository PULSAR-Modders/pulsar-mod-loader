using HarmonyLib;

namespace PulsarModLoader.Content.Components.Hull
{
    public class HullModManager : ComponentModManager<HullMod, EHullType>
    {
        private static HullModManager m_instance = null;
        public static HullModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new HullModManager();
                }
                return m_instance;
            }
        }

        HullModManager() {}

        public static PLHull CreateHull(int Subtype, int level)
        {
            PLHull InHull;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InHull = new PLHull(EHullType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    HullMod HullType = Instance.types[Subtype - Instance.VanillaMaxType];
                    InHull.SubType = Subtype;
                    InHull.Name = HullType.Name;
                    InHull.Desc = HullType.Description;
                    InHull.m_IconTexture = HullType.IconTexture;
                    InHull.Max = HullType.HullMax;
                    InHull.Armor = HullType.Armor;
                    InHull.Defense = HullType.Defense;
                    InHull.m_MarketPrice = HullType.MarketPrice;
                    InHull.CargoVisualPrefabID = HullType.CargoVisualID;
                    InHull.CanBeDroppedOnShipDeath = HullType.CanBeDroppedOnShipDeath;
                    InHull.Experimental = HullType.Experimental;
                    InHull.Unstable = HullType.Unstable;
                    InHull.Contraband = HullType.Contraband;
                    InHull.Price_LevelMultiplierExponent = HullType.Price_LevelMultiplierExponent;
                    InHull.Max *= 2f;
                    InHull.Current = InHull.Max;
                }
            }
            else
            {
                InHull = new PLHull((EHullType)Subtype, level);
            }
            return InHull;
        }

        //Converts hashes to Hulls.
        [HarmonyPatch(typeof(PLHull), "CreateHullFromHash")]
        class HullHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                __result = HullModManager.CreateHull(inSubType, inLevel);
                return false;
            }
        }
        [HarmonyPatch(typeof(PLHull), "Tick")]
        class TickPatch
        {
            static void Postfix(PLHull __instance)
            {
                int subtypeformodded = __instance.SubType - HullModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < HullModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    HullModManager.Instance.types[subtypeformodded].Tick(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLHull), "GetStatLineLeft")]
        class LeftDescFix
        {
            static void Postfix(PLHull __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - HullModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < HullModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    __result = HullModManager.Instance.types[subtypeformodded].GetStatLineLeft(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLHull), "GetStatLineRight")]
        class RightDescFix
        {
            static void Postfix(PLHull __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - HullModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < HullModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    __result = HullModManager.Instance.types[subtypeformodded].GetStatLineRight(__instance);
                }
            }
        }
    }
}

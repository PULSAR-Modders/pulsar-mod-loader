using HarmonyLib;

namespace PulsarModLoader.Content.Components.Extractor
{
    public class ExtractorModManager : ComponentModManager<ExtractorMod, EExtractorType>
    {
        private static ExtractorModManager m_instance = null;
        public static ExtractorModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ExtractorModManager();
                }
                return m_instance;
            }
        }

        ExtractorModManager() {}

        public static PLExtractor CreateExtractor(int Subtype, int level)
        {
            PLExtractor InExtractor;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InExtractor = new PLExtractor(EExtractorType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    ExtractorMod ExtractorType = Instance.types[Subtype - Instance.VanillaMaxType];
                    InExtractor.SubType = Subtype;
                    InExtractor.Name = ExtractorType.Name;
                    InExtractor.Desc = ExtractorType.Description;
                    InExtractor.m_IconTexture = ExtractorType.IconTexture;
                    InExtractor.m_Stability = ExtractorType.Stability;
                    InExtractor.m_MarketPrice = ExtractorType.MarketPrice;
                    InExtractor.CargoVisualPrefabID = ExtractorType.CargoVisualID;
                    InExtractor.CanBeDroppedOnShipDeath = ExtractorType.CanBeDroppedOnShipDeath;
                    InExtractor.Experimental = ExtractorType.Experimental;
                    InExtractor.Unstable = ExtractorType.Unstable;
                    InExtractor.Contraband = ExtractorType.Contraband;
                    InExtractor.Price_LevelMultiplierExponent = ExtractorType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InExtractor = new PLExtractor((EExtractorType)Subtype, level);
            }
            return InExtractor;
        }
    }
    //Converts hashes to Extractors.
    [HarmonyPatch(typeof(PLExtractor), "CreateExtractorFromHash")]
    class ExtractorHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = ExtractorModManager.CreateExtractor(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLExtractor), "GetStatLineLeft")]
    class LeftDescFix
    {
        static void Postfix(PLExtractor __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - ExtractorModManager.Instance.VanillaMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ExtractorModManager.Instance.types.Count && __instance.ShipStats != null)
            {
                __result = ExtractorModManager.Instance.types[subtypeformodded].GetStatLineLeft(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PLExtractor), "GetStatLineRight")]
    class RightDescFix
    {
        static void Postfix(PLExtractor __instance, ref string __result)
        {
            int subtypeformodded = __instance.SubType - ExtractorModManager.Instance.VanillaMaxType;
            if (subtypeformodded > -1 && subtypeformodded < ExtractorModManager.Instance.types.Count && __instance.ShipStats != null)
            {
                __result = ExtractorModManager.Instance.types[subtypeformodded].GetStatLineRight(__instance);
            }
        }
    }
}

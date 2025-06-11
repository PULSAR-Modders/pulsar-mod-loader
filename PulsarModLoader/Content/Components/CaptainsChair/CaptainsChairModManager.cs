using HarmonyLib;

namespace PulsarModLoader.Content.Components.CaptainsChair
{
    /// <summary>
    /// Manages Modded Captains Chairs
    /// </summary>
    public class CaptainsChairModManager : ComponentModManager<CaptainsChairMod, ECaptainsChairType>
    {
        private static CaptainsChairModManager m_instance = null;

        /// <summary>
        /// Static Manager Instance.
        /// </summary>
        public static CaptainsChairModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new CaptainsChairModManager();
                }
                return m_instance;
            }
        }

        CaptainsChairModManager() {}

        public static PLCaptainsChair CreateCaptainsChair(int Subtype, int level)
        {
            PLCaptainsChair InCaptainsChair;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InCaptainsChair = new PLCaptainsChair(ECaptainsChairType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    CaptainsChairMod CaptainsChairType = Instance.types[Subtype - Instance.VanillaMaxType];
                    InCaptainsChair.SubType = Subtype;
                    InCaptainsChair.Name = CaptainsChairType.Name;
                    InCaptainsChair.Desc = CaptainsChairType.Description;
                    InCaptainsChair.m_IconTexture = CaptainsChairType.IconTexture;
                    InCaptainsChair.m_MarketPrice = CaptainsChairType.MarketPrice;
                    InCaptainsChair.CargoVisualPrefabID = CaptainsChairType.CargoVisualID;
                    InCaptainsChair.CanBeDroppedOnShipDeath = CaptainsChairType.CanBeDroppedOnShipDeath;
                    InCaptainsChair.Experimental = CaptainsChairType.Experimental;
                    InCaptainsChair.Unstable = CaptainsChairType.Unstable;
                    InCaptainsChair.Contraband = CaptainsChairType.Contraband;
                    InCaptainsChair.Price_LevelMultiplierExponent = CaptainsChairType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InCaptainsChair = new PLCaptainsChair((ECaptainsChairType)Subtype, level);
            }
            return InCaptainsChair;
        }

        //Converts hashes to CaptainsChairs.
        [HarmonyPatch(typeof(PLCaptainsChair), "CreateCaptainsChairFromHash")]
        class CaptainsChairHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                __result = CaptainsChairModManager.CreateCaptainsChair(inSubType, inLevel);
                return false;
            }
        }
        [HarmonyPatch(typeof(PLCaptainsChair), "LateAddStats")]
        class CaptainsChairLateAddStatsPatch
        {
            static void Postfix(PLShipStats inStats, PLCaptainsChair __instance)
            {
                int subtypeformodded = __instance.SubType - CaptainsChairModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < CaptainsChairModManager.Instance.types.Count && inStats != null)
                {
                    CaptainsChairModManager.Instance.types[subtypeformodded].LateAddStats(__instance);
                }
            }
        }
    }
}

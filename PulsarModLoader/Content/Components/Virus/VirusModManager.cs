using HarmonyLib;

namespace PulsarModLoader.Content.Components.Virus
{
    public class VirusModManager : ComponentModManager<VirusMod, EVirusType>
    {
        private static VirusModManager m_instance = null;
        public static VirusModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new VirusModManager();
                }
                return m_instance;
            }
        }

        VirusModManager() { }

        public static PLVirus CreateVirus(int Subtype, int level)
        {
            PLVirus InVirus;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InVirus = new PLVirus(EVirusType.NONE, level);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    VirusMod VirusType = Instance.types[Subtype - Instance.VanillaMaxType];
                    InVirus.SubType = Subtype;
                    InVirus.Name = VirusType.Name;
                    InVirus.Desc = VirusType.Description;
                    InVirus.m_IconTexture = VirusType.IconTexture;
                    InVirus.m_MarketPrice = VirusType.MarketPrice;
                    InVirus.CargoVisualPrefabID = VirusType.CargoVisualID;
                    InVirus.CanBeDroppedOnShipDeath = VirusType.CanBeDroppedOnShipDeath;
                    InVirus.Experimental = VirusType.Experimental;
                    InVirus.Unstable = VirusType.Unstable;
                    InVirus.Contraband = VirusType.Contraband;
                    InVirus.InfectionTimeLimitMs = VirusType.InfectionTimeLimitMs;
                    InVirus.Price_LevelMultiplierExponent = VirusType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InVirus = new PLVirus((EVirusType)Subtype, level);
            }
            return InVirus;
        }

        //Converts hashes to Viruss.
        [HarmonyPatch(typeof(PLVirus), "CreateVirusFromHash")]
        class VirusHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                __result = VirusModManager.CreateVirus(inSubType, inLevel);
                return false;
            }
        }
        [HarmonyPatch(typeof(PLVirus), "FinalLateAddStats")]
        class VirusFinalLateAddStatsPatch
        {
            static void Postfix(PLVirus __instance)
            {
                int subtypeformodded = __instance.SubType - VirusModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < VirusModManager.Instance.types.Count)
                {
                    VirusModManager.Instance.types[subtypeformodded].FinalLateAddStats(__instance);
                }
            }
        }
    }
}

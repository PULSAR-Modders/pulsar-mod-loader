using HarmonyLib;

namespace PulsarModLoader.Content.Components.HullPlating
{
    public class HullPlatingModManager : ComponentModManager<HullPlatingMod, EHullPlatingType>
    {
        private static HullPlatingModManager m_instance = null;
        public static HullPlatingModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new HullPlatingModManager();
                }
                return m_instance;
            }
        }

        HullPlatingModManager() { }

        //Converts hashes to HullPlatings.
        [HarmonyPatch(typeof(PLHullPlating), "CreateHullPlatingFromHash")]
        class HullPlatingHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                int subtypeformodded = inSubType - HullPlatingModManager.Instance.VanillaMaxType;
                if (subtypeformodded <= HullPlatingModManager.Instance.types.Count && subtypeformodded > -1)
                {
                    //Logger.Info("Creating HullPlating from list info");
                    __result = HullPlatingModManager.Instance.types[subtypeformodded].PLHullPlating;
                    __result.SubType = inSubType;
                    __result.Level = inLevel;
                    return false;
                }
                return true;
            }
        }
    }
}

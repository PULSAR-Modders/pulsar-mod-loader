using HarmonyLib;

namespace PulsarModLoader.Content.Components.AutoTurret
{
    public class AutoTurretModManager : ComponentModManager<AutoTurretMod, EAutoTurretType>
    {
        private static AutoTurretModManager m_instance = null;

        public static AutoTurretModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new AutoTurretModManager();
                }
                return m_instance;
            }
        }

        AutoTurretModManager() : base(1) {}

        //Converts hashes to AutoTurrets.
        [HarmonyPatch(typeof(PLAutoTurret), "CreateAutoTurretFromHash")]
        class AutoTurretHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                int subtypeformodded = inSubType - AutoTurretModManager.Instance.VanillaMaxType;
                if (subtypeformodded <= AutoTurretModManager.Instance.types.Count && subtypeformodded > -1)
                {
                    //Logger.Info("Creating AutoTurret from list info");
                    __result = AutoTurretModManager.Instance.types[subtypeformodded].PLAutoTurret;
                    __result.SubType = inSubType;
                    __result.Level = inLevel;
                    return false;
                }
                return true;
            }
        }
    }
}

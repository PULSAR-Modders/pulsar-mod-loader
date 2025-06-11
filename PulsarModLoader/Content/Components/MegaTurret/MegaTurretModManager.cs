using HarmonyLib;
using System.Collections.Generic;

namespace PulsarModLoader.Content.Components.MegaTurret
{
    public class MegaTurretModManager : ComponentModManager<MegaTurretMod, Empty>
    {
        private static MegaTurretModManager m_instance = null;
        public static MegaTurretModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MegaTurretModManager();
                }
                return m_instance;
            }
        }
        MegaTurretModManager() : base(8) {}

        //Converts hashes to MegaTurrets.
        [HarmonyPatch(typeof(PLMegaTurret), "CreateMainTurretFromHash")]
        class MegaTurretHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                int subtypeformodded = inSubType - MegaTurretModManager.Instance.VanillaMaxType;
                if (subtypeformodded <= MegaTurretModManager.Instance.types.Count && subtypeformodded > -1)
                {
                    //Logger.Info("Creating MegaTurret from list info");
                    __result = MegaTurretModManager.Instance.types[subtypeformodded].PLMegaTurret;
                    __result.SubType = inSubType;
                    __result.Level = inLevel;
                    return false;
                }
                return true;
            }
        }
    }
}

using HarmonyLib;
using System.Collections.Generic;

namespace PulsarModLoader.Content.Components.MegaTurret
{
    public enum EMegaTurretType
    {
        max = 8
    }
    public class MegaTurretModManager : ComponentModManager<MegaTurretMod, EMegaTurretType>
    {
        public readonly int VanillaMegaTurretMaxType = 0;
        private static MegaTurretModManager m_instance = null;
        public readonly List<MegaTurretMod> MegaTurretTypes = new List<MegaTurretMod>();
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
        MegaTurretModManager() { }

        //Converts hashes to MegaTurrets.
        [HarmonyPatch(typeof(PLMegaTurret), "CreateMainTurretFromHash")]
        class MegaTurretHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                int subtypeformodded = inSubType - MegaTurretModManager.Instance.VanillaMegaTurretMaxType;
                if (subtypeformodded <= MegaTurretModManager.Instance.MegaTurretTypes.Count && subtypeformodded > -1)
                {
                    //Logger.Info("Creating MegaTurret from list info");
                    __result = MegaTurretModManager.Instance.MegaTurretTypes[subtypeformodded].PLMegaTurret;
                    __result.SubType = inSubType;
                    __result.Level = inLevel;
                    return false;
                }
                return true;
            }
        }
    }
}

using HarmonyLib;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.Turret
{
    public class TurretModManager : ComponentModManager<TurretMod, ETurretType>
    {
        private static TurretModManager m_instance = null;
        public static TurretModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new TurretModManager();
                }
                return m_instance;
            }
        }

        TurretModManager() {}


        //Converts hashes to Turrets.
        [HarmonyPatch(typeof(PLTurret), "CreateTurretFromHash")]
        class TurretHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                int subtypeformodded = inSubType - TurretModManager.Instance.VanillaMaxType;
                if (subtypeformodded <= TurretModManager.Instance.types.Count && subtypeformodded > -1)
                {
                    Logger.Info("Creating Turret from list info");
                    __result = TurretModManager.Instance.types[subtypeformodded].PLTurret;
                    __result.SubType = inSubType;
                    __result.Level = inLevel;
                    return false;
                }
                return true;
            }
        }
        /*[HarmonyPatch(typeof(PLTurret), "LateAddStats")]
        class TurretLateAddStatsPatch
        {
            static void Postfix(PLShipStats inStats, PLTurret __instance)
            {
                int subtypeformodded = __instance.SubType - TurretModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < TurretModManager.Instance.TurretTypes.Count && inStats != null)
                {
                    TurretModManager.Instance.TurretTypes[subtypeformodded].LateAddStats(inStats);
                }
            }
        }*/
    }
}

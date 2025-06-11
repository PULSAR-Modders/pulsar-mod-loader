using HarmonyLib;

namespace PulsarModLoader.Content.Components.MissionShipComponent
{
    public class MissionShipComponentModManager : ComponentModManager<MissionShipComponentMod, Empty>
    {
        private static MissionShipComponentModManager m_instance = null;
        public static MissionShipComponentModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MissionShipComponentModManager();
                }
                return m_instance;
            }
        }

        MissionShipComponentModManager() : base(13) {}

        public static PLMissionShipComponent CreateMissionShipComponent(int Subtype, int level)
        {
            PLMissionShipComponent InMissionShipComponent;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InMissionShipComponent = new PLMissionShipComponent(0, level);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    MissionShipComponentMod MissionShipComponentType = Instance.types[Subtype - Instance.VanillaMaxType];
                    InMissionShipComponent.SubType = Subtype;
                    InMissionShipComponent.Name = MissionShipComponentType.Name;
                    InMissionShipComponent.Desc = MissionShipComponentType.Description;
                    InMissionShipComponent.m_IconTexture = MissionShipComponentType.IconTexture;
                    InMissionShipComponent.m_MarketPrice = MissionShipComponentType.MarketPrice;
                    InMissionShipComponent.CargoVisualPrefabID = MissionShipComponentType.CargoVisualID;
                    InMissionShipComponent.CanBeDroppedOnShipDeath = MissionShipComponentType.CanBeDroppedOnShipDeath;
                    InMissionShipComponent.Experimental = MissionShipComponentType.Experimental;
                    InMissionShipComponent.Unstable = MissionShipComponentType.Unstable;
                    InMissionShipComponent.Contraband = MissionShipComponentType.Contraband;
                    InMissionShipComponent.Price_LevelMultiplierExponent = MissionShipComponentType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InMissionShipComponent = new PLMissionShipComponent(Subtype, level);
            }
            return InMissionShipComponent;
        }

        //Converts hashes to MissionShipComponents.
        [HarmonyPatch(typeof(PLMissionShipComponent), "CreateMissionComponentFromHash")]
        class MissionShipComponentHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                __result = MissionShipComponentModManager.CreateMissionShipComponent(inSubType, inLevel);
                return false;
            }
        }
    }
}

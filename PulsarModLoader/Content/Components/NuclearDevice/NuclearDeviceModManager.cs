using HarmonyLib;

namespace PulsarModLoader.Content.Components.NuclearDevice
{
    public class NuclearDeviceModManager : ComponentModManager<NuclearDeviceMod, ENuclearDeviceType>
    {
        private static NuclearDeviceModManager m_instance = null;
        public static NuclearDeviceModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new NuclearDeviceModManager();
                }
                return m_instance;
            }
        }

        NuclearDeviceModManager() {}

        public static PLNuclearDevice CreateNuclearDevice(int Subtype, int level)
        {
            PLNuclearDevice InNuclearDevice;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InNuclearDevice = new PLNuclearDevice(ENuclearDeviceType.MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    NuclearDeviceMod NuclearDeviceType = Instance.types[Subtype - Instance.VanillaMaxType];
                    InNuclearDevice.SubType = Subtype;
                    InNuclearDevice.Name = NuclearDeviceType.Name;
                    InNuclearDevice.Desc = NuclearDeviceType.Description;
                    InNuclearDevice.m_IconTexture = NuclearDeviceType.IconTexture;
                    InNuclearDevice.m_MaxDamage = NuclearDeviceType.MaxDamage;
                    InNuclearDevice.m_Range = NuclearDeviceType.Range;
                    InNuclearDevice.m_FuelBurnRate = NuclearDeviceType.FuelBurnRate;
                    InNuclearDevice.m_TurnRate = NuclearDeviceType.TurnRate;
                    InNuclearDevice.m_IntimidationBonus = NuclearDeviceType.IntimidationBonus;
                    InNuclearDevice.m_TurnRate = NuclearDeviceType.TurnRate;
                    InNuclearDevice.m_Health = NuclearDeviceType.Health;
                    InNuclearDevice.m_MarketPrice = NuclearDeviceType.MarketPrice;
                    InNuclearDevice.CargoVisualPrefabID = NuclearDeviceType.CargoVisualID;
                    InNuclearDevice.CanBeDroppedOnShipDeath = NuclearDeviceType.CanBeDroppedOnShipDeath;
                    InNuclearDevice.Experimental = NuclearDeviceType.Experimental;
                    InNuclearDevice.Unstable = NuclearDeviceType.Unstable;
                    InNuclearDevice.Contraband = NuclearDeviceType.Contraband;
                    InNuclearDevice.Price_LevelMultiplierExponent = NuclearDeviceType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InNuclearDevice = new PLNuclearDevice((ENuclearDeviceType)Subtype, level);
            }
            return InNuclearDevice;
        }

        //Converts hashes to NuclearDevices.
        [HarmonyPatch(typeof(PLNuclearDevice), "CreateNuclearDeviceFromHash")]
        class NuclearDeviceHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                __result = NuclearDeviceModManager.CreateNuclearDevice(inSubType, inLevel);
                return false;
            }
        }
        [HarmonyPatch(typeof(PLNuclearDevice), "GetStatLineLeft")]
        class LeftDescFix
        {
            static void Postfix(PLNuclearDevice __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - NuclearDeviceModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < NuclearDeviceModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    __result = NuclearDeviceModManager.Instance.types[subtypeformodded].GetStatLineLeft(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLNuclearDevice), "GetStatLineRight")]
        class RightDescFix
        {
            static void Postfix(PLNuclearDevice __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - NuclearDeviceModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < NuclearDeviceModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    __result = NuclearDeviceModManager.Instance.types[subtypeformodded].GetStatLineRight(__instance);
                }
            }
        }
    }
}

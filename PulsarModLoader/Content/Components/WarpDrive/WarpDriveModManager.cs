using HarmonyLib;

namespace PulsarModLoader.Content.Components.WarpDrive
{
    public class WarpDriveModManager : ComponentModManager<WarpDriveMod, EWarpDriveType>
    {
        private static WarpDriveModManager m_instance = null;
        public static WarpDriveModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new WarpDriveModManager();
                }
                return m_instance;
            }
        }

        WarpDriveModManager() {}

        public static PLWarpDrive CreateWarpDrive(int Subtype, int level, short SubTypeData)
        {
            PLWarpDrive InWarpDrive;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InWarpDrive = new PLWarpDrive(EWarpDriveType.E_MAX, level, SubTypeData);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    WarpDriveMod WarpDriveType = Instance.types[Subtype - Instance.VanillaMaxType];
                    InWarpDrive.SubType = Subtype;
                    InWarpDrive.Name = WarpDriveType.Name;
                    InWarpDrive.Desc = WarpDriveType.Description;
                    InWarpDrive.m_IconTexture = WarpDriveType.IconTexture;
                    InWarpDrive.ChargeSpeed = WarpDriveType.ChargeSpeed;
                    InWarpDrive.WarpRange = WarpDriveType.WarpRange;
                    InWarpDrive.EnergySignatureAmt = WarpDriveType.EnergySignature;
                    InWarpDrive.NumberOfChargingNodes = WarpDriveType.NumberOfChargesPerFuel;
                    InWarpDrive.m_MaxPowerUsage_Watts = WarpDriveType.MaxPowerUsage_Watts;
                    InWarpDrive.m_MarketPrice = WarpDriveType.MarketPrice;
                    InWarpDrive.CargoVisualPrefabID = WarpDriveType.CargoVisualID;
                    InWarpDrive.CanBeDroppedOnShipDeath = WarpDriveType.CanBeDroppedOnShipDeath;
                    InWarpDrive.Experimental = WarpDriveType.Experimental;
                    InWarpDrive.Unstable = WarpDriveType.Unstable;
                    InWarpDrive.Contraband = WarpDriveType.Contraband;
                    InWarpDrive.Price_LevelMultiplierExponent = WarpDriveType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InWarpDrive = new PLWarpDrive((EWarpDriveType)Subtype, level, SubTypeData);
            }
            return InWarpDrive;
        }

        //Converts hashes to WarpDrives.
        [HarmonyPatch(typeof(PLWarpDrive), "CreateWarpDriveFromHash")]
        class WarpDriveHashFix
        {
            static bool Prefix(int inSubType, int inLevel, short inSubTypeData, ref PLShipComponent __result)
            {
                __result = WarpDriveModManager.CreateWarpDrive(inSubType, inLevel, inSubTypeData);
                return false;
            }
        }
        [HarmonyPatch(typeof(PLWarpDrive), "GetStatLineLeft")]
        class LeftDescFix
        {
            static void Postfix(PLWarpDrive __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - WarpDriveModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < WarpDriveModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    __result = WarpDriveModManager.Instance.types[subtypeformodded].GetStatLineLeft(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLWarpDrive), "GetStatLineRight")]
        class RightDescFix
        {
            static void Postfix(PLWarpDrive __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - WarpDriveModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < WarpDriveModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    __result = WarpDriveModManager.Instance.types[subtypeformodded].GetStatLineRight(__instance);
                }
            }
        }
    }
}

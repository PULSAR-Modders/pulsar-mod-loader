using HarmonyLib;
using UnityEngine;

namespace PulsarModLoader.Content.Components.Shield
{
    public class ShieldModManager : ComponentModManager<ShieldMod, EShieldGeneratorType>
    {
        private static ShieldModManager m_instance = null;
        public static ShieldModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ShieldModManager();
                }
                return m_instance;
            }
        }

        ShieldModManager() {}

        public static PLShieldGenerator CreateShield(int Subtype, int level)
        {
            PLShieldGenerator InShield;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InShield = new PLShieldGenerator(EShieldGeneratorType.E_SG_ID_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    ShieldMod ShieldType = Instance.types[Subtype - Instance.VanillaMaxType];
                    InShield.SubType = Subtype;
                    InShield.Name = ShieldType.Name;
                    InShield.Desc = ShieldType.Description;
                    InShield.m_IconTexture = ShieldType.IconTexture;
                    InShield.Max = ShieldType.ShieldMax;
                    InShield.ChargeRateMax = ShieldType.ChargeRateMax;
                    InShield.RecoveryRate = ShieldType.RecoveryRate;
                    InShield.Deflection = ShieldType.Deflection;
                    InShield.MinIntegrityPercentForQuantumShield = ShieldType.MinIntegrityPercentForQuantumShield;
                    InShield.MinIntegrityAfterDamage = ShieldType.MinIntegrityAfterDamage;
                    InShield.m_MaxPowerUsage_Watts = (ShieldType.MaxPowerUsage_Watts * 1.4f);
                    InShield.m_MarketPrice = ShieldType.MarketPrice;
                    InShield.CargoVisualPrefabID = ShieldType.CargoVisualID;
                    InShield.CanBeDroppedOnShipDeath = ShieldType.CanBeDroppedOnShipDeath;
                    InShield.Experimental = ShieldType.Experimental;
                    InShield.Unstable = ShieldType.Unstable;
                    InShield.Contraband = ShieldType.Contraband;
                    InShield.Price_LevelMultiplierExponent = ShieldType.Price_LevelMultiplierExponent;
                    if (InShield.MinIntegrityAfterDamage == -1)
                    {
                        InShield.MinIntegrityAfterDamage = Mathf.RoundToInt(InShield.Max * 0.15f);
                    }
                    InShield.MinIntegrityAfterDamage = Mathf.RoundToInt(InShield.MinIntegrityAfterDamage * (1f - Mathf.Clamp(0.05f * InShield.Level, 0f, 0.8f)));
                    InShield.CurrentMax = InShield.Max;
                    InShield.Current = InShield.Max;
                }
            }
            else
            {
                InShield = new PLShieldGenerator((EShieldGeneratorType)Subtype, level);
            }
            return InShield;
        }

        //Converts hashes to Shields.
        [HarmonyPatch(typeof(PLShieldGenerator), "CreateShieldGeneratorFromHash")]
        class ShieldHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                __result = ShieldModManager.CreateShield(inSubType, inLevel);
                return false;
            }
        }
        //Applies the Tick of the modded shields
        [HarmonyPatch(typeof(PLShieldGenerator), "Tick")]
        class TickPatch
        {
            static void Postfix(PLShieldGenerator __instance)
            {
                int subtypeformodded = __instance.SubType - ShieldModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ShieldModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    ShieldModManager.Instance.types[subtypeformodded].Tick(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLShieldGenerator), "GetStatLineLeft")]
        class LeftDescFix
        {
            static void Postfix(PLShieldGenerator __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - ShieldModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ShieldModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    __result = ShieldModManager.Instance.types[subtypeformodded].GetStatLineLeft(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PLShieldGenerator), "GetStatLineRight")]
        class RightDescFix
        {
            static void Postfix(PLShieldGenerator __instance, ref string __result)
            {
                int subtypeformodded = __instance.SubType - ShieldModManager.Instance.VanillaMaxType;
                if (subtypeformodded > -1 && subtypeformodded < ShieldModManager.Instance.types.Count && __instance.ShipStats != null)
                {
                    __result = ShieldModManager.Instance.types[subtypeformodded].GetStatLineRight(__instance);
                }
            }
        }
    }
}

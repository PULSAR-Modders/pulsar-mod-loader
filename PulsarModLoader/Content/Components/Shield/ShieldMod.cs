using UnityEngine;

namespace PulsarModLoader.Content.Components.Shield
{
    public abstract class ShieldMod : ComponentModBase
    {
        public ShieldMod()
        {
        }
        public override Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("Icons/39_Sheilds"); }
        }
        public virtual float ShieldMax
        {
            get { return 70f; }
        }
        public virtual float ChargeRateMax
        {
            get { return 12f; }
        }
        public virtual float RecoveryRate
        {
            get { return 15f; }
        }
        public virtual float Deflection
        {
            get { return 1f; }
        }
        public virtual float MinIntegrityPercentForQuantumShield
        {
            get { return .9f; }
        }
        public virtual float MaxPowerUsage_Watts
        {
            get { return 4600; }
        }
        public virtual int MinIntegrityAfterDamage
        {
            get { return -1; }
        }
        public override int CargoVisualID => 39;

        public override string GetStatLineLeft(PLShipComponent InComp)
        {
            return string.Concat(new string[]
            {
            PLLocalize.Localize("Integrity", false),
            "\n",
            PLLocalize.Localize("Charge Rate", false),
            "\n",
            PLLocalize.Localize("Min For QT Shields", false)
            });
        }

        public override string GetStatLineRight(PLShipComponent InComp)
        {
            PLShieldGenerator me = InComp as PLShieldGenerator;
            return string.Concat(new string[]
            {
            (me.Max * me.LevelMultiplier(0.25f,1f)).ToString("0"),
            "\n",
            (me.ChargeRateMax * me.LevelMultiplier(0.5f,1f)).ToString("0"),
            "\n",
            (me.MinIntegrityPercentForQuantumShield * 100f).ToString("0"),
            "%"
            });
        }
    }
}

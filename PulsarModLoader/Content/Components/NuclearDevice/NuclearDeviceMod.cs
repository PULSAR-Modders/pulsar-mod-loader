using UnityEngine;

namespace PulsarModLoader.Content.Components.NuclearDevice
{
    public abstract class NuclearDeviceMod : ComponentModBase
    {
        public NuclearDeviceMod()
        {
        }
        public override Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("Icons/80_Thrusters"); }
        }
        public virtual float MaxDamage
        {
            get { return 3800f; }
        }
        public virtual float Range
        {
            get { return 4000f; }
        }
        public virtual float FuelBurnRate
        {
            get { return 6f; }
        }
        public virtual float TurnRate
        {
            get { return .12f; }
        }
        public virtual float IntimidationBonus
        {
            get { return 10f; }
        }
        public virtual float Health
        {
            get { return 200f; }
        }
        public override int CargoVisualID => 15;

        public override string GetStatLineLeft(PLShipComponent InComp)
        {
            return string.Concat(new string[]
            {
            PLLocalize.Localize("Max Damage", false),
            "\n",
            PLLocalize.Localize("Damage Range", false),
            "\n",
            PLLocalize.Localize("Fuel Burn Rate", false),
            "\n",
            PLLocalize.Localize("Turn Speed", false)
            });
        }
        public override string GetStatLineRight(PLShipComponent InComp)
        {
            PLNuclearDevice me = InComp as PLNuclearDevice;
            return string.Concat(new string[]
            {
            (me.MaxDamage * me.LevelMultiplier(0.15f, 1f)).ToString("0"),
            "\n",
            (me.Range * me.LevelMultiplier(0.2f, 1f)).ToString("0"),
            "\n",
            me.FuelBurnRate.ToString("0.0"),
            "\n",
            (me.TurnRate * 100f * me.LevelMultiplier(0.2f, 1f)).ToString("0")
            });
        }
    }
}

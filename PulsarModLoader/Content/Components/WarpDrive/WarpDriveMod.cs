using UnityEngine;

namespace PulsarModLoader.Content.Components.WarpDrive
{
    public abstract class WarpDriveMod : ComponentModBase
    {
        public WarpDriveMod()
        {
        }
        public override Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("Icons/17_Warp"); }
        }
        public virtual float ChargeSpeed
        {
            get { return 3.3f; }
        }
        public virtual float WarpRange
        {
            get { return .06f; }
        }
        public virtual float EnergySignature
        {
            get { return 8f; }
        }
        public virtual int NumberOfChargesPerFuel
        {
            get { return 3; }
        }
        public virtual float MaxPowerUsage_Watts
        {
            get { return 3000f; }
        }
        public override int CargoVisualID => 16;
        public override string GetStatLineLeft(PLShipComponent InComp)
        {
            return string.Concat(new string[]
            {
            PLLocalize.Localize("Charge Rate", false),
            "\n",
            PLLocalize.Localize("Range", false),
            "\n",
            PLLocalize.Localize("Charges Per Fuel", false)
            });
        }
        public override string GetStatLineRight(PLShipComponent InComp)
        {
            PLWarpDrive me = InComp as PLWarpDrive;
            return string.Concat(new string[]
            {
            (me.ChargeSpeed * me.LevelMultiplier(0.25f, 1f)).ToString("0"),
            "\n",
            (me.WarpRange * 100f * me.LevelMultiplier(0.2f, 1f)).ToString("0"),
            "\n",
            me.NumberOfChargingNodes.ToString("0")
            });
        }
    }
}

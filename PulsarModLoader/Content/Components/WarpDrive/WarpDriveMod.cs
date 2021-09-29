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
    }
}

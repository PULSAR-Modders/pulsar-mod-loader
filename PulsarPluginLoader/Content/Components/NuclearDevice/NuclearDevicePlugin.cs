using UnityEngine;

namespace PulsarPluginLoader.Content.Components.NuclearDevice
{
    public abstract class NuclearDevicePlugin : ComponentPluginBase
    {
        public NuclearDevicePlugin()
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
    }
}

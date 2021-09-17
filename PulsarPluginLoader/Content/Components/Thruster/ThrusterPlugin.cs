using UnityEngine;

namespace PulsarModLoader.Content.Components.Thruster
{
    public abstract class ThrusterPlugin : ComponentPluginBase
    {
        public ThrusterPlugin()
        {
        }
        public override Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("Icons/81_Thrusters"); }
        }
        public virtual float MaxOutput
        {
            get { return .115f; }
        }
        public virtual float MaxPowerUsage_Watts
        {
            get { return 1200f; }
        }
        public override int CargoVisualID => 8;
    }
}

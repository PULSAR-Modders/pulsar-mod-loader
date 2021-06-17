using UnityEngine;

namespace PulsarPluginLoader.Content.Components.InertiaThruster
{
    public abstract class InertiaThrusterPlugin : ComponentPluginBase
    {
        public InertiaThrusterPlugin()
        {
        }
        public override Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("Icons/81_Thrusters"); }
        }
        public virtual float MaxOutput
        {
            get { return .4f; }
        }
        public virtual float MaxPowerUsage_Watts
        {
            get { return 2600f; }
        }
    }
}

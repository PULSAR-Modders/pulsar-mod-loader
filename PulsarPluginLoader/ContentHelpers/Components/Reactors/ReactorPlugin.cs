using UnityEngine;

namespace PulsarPluginLoader.ContentHelpers.Components.Reactor
{
    public abstract class ReactorPlugin : ComponentPluginBase
    {
        public ReactorPlugin()
        {
        }
        public override Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("Icons/28_Reactor"); }
        }
        public virtual float EnergyOutputMax
        {
            get { return 15000f; }
        }
        public virtual float EnergySignatureAmount
        {
            get { return 18f; }
        }
        public virtual float MaxTemp
        {
            get { return 1800f; }
        }
        public virtual float EmergencyCooldownTime
        {
            get { return 20f; }
        }
        public virtual float HeatOutput
        {
            get { return 1f; }
        }
    }
}

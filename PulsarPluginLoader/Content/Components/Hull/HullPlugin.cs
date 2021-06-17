using UnityEngine;

namespace PulsarPluginLoader.Content.Components.Hull
{
    public abstract class HullPlugin : ComponentPluginBase
    {
        public HullPlugin()
        {
        }
        public override Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("Icons/20_Hull"); }
        }
        public virtual float HullMax
        {
            get { return 750f; }
        }
        public virtual float Armor
        {
            get { return .15f; }
        }
        public virtual float Defense
        {
            get { return .2f; }
        }
    }
}

using UnityEngine;

namespace PulsarPluginLoader.Content.Components.Virus
{
    public abstract class VirusPlugin : ComponentPluginBase
    {
        public VirusPlugin()
        {
        }
        public virtual int InfectionTimeLimitMs
        {
            get { return 40000; }
        }
        public override Texture2D IconTexture
        {
            get { return PLGlobal.Instance.VirusBGTexture;  }
        }
    }
}

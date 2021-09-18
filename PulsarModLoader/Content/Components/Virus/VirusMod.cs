using UnityEngine;

namespace PulsarModLoader.Content.Components.Virus
{
    public abstract class VirusMod : ComponentModBase
    {
        public VirusMod()
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

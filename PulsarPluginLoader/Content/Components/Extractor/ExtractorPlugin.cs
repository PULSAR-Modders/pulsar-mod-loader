namespace PulsarModLoader.Content.Components.Extractor
{
    public abstract class ExtractorPlugin : ComponentPluginBase
    {
        public ExtractorPlugin()
        {
        }
        public virtual float Stability
        {
            get { return 1f; }
        }
    }
}

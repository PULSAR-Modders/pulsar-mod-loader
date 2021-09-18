namespace PulsarModLoader.Content.Components.Extractor
{
    public abstract class ExtractorMod : ComponentModBase
    {
        public ExtractorMod()
        {
        }
        public virtual float Stability
        {
            get { return 1f; }
        }
    }
}

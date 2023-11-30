using System.Reflection;
namespace PulsarModLoader.Content.Components.Extractor
{
    /// <summary>
    /// Extractor Modded Component Abstraction
    /// </summary>
    public abstract class ExtractorMod : ComponentModBase
    {
        public ExtractorMod()
        {
        }
        public virtual float Stability
        {
            get { return 1f; }
        }
        public override string GetStatLineLeft(PLShipComponent InComp)
        {
            return PLLocalize.Localize("Stability", false) + "\n";
        }
        public override string GetStatLineRight(PLShipComponent InComp)
        {
            PLExtractor me = InComp as PLExtractor;
            return ((float)me.m_Stability * 10f).ToString("0");
        }
    }
}

using System.Reflection;
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
        public override string GetStatLineLeft(PLShipComponent InComp)
        {
            return PLLocalize.Localize("Stability", false) + "\n";
        }
        public override string GetStatLineRight(PLShipComponent InComp)
        {
            PLExtractor me = InComp as PLExtractor;
            return ((float)me.GetType().GetField("m_Stability", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(me) * 10f).ToString("0");
        }
    }
}

namespace PulsarPluginLoader.Content.Components.HullPlating
{
    public abstract class HullPlatingPlugin
    {
        public HullPlatingPlugin()
        {
        }
        public virtual string Name
        {
            get
            {
                { return ""; }
            }
        }
        public virtual PLShipComponent PLHullPlating
        {
            get
            {
                { return new PLHullPlating(EHullPlatingType.E_HULLPLATING_CCGE, 0); }
            }
        }
    }
}

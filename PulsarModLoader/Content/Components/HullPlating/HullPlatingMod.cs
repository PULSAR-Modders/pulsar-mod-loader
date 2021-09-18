namespace PulsarModLoader.Content.Components.HullPlating
{
    public abstract class HullPlatingMod
    {
        public HullPlatingMod()
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

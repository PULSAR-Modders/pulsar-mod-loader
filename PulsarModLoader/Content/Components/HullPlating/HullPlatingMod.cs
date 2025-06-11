namespace PulsarModLoader.Content.Components.HullPlating
{
    public abstract class HullPlatingMod : ComponentModBase
    {
        public HullPlatingMod()
        {
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

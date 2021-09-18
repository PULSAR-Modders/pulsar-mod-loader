namespace PulsarModLoader.Content.Components.MegaTurret
{
    public abstract class MegaTurretMod
    {
        public MegaTurretMod()
        {
        }
        public virtual string Name
        {
            get
            {
                { return ""; }
            }
        }
        public virtual PLShipComponent PLMegaTurret
        {
            get
            {
                { return new PLMegaTurret(); }
            }
        }
    }
}

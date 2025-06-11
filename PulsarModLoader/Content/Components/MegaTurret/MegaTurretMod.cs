namespace PulsarModLoader.Content.Components.MegaTurret
{
    public abstract class MegaTurretMod : ComponentModBase
    {
        public MegaTurretMod()
        {
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

namespace PulsarModLoader.Content.Components.AutoTurret
{
    public abstract class AutoTurretMod
    {
        public AutoTurretMod()
        {
        }
        public virtual string Name
        {
            get
            {
                { return ""; }
            }
        }
        public virtual PLShipComponent PLAutoTurret
        {
            get
            {
                { return new PLAutoTurret(); }
            }
        }
    }
}

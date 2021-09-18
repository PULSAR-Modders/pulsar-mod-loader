namespace PulsarModLoader.Content.Components.Turret
{
    public abstract class TurretMod
    {
        public TurretMod()
        {
        }
        public virtual string Name
        {
            get
            {
                { return ""; }
            }
        }
        public virtual PLShipComponent PLTurret
        {
            get
            {
                { return new PLLaserTurret(); }
            }
        }
    }
}

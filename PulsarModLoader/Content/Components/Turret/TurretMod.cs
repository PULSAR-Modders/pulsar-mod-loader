namespace PulsarModLoader.Content.Components.Turret
{
    public abstract class TurretMod : ComponentModBase
    {
        public TurretMod()
        {
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

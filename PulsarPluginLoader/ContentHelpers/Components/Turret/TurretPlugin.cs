namespace PulsarPluginLoader.ContentHelpers.Components.Turret
{
    public abstract class TurretPlugin
    {
        public TurretPlugin()
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

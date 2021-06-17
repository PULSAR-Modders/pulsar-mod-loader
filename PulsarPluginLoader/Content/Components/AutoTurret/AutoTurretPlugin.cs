namespace PulsarPluginLoader.Content.Components.AutoTurret
{
    public abstract class AutoTurretPlugin
    {
        public AutoTurretPlugin()
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

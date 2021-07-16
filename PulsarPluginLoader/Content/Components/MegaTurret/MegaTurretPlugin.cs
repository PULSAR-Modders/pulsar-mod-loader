namespace PulsarPluginLoader.Content.Components.MegaTurret
{
    public abstract class MegaTurretPlugin
    {
        public MegaTurretPlugin()
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

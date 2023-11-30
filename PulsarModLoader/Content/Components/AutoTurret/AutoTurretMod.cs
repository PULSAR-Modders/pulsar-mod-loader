namespace PulsarModLoader.Content.Components.AutoTurret
{
    /// <summary>
    /// Implements an AutoTurret to be loaded.
    /// </summary>
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

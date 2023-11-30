using UnityEngine;

namespace PulsarModLoader.Content.Components.CPU
{
    /// <summary>
    /// Implements a CPU to be loaded.
    /// </summary>
    public abstract class CPUMod : ComponentModBase
    {
        public CPUMod()
        {
        }
        public virtual float Speed
        {
            get { return .7f; }
        }
        public virtual float Defense
        {
            get { return .1f; }
        }
        public override Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("Icons/62_Processer");  }
        }
        public virtual int SysInstConduit
        {
            get { return -1; }
        }
        public virtual float MaxPowerUsage_Watts
        {
            get { return 1f; }
        }
        public virtual int MaxCompUpgradeLevelBoost
        {
            get { return 0; }
        }
        public virtual int MaxItemUpgradeLevelBoost
        {
            get { return 0; }
        }
        public override int CargoVisualID => 36;
        public virtual void WhenProgramIsRun(PLWarpDriveProgram InProgram)
        {

        }
    }
}

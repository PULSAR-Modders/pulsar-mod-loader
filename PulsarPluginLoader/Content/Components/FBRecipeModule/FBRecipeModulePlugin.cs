using UnityEngine;

namespace PulsarPluginLoader.Content.Components.FBRecipeModule
{
    public abstract class FBRecipeModulePlugin : ComponentPluginBase
    {
        public FBRecipeModulePlugin()
        { 
        }
        public virtual int[] ItemTypeToProduce
        {
            get { return new int[2] { 5, 27 }; }
        }
        public virtual int CookDurationMs
        {
            get { return 2000; }
        }
        public virtual float CookedTimingOffsetMidpoint
        {
            get { return 0.66f; }
        }
        public virtual float PerfectlyCookedMaxTimingOffset
        {
            get { return 0.04f; }
        }
        public virtual float CookedMaxTimingOffset
        {
            get { return 0.1f; }
        }
        public virtual int FoodSupplyCost
        {
            get { return 10; }
        }
        public virtual Sprite OvenIcon
        {
            get { return Resources.Load<Sprite>("BiscuitIcons/Icon_Classic"); }
        }
    }
}

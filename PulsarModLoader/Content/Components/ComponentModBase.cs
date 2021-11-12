using UnityEngine;

namespace PulsarModLoader.Content.Components
{
    public abstract class ComponentModBase
    {
        public virtual string Name
        {
            get { return ""; }
        }
        public virtual string Description
        {
            get { return ""; }
        }
        public virtual int MarketPrice
        {
            get { return 0; }
        }
        public virtual int CargoVisualID
        {
            get { return 1; }
        }
        public virtual bool CanBeDroppedOnShipDeath
        {
            get { return true; }
        }
        public virtual bool Experimental
        {
            get { return false; }
        }
        public virtual bool Unstable
        {
            get { return false; }
        }
        public virtual bool Contraband
        {
            get { return false; }
        }
        public virtual float Price_LevelMultiplierExponent
        {
            get { return 1.4f; }
        }
        public virtual Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("defaultShipCompIcon"); }
        }
        /*public virtual float SalvagePercentMultiplier//not implimented
        {
            get { return 1f; }
        }*/
        public virtual string GetStatLineRight(PLShipComponent InComp)
        {
            return "";
        }
        public virtual string GetStatLineLeft(PLShipComponent InComp)
        {
            return "";
        }
        public virtual void FinalLateAddStats(PLShipComponent InComp)
        {

        }
        public virtual void LateAddStats(PLShipComponent InComp)
        {

        }
        public virtual void AddStats(PLShipComponent InComp)
        {

        }
        public virtual void OnWarp(PLShipComponent InComp)
        {
            if (InComp.Unstable)
		    {
		    	int num = InComp.Unstable_JumpCounter;
		    	InComp.Unstable_JumpCounter = num + 1;
		    }
		    if (InComp.Level == 0)
		    {
		    	InComp.Unstable_JumpCounter = 0;
		    }
		    if (InComp.Unstable_JumpCounter > 6)
		    {
		    	InComp.Unstable_JumpCounter = 0;
		    	if (InComp.Level > 0)
		    	{
		    		int num = InComp.Level;
		    		InComp.Level = num - 1;
                    PulsarModLoader.Utilities.Messaging.Notification(InComp.GetItemName(true) + " has degraded to Level " + (InComp.Level + 1));
		    	}
		    }
        }
        public virtual void Tick(PLShipComponent InComp)
        {

        }
    }
}

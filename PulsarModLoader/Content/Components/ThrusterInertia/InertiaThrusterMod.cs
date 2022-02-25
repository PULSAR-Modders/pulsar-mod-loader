using UnityEngine;
using System.Reflection;
namespace PulsarModLoader.Content.Components.InertiaThruster
{
    public abstract class InertiaThrusterMod : ComponentModBase
    {
        public InertiaThrusterMod()
        {
        }
        public override Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("Icons/81_Thrusters"); }
        }
        public virtual float MaxOutput
        {
            get { return .4f; }
        }
        public virtual float MaxPowerUsage_Watts
        {
            get { return 2600f; }
        }
        public override int CargoVisualID => 8;
        public override string GetStatLineLeft(PLShipComponent InComp)
        {
            return PLLocalize.Localize("Inertia", false) + "\n";
        }
        public override string GetStatLineRight(PLShipComponent InComp)
        {
            PLInertiaThruster me = InComp as PLInertiaThruster;
            return ((float)me.m_MaxOutput * me.LevelMultiplier(0.18f, 1f) * 100f).ToString("0") + "\n";
        }
    }
}

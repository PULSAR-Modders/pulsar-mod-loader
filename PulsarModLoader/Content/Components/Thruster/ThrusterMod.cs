using UnityEngine;
using System.Reflection;
namespace PulsarModLoader.Content.Components.Thruster
{
    public abstract class ThrusterMod : ComponentModBase
    {
        public ThrusterMod()
        {
        }
        public override Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("Icons/81_Thrusters"); }
        }
        public virtual float MaxOutput
        {
            get { return .115f; }
        }
        public virtual float MaxPowerUsage_Watts
        {
            get { return 1200f; }
        }
        public override int CargoVisualID => 8;

        public override string GetStatLineLeft(PLShipComponent InComp)
        {
            return PLLocalize.Localize("Thrust", false) + "\n";
        }
        public override string GetStatLineRight(PLShipComponent InComp)
        {
            PLThruster me = InComp as PLThruster;
            return ((float)me.m_MaxOutput * me.LevelMultiplier(0.18f, 1f) * 100f).ToString("0") + "\n";
        }
    }
}

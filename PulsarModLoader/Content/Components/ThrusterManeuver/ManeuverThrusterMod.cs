using UnityEngine;
using System.Reflection;
namespace PulsarModLoader.Content.Components.ManeuverThruster
{
    public abstract class ManeuverThrusterMod : ComponentModBase
    {
        public ManeuverThrusterMod()
        {
        }
        public override Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("Icons/81_Thrusters"); }
        }
        public virtual float MaxOutput
        {
            get { return .1f; }
        }
        public virtual float MaxPowerUsage_Watts
        {
            get { return 2000f; }
        }
        public override int CargoVisualID => 8;
        public override string GetStatLineLeft(PLShipComponent InComp)
        {
            return PLLocalize.Localize("Maneuver Thrust", false) + "\n";
        }

        public override string GetStatLineRight(PLShipComponent InComp)
        {
            PLManeuverThruster me = InComp as PLManeuverThruster;
            return ((float)me.GetType().GetField("MaxOutput", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(me) * me.LevelMultiplier(0.18f, 1f) * 100f).ToString("0") + "\n";
        }
    }
}

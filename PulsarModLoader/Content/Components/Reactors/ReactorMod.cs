using UnityEngine;
using System.Reflection;

namespace PulsarModLoader.Content.Components.Reactor
{
    public abstract class ReactorMod : ComponentModBase
    {
        public ReactorMod()
        {
        }
        public override Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("Icons/28_Reactor"); }
        }
        public virtual float EnergyOutputMax
        {
            get { return 15000f; }
        }
        public virtual float EnergySignatureAmount
        {
            get { return 18f; }
        }
        public virtual float MaxTemp
        {
            get { return 1800f; }
        }
        public virtual float EmergencyCooldownTime
        {
            get { return 20f; }
        }
        public virtual float HeatOutput
        {
            get { return 1f; }
        }
        public override int CargoVisualID => 15;

        public override string GetStatLineLeft(PLShipComponent InComp)
        {
            return string.Concat(new string[]
            {
            PLLocalize.Localize("Max Temp", false),
            "\n",
            PLLocalize.Localize("Emer. Cooldown", false),
            "\n",
            PLLocalize.Localize("Output", false)
            });
        }

        public override string GetStatLineRight(PLShipComponent InComp)
        {
            PLReactor me = InComp as PLReactor;
            return string.Concat(new string[]
            {
            (me.TempMax * me.LevelMultiplier(0.1f, 1f)).ToString("0"),
            " kP\n",
            me.EmergencyCooldownTime.ToString("0.0"),
            " sec\n",
            ((float)me.GetType().GetField("OriginalEnergyOutputMax", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(me) * me.LevelMultiplier(0.1f, 1f)).ToString("0"),
            " MW"
            });
        }
    }
}

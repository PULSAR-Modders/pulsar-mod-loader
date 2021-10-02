using UnityEngine;

namespace PulsarModLoader.Content.Components.Hull
{
    public abstract class HullMod : ComponentModBase
    {
        public HullMod()
        {
        }
        public override Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("Icons/20_Hull"); }
        }
        public virtual float HullMax
        {
            get { return 750f; }
        }
        public virtual float Armor
        {
            get { return .15f; }
        }
        public virtual float Defense
        {
            get { return .2f; }
        }
        public override int CargoVisualID => 6;
        public override string GetStatLineLeft(PLShipComponent InComp)
        {
            PLHull me = InComp as PLHull;
            if (me.SubType == 9)
            {
                return string.Concat(new string[]
                {
                PLLocalize.Localize("Integrity", false),
                "\n",
                PLLocalize.Localize("Armor", false),
                "\n",
                PLLocalize.Localize("Armor (Max)", false)
                });
            }
            return PLLocalize.Localize("Integrity", false) + "\n" + PLLocalize.Localize("Armor", false);
        }
        public override string GetStatLineRight(PLShipComponent InComp)
        {
            PLHull me = InComp as PLHull;
            if (me.SubType == 9)
            {
                return string.Concat(new string[]
                {
                (me.Max * me.LevelMultiplier(0.2f, 1f)).ToString("0"),
                "\n",
                (me.Armor * 250f * me.LevelMultiplier(0.15f, 1f)).ToString("0"),
                "\n",
                (500f * me.LevelMultiplier(0.15f, 1f)).ToString("0")
                });
            }
            return (me.Max * me.LevelMultiplier(0.2f, 1f)).ToString("0") + "\n" + (this.Armor * 250f * me.LevelMultiplier(0.15f, 1f)).ToString("0");
        }
    }
}

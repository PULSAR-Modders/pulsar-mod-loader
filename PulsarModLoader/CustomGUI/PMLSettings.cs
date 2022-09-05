using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.GUILayout;


namespace PulsarModLoader.CustomGUI
{
    class PMLSettings : ModSettingsMenu
    {
        public override string Name() => "PulsarModLoader";
        public override void Draw()
        {
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            BeginHorizontal();
            {
                Label($"ModInfoTextAnchor: {PMLConfig.ModInfoTextAnchor.ToString()}");

                if (Button("<"))
                    PMLConfig.ModInfoTextAnchor.Value = Enum.GetValues(typeof(TextAnchor)).Cast<TextAnchor>().SkipWhile(e => (int)e != (int)PMLConfig.ModInfoTextAnchor.Value - 1).First();
                if (Button(">"))
                    PMLConfig.ModInfoTextAnchor.Value = Enum.GetValues(typeof(TextAnchor)).Cast<TextAnchor>().SkipWhile(e => (int)e != (int)PMLConfig.ModInfoTextAnchor.Value).Skip(1).First();
            }
            EndHorizontal();
            if (Button("Reset to default")) PMLConfig.SetDefault();
        }
    }
}

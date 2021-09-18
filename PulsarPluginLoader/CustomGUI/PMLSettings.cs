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
                Label($"ModInfoTextAnchor: {PMLConfig.instance.ModInfoTextAnchor.ToString()}");

                if (Button("<"))
                    PMLConfig.instance.ModInfoTextAnchor = Enum.GetValues(typeof(TextAnchor)).Cast<TextAnchor>().SkipWhile(e => (int)e != (int)PMLConfig.instance.ModInfoTextAnchor - 1).First();
                if (Button(">"))
                    PMLConfig.instance.ModInfoTextAnchor = Enum.GetValues(typeof(TextAnchor)).Cast<TextAnchor>().SkipWhile(e => (int)e != (int)PMLConfig.instance.ModInfoTextAnchor).Skip(1).First();
            }
            EndHorizontal();
            BeginHorizontal();
            {
                if (Button("Save")) PMLConfig.SaveConfig(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/PulsarModLoaderConfig.json");
                if (Button("Reset to default")) PMLConfig.CreateDefaultConfig(string.Empty, false);
            }
            EndHorizontal();
        }
    }
}

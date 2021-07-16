using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.GUILayout;


namespace PulsarPluginLoader.CustomGUI
{
    class PPLSettings : ModSettingsMenu
    {
        public override string Name() => "PulsarPluginLoader";
        public override void Draw()
        {
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            BeginHorizontal();
            {
                Label($"ModInfoTextAnchor: {PPLConfig.instance.ModInfoTextAnchor.ToString()}");

                if (Button("<"))
                    PPLConfig.instance.ModInfoTextAnchor = Enum.GetValues(typeof(TextAnchor)).Cast<TextAnchor>().SkipWhile(e => (int)e != (int)PPLConfig.instance.ModInfoTextAnchor - 1).First();
                if (Button(">"))
                    PPLConfig.instance.ModInfoTextAnchor = Enum.GetValues(typeof(TextAnchor)).Cast<TextAnchor>().SkipWhile(e => (int)e != (int)PPLConfig.instance.ModInfoTextAnchor).Skip(1).First();
            }
            EndHorizontal();
            BeginHorizontal();
            {
                if (Button("Save")) PPLConfig.SaveConfig(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/PulsarPluginLoaderConfig.json");
                if (Button("Reset to default")) PPLConfig.CreateDefaultConfig(string.Empty, false);
            }
            EndHorizontal();
        }
    }
}

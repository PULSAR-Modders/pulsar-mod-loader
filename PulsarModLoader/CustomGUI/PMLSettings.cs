using System;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEngine.GUILayout;


namespace PulsarModLoader.CustomGUI
{
    class PMLSettings : ModSettingsMenu
    {
        public override string Name() => "PulsarModLoader";
        public override void Draw()
        {
            if (Button("Debug Mode: " + (PMLConfig.DebugMode ? "Enabled" : "Disabled")))
            {
                PMLConfig.DebugMode.Value = !PMLConfig.DebugMode.Value;

                if (!PMLConfig.DebugMode)
                {
                    PLInGameUI.Instance.CurrentVersionLabel.text = PulsarModLoader.Patches.GameVersion.Version;
                }
            }

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

            //Zip Mod Settings
            HorizontalSlider(0, 100, 100);
            BeginHorizontal();
            FlexibleSpace();
            Label("Zip Mods");
            FlexibleSpace();
            EndHorizontal();
            BeginHorizontal();
            Label($"Load Mods From Zips: {PMLConfig.ZipModLoad.Value}");
            if (Button("Toggle Loading Of Zip Mods"))
            {
                PMLConfig.ZipModLoad.Value = !PMLConfig.ZipModLoad.Value;
            }

            EndHorizontal();
            BeginHorizontal();
            Label($"Delete Zips After Load: {PMLConfig.ZipModMode}");
            if (Button("Toggle Zip Mod Mode"))
            {
                PMLConfig.ZipModMode.Value = !PMLConfig.ZipModMode.Value;
            }
            EndHorizontal();

            //Max Load Size Settings
            HorizontalSlider(0, 100, 100);
            BeginHorizontal();
            FlexibleSpace();
            Label($"File Size Loading Limits\nCurrent Size: {PMLConfig.MaxLoadSizeBytes.Value / 1048576}MiB");
            FlexibleSpace();
            EndHorizontal();
            BeginHorizontal();
            if(Button("-10MiB") && PMLConfig.MaxLoadSizeBytes.Value > PMLConfig.DefaultMaxLoadSizeBytes)
            {
                PMLConfig.MaxLoadSizeBytes.Value = PMLConfig.MaxLoadSizeBytes.Value - PMLConfig.DefaultMaxLoadSizeBytes;
            }
            if(Button("Default"))
            {
                PMLConfig.MaxLoadSizeBytes.Value = PMLConfig.DefaultMaxLoadSizeBytes;
            }
            if (Button("+10MiB") && PMLConfig.MaxLoadSizeBytes.Value <= (uint.MaxValue - PMLConfig.DefaultMaxLoadSizeBytes))
            {
                PMLConfig.MaxLoadSizeBytes.Value = PMLConfig.MaxLoadSizeBytes.Value + PMLConfig.DefaultMaxLoadSizeBytes;
            }
            EndHorizontal();
            Label("Dont Change This Unless You Understand What It Does");

            //Readme Loading Settings
            HorizontalSlider(0, 0, 0);
            BeginHorizontal();
            FlexibleSpace();
            Label("Readme Settings");
            FlexibleSpace();
            EndHorizontal();
            if (Button($"Readmes Will be loaded: {(PMLConfig.AutoPullReadme.Value ? "Automatically" : "Manually")}"))
            {
                PMLConfig.AutoPullReadme.Value = !PMLConfig.AutoPullReadme.Value;
            }
        }
    }
}
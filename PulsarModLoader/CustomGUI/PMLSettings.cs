using System;
using System.Linq;
using UnityEngine;
using static UnityEngine.GUILayout;


namespace PulsarModLoader.CustomGUI
{
    class PMLSettings : ModSettingsMenu
    {
        public override string Name() => "PulsarModLoader";

        string SizeX = string.Empty;
        string SizeY = string.Empty;
        string ModListSizeX = string.Empty;
        string SizeErrString = string.Empty;

        public override void OnOpen()
        {
            SizeX = GUIMain.Width.ToString();
            SizeY = GUIMain.Height.ToString();
            ModListSizeX = GUIMain.ModlistWidth.ToString();
        }


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


            //Size Settings
            HorizontalSlider(0, 100, 100);
            UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            Label("ModManager Size");

            UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleRight;
            BeginHorizontal();
            Label("Width:");
            SizeX = TextField(SizeX);
            EndHorizontal();
            BeginHorizontal();
            Label("Height:");
            SizeY = TextField(SizeY);
            EndHorizontal();
            BeginHorizontal();
            Label("Modlist Scrollbar Width:");
            ModListSizeX = TextField(ModListSizeX);
            EndHorizontal();

            UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            if (SizeErrString != string.Empty)
            {
                Label($"<color=red>{SizeErrString}</color>");
            }

            if(Button("Apply Size"))
            {
                if(!float.TryParse(SizeX, out float X) || !float.TryParse(SizeY, out float Y) || !float.TryParse(ModListSizeX, out float MLx))
                {
                    SizeErrString = "Size values are not numbers";
                }
                else
                {
                    if (X < .3 || Y < .3 || MLx < .2)
                    {
                        SizeErrString = "Size Values cannot be smaller than .3, .3, and .2";
                    }
                    else
                    {
                        GUIMain.Height.Value = Y;
                        GUIMain.Width.Value= X;
                        GUIMain.ModlistWidth.Value = MLx;
                        SizeErrString = string.Empty;
                        GUIMain.Instance.updateWindowSize();
                    }
                }
            }

            //Cursor Unlock Toggle
            HorizontalSlider(0, 100, 100);
            if (Button($"Unlock Cursor While Open: {(GUIMain.UnlockCursorWhileOpen.Value ? "Enabled" : "Disabled")}"))
            {
                GUIMain.UnlockCursorWhileOpen.Value = !GUIMain.UnlockCursorWhileOpen.Value;
            }


            //Zip Mod Settings
            HorizontalSlider(0, 100, 100);
            Label("Zip Mods");

            UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
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


            UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            //Max Load Size Settings
            HorizontalSlider(0, 100, 100);
            Label($"File Size Loading Limits\nCurrent Size: {PMLConfig.MaxLoadSizeBytes.Value / 1048576}MiB");
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
            Label("Readme Settings");
            if (Button($"Readmes Will be loaded: {(PMLConfig.AutoPullReadme.Value ? "Automatically" : "Manually")}"))
            {
                PMLConfig.AutoPullReadme.Value = !PMLConfig.AutoPullReadme.Value;
            }
        }
    }
}
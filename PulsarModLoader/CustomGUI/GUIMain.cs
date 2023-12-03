using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using PulsarModLoader.Utilities;
using UnityEngine;
using static UnityEngine.GUILayout;
using System.Net.Http;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace PulsarModLoader.CustomGUI
{
    internal class GUIMain : MonoBehaviour
    {
        Dictionary<string, string> Readme = new Dictionary<string, string>();
        readonly CultureInfo ci;
        public static GUIMain Instance = null;
        GameObject Background;
        UnityEngine.UI.Image Image;
        public bool GUIActive = false;
        internal static SaveValue<float> Height = new SaveValue<float>("ModManagerHight", .40f);
        internal static SaveValue<float> Width = new SaveValue<float>("ModManagerWidth", .40f);
        internal static SaveValue<float> ModlistWidth = new SaveValue<float>("ModManagerModlistWidth", .30f);
        internal static SaveValue<bool> UnlockCursorWhileOpen = new SaveValue<bool>("UnlockCursorWhileOpen", true);
        Rect Window;
        byte Tab = 0;
        
        List<PulsarMod> mods = new List<PulsarMod>(8);
        ushort selectedMod = ushort.MaxValue;

        Rect ModListArea;
        Vector2 ModListScroll = Vector2.zero;

        Rect ModInfoArea;
        Vector2 ModInfoScroll = Vector2.zero;

        Rect ModSettingsArea;
        Vector2 ModSettingsScroll = Vector2.zero;
        List<ModSettingsMenu> settings = new List<ModSettingsMenu>(3);
        ushort selectedSettings = ushort.MaxValue;

        public bool ShouldUnlockCursor()
        {
            return UnlockCursorWhileOpen && GUIActive;
        }

        internal void updateWindowSize()
        {
            Window = new Rect((Screen.width * .5f - ((Screen.width * Width) / 2)), Screen.height * .5f - ((Screen.height * Height) / 2), Screen.width * Width, Screen.height * Height);
            ModListArea = new Rect(6, 43, Window.width * ModlistWidth, Screen.height * Height - 45);
            ModInfoArea = new Rect(ModListArea.width + 15, 43, Screen.width * Width - (ModListArea.width + 11), Screen.height * Height - 45);
            ModSettingsArea = new Rect(6, 43, Screen.width * Width - 12, Screen.height * Height - 45);
        }

        internal GUIMain()
        {
            Instance = this;
            updateWindowSize();
            settings.Add(new PMLSettings());
            ModManager.Instance.OnModUnloaded += UpdateOnModRemoved;
            ModManager.Instance.OnModSuccessfullyLoaded += UpdateOnModLoaded;

            //Background image to block mouse clicks passing IMGUI
            Background = new GameObject("GUIMainBG");
            Image = Background.AddComponent<UnityEngine.UI.Image>();
            Image.color = Color.clear;
            Background.transform.SetParent(PLUIMainMenu.Instance.gameObject.transform);
            UnityEngine.Object.DontDestroyOnLoad(Background);
            Background.SetActive(false);
        }

        void Awake()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                GUIActive = !GUIActive;
                if(GUIActive)
                {
                    GUIOpen();
                }
                else
                {
                    GUIClose();
                }
            }
        }

        void GUIOpen()
        {
            if (selectedSettings != ushort.MaxValue) //Menu Opening and MM selected
            {
                settings[selectedSettings].OnOpen();
            }

            Background.SetActive(true);
        }

        void GUIClose()
        {
            if (selectedSettings != ushort.MaxValue) //Menu Closing and MM Selected
            {
                settings[selectedSettings].OnClose();
            }

            Background.SetActive(false);
        }
        
        void OnGUI()
        {
            if (GUIActive)
            {
                GUI.skin = ChangeSkin();
                Window = GUI.Window(999910, Window, WindowFunction, "ModManager");

                //float y = Window.center.y * 2 * -1;
                Image.rectTransform.position = new Vector3(Window.center.x, (Window.center.y * - 1) + Screen.height, 0);
                Image.rectTransform.sizeDelta = Window.size;
            }
        }

        private void GetReadme(string ModName, string ModURL)
        {
            bool ReadmeLocked = Readme.ContainsKey(ModName);
            if (!ReadmeLocked)
            {
                Readme.Add(ModName, String.Empty);
                if (ModURL.StartsWith("file://", false, ci))
                {
                    string ModZip = Path.Combine(Directory.GetCurrentDirectory(), "Mods", ModName + ".zip");
                    if (PMLConfig.ZipModLoad && !PMLConfig.ZipModMode && File.Exists(ModZip))
                    {
                        using (ZipArchive Archive = ZipFile.OpenRead(ModZip))
                        {
                            foreach (ZipArchiveEntry Entry in Archive.Entries)
                            {
                                if (Entry.FullName.EndsWith(ModURL.Replace("file://", String.Empty).Trim('/'), StringComparison.OrdinalIgnoreCase))
                                {
                                    if (Entry.Length > PMLConfig.MaxLoadSizeBytes.Value)
                                    {
                                        StreamReader StreamReadme = new StreamReader(Entry.Open());
                                        Readme[ModName] = StreamReadme.ReadToEnd();
                                    }
                                    else
                                    {
                                        Readme[ModName] = $"Error: Readme is too large.";
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Readme[ModName] = "Error: Readme not found.";
                    }
                }
                else
                {
                    using (var Client = new HttpClient())
                    {
                        Client.MaxResponseContentBufferSize = PMLConfig.MaxLoadSizeBytes.Value;
                        using (HttpResponseMessage Response = Client.GetAsync(ModURL).Result)
                        {
                            if (Response.IsSuccessStatusCode)
                            {
                                //Readme.Add(ModName, Response.Content.ReadAsStringAsync().Result); //Since we lock using string.empty, we must replace the value. 
                                Readme[ModName] = Response.Content.ReadAsStringAsync().Result;
                            }
                            else
                            {
                                Readme[ModName] = $"Error: HTTP Code {Response.StatusCode}.";
                            }

                        }
                    }
                }
            }
        }

        void WindowFunction(int WindowID)
        {
            
            BeginHorizontal(); // TAB Start
            {
                if (Button("Mod Info"))
                    Tab = 0;
                if (Button("Mod Settings"))
                    Tab = 1;
                if (Button("About"))
                    Tab = 2;
            }
            EndHorizontal(); // TAB End
            switch (Tab)
            {
                #region ModList and ModInfo
                case 0:
                    GUI.skin.label.alignment = PMLConfig.ModInfoTextAnchor;
                    BeginArea(ModListArea);
                    {
                        ModListScroll = BeginScrollView(ModListScroll);
                        {
                            for (ushort p = 0; p < mods.Count; p++)
                            {
                                var mod = mods[p];
                                var name = mods[p].Name;
                                if (ModManager.Instance.UpdatesAviable.Any(m => m.Mod == mod))
                                    name = "(!) " + name;
								if (Button(name))
									selectedMod = p;
							}
                        }
                        EndScrollView();
                    }
                    EndArea();
                    BeginArea(ModInfoArea);
                    {
                        if (selectedMod != ushort.MaxValue)
                        {
                            ModInfoScroll = BeginScrollView(ModInfoScroll);
                            {
                                PulsarMod mod = mods[selectedMod];
                                BeginHorizontal();
                                {
                                    if (Button("Unload"))
                                        mod.Unload();
                                }
                                EndHorizontal();
                                Label($"Author: {mod.Author}");
                                Label($"Name: {mod.Name}");
                                Label($"Version: {mod.Version}");
                                Label($"License: {mod.License}");
                                if(!string.IsNullOrEmpty(mod.SourceURL))
                                {
                                    Label($"SourceURL: {mod.SourceURL}");
                                }
                                if (mod.ShortDescription != string.Empty)
                                    Label($"Short Description: {mod.ShortDescription}");
                                if (mod.LongDescription != string.Empty)
                                    Label($"Long Description: {mod.LongDescription}");
                                Label($"MPRequirement: {((MPModChecks.MPRequirement)mod.MPRequirements).ToString()}");
                                Space(1f);
                                var result = ModManager.Instance.UpdatesAviable.FirstOrDefault(av => av.Mod == mod);
								if (result != null)
                                {
                                    if (result.IsUpdated)
                                        Label("Restart the game to apply the changes!");
									else if(Button($"Update this mod to version {result.Data.Version}?"))
                                            ModUpdateCheck.UpdateMod(result);
								}

                                //Get Readme from URL
                                if (!string.IsNullOrEmpty(mod.ReadmeURL)) 
                                {
                                    bool ReadmeLocked = Readme.TryGetValue(mod.Name, out string ReadmeValue);
                                    bool ReadmeEmpty = string.IsNullOrEmpty(ReadmeValue);
//                                    Logger.Info($"locked,empty:{ReadmeLocked},{ReadmeEmpty}");
                                    if (ReadmeEmpty && !ReadmeLocked)
                                    {
                                        if (PMLConfig.AutoPullReadme.Value || Button("Load Readme"))
                                        {
                                            new Thread(() => { GetReadme(mod.Name, mod.ReadmeURL); }).Start();
                                        }
                                    }
                                   else
                                   {
                                        Label($"Readme:\n\n{Readme[mod.Name]}");
                                   }
                                }
                            }
                            EndScrollView();
                        }
                    }
                    EndArea();
                    break;
                #endregion
                #region ModSettings
                case 1:
                    GUI.skin.label.alignment = TextAnchor.UpperLeft;
                    BeginArea(ModSettingsArea);
                    {
                        ModSettingsScroll = BeginScrollView(ModSettingsScroll);
                        {
                            if (selectedSettings == ushort.MaxValue)
                            {
                                for (ushort msm = 0; msm < settings.Count; msm++)
                                {
                                    if (Button(settings[msm].Name()))
                                    {
                                        settings[msm].OnOpen();
                                        selectedSettings = msm;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (Button("Back"))
                                {
                                    settings[selectedSettings].OnClose();
                                    selectedSettings = ushort.MaxValue;
                                }
                                else
                                {
                                    settings[selectedSettings].Draw();
                                }
                            }
                        }
                        EndScrollView();
                    }
                    EndArea();
                    break;
                #endregion
                #region About
                case 2:
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    Label($"PulsarModLoader - Unofficial mod loader for PULSAR: Lost Colony.");
                    Label($"Version: {ModManager.Instance.PMLVersionInfo.FileVersion}");
                    Label($"\n\nDeveloped by Tom Richter");
                    Label($"Contributors:\nDragonFire47\n18107\nBadRyuner");
                    BeginHorizontal();
                    if (Button("Github"))
                        Process.Start("https://github.com/PULSAR-Modders/pulsar-mod-loader");
                    if (Button("Discord"))
                        Process.Start("https://discord.gg/j3Pydn6");
                    EndHorizontal();
                    break;
                    #endregion
            }
            GUI.DragWindow();
        }

        internal static GUISkin _cachedSkin;
        private static readonly Color32 _classicMenuBackground = new Color32(32,32,32, 255);
        private static readonly Color32 _classicButtonBackground = new Color32(40,40,40, 255);
        private static readonly Color32 _hoverButtonFromMenu = new Color32(18,79,179, 255);
        GUISkin ChangeSkin()
        {
            if (_cachedSkin is null || _cachedSkin.window.active.background is null)
            {
                _cachedSkin = GUI.skin;
                Texture2D windowBackground = BuildTexFrom1Color(_classicMenuBackground);
                _cachedSkin.window.active.background = windowBackground;
                _cachedSkin.window.onActive.background = windowBackground;
                _cachedSkin.window.focused.background = windowBackground;
                _cachedSkin.window.onFocused.background = windowBackground;
                _cachedSkin.window.hover.background = windowBackground;
                _cachedSkin.window.onHover.background = windowBackground;
                _cachedSkin.window.normal.background = windowBackground;
                _cachedSkin.window.onNormal.background = windowBackground;
                
                _cachedSkin.window.hover.textColor = Color.white;
                _cachedSkin.window.onHover.textColor = Color.white;

                Color32 hoverbutton = PLServer.Instance == null || PLNetworkManager.Instance?.LocalPlayer == null
	                ? _hoverButtonFromMenu
	                : (Color32)PLPlayer.GetClassColorFromID(PLNetworkManager.Instance.LocalPlayer.ClassID);

                Texture2D buttonBackground = BuildTexFrom1Color(_classicButtonBackground);
                Texture2D hbuttonBackground = BuildTexFrom1Color(hoverbutton);
                _cachedSkin.button.active.background = buttonBackground;
                _cachedSkin.button.onActive.background = buttonBackground;
                _cachedSkin.button.focused.background = buttonBackground;
                _cachedSkin.button.onFocused.background = buttonBackground;
                _cachedSkin.button.hover.background = hbuttonBackground;
                _cachedSkin.button.onHover.background = hbuttonBackground;
                _cachedSkin.button.normal.background = buttonBackground;
                _cachedSkin.button.onNormal.background = buttonBackground;

                _cachedSkin.horizontalSlider.active.background = PLGlobal.Instance.SliderBG;
                _cachedSkin.horizontalSlider.onActive.background = PLGlobal.Instance.SliderBG;
                _cachedSkin.horizontalSlider.focused.background = PLGlobal.Instance.SliderBG;
                _cachedSkin.horizontalSlider.onFocused.background = PLGlobal.Instance.SliderBG;
                _cachedSkin.horizontalSlider.hover.background = PLGlobal.Instance.SliderBG;
                _cachedSkin.horizontalSlider.onHover.background = PLGlobal.Instance.SliderBG;
                _cachedSkin.horizontalSlider.normal.background = PLGlobal.Instance.SliderBG;
                _cachedSkin.horizontalSlider.onNormal.background = PLGlobal.Instance.SliderBG;

                _cachedSkin.horizontalSliderThumb.active.background = PLGlobal.Instance.SliderHandle;
                _cachedSkin.horizontalSliderThumb.onActive.background = PLGlobal.Instance.SliderHandle;
                _cachedSkin.horizontalSliderThumb.focused.background = PLGlobal.Instance.SliderHandle;
                _cachedSkin.horizontalSliderThumb.onFocused.background = PLGlobal.Instance.SliderHandle;
                _cachedSkin.horizontalSliderThumb.hover.background = PLGlobal.Instance.SliderHandle;
                _cachedSkin.horizontalSliderThumb.onHover.background = PLGlobal.Instance.SliderHandle;
                _cachedSkin.horizontalSliderThumb.normal.background = PLGlobal.Instance.SliderHandle;
                _cachedSkin.horizontalSliderThumb.onNormal.background = PLGlobal.Instance.SliderHandle;

                Texture2D textfield = BuildTexFromColorArray(new Color[] { _classicButtonBackground, _classicButtonBackground, _classicMenuBackground,
                _classicMenuBackground, _classicMenuBackground, _classicMenuBackground , _classicMenuBackground}, 1, 7);
                _cachedSkin.textField.active.background = textfield;
                _cachedSkin.textField.onActive.background = textfield;
                _cachedSkin.textField.focused.background = textfield;
                _cachedSkin.textField.onFocused.background = textfield;
                _cachedSkin.textField.hover.background = textfield;
                _cachedSkin.textField.onHover.background = textfield;
                _cachedSkin.textField.normal.background = textfield;
                _cachedSkin.textField.onNormal.background = textfield;

                _cachedSkin.textField.active.textColor = hoverbutton;
                _cachedSkin.textField.onActive.textColor = hoverbutton;
                _cachedSkin.textField.hover.textColor = hoverbutton;
                _cachedSkin.textField.onHover.textColor = hoverbutton;

                UnityEngine.Object.DontDestroyOnLoad(windowBackground);
                UnityEngine.Object.DontDestroyOnLoad(buttonBackground);
                UnityEngine.Object.DontDestroyOnLoad(hbuttonBackground);
                UnityEngine.Object.DontDestroyOnLoad(textfield);
                UnityEngine.Object.DontDestroyOnLoad(_cachedSkin);
                // TODO: Add custom skin for Toggle and other items
            }

            return _cachedSkin;
        }

        Texture2D BuildTexFrom1Color(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0,0, color);
            tex.Apply();
            return tex;
        }

        Texture2D BuildTexFromColorArray(Color[] color, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height);
            tex.SetPixels(color);
            tex.Apply();
            return tex;
        }

        void UpdateOnModRemoved(PulsarMod mod)
        {
            selectedMod = ushort.MaxValue;
            mods.Remove(mod);
            List<ModSettingsMenu> settingsToRemove = new List<ModSettingsMenu>();
            Assembly asm = mod.GetType().Assembly;
            settings.AsParallel().ForAll((arg) => { if (arg.GetType().Assembly == asm) settingsToRemove.Add(arg);});
            for (byte s = 0; s < settingsToRemove.Count; s++)
                settings.Remove(settingsToRemove[s]);
            settingsToRemove = null;
        }

        void UpdateOnModLoaded(string modName, PulsarMod mod)
        {
            mods.Add(mod);
            var modsettingstype = typeof(ModSettingsMenu);
            mod.GetType().Assembly.GetTypes().AsParallel().ForAll((type) =>
            {
                if (modsettingstype.IsAssignableFrom(type))
                {
                    #if DEBUG
                    Utilities.Logger.Info($"Loaded new settings! {type.FullName}"); 
                    #endif
                    settings.Add(Activator.CreateInstance(type) as ModSettingsMenu);
                }
            });
        }
    }

    [HarmonyPatch(typeof(PLPlayer), nameof(PLPlayer.SetClassID))]
    static class UpdateGUIColorOnClassChanged
    {
	    static void Postfix() => GUIMain._cachedSkin = null;
    }
}

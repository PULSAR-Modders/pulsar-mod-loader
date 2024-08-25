using HarmonyLib;
using PulsarModLoader.MPModChecks;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using UnityEngine;
using static UnityEngine.GUILayout;

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
        internal static SaveValue<float> Height = new SaveValue<float>("ModManagerHight", .50f);
        internal static SaveValue<float> Width = new SaveValue<float>("ModManagerWidth", .50f);
        internal static SaveValue<float> ModlistWidth = new SaveValue<float>("ModManagerModlistWidth", .30f);
        internal static SaveValue<float> PlayerlistWidth = new SaveValue<float>("ModManagerModlistWidth", .30f);
        internal static SaveValue<bool> UnlockCursorWhileOpen = new SaveValue<bool>("UnlockCursorWhileOpen", true);
        Rect Window;
        byte Tab = 0;

        List<PulsarMod> mods = new List<PulsarMod>(8);
        PulsarMod selectedMod;

        Rect ModListArea;
        Vector2 ModListScroll = Vector2.zero;

        Rect ModInfoArea;
        Vector2 ModInfoScroll = Vector2.zero;

        Rect PlayerListArea;
        Vector2 PlayerListScroll = Vector2.zero;
        PhotonPlayer selectedPlayer;

        Rect PlayerModInfoArea;
        Vector2 PlayerModInfoScroll = Vector2.zero;


        Rect ModSettingsArea;
        Vector2 ModSettingsScroll = Vector2.zero;
        List<ModSettingsMenu> settings = new List<ModSettingsMenu>(3);
        ModSettingsMenu selectedSettings;

        public bool ShouldUnlockCursor()
        {
            return UnlockCursorWhileOpen && GUIActive;
        }

        internal void UpdateWindowSize()
        {
            Window = new Rect((Screen.width * .5f - ((Screen.width * Width) / 2)), Screen.height * .5f - ((Screen.height * Height) / 2), Screen.width * Width, Screen.height * Height);
            ModListArea = new Rect(6, 43, Window.width * ModlistWidth, Screen.height * Height - 45);
            ModInfoArea = new Rect(ModListArea.width + 15, 43, (Screen.width * Width - (ModListArea.width + 11)) - 10, Screen.height * Height - 45);
            PlayerListArea = new Rect(6, 43, Window.width * PlayerlistWidth, Screen.height * Height - 45);
            PlayerModInfoArea = new Rect(PlayerListArea.width + 15, 43, (Screen.width * Width - (PlayerListArea.width + 11)) - 10, Screen.height * Height - 45);
            ModSettingsArea = new Rect(6, 43, Screen.width * Width - 12, Screen.height * Height - 45);
        }

        internal GUIMain()
        {
            Instance = this;
            UpdateWindowSize();
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        void Awake()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                GUIActive = !GUIActive;
                if (GUIActive)
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
            selectedSettings?.OnOpen(); //Menu Opening and MM selected

            Background.SetActive(true);
        }

        void GUIClose()
        {
            selectedSettings?.OnClose(); //Menu Closing and MM Selected

            Background.SetActive(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        void OnGUI()
        {
            if (GUIActive)
            {
                GUI.skin = ChangeSkin();
                Window = GUI.Window(999910, Window, WindowFunction, "PML F5 Menu");

                //float y = Window.center.y * 2 * -1;
                Image.rectTransform.position = new Vector3(Window.center.x, (Window.center.y * -1) + Screen.height, 0);
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
                if (DrawButtonSelected("Mod Info", Tab == 0))
                    Tab = 0;
                if (DrawButtonSelected("Mod Settings", Tab == 1))
                    Tab = 1;
                if (DrawButtonSelected("Player List", Tab == 2))
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
                            if (DrawButtonSelected("Pulsar Mod Loader", selectedMod == null))
                            {
                                selectedMod = null;
                            }
                            foreach (PulsarMod mod in mods)
                            {
                                DrawModListModButton(mod);
                            }
                            Label("Overall MPType: " + GetColoredMPTypeText(MPModCheckManager.Instance.HighestLevelOfMPMods));
                        }
                        EndScrollView();
                    }
                    EndArea();
                    BeginArea(ModInfoArea);
                    {
                        ModInfoScroll = BeginScrollView(ModInfoScroll);
                        {
                            if (selectedMod != null)
                            {
                                BeginHorizontal();
                                {
                                    if (Button("Unload"))
                                        selectedMod.Unload();
                                }
                                EndHorizontal();
                                Label($"Author: {selectedMod.Author}");
                                Label($"Name: {selectedMod.Name}");
                                Label($"Version: {selectedMod.Version}");
                                Label($"License: {selectedMod.License}");
                                if (!string.IsNullOrEmpty(selectedMod.SourceURL))
                                {
                                    Label($"SourceURL: {selectedMod.SourceURL}");
                                }
                                if (selectedMod.ShortDescription != string.Empty)
                                    Label($"Short Description: {selectedMod.ShortDescription}");
                                if (selectedMod.LongDescription != string.Empty)
                                    Label($"Long Description: {selectedMod.LongDescription}");
                                Label($"MPRequirement: {(MPModChecks.MPRequirement)selectedMod.MPRequirements}");
                                Space(1f);
                                var result = ModManager.Instance.UpdatesAviable.FirstOrDefault(av => av.Mod == selectedMod);
                                if (result != null)
                                {
                                    if (result.IsUpdated)
                                        Label("Restart the game to apply the changes!");
                                    else if (Button($"Update this mod to version {result.Data.Version}?"))
                                        ModUpdateCheck.UpdateMod(result);
                                }

                                //Get Readme from URL
                                if (!string.IsNullOrEmpty(selectedMod.ReadmeURL))
                                {
                                    bool ReadmeLocked = Readme.TryGetValue(selectedMod.Name, out string ReadmeValue);
                                    bool ReadmeEmpty = string.IsNullOrEmpty(ReadmeValue);
                                    //                                    Logger.Info($"locked,empty:{ReadmeLocked},{ReadmeEmpty}");
                                    if (ReadmeEmpty && !ReadmeLocked)
                                    {
                                        if (PMLConfig.AutoPullReadme.Value || Button("Load Readme"))
                                        {
                                            new Thread(() => { GetReadme(selectedMod.Name, selectedMod.ReadmeURL); }).Start();
                                        }
                                    }
                                    else
                                    {
                                        Label($"Readme:\n\n{Readme[selectedMod.Name]}");
                                    }
                                }

                                Label("\nSettings menus:");
                                foreach (ModSettingsMenu MSM in settings)
                                {
                                    if (MSM.MyMod == selectedMod && Button(MSM.Name()))
                                    {
                                        OpenSettingsMenu(MSM);
                                    }
                                }
                            }
                            else
                            {
                                //PML About page when no mod selected
                                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                                Label($"PulsarModLoader - Unofficial mod loader for PULSAR: Lost Colony.");
                                Label($"Version: {ModManager.Instance.PMLVersionInfo.FileVersion}");
                                Label($"\n\nDeveloped by Tom Richter, Dragon, 18107, BadRyuner");
                                BeginHorizontal();
                                FlexibleSpace();
                                if (Button("Github"))
                                    Application.OpenURL("https://github.com/PULSAR-Modders/pulsar-mod-loader");
                                if (Button("Discord"))
                                    Application.OpenURL("https://discord.gg/j3Pydn6");
                                FlexibleSpace();
                                EndHorizontal();
                            }
                        }
                        EndScrollView();
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
                            if (selectedSettings == null)
                            {
                                foreach (ModSettingsMenu msm in settings)
                                {
                                    if (Button(msm.Name()))
                                    {
                                        OpenSettingsMenu(msm);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (Button("Back"))
                                {
                                    selectedSettings.OnClose();
                                    selectedSettings = null;
                                }
                                else
                                {
                                    selectedSettings.Draw();
                                }
                            }
                        }
                        EndScrollView();
                    }
                    EndArea();
                    break;
                #endregion
                #region Player List
                case 2:
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    if (PhotonNetwork.room == null || PLServer.Instance == null)
                    {
                        Label("Not in a game");
                        break;
                    }
                    BeginArea(PlayerListArea);
                    {
                        PlayerListScroll = BeginScrollView(PlayerListScroll);
                        {
                            foreach (PhotonPlayer player in PhotonNetwork.playerList)
                            {
                                PLPlayer plplayer = PLServer.GetPlayerForPhotonPlayer(player);
                                if (player.IsLocal || plplayer == null)
                                    continue;
                                if (DrawButtonSelected(plplayer.GetPlayerName(), selectedPlayer == player))
                                    selectedPlayer = player;
                            }
                        }
                        EndScrollView();
                    }
                    EndArea();
                    BeginArea(PlayerModInfoArea);
                    {
                        PlayerModInfoScroll = BeginScrollView(PlayerModInfoScroll);
                        {
                            if (selectedPlayer != null)
                            {
                                Label($"Player: {selectedPlayer.NickName} {(selectedPlayer.IsMasterClient ? "(Host)" : string.Empty)}");

                                DrawPlayerModList(selectedPlayer);
                            }
                        }
                        EndScrollView();
                    }
                    EndArea();
                    break;
                    #endregion
            }
            GUI.DragWindow();
        }

        internal static GUISkin _cachedSkin;
        internal static GUIStyle _SelectedButtonStyle;
        Texture2D _buttonBackground;
        Texture2D _hbuttonBackground;
        private static readonly Color32 _classicMenuBackground = new Color32(32, 32, 32, 255);
        private static readonly Color32 _classicButtonBackground = new Color32(40, 40, 40, 255);
        private static readonly Color32 _hoverButtonFromMenu = new Color32(18, 79, 179, 255);
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

                _buttonBackground = BuildTexFrom1Color(_classicButtonBackground);
                _hbuttonBackground = BuildTexFrom1Color(hoverbutton);
                _cachedSkin.button.active.background = _buttonBackground;
                _cachedSkin.button.onActive.background = _buttonBackground;
                _cachedSkin.button.focused.background = _buttonBackground;
                _cachedSkin.button.onFocused.background = _buttonBackground;
                _cachedSkin.button.hover.background = _hbuttonBackground;
                _cachedSkin.button.onHover.background = _hbuttonBackground;
                _cachedSkin.button.normal.background = _buttonBackground;
                _cachedSkin.button.onNormal.background = _buttonBackground;

                _SelectedButtonStyle = new GUIStyle(_cachedSkin.button);
                _SelectedButtonStyle.active.background = _hbuttonBackground;
                _SelectedButtonStyle.focused.background = _hbuttonBackground;
                _SelectedButtonStyle.normal.background = _hbuttonBackground;

                _cachedSkin.horizontalSlider.active.background = PLGlobal.Instance.SliderBG;
                _cachedSkin.horizontalSlider.focused.background = PLGlobal.Instance.SliderBG;
                _cachedSkin.horizontalSlider.hover.background = PLGlobal.Instance.SliderBG;
                _cachedSkin.horizontalSlider.normal.background = PLGlobal.Instance.SliderBG;
                _cachedSkin.horizontalSlider.onActive.background = PLGlobal.Instance.SliderBG;
                _cachedSkin.horizontalSlider.onFocused.background = PLGlobal.Instance.SliderBG;
                _cachedSkin.horizontalSlider.onHover.background = PLGlobal.Instance.SliderBG;
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
                UnityEngine.Object.DontDestroyOnLoad(_buttonBackground);
                UnityEngine.Object.DontDestroyOnLoad(_hbuttonBackground);
                UnityEngine.Object.DontDestroyOnLoad(textfield);
                UnityEngine.Object.DontDestroyOnLoad(_cachedSkin);
                // TODO: Add custom skin for Toggle and other items
            }

            return _cachedSkin;
        }

        Texture2D BuildTexFrom1Color(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
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

        static string GetColorTextForMPType(MPRequirement mptype)
        {
            switch (mptype)
            {
                //case MPRequirement.Client:
                //    return "green";
                //case MPRequirement.Unspecified:
                //    return "#FFFF99";
                case MPRequirement.All:
                    return "#FF3333";
                default:
                    return string.Empty;
            }
        }

        static string GetColoredMPTypeText(MPRequirement mptype)
        {
            switch (mptype)
            {
                case MPRequirement.None:
                    return "<color=#00CC00>Client</color>";
                case MPRequirement.Host:
                    return "<color=#00CC00>Host</color>";
                //case MPRequirement.Unspecified:
                //    return "<color=#FFFF99>Unspecified</color>";
                case MPRequirement.All:
                    return "<color=#FF3333>All</color>";
                default:
                    return mptype.ToString();
            }
        }

        void DrawModListModButton(PulsarMod pulsarMod)
        {
            bool UpdateAvailable = ModManager.Instance.UpdatesAviable.Any(m => m.Mod == pulsarMod);
            string ModName = UpdateAvailable ? $"(!) {pulsarMod.Name}" : pulsarMod.Name;

            if (pulsarMod.MPRequirements > (int)MPRequirement.Host)
            {
                if (DrawButtonSelected($"<color={GetColorTextForMPType((MPRequirement)pulsarMod.MPRequirements)}>{ModName}</color>", selectedMod == pulsarMod)) //FFFF99
                    selectedMod = pulsarMod;
            }
            else
            {
                if (DrawButtonSelected(ModName, selectedMod == pulsarMod))
                    selectedMod = pulsarMod;
            }
        }

        public static bool DrawButtonSelected(string text, bool selected)
        {
            if (selected)
            {
                bool returnvalue = Button(text, _SelectedButtonStyle);
                return returnvalue;
            }
            else
            {
                return Button(text);
            }
        }

        void DrawPlayerModList(PhotonPlayer player)
        {
            MPUserDataBlock userData = MPModCheckManager.Instance.GetNetworkedPeerMods(player);
            if (userData != null)
            {
                Label($"User PML version: {userData.PMLVersion}");
                Label("ModList:");
                string ModListText = string.Empty;
                bool first = true;
                foreach (MPModDataBlock modData in userData.ModData)
                {
                    if (first)
                        first = false;
                    else
                        ModListText += "\n";

                    ModListText += $"- {modData.ModName} v{modData.Version}, MPType: {GetColoredMPTypeText(modData.MPRequirement)}";
                }
                Label(ModListText);
            }
            else
            {
                Label("No Mod data.");
            }
        }

        public void OpenSettingsMenu(ModSettingsMenu menu)
        {
            if (Tab != 1)
                Tab = 1;
            else selectedSettings?.OnClose();

            selectedSettings = menu;
            selectedSettings.OnOpen();
        }

        void UpdateOnModRemoved(PulsarMod mod)
        {
            selectedMod = null;
            mods.Remove(mod);
            List<ModSettingsMenu> settingsToRemove = new List<ModSettingsMenu>();
            Assembly asm = mod.GetType().Assembly;
            settings.AsParallel().ForAll((arg) => { if (arg.GetType().Assembly == asm) settingsToRemove.Add(arg); });
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

using PulsarPluginLoader.Utilities;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace PulsarPluginLoader.Chat.Commands
{
    class ScreenshotCommand : IChatCommand
    {
        public string[] CommandAliases()
        {
            return new string[] { "screenshot", "ss" };
        }

        public string Description()
        {
            return "Saves a screenshot to disk.  Specifying \"ui\" will toggle the UI during capture.";
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]} [ui]";
        }

        public bool Execute(string arg, int SenderID)
        {
            string baseDir = Environment.ExpandEnvironmentVariables(@"%localappdata%low\Leafy Games, LLC\PULSAR Lost Colony");
            Directory.CreateDirectory(baseDir);

            // Make sure output directory for screenshots exists
            string outputDir = Path.Combine(baseDir, "screenshots");
            Directory.CreateDirectory(outputDir);

            // Create file namw
            int levelID = PLEncounterManager.Instance.GetCurrentPersistantEncounterInstance().LevelID;
            string visualType = PLServer.GetCurrentSector().VisualIndication.ToString();
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string screenshotPath = Path.Combine(outputDir, $"{levelID}_{visualType}_{timestamp}.png");

            // Take screenshot
            bool shouldToggleUI = arg.ToLower().Trim() == "ui";
            // Need a MonoBehavior to advance frames this way but chat commands can't be MonoBehaviors,
            // so hijack the UI instance since it does derive from MonoBehavior.
            PLInGameUI.Instance.StartCoroutine(WaitForScreenshot(screenshotPath, shouldToggleUI));

            Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Screenshot saved: {screenshotPath}");
            Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"File path copied to clipboard.");

            return false;
        }

        private IEnumerator WaitForScreenshot(string screenshotPath, bool shouldToggleUI)
        {
            yield return TakeScreenshot(screenshotPath, shouldToggleUI);
        }

        private IEnumerator TakeScreenshot(string screenshotPath, bool shouldToggleUI)
        {
            if (shouldToggleUI)
            {
                ToggleUI();
                yield return null;
            }

            ScreenCapture.CaptureScreenshot(screenshotPath);
            yield return null;


            if (shouldToggleUI)
            {
                ToggleUI();
                yield return null;
            }

            Clipboard.Copy(screenshotPath);
        }

        private void ToggleUI()
        {
            if (PLInGameUI.Instance != null)
            {
                PLInGameUI.Instance.UIIsHidden = !PLInGameUI.Instance.UIIsHidden;
            }
        }
        public bool PublicCommand()
        {
            return false;
        }
    }
}

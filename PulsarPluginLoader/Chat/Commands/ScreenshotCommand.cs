using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            return "Saves a screenshot to disk.";
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]}";
        }

        public bool Execute(string bugDescription)
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
            ScreenCapture.CaptureScreenshot(screenshotPath);
            Clipboard.Copy(screenshotPath);

            Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Screenshot saved: {screenshotPath}");
            Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"File path copied to clipboard.");

            return false;
        }
    }
}

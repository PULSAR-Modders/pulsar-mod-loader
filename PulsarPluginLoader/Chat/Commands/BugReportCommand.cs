using Pathfinding.Ionic.Zip;
using PulsarPluginLoader.Utilities;
using PulsarPluginLoader.Utilities.Uploaders;
using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace PulsarPluginLoader.Chat.Commands
{
    public class BugReport : IChatCommand
    {
        private static readonly IUploader uploader = new TransferShUploader();

        public string[] CommandAliases()
        {
            return new string[] { "bugreport", "br" };
        }

        public string Description()
        {
            return "Zips current debug log, screenshot, and bug description, then places zip file path in your paste buffer for easy upload.";
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]} [bug description]";
        }

        public bool Execute(string bugDescription)
        {
            string baseDir = Environment.ExpandEnvironmentVariables(@"%localappdata%low\Leafy Games, LLC\PULSAR Lost Colony");
            Directory.CreateDirectory(baseDir);

            // Make sure output directory for ZIPs exists
            string outputDir = Path.Combine(baseDir, "reports");
            Directory.CreateDirectory(outputDir);

            // Recreate clean working directory to hold archive contents
            string workingDir = Path.Combine(baseDir, "temp");
            PrepWorkingDirectory(workingDir);

            // Collect bug description, debug log, and screenshot in working directory
            File.WriteAllText(Path.Combine(workingDir, "description.txt"), bugDescription);
            File.Copy(Path.Combine(baseDir, "output_log.txt"), Path.Combine(workingDir, "output_log.txt"), overwrite: true);
            ScreenCapture.CaptureScreenshot(Path.Combine(workingDir, "screenshot.png"));

            // Compress working directory into single archive file
            Thread.Sleep(1000); // Wait for disk activity to finish so we don't miss certain files (TODO: Better option?)
            string archivePath = Path.Combine(outputDir, CreateArchiveName());
            ArchiveDirectory(workingDir, archivePath);

            // Upload archive
            //string resultUrl = uploader.UploadFile(archivePath);
            Clipboard.Copy(archivePath);
            Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Bug Report zipped and file path copied to clipboard.");

            // Clean up working directory
            Directory.Delete(workingDir, recursive: true);

            return false;
        }

        private void PrepWorkingDirectory(string workingDir)
        {
            // Make sure the directory exists
            Directory.CreateDirectory(workingDir);

            // Clear any files from failed previous runs
            foreach (string filePath in Directory.GetFiles(workingDir))
            {
                File.Delete(filePath);
            }
        }

        private string CreateArchiveName()
        {
            string gameVersion = PLNetworkManager.Instance.VersionString;
            string playerName = PLNetworkManager.Instance.LocalPlayer.GetPlayerName();
            string timestamp = DateTime.UtcNow.ToString("yyyy-dd-MM_HH-mm-ss");
            string tempName = $"PULSAR_{gameVersion}_Report_{playerName}_{timestamp}.zip";

            return String.Join("-", tempName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))
                .Trim()
                .TrimEnd('.');
        }

        private void ArchiveDirectory(string targetDir, string archivePath)
        {
            ZipFile zip = new ZipFile();
            zip.AddDirectory(targetDir);
            zip.Save(archivePath);
        }
    }
}

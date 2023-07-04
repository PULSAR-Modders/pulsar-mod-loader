using Microsoft.Win32;
using PulsarModLoader.Injections;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PulsarInjector
{
    class Injector
    {
        [STAThread] //Required for file dialog to work
        static void Main(string[] args)
        {
            string targetAssemblyPath = null;

            //Attempt install to argument path
            if (args.Length > 0)
            {
                targetAssemblyPath = args[0];
                if (AttemptInstallModLoader(targetAssemblyPath))
                {
                    return;
                }
            }

            //Attempt install to steam
            Logger.Info("Searching for Steam installation.");
            string steamPath = FindSteam();
            if (steamPath != null)
            {
                Logger.Info("Found Steam at " + steamPath);
                targetAssemblyPath = GetPulsarPathFromSteam(steamPath); 
                //^^^ writes it's own lines to the console.
                //If found:
                //Logger.Info("Found Pulsar: Lost Colony Installation at " + pulsarPath);

                if (targetAssemblyPath != null)
                {
                    Logger.Info("Install the mod loader here?");
                    Logger.Info("(Y/N)");
                    string answer = Console.ReadLine();
                    if (answer.ToLower().StartsWith("y") && AttemptInstallModLoader(targetAssemblyPath))
                    {
                        return;
                    }
                }
            }
            else
            {
                Logger.Info("Steam Installation not found.");
            }

            //Attempt install from windows OFD
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Logger.Info("Setting up OFD for windows. Please select a pulsar directory.");
                OpenFileDialog ofd = new OpenFileDialog
                {
                    InitialDirectory = "c:\\",
                    Filter = "Dynamic Linked Library (*.dll)|*.dll"
                };

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    targetAssemblyPath = ofd.FileName;
                    Logger.Info("Selected " + targetAssemblyPath);
                    if (AttemptInstallModLoader(targetAssemblyPath))
                    {
                        return;
                    }
                }
                else
                {
                    Logger.Info("OFD failed.");
                }
            }

            //Previous install attempts unsuccessfull, finishing dialogue.

            Console.ForegroundColor = ConsoleColor.Red;
            Logger.Info("Unable to find file.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Logger.Info("Please specify an assembly to inject (e.g., PULSARLostColony/PULSAR_LostColony_Data/Managed/Assembly-CSharp.dll)");
            Logger.Info("Ensure you have a mono branch copy of Pulsar: Lost Colony.");
            Logger.Info("Steam Users: Library > Pulsar: Lost Colony > Properties > Betas > 'mono - Mono branch'");
            Console.ForegroundColor = ConsoleColor.Gray;

            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        static string FindSteam()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                //Default steam install directory
                if (Directory.Exists(home + "/.steam/steam"))
                {
                    return home + "/.steam/steam";
                }
                //Flatpack steam install directory
                else if (Directory.Exists(home + "/.var/app/com.valvesoftware.Steam/.steam/steam"))
                {
                    return home + "/.var/app/com.valvesoftware.Steam/.steam/steam";
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //Get steam location from registry
                return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null);
            }
            return null;
        }

        static string GetPulsarPathFromSteam(string steamDir)
        {
            string libraryFolders = steamDir + Path.DirectorySeparatorChar + "steamapps" + Path.DirectorySeparatorChar + "libraryfolders.vdf";
            if (!File.Exists(libraryFolders))
            {
                return null;
            }
            Logger.Info("Reading " + libraryFolders);
            string fileContents = File.ReadAllText(libraryFolders);
            List<string> paths = new List<string>();
            paths.Add(steamDir);
            while (fileContents.Contains("\"path\"\t\t\""))
            {
                int index = fileContents.IndexOf("\"path\"\t\t\"") + 9;
                int index2;
                for (index2 = index; fileContents[index2] != '"'; index2++) ;
                paths.Add(fileContents.Substring(index, index2 - index));
                fileContents = fileContents.Substring(index2);
            }

            string pulsarPath = null;
            foreach (string path in paths)
            {
                pulsarPath = path + Path.DirectorySeparatorChar + "steamapps" + Path.DirectorySeparatorChar + "common" + Path.DirectorySeparatorChar + "PULSARLostColony";
                Logger.Info("Checking " + pulsarPath);
                if (Directory.Exists(pulsarPath))
                {
                    Logger.Info("Found Pulsar: Lost Colony Installation at " + pulsarPath);
                    return pulsarPath + Path.DirectorySeparatorChar + "PULSAR_LostColony_Data" + Path.DirectorySeparatorChar + "Managed" + Path.DirectorySeparatorChar + "Assembly-CSharp.dll";
                }
            }
            Logger.Info("Could not find Pulsar: Lost Colony installation in steam");
            return null;
        }

        static bool AttemptInstallModLoader(string inputDir)
        {
            Logger.Info("Checking file at " + inputDir);
            if (!inputDir.EndsWith("Assembly-CSharp.dll") && inputDir.Contains("PULSARLostColony"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Logger.Info("Path contains game directory, but isn't pointing to 'Assembly-CSharp.dll'");
                Logger.Info("Attempting to fix path.");
                Console.ForegroundColor = ConsoleColor.Gray;

                if (inputDir.Contains("PULSAR_LostColony_Data"))
                {
                    int Index = inputDir.LastIndexOf("PULSAR_LostColony_Data");
                    inputDir = inputDir.Remove(Index);
                    inputDir += "PULSARLostColony" + Path.DirectorySeparatorChar + "PULSAR_LostColony_Data" + Path.DirectorySeparatorChar + "Managed" + Path.DirectorySeparatorChar + "Assembly-CSharp.dll";
                }
                else
                {
                    int Index = inputDir.LastIndexOf("PULSARLostColony");
                    inputDir = inputDir.Remove(Index);
                    inputDir += "PULSARLostColony" + Path.DirectorySeparatorChar + "PULSAR_LostColony_Data" + Path.DirectorySeparatorChar + "Managed" + Path.DirectorySeparatorChar + "Assembly-CSharp.dll";
                }
            }
            if (inputDir.EndsWith("Assembly-CSharp.dll") && File.Exists(inputDir))
            {
                Logger.Info("File valid, Attempting installation at " + inputDir);
                InstallModLoader(inputDir);
                return true;
            }

            if (!File.Exists(inputDir) && inputDir.Contains("PULSARLostColony"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Logger.Info("A Pulsar: Lost Colony installation was detected but doesn't contain Mono branch files.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Logger.Info("File not valid.");
            Console.ForegroundColor = ConsoleColor.Gray;
            return false;
        }

        static void InstallModLoader(string targetAssemblyPath)
        {
            Logger.Info("=== Backups ===");
            string backupPath = Path.ChangeExtension(targetAssemblyPath, "bak");
            if (InjectionTools.IsModified(targetAssemblyPath))
            {
                if (File.Exists(backupPath))
                {
                    //Load from backup
                    File.Copy(backupPath, targetAssemblyPath, true);
                }
                else
                {
                    Logger.Info("The assembly is already modified, and a backup could not be found.");

                    Logger.Info("Press any key to continue...");
                    Console.ReadKey();

                    return;
                }
            }
            else
            {
                //Create backup
                Logger.Info("Making backup of hopefully clean assembly.");
                File.Copy(targetAssemblyPath, backupPath, true);
            }

            Logger.Info("=== Creating directories ===");
            string Modsdir = Path.Combine(Directory.GetParent(Path.GetDirectoryName(targetAssemblyPath)).Parent.FullName, "Mods");
            if (!Directory.Exists(Modsdir))
            {
                Logger.Info("Creating Mods Directory");
                Directory.CreateDirectory(Modsdir);
            }

            Logger.Info("=== Anti-Cheat ===");
            AntiCheatBypass.Inject(targetAssemblyPath);

            Logger.Info("=== Logging Modifications ===");
            InjectionTools.PatchMethod(targetAssemblyPath, "PLGlobal", "Start", typeof(LoggingInjections), "LoggingCleanup");

            Logger.Info("=== Injecting Harmony Initialization ===");
            InjectionTools.PatchMethod(targetAssemblyPath, "PLGlobal", "Awake", typeof(HarmonyInjector), "InitializeHarmony");

            Logger.Info("=== Copying Assemblies ===");
            CopyAssemblies(Path.GetDirectoryName(targetAssemblyPath));

            Logger.Info("Success!  You may now run the game normally.");

            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        static void CopyAssemblies(string targetAssemblyDir)
        {
            string PulsarModLoaderDll = CheckForUpdates(typeof(PulsarModLoader.PulsarMod).Assembly.Location);

            /* Copy important assemblies to target assembly's directory */
            string sourceDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] copyables = new string[] {
                PulsarModLoaderDll,
                Path.Combine(sourceDir, "0Harmony.dll")
            };

            foreach (string sourcePath in copyables)
            {
                string destPath = Path.Combine(targetAssemblyDir, Path.GetFileName(sourcePath).Replace("_updated.dll", ".dll"));
                Logger.Info($"Copying {Path.GetFileName(destPath)} to {Path.GetDirectoryName(destPath)}");
                try
                {
                    File.Copy(sourcePath, destPath, overwrite: true);
                }
                catch (IOException)
                {
                    Logger.Info("Copying failed!  Close the game and try again.");
                    Environment.Exit(0);
                }
            };
        }

        static string CheckForUpdates(string CurrentPMLDll)
        {
            string version = System.Diagnostics.FileVersionInfo.GetVersionInfo(CurrentPMLDll).FileVersion;
            bool useOtherDll = false;

            if (File.Exists(CurrentPMLDll.Replace(".dll", "_updated.dll")))
            {
                string UpdatedPMLDll = CurrentPMLDll.Replace(".dll", "_updated.dll");
                string UpdatedVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(UpdatedPMLDll).FileVersion;
                short[] versionAsNum = version.Split('.').Select(s => short.Parse(s)).ToArray();
                short[] UpdatedVersionAsNum = UpdatedVersion.Split('.').Select(s => short.Parse(s)).ToArray();

                for (byte i = 0; i < 4; i++)
                    if (UpdatedVersionAsNum[i] > versionAsNum[i])
                    {
                        CurrentPMLDll = UpdatedPMLDll;
                        version = UpdatedPMLDll;
                        useOtherDll = true;
                        break;
                    }
                    else if (UpdatedVersionAsNum[i] < versionAsNum[i])
                    {
                        break;
                    }
            }

            Logger.Info("=== Updates ===");
            Logger.Info("Check for a newer version of PML?");
            Logger.Info("(Y/N)");

            if (Console.ReadLine().ToUpper() == "N")
                return CurrentPMLDll;

            using (var web = new System.Net.WebClient())
            {
                web.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.71 Safari/537.36");

                string[] info = web.DownloadString("https://api.github.com/repos/PULSAR-Modders/pulsar-mod-loader/releases/latest").Split('\n');
                string versionFromInfo = info.First(i => i.Contains("tag_name"))
                    .Replace(@"  ""tag_name"": """, string.Empty)
                    .Replace(@""",", string.Empty); // for example: returns "0.10.4"

                if (version.StartsWith(versionFromInfo))
                    return CurrentPMLDll;

                Logger.Info($"New update available! Download {versionFromInfo}?");
                Logger.Info("(Y/N)");

                if (Console.ReadLine().ToUpper() == "N")
                    return CurrentPMLDll;

                string downloadLink = info.First(i => i.Contains("https://github.com/PULSAR-Modders/pulsar-mod-loader/releases/download") && i.Contains(".dll"))
                    .Replace(@"      ""browser_download_url"": """, string.Empty).Replace(@"""", string.Empty);
                string zipPath = CurrentPMLDll.Replace(".dll", ".zip");
                File.WriteAllBytes(zipPath, web.DownloadData(downloadLink));

                string newDllPath = useOtherDll ? CurrentPMLDll : CurrentPMLDll.Replace(".dll", "_updated.dll");

                using (var zipfile = Pathfinding.Ionic.Zip.ZipFile.Read(zipPath))
                {
                    var dll = zipfile.First(z => z.FileName.EndsWith("PulsarModLoader.dll"));
                    List<byte> bytes = new List<byte>();
                    using (var reader = dll.OpenReader())
                    {
                        while (reader.Position != reader.Length)
                            bytes.Add((byte)reader.ReadByte());
                    }
                    File.WriteAllBytes(newDllPath, bytes.ToArray());
                }

                File.Delete(zipPath);

                Logger.Info("Successfully updated!");

                return newDllPath;
            }
        }
    }
}

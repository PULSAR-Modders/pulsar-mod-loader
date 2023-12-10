using Microsoft.Win32;
using PulsarInjector.Injections;
using PulsarModLoader.Injections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PulsarInjector
{
    class Injector
    {
        static bool QuietMode = false;
        [STAThread] //Required for file dialog to work
        static void Main(string[] args)
        {
            try
            {
                Run(args);
            }
            catch (Exception e)
            {
                PMLWriteLine("Installer Crash!\n" + e);

                Console.ForegroundColor = ConsoleColor.White;
                PMLWriteLine("Press any key to close...");
                Console.ReadKey();
            }

        }

        static void Run(string[] args)
        {
            string targetAssemblyPath;

            foreach (string arg in args)
            {
                if (arg.ToLower().Contains("-q") && !arg.Contains(Path.DirectorySeparatorChar))
                {
                    QuietMode = true;
                    break;
                }
            }

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
            PMLWriteLine("Searching for Steam installation.");
            string steamPath = FindSteam();
            if (steamPath != null)
            {
                PMLWriteLine("Found Steam at " + steamPath);
                targetAssemblyPath = GetPulsarPathFromSteam(steamPath);
                //^^^ writes it's own lines to the console.
                //If found:
                //PMLWriteLine("Found Pulsar: Lost Colony Installation at " + pulsarPath);

                if (targetAssemblyPath != null)
                {
                    if (QuietMode && AttemptInstallModLoader(targetAssemblyPath))
                    {
                        return;
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    PMLWriteLine("Install the mod loader here?");
                    PMLWriteLine("(Y/N)");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    string answer = Console.ReadLine();
                    if (answer.ToLower().StartsWith("y") && AttemptInstallModLoader(targetAssemblyPath))
                    {
                        return;
                    }
                }
            }
            else
            {
                PMLWriteLine("Steam Installation not found.");
            }

            //Attempt install from windows OFD
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                PMLWriteLine("Setting up OFD for windows. Please select a pulsar directory.");
                OpenFileDialog ofd = new OpenFileDialog
                {
                    InitialDirectory = "c:\\",
                    Filter = "Dynamic Linked Library (*.dll)|*.dll"
                };

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    targetAssemblyPath = ofd.FileName;
                    PMLWriteLine("Selected " + targetAssemblyPath);
                    if (AttemptInstallModLoader(targetAssemblyPath))
                    {
                        return;
                    }
                }
                else
                {
                    PMLWriteLine("OFD failed.");
                }
            }

            //Previous install attempts unsuccessfull, finishing dialogue.

            Console.ForegroundColor = ConsoleColor.Red;
            PMLWriteLine("Unable to find file.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            PMLWriteLine("Please specify an assembly to inject (e.g., PULSARLostColony/PULSAR_LostColony_Data/Managed/Assembly-CSharp.dll)");
            PMLWriteLine("Ensure you have a mono branch copy of Pulsar: Lost Colony.");
            PMLWriteLine("Steam Users: Library > Pulsar: Lost Colony > Properties > Betas > 'mono - Mono branch'");
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.ForegroundColor = ConsoleColor.White;
            PMLWriteLine("Press any key to continue...");
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
            PMLWriteLine("Reading " + libraryFolders);
            string fileContents = File.ReadAllText(libraryFolders);
            List<string> paths = new List<string> {steamDir};
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
                PMLWriteLine("Checking " + pulsarPath);
                if (Directory.Exists(pulsarPath))
                {
                    PMLWriteLine("Found Pulsar: Lost Colony Installation at " + pulsarPath);
                    return pulsarPath + Path.DirectorySeparatorChar + "PULSAR_LostColony_Data" + Path.DirectorySeparatorChar + "Managed" + Path.DirectorySeparatorChar + "Assembly-CSharp.dll";
                }
            }
            PMLWriteLine("Could not find Pulsar: Lost Colony installation in steam");
            return null;
        }

        static bool AttemptInstallModLoader(string inputDir)
        {
            PMLWriteLine("Checking file at " + inputDir);
            if (!inputDir.EndsWith("Assembly-CSharp.dll") && inputDir.Contains("PULSARLostColony"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                PMLWriteLine("Path contains game directory, but isn't pointing to 'Assembly-CSharp.dll'");
                PMLWriteLine("Attempting to fix path.");
                Console.ForegroundColor = ConsoleColor.Gray;

                if (inputDir.Contains("PULSAR_LostColony_Data"))
                {
                    int Index = inputDir.LastIndexOf("PULSAR_LostColony_Data");
                    inputDir = inputDir.Remove(Index);
                    inputDir += "PULSAR_LostColony_Data" + Path.DirectorySeparatorChar + "Managed" + Path.DirectorySeparatorChar + "Assembly-CSharp.dll";
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
                PMLWriteLine("File valid, Attempting installation at " + inputDir);
                InstallModLoader(inputDir);
                return true;
            }

            if (!File.Exists(inputDir) && inputDir.Contains("PULSARLostColony"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                PMLWriteLine("A Pulsar: Lost Colony installation was detected but doesn't contain Mono branch files.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            PMLWriteLine("File not valid.");
            Console.ForegroundColor = ConsoleColor.Gray;
            return false;
        }

        static void InstallModLoader(string targetAssemblyPath)
        {
            PMLWriteLine("=== Backups ===");
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
                    PMLWriteLine("The assembly is already modified, and a backup could not be found.");

                    PMLWriteLine("Press any key to continue...");
                    Console.ReadKey();

                    return;
                }
            }
            else
            {
                //Create backup
                PMLWriteLine("Making backup of hopefully clean assembly.");
                File.Copy(targetAssemblyPath, backupPath, true);
            }

            PMLWriteLine("=== Creating directories ===");
            string Modsdir = Path.Combine(Directory.GetParent(Path.GetDirectoryName(targetAssemblyPath)).Parent.FullName, "Mods");
            if (!Directory.Exists(Modsdir))
            {
                PMLWriteLine("Creating Mods Directory");
                Directory.CreateDirectory(Modsdir);
            }

            PMLWriteLine("=== Logging Modifications ===");
            InjectionTools.PatchMethod(targetAssemblyPath, "PLGlobal", "Start", typeof(LoggingInjections), "LoggingCleanup");

            PMLWriteLine("=== Injecting Harmony Initialization ===");
            InjectionTools.PatchMethod(targetAssemblyPath, "PLGlobal", "Awake", typeof(HarmonyInjector), "InitializeHarmony");

            //CopyAssemblies. Has loggers in method.
            CopyAssemblies(Path.GetDirectoryName(targetAssemblyPath));

            PMLWriteLine("Success!  You may now run the game normally.");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                PMLWriteLine("The mods folder is being opened.");
                Process.Start("explorer.exe", Modsdir);
            }

            Console.ForegroundColor = ConsoleColor.White;
            PMLWriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        static void CopyAssemblies(string targetAssemblyDir)
        {
            string PulsarModLoaderDll = CheckForUpdates(typeof(PulsarModLoader.PulsarMod).Assembly.Location);

            PMLWriteLine("=== Copying Assemblies ===");
            // Copy important assemblies to target assembly's directory
            string sourceDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            CopyFileToDir(sourceDir, targetAssemblyDir, PulsarModLoaderDll);
            CopyFileToDir(sourceDir, targetAssemblyDir, "0Harmony.dll");
        }

        static void CopyFileToDir(string sourceDir, string destinationDir, string fileName)
        {
            string sourcePath = Path.Combine(sourceDir, fileName);
            string destPath = Path.Combine(destinationDir, Path.GetFileName(fileName.Replace("_updated.dll", ".dll")));
            PMLWriteLine($"Copying {fileName} to {Path.GetDirectoryName(destPath)}");
            try
            {
                File.Copy(sourcePath, destPath, overwrite: true);
            }
            catch (IOException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                PMLWriteLine("Copying failed! Ensure the game isn't running.");
                Console.ForegroundColor = ConsoleColor.Gray;
                PMLWriteLine(e.ToString());
                Console.ForegroundColor = ConsoleColor.White;
                PMLWriteLine("Press any key to close..");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        static string CheckForUpdates(string CurrentPMLDll)
        {
            if (QuietMode)
            {
                return CurrentPMLDll;
            }

            string version = System.Diagnostics.FileVersionInfo.GetVersionInfo(CurrentPMLDll).FileVersion;
            bool useOtherDll = false;

            if (File.Exists(CurrentPMLDll.Replace(".dll", "_updated.dll")))
            {
                string UpdatedPMLDll = CurrentPMLDll.Replace(".dll", "_updated.dll");
                string UpdatedVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(UpdatedPMLDll).FileVersion;
                short[] versionAsNum = version.Split('.').Select(s => short.Parse(s)).ToArray();
                short[] UpdatedVersionAsNum = UpdatedVersion.Split('.').Select(s => short.Parse(s)).ToArray();

                for (byte i = 0; i < 4; i++)
                {
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
            }

            PMLWriteLine("=== Updates ===");
            PMLWriteLine("Checking for a newer version of PML...");
            //PMLWriteLine("(Y/N)");
            //
            //if (Console.ReadLine().ToUpper() == "N")
            //    return CurrentPMLDll;

            using (var web = new System.Net.WebClient())
            {
                web.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.71 Safari/537.36");

                string[] info = web.DownloadString("https://api.github.com/repos/PULSAR-Modders/pulsar-mod-loader/releases/latest").Split('\n');
                string versionFromInfo = info.First(i => i.Contains("tag_name"))
                    .Replace(@"  ""tag_name"": """, string.Empty)
                    .Replace(@""",", string.Empty); // for example: returns "0.10.4"

                if (version.StartsWith(versionFromInfo))
                    return CurrentPMLDll;

                Console.ForegroundColor = ConsoleColor.White;
                PMLWriteLine($"New update available! Download {versionFromInfo}? (Current Verson: {version})");
                PMLWriteLine("(Y/N)");
                Console.ForegroundColor = ConsoleColor.Gray;

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

                PMLWriteLine("Successfully updated!");

                return newDllPath;
            }
        }

        static void PMLWriteLine(string text)
        {
            Console.WriteLine("[PMLInstaller]>" + text);
        }
    }
}

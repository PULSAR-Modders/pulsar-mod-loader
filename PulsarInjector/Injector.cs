using Microsoft.Win32;
using PulsarModLoader.Injections;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
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

            if (args.Length > 0)
            {
                targetAssemblyPath = args[0];
            }
            else
            {
                string steamPath = FindSteam();
                if (steamPath != null)
                {
                    Logger.Info("Found Steam at " + steamPath);
                    string pulsarPath = GetPulsarPath(steamPath);
                    if (pulsarPath != null)
                    {
                        Logger.Info("Found Pulsar at " + pulsarPath);
                        targetAssemblyPath = pulsarPath + Path.DirectorySeparatorChar + "PULSAR_LostColony_Data" +
                            Path.DirectorySeparatorChar + "Managed" + Path.DirectorySeparatorChar + "Assembly-CSharp.dll";
                    }
                }
            }

            Logger.Info("Searching for " + targetAssemblyPath);

            if (File.Exists(targetAssemblyPath))
            {
                if (args.Length > 0)
                {
                    InstallModLoader(targetAssemblyPath);
                    return;
                }
                else
                {
                    Logger.Info("File found. Install the mod loader here?");
                    Logger.Info("(Y/N)");
                    string answer = Console.ReadLine();
                    if (answer.ToLower().StartsWith("y"))
                    {
                        InstallModLoader(targetAssemblyPath);
                        return;
                    }
                }
            }
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OpenFileDialog ofd = new OpenFileDialog
                {
                    InitialDirectory = "c:\\",
                    Filter = "Dynamic Linked Library (*.dll)|*.dll"
                };

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    targetAssemblyPath = ofd.FileName;
                    Logger.Info("Selected " + targetAssemblyPath);
                    if (File.Exists(targetAssemblyPath))
                    {
                        InstallModLoader(targetAssemblyPath);
                        return;
                    }
                }
            }

            Logger.Info("Unable to find file");
            Logger.Info("Please specify an assembly to inject (e.g., PULSARLostColony/PULSAR_LostColony_Data/Managed/Assembly-CSharp.dll)");

            Logger.Info("Press any key to continue...");
            Console.ReadKey();
        }

        public static string FindSteam()
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

        public static string GetPulsarPath(string steamDir)
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
                for (index2 = index; fileContents[index2] != '"'; index2++);
                paths.Add(fileContents.Substring(index, index2 - index));
                fileContents = fileContents.Substring(index2);
            }
            foreach (string path in paths)
            {
                string pulsarPath = path + Path.DirectorySeparatorChar + "steamapps" + Path.DirectorySeparatorChar + "common" + Path.DirectorySeparatorChar + "PULSARLostColony";
                Logger.Info("Checking " + pulsarPath);
                if (Directory.Exists(pulsarPath))
                {
                    return pulsarPath;
                }
            }

            return null;
        }

        public static void InstallModLoader(string targetAssemblyPath)
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

        public static void CopyAssemblies(string targetAssemblyDir)
        {
            /* Copy important assemblies to target assembly's directory */
            string sourceDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] copyables = new string[] {
                typeof(PulsarModLoader.PulsarMod).Assembly.Location,
                Path.Combine(sourceDir, "0Harmony.dll")
            };

            foreach (string sourcePath in copyables)
            {
                string destPath = Path.Combine(targetAssemblyDir, Path.GetFileName(sourcePath));
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
    }
}

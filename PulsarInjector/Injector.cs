using PulsarModLoader.Injections;
using PulsarModLoader.Utilities;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PulsarInjector
{
    class Injector
    {
        static readonly string defaultPath = @"C:\Program Files (x86)\Steam\steamapps\common\PULSARLostColony\PULSAR_LostColony_Data\Managed\Assembly-CSharp.dll";
        static readonly string defaultLinuxPath = "~/.steam/steam/steamapps/common/PULSARLostColony/PULSAR_LostColony_Data/Managed/Assembly-CSharp.dll";
        //Default path does not work for OSX
        //static readonly string defaultMacPath = "~/Library/Application Support/Steam/steamapps/common/PULSARLostColony/PULSARLostColony.app/Contents/Resources/Data/Managed/Assembly-CSharp.dll";

        [STAThread] //Required for file dialog to work
        static void Main(string[] args)
        {
            string targetAssemblyPath = defaultPath;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                targetAssemblyPath = defaultLinuxPath;
            }
            //else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            //{
                //targetAssemblyPath = defaultMacPath;
            //}

            if (args.Length > 0)
            {
                targetAssemblyPath = args[0];
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
            Logger.Info("Please specify an assembly to inject (e.g., PULSARLostColony\\PULSAR_LostColony_Data\\Managed\\Assembly-CSharp.dll)");

            Logger.Info("Press any key to continue...");
            Console.ReadKey();
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
            string pluginsdir = Path.Combine(Path.GetDirectoryName(targetAssemblyPath), "Plugins");
            string Modsdir = Path.Combine(Directory.GetParent(Path.GetDirectoryName(targetAssemblyPath)).Parent.FullName, "Mods");
            if (Directory.Exists(pluginsdir))
            {
                Logger.Info("Moving Old Plugins Directory"); ;
                Directory.Move(pluginsdir, Modsdir);
            }
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

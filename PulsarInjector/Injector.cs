using PulsarPluginLoader.Injections;
using PulsarPluginLoader.Utils;
using System;
using System.IO;
using System.Reflection;

namespace PulsarInjector
{
    class Injector
    {
        static readonly string defaultPath = @"C:\Program Files (x86)\Steam\steamapps\common\PULSARLostColony\PULSAR_LostColony_Data\Managed\Assembly-CSharp.dll";

        static void Main(string[] args)
        {
            string targetAssemblyPath = defaultPath;

            if (args.Length > 0)
            {
                targetAssemblyPath = args[0];
            }

            if (!File.Exists(targetAssemblyPath))
            {
                Logger.Info("Please specify an assembly to inject (e.g., PULSARLostColony\\PULSAR_LostColony_Data\\Managed\\Assembly-CSharp.dll)");

                Logger.Info("Press any key to continue...");
                Console.ReadKey();

                return;
            }

            string backupPath = targetAssemblyPath.Substring(0, targetAssemblyPath.Length - 3) + "bak";
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
                File.Copy(targetAssemblyPath, backupPath, true);
            }

            Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(targetAssemblyPath), "Plugins"));

            AntiCheatBypass.Inject(targetAssemblyPath);

            InjectionTools.CreateMethod(targetAssemblyPath, "PLGlobal", "Start", typeof(void), null);
            InjectionTools.PatchMethod(targetAssemblyPath, "PLGlobal", "Start", typeof(LoggingInjections), "LoggingCleanup");

            InjectionTools.PatchMethod(targetAssemblyPath, "PLGlobal", "Awake", typeof(HarmonyInjector), "InitializeHarmony");
            
            InjectionTools.CreateModMessage(targetAssemblyPath);

            EventInjector.InjectEvents(targetAssemblyPath);

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
                typeof(PulsarPluginLoader.PulsarPlugin).Assembly.Location,
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

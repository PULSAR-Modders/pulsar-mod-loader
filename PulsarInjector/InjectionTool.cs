using PulsarPluginLoader;
using System;
using System.IO;

namespace PulsarInjector
{
    class InjectionTool
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
                Loader.Log("Please specify an assembly to inject (e.g., Assembly-CSharp.dll)");
                return;
            }

            Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(targetAssemblyPath), "Plugins"));
            Loader.CreateMethod(targetAssemblyPath, "PLGlobal", "Start", typeof(void), null);
            Loader.PatchMethod(targetAssemblyPath, "PLGlobal", "Awake", typeof(Loader), "InitializeHarmony");
            Loader.CopyAssemblies(Path.GetDirectoryName(targetAssemblyPath));

            Loader.Log("Success!  You may now run the game normally.");

            Loader.Log("Press any key to continue...");
            Console.ReadKey();
        }
    }
}

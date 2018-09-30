using System;
using System.IO;
using PulsarPluginLoader;

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
                Console.WriteLine("Please specify an assembly to inject (e.g., Assembly-CSharp.dll)");
                return;
            }

            Loader.Patch(targetAssemblyPath);
        }
    }
}

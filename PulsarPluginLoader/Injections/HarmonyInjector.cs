using Harmony;
using System.Reflection;

namespace PulsarPluginLoader.Injections
{
    public static class HarmonyInjector
    {
        public static void InitializeHarmony()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("wiki.pulsar.ppl");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

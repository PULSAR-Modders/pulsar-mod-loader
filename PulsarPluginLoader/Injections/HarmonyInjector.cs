using HarmonyLib;
using System.Reflection;

namespace PulsarPluginLoader.Injections
{
    public static class HarmonyInjector
    {
        public static void InitializeHarmony()
        {
            var harmony = new Harmony("wiki.pulsar.ppl");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

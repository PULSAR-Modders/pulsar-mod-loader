using HarmonyLib;
using System.Reflection;

namespace PulsarModLoader.Injections
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

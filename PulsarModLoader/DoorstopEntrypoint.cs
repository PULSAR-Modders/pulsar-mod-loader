using HarmonyLib;
using PulsarModLoader;
using PulsarModLoader.Patches;
using System;

namespace Doorstop
{
    class Entrypoint
    {
        public static void Start()
        {
            try
            {
                //File.WriteAllText("doorstop_hello.log", "Hello from Unity!");
                PMLInject();
            }
            catch(Exception e)
            {
                PulsarModLoader.Utilities.Logger.Info($"PML Init Exception\n{e}");
            }
        }

        private static void PMLInject()
        {
            //Get PLGlobal::Start Patched
            ModManager.Harmony = new Harmony("wiki.pulsar.pml");
            ModManager.Harmony.Patch(AccessTools.Method(typeof(PLGlobal), "Start"), new HarmonyMethod(typeof(PLGlobalStart), "Prefix"));
        }
    }
}
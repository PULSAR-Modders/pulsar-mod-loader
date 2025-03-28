using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PulsarModLoader.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulsarModLoader
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.USERS_PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("PULSAR_LostColony.exe")]
    public class BepinPlugin : BaseUnityPlugin
    {
        internal static BepinPlugin instance;
        internal static readonly Harmony Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Log;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "N/A")]
        private void Awake()
        {
            instance = this;
            Log = Logger;

            try
            {
                //File.WriteAllText("doorstop_hello.log", "Hello from Unity!");
                PMLInject();
                Log.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Initialized.");
            }
            catch (Exception e)
            {
                Log.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Init Exception\n{e}");
            }

        }

        private static void PMLInject()
        {
            //Get PLGlobal::Start Patched
            Harmony.Patch(AccessTools.Method(typeof(PLGlobal), "Start"), new HarmonyMethod(typeof(PLGlobalStart), "Prefix"));
        }
    }
}
